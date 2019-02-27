using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using VSLangProj;

namespace ReferenceAnalyzerTool
{
    public partial class Form1 : Form
    {
        public List<string> ProjectReferences
        {
            set
            {
                lvProjectReferences.Clear();
                foreach (var reference in value)
                {
                   var item = lvProjectReferences.Items.Add(reference);
                   item.Checked = true;
                }
            }
        }
        public Dictionary<string, string> FileReferences
        {
            set
            {
                lvFileReferences.Clear();
                lvFileReferences.Columns.Add("Name");
                lvFileReferences.Columns.Add("Path");
                foreach (var reference in value)
                {
                    var item = lvFileReferences.Items.Add(reference.Key);
                    item.SubItems.Add(reference.Value);
                    item.Checked = true;
                }
            }
        }

        public string RootNode;

        public Dictionary<string, ReferenceAnalyzer.ProjectFixDefinition> ProposedFixes
        {
            set
            {
                tvwProposedFixes.Nodes.Clear();
                lvLacking.Items.Clear();
                lvLacking.Columns.Add("Required Reference");
                tvwProposedFixes.ShowRootLines = true;
                var root = tvwProposedFixes.Nodes.Add(RootNode);
                foreach (var item in value)
                {
                    var projectnode = root.Nodes.Add(item.Key);
                    bool hasfixes = false;
                    bool hasbroken = false;
                    foreach (var fix in item.Value.FixDefinitions)
                    {
                        string name = string.Empty;
                        if (fix.orgType == ReferenceAnalyzer.FixDefinition.OrgType.File)
                            name = ((Reference)fix.orgReference).Path;
                        if(name==string.Empty)
                            name = ((Reference)fix.orgReference).Name;

                        string newname = string.Empty;
                        if (fix.newType == ReferenceAnalyzer.FixDefinition.NewType.Project)
                            newname = ((VSProject)fix.newReference).Project.Name;
                        else if (fix.newType == ReferenceAnalyzer.FixDefinition.NewType.File)
                            newname = (string)fix.newReference;
                                                
                        else if(fix.newType == ReferenceAnalyzer.FixDefinition.NewType.NoneFound)
                        {
                            var brokenchild = projectnode.Nodes.Add($"{fix.orgType.ToString()} Reference | {name}");
                            if(!lvLacking.Items.ContainsKey(name))
                            {
                                lvLacking.Items.Add(name, name, -1);
                            }

                            var nofix = brokenchild.Nodes.Add($"NO FIX WAS FOUND WITHIN THE SOLUTION");
                            nofix.ForeColor = Color.Red;
                            nofix.NodeFont = new Font(tvwProposedFixes.Font, FontStyle.Bold);
                            brokenchild.Checked = false;
                            nofix.Checked = false;
                            hasbroken = true;
                            continue;
                        }
                        var child = projectnode.Nodes.Add($"{fix.orgType.ToString()} Reference | {name}");
                        var fixchild = child.Nodes.Add($"Replace with {fix.newType.ToString()} Reference | {newname}");
                        child.Checked = true;
                        fixchild.Checked = true;
                        hasfixes = true;

                        if(fix.altReference != null)
                        {
                            newname = fix.altReference.ToString();
                            var altfix = child.Nodes.Add($"Alternate {fix.altType.ToString()} Reference | {newname}");
                            altfix.Checked = false;
                        }
                    }
                    if(hasfixes && hasbroken)
                    {
                        projectnode.Checked = true;
                        projectnode.BackColor = Color.Yellow;
                    }
                    else if(hasfixes)
                    {
                        projectnode.Checked = true;
                        projectnode.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        projectnode.ForeColor = Color.Wheat;
                        projectnode.BackColor = Color.OrangeRed;
                    }
                }
                root.Expand();
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        bool hasshown = false;
        private void Form1_Shown(object sender, EventArgs e)
        {
            if(!hasshown)
            {
                hasshown = true;
                lvLacking.Columns[0].Width = lvLacking.Width;
                lvFileReferences.Columns[0].Width = (int)(.25 * lvFileReferences.Width);
                lvFileReferences.Columns[1].Width = (int)(.75 * lvFileReferences.Width);
            }
        }
    }

}
