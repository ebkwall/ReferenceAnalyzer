namespace ReferenceAnalyzerTool
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Controls;
    using VSLangProj;

    /// <summary>
    /// Interaction logic for AnalyzerWindowControl.
    /// </summary>
    public partial class AnalyzerWindowControl : UserControl
    {
        private ReferenceAnalyzer _analyzer;
        public ReferenceAnalyzer Analyzer { set { _analyzer = value; } }


        public class ReferenceView
        {
            public bool UseIt { get; set; }
            public string Name{ get; set; }
            public string Path { get; set; }
        }

        public ReferenceAnalyzer.SelectedType SelectedType
        {
            set
            {
                lblSelectedType.Content = value.ToString();
            }
        }

        private string _selectedNode;
        public string SelectedNode
        {
            set
            {
                _selectedNode = value;
                lblSelectedItem.Content = _selectedNode;
            }
        }

        public List<ReferenceView> ProjectReferences
        {
            set
            {
                lvProjectReferences.ItemsSource = value;
            }
        }
        public List<ReferenceView> FileReferences
        {
            set
            {
                lvFileReferences.ItemsSource = value;
            }
        }

        public List<string> MissingReferences
        {
            set
            {
                // This doesnt have actual databinding
                // need old fashion grease
                lvMissingReferences.Items.Clear();
                foreach (var item in value)
                {
                    lvMissingReferences.Items.Add(item);
                }
 }
        }

        public Dictionary<string, ReferenceAnalyzer.ProjectFixDefinition> ProposedFixes
        {
            set
            {
                tvwProposedFixes.Items.Clear();
                var root = new TreeViewItem();
                root.Header = _selectedNode;
                root.Foreground = System.Windows.Media.Brushes.Wheat;
                tvwProposedFixes.Items.Add(root);

                foreach (var item in value)
                {
                    var projectnode = new TreeViewItem
                    {
                        Header = item.Key,
                        Foreground = System.Windows.Media.Brushes.Wheat
                    };
                    root.Items.Add(projectnode);

                    bool hasfixes = false;
                    bool hasbroken = false;
                    foreach (var fix in item.Value.FixDefinitions)
                    {
                        string name = string.Empty;
                        if (fix.orgType == ReferenceAnalyzer.FixDefinition.OrgType.File)
                            name = ((Reference)fix.orgReference).Path;
                        if (name == string.Empty)
                            name = ((Reference)fix.orgReference).Name;

                        string newname = string.Empty;
                        if (fix.newType == ReferenceAnalyzer.FixDefinition.NewType.Project)
                            newname = ((VSProject)fix.newReference).Project.Name;
                        else if (fix.newType == ReferenceAnalyzer.FixDefinition.NewType.File)
                            newname = (string)fix.newReference;

                        else if (fix.newType == ReferenceAnalyzer.FixDefinition.NewType.NoneFound)
                        {
                            var brokenchild = new TreeViewItem
                            {
                                Header = $"{fix.orgType.ToString()} Reference | {name}",
                                Foreground = System.Windows.Media.Brushes.IndianRed 
                            };
                            projectnode.Items.Add(brokenchild);

                            hasbroken = true;
                            continue;
                        }
                        var child = new TreeViewItem();
                        child.Header = $"{fix.orgType.ToString()} Reference | {name}";
                        child.Foreground = System.Windows.Media.Brushes.Wheat;
                        projectnode.Items.Add(child);

                        var fixchild = new TreeViewItem();
                        fixchild.Header = $"Replace with {fix.newType.ToString()} Reference | {newname}";
                        fixchild.Foreground = System.Windows.Media.Brushes.Wheat;
                        child.Items.Add(fixchild);
                        child.IsSelected = true;
                        fixchild.IsSelected = true;
                        hasfixes = true;

                        if (fix.altReference != null)
                        {
                            newname = fix.altReference.ToString();
                            var altfix = new TreeViewItem();
                            altfix.Header = $"Alternate {fix.altType.ToString()} Reference | {newname}";
                            altfix.Foreground = System.Windows.Media.Brushes.Wheat;
                            child.Items.Add(altfix);
                            altfix.IsSelected = false;
                        }
                    }
                    if (hasfixes && hasbroken)
                    {
                        projectnode.IsSelected = true;
                        projectnode.Foreground = System.Windows.Media.Brushes.Yellow;
                    }
                    else if (hasfixes)
                    {
                        projectnode.IsSelected = true;
                        projectnode.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    else
                    {
                        projectnode.Foreground = System.Windows.Media.Brushes.IndianRed;
                    }
                }
                root.IsExpanded = true;

            }
        }

        private ObservableCollection<ReferenceView> _projectReferencesObservableCollection;
        public ObservableCollection<ReferenceView> ProjectReferencesObservableCollection
        {
            get { return _projectReferencesObservableCollection; }
            set
            {
                _projectReferencesObservableCollection = value;
                //RaisePropertyChanged(() => Privileges);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzerWindowControl"/> class.
        /// </summary>
        public AnalyzerWindowControl()
        {
            this.InitializeComponent();
        }

        private void btnScanIt_Click(object sender, RoutedEventArgs e)
        {
            _analyzer.Execute(sender, e);
        }
        private void btnFixIt_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}