namespace ReferenceAnalyzerTool
{
    using Microsoft.VisualStudio.Shell;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using VSLangProj;
    using static ReferenceAnalyzerTool.ReferenceAnalyzerWorker;

    /// <summary>
    /// Interaction logic for AnalyzerWindowControl.
    /// </summary>
    public partial class AnalyzerWindowControl : UserControl
    {
        public class ReferenceView
        {
            public bool UseIt { get; set; }
            public string Name{ get; set; }
            public string Path { get; set; }
        }

        public SelectedType SelectedType
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
                // This doesn't have actual data binding
                // need old fashion grease
                lvMissingReferences.Items.Clear();
                foreach (var item in value)
                {
                    lvMissingReferences.Items.Add(item);
                }
 }
        }

        public Dictionary<string, ProjectFixDefinition> ProposedFixes
        {
            set
            {
                tvwProposedFixes.Items.Clear();
                var root = new TreeViewItem
                {
                    Header = _selectedNode,
                    Foreground = System.Windows.Media.Brushes.Wheat
                };
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
                        if (fix.orgType == FixDefinition.OrgType.File)
                            name = ((Reference)fix.orgReference).Path;
                        if (name == string.Empty)
                            name = ((Reference)fix.orgReference).Name;

                        string newname = string.Empty;
                        if (fix.newType == FixDefinition.NewType.Project)
                            newname = ((VSProject)fix.newReference).Project.Name;
                        else if (fix.newType == FixDefinition.NewType.File)
                            newname = (string)fix.newReference;

                        else if (fix.newType == FixDefinition.NewType.NoneFound)
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
                        var child = new TreeViewItem
                        {
                            Header = $"{fix.orgType.ToString()} Reference | {name}",
                            Foreground = System.Windows.Media.Brushes.Wheat
                        };
                        projectnode.Items.Add(child);

                        var fixchild = new TreeViewItem
                        {
                            Header = $"Replace with {fix.newType.ToString()} Reference | {newname}",
                            Foreground = System.Windows.Media.Brushes.Wheat
                        };
                        child.Items.Add(fixchild);
                        child.IsSelected = true;
                        fixchild.IsSelected = true;
                        hasfixes = true;

                        if (fix.altReference != null)
                        {
                            newname = fix.altReference.ToString();
                            var altfix = new TreeViewItem
                            {
                                Header = $"Alternate {fix.altType.ToString()} Reference | {newname}",
                                Foreground = System.Windows.Media.Brushes.Wheat
                            };
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
                root.ExpandSubtree();

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

        public void btnScanIt_Click(object sender, RoutedEventArgs e)
        {
            Common.Worker.Execute();
        }
        public void btnFixIt_Click(object sender, RoutedEventArgs e)
        {

        }

        public void CboPerferedType_DropDownOpened(object sender, System.EventArgs e)
        {
            cboPerferedType.Background = System.Windows.Media.Brushes.Black;
        }

        public void CboPerferedType_DropDownClosed(object sender, System.EventArgs e)
        {
            cboPerferedType.Background = System.Windows.Media.Brushes.Transparent;
        }
    }
}