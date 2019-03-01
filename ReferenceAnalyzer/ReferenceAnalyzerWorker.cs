using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;
using VSLangProj;
using System.Windows;

namespace ReferenceAnalyzerTool
{
    public class ReferenceAnalyzerWorker
    {
        //ProjectItemTypes:
        private const string VirtualFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
        private const string CSharpProject = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
        private const string VbProject = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}";
        private const string DatabaseProject = "{00D1A9C2-B5F0-4AF3-8072-F6C62B433612}";
        private const string TestProject = "{F088123C-0E9E-452A-89E6-6BA2F21D5CAC}";

        private DTE _dte;
        private DTE2 _dte2;
        private UIHierarchy _solutionExplorer;
        private OutputWindowPane _output;
        private StatusBar _statusbar;
        private IVsThreadedWaitDialog2 _progressDialog;

        private AnalyzerWindow _analyzerWindow;
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider _serviceProvider;

        /// <summary>
        /// List of VSProjects by Project Name
        /// Used to update references 
        /// Key = ProjectName, Value = VSProject
        /// </summary>
        private Dictionary<string, VSProject> _dicProjects = new Dictionary<string, VSProject>();

        /// <summary>
        /// List of Projects where values are the File References we will be looking to replaced with Project references
        /// 
        /// Look to see if we can replace with 1) Project Reference 2) Solution File
        /// Key = ProjectName, Value = List<Reference> 
        /// </summary>
        private Dictionary<string, List<Reference>> _dicProjectFileReferences = new Dictionary<string, List<Reference>>();

        /// <summary>
        /// List of Projects where values are Lists of dll names that need to be replaced with Project references
        /// 
        /// Look to see if we can replace with 1) Project Reference 2) Solution File
        /// Key = ProjectName, Value = List<Reference> -->> We only have name of broken dll
        /// </summary>
        private Dictionary<string, List<Reference>> _dicProejctBrokenReferences = new Dictionary<string, List<Reference>>();


        /// <summary>
        /// The Dictionary of solution projects that are available to use as references
        /// Key is DLL File name built by the Project
        /// Value is the Project object that can be used to create a new ProejctReference
        /// </summary>
        private Dictionary<string, VSProject> _dicSolutionProjectReferences = new Dictionary<string, VSProject>();

        /// <summary>
        /// The Dictionary of solution File References that can be used as references
        /// Key is File Name, Value is the File Path that can be used to create a new Reference
        /// NOTE : Value here is the ONE thing we need to get from the Solution Explorer... the File Path
        /// </summary>
        private Dictionary<string, string> _dicSolutionFileReferences = new Dictionary<string, string>();

        /// <summary>
        /// The dictionary defines a File reference to Projects that are used to create ProejctReference
        /// </summary>
        private Dictionary<string, VSProject> _dicFileToProject = new Dictionary<string, VSProject>();

        /// <summary>
        /// The dictionary defines a Broken reference to Projects that are used to create ProejctReference - preferred
        /// </summary>
        private Dictionary<string, VSProject> _dicBrokenToProject = new Dictionary<string, VSProject>();
        /// <summary>
        /// The dictionary defines a Broken reference to File Reference table - if Project Reference is not found
        /// </summary>
        private Dictionary<string, string> _dicBrokenToFileReferencePath = new Dictionary<string, string>();

        /// <summary>
        /// The missing references to pass to the UI
        /// </summary>
        private List<string> _missingReferences = new List<string>();

        public enum SelectedType
        {
            Solution,
            Project,
            Folder
        }
        public static SelectedType _selectedType;



        /// <summary>
        /// Defines what is wrong with an individual reference and provides reference fixes if they exist
        /// Also provides user override to NOT fix if the user chooses to not fix it. This will 
        /// have to be set in a UI.
        /// </summary>
        public class FixDefinition
        {
            public enum OrgType { File, Broken };
            public enum NewType { Project, File, NoneFound };

            public bool fixit = true; // default to true, let the user say no

            public OrgType orgType;
            public Reference orgReference;

            // These types need to be object, NewType will determine if they are cast as Project or String (for filepath)
            public NewType newType;
            public object newReference; // Needs to be object type. OrgType determines if a VSProject or FilePath

            public bool useAlt;
            public NewType altType;
            public object altReference; // Needs to be object type. OrgType determines if a VSProject or FilePath
        }
        public class ProjectFixDefinition
        {
            public VSProject project;
            public List<FixDefinition> FixDefinitions = new List<FixDefinition>();

        }
        private Dictionary<string, ProjectFixDefinition> _proposedReferenceFixes =
            new Dictionary<string, ProjectFixDefinition>();

        private EnvDTE.SelectionEvents dteSelectionEvents;

        public void HandleNewFileSelection()
        {
            UIHierarchyItem selectedItem = null;
            //handler for SolutionExplorer click
            Array selectedItems = (Array)_solutionExplorer.SelectedItems;
            foreach (UIHierarchyItem item in selectedItems)
            {
                // Just take first selected item
                selectedItem = item;
                break;
            }

            if (selectedItem == null) return;

            _analyzerWindow.SelectedNode = selectedItem.Name;

            // Need to determine if Solution  
            //picked at this point because the Solution Name
            // can be the same as one of the Projects
            bool isSolution = false;
            var solution = _dte.Solution;
            try
            {
                isSolution = (selectedItem.Object.GetType() == typeof(EnvDTE.SolutionClass));
            }
            catch (Exception)
            {
                // Not a solution
            }


            Project selectedProject = null;

            // if IsSolution then we need to iterate all the projects and folders...
            if (isSolution)
            {
                _selectedType = SelectedType.Solution;
            }
            else
            {
                selectedProject = GetProject(solution, selectedItem.Name);
                if (selectedProject.Kind.ToUpper() == VirtualFolder)
                    _selectedType = SelectedType.Folder;
                else
                    _selectedType = SelectedType.Project;
            }
            _analyzerWindow.SelectedType = _selectedType;

        }

        public ReferenceAnalyzerWorker(AnalyzerWindow analyzerWindow,
            Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _analyzerWindow = analyzerWindow;
            _serviceProvider = serviceProvider;

            _dte = GetDte();
            _dte2 = _dte as DTE2;
            _statusbar = _dte2.StatusBar;

            // Not using this yet, but it looks like it could be useful
            var ivsSolution = GetSolution();

            _solutionExplorer = _dte2.ToolWindows.SolutionExplorer;

            this.dteSelectionEvents = _dte.Events.SelectionEvents;
            this.dteSelectionEvents.OnChange += 
                new _dispSelectionEvents_OnChangeEventHandler(this.HandleNewFileSelection);
        }


        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        public void Execute()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            UIHierarchyItem selectedItem = null;
            try
            {

                _dicProjects.Clear();
                _dicProjectFileReferences.Clear();
                _dicProejctBrokenReferences.Clear();
                _dicSolutionProjectReferences.Clear();
                _dicSolutionFileReferences.Clear();
                
                _progressDialog = GetProgressDialog();
                _output = CreateOrFindReferenceAnalyzerOutputWindow(
                    _dte2.ToolWindows.OutputWindow.OutputWindowPanes);

                _progressDialog.StartWaitDialog("Reference Analyzer",
                    "Scanning Solution Folders and Projects", "", null, "", 0, false, true);

                _output.Clear();
                _output.Activate();

                Array selectedItems = (Array)_solutionExplorer.SelectedItems;
                foreach (UIHierarchyItem item in selectedItems)
                {
                    // Just take first selected item
                    selectedItem = item;
                    break;
                }

                if (selectedItem == null) return;

                _analyzerWindow.SelectedNode = selectedItem.Name;

                // Need to determine if Solution was picked at this point because the Solution Name
                // can be the same as one of the Projects
                bool isSolution = false;
                var solution = _dte.Solution;
                try
                {
                    isSolution = (selectedItem.Object.GetType() == typeof(EnvDTE.SolutionClass));
                }
                catch (Exception)
                {
                    // Not a solution
                }


                Project selectedProject = null;

                // if IsSolution then we need to iterate all the projects and folders...
                if (isSolution)
                {
                    _selectedType = SelectedType.Solution;
                    TraverseSolution(solution);
                }
                else 
                {
                    selectedProject = GetProject(solution, selectedItem.Name);
                    if (selectedProject.Kind.ToUpper() == VirtualFolder)
                        _selectedType = SelectedType.Folder;
                    else
                        _selectedType = SelectedType.Project;

                    TraverseProjects(selectedProject);
                }
                _analyzerWindow.SelectedType = _selectedType;

                // Should eventually test to see what other types of projects we might have

                SetupProposedFixes();

                _output.OutputString($"\n\nProjects Analyzed ({_dicProjects.Count})\n");
                foreach (var item in _dicProjects)
                {
                    var project = item.Value;
                    _output.OutputString($"  {project.Project.Name} | {project.Project.FullName}\n");

                }

                _output.OutputString($"\n\nProjects That can be used as Project References\n");
                foreach (var item in _dicProjects)
                {
                    try
                    {
                        var project = item.Value;
                        var outputType = project.Project.Properties.Item("OutputType");
                        if (outputType != null && (int)outputType.Value == 2)
                        {
                            _output.OutputString($"  {project.Project.Name} | {project.Project.FullName}\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        _output.OutputString($"ERROR - {ex.Message}\n");
                    }
                }

                _output.OutputString($"\n\nFile References\n");
                foreach (var item in _dicProjectFileReferences)
                {
                    var name = item.Key;
                    var references = item.Value;
                    if (references.Count > 0)
                    {
                        _output.OutputString($"  Project - {name}\n");
                        foreach (Reference reference in references)
                        {
                            _output.OutputString($"    Reference - {reference.Name}\n");
                        }
                    }
                }

                _output.OutputString($"\n\nBroken References\n");
                foreach (var item in _dicProejctBrokenReferences)
                {
                    var name = item.Key;
                    var references = item.Value;
                    if (references.Count > 0)
                    {
                        _output.OutputString($"  Project - {name}\n");
                        foreach (var reference in references)
                        {
                            _output.OutputString($"    Reference - {reference.Name}\n");
                        }
                    }
                }

                //var referenceProject = _htProjects["ClassLibrary2"] as VSProject;
                //var targetProejct = _htProjects["FolderProcessor1"] as VSProject;
                //targetProejct.References.AddProject(referenceProject.Project);

            }
            catch (Exception ex)
            {
                _output.OutputString($"\n An exception was thrown processing Reference Analysis\n{ex.Message}");
            }
            finally
            {
                _statusbar.Text = "Ready";
                if (_progressDialog != null)
                {
                    _progressDialog.EndWaitDialog();
                }

                List<AnalyzerWindowControl.ReferenceView> projectReferenceView =
                        new List<AnalyzerWindowControl.ReferenceView>();
                if (_dicSolutionProjectReferences.Count > 0)
                {
                    foreach (var item in _dicSolutionProjectReferences)
                    {
                        var fullpath = item.Value.Project.Properties.Item("FullPath").Value.ToString();
                        var filename = item.Value.Project.Properties.Item("OutputFileName").Value.ToString();
                        var path = item.Value.Project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
                        projectReferenceView.Add(new AnalyzerWindowControl.ReferenceView
                        { UseIt = true, Name = item.Key, Path = (fullpath + path + filename) });

                    }
                }

                List<AnalyzerWindowControl.ReferenceView> fileReferenceView =
                    new List<AnalyzerWindowControl.ReferenceView>();
                if (_dicSolutionFileReferences.Count > 0)
                {
                    foreach (var item in _dicSolutionFileReferences)
                    {
                        fileReferenceView.Add(new AnalyzerWindowControl.ReferenceView
                        { UseIt = true, Name = item.Key, Path = item.Value });
                    }
                    _analyzerWindow.FileReferences = fileReferenceView;
                }

                _analyzerWindow.ProjectReferences = projectReferenceView;
                _analyzerWindow.FileReferences = fileReferenceView;

                _analyzerWindow.MissingReferences = _missingReferences;
                _analyzerWindow.ProposedFixes = _proposedReferenceFixes;

                // Make sure the window is showing
                IVsWindowFrame windowFrame = (IVsWindowFrame)_analyzerWindow.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
        }

        private void SetupProposedFixes()
        {
            // Sort everything in the solution we can use for references

            _dicSolutionProjectReferences.OrderBy(key => key.Key);
            _dicSolutionFileReferences.OrderBy(key => key.Key);

            // This is what we want to load up with problems and fixes
            _proposedReferenceFixes.Clear();

            // Temp lookup dictionaries
            _dicFileToProject.Clear();
            _dicBrokenToProject.Clear();
            _dicBrokenToFileReferencePath.Clear();
            _missingReferences.Clear();


            // 1) First need to search all the projects with file references an see if we have
            // Projects for them in the solution.
            // 2) Then need to search all the projects with broken references to see if we can fix them

            // Along the way, we will create a xref table of file reference name to project object
            // and then an xref table of broken references to project objecs
            // and finally a table of broken references to file references (paths)

            // See if each file reference in each project can be modified to be a project reference
            // While doing so, create a FileReferenceToProject for quicker processing
            foreach (var project in _dicProjectFileReferences)
            {
                // iterate File References in this project
                foreach (var reference in project.Value)
                {
                    // 1) See if this is a reference we CAN replace...
                    if (!_dicSolutionProjectReferences.ContainsKey(reference.Name))
                        continue; // We can't do anything with this FileReference

                    // Bingo! Here is a File Reference that we CAN replace with a Project Reference

                    // 2) Have we created an entry in the File to Project xref table?
                    VSProject vsproject = _dicSolutionProjectReferences[reference.Name];
                    if (!_dicFileToProject.ContainsKey(reference.Name))
                    {
                        // 2.a Then create one
                        _dicFileToProject.Add(reference.Name, vsproject);
                    }
                    // 2.b Check to see if we have a proposed fix for this... 
                    // we are after all looping thru projects with file references and we found a 
                    // file reference we can replace with a project reference
                    ProjectFixDefinition pfd = null;
                    if (!_proposedReferenceFixes.ContainsKey(project.Key))
                    {
                        pfd = new ProjectFixDefinition();
                        pfd.project = _dicProjects[project.Key];
                        _proposedReferenceFixes.Add(project.Key, pfd);
                    }
                    else
                    {
                        // We already have an entry for this project.
                        // Update the Fix Definition
                        // Since we are iterating the File References, this would not have the preferred 
                        // Project Reference fix for THIS reference!
                        pfd = _proposedReferenceFixes[project.Key];
                    }
                    if (pfd == null) continue;

                    var fd = new FixDefinition();
                    fd.orgType = FixDefinition.OrgType.File;
                    fd.orgReference = reference;
                    fd.newType = FixDefinition.NewType.Project;
                    fd.newReference = vsproject;
                    pfd.FixDefinitions.Add(fd);


                }

            }

            // Broken references may have to choices, File and Project references
            foreach (var project in _dicProejctBrokenReferences)
            {
                // iterate broken References in this project
                foreach (var reference in project.Value)
                {
                    // 1) See if this is a reference we CAN replace...
                    bool hasProjectRef = _dicSolutionProjectReferences.ContainsKey(reference.Name);
                    bool hasFileRef = _dicSolutionFileReferences.ContainsKey(reference.Name + ".dll");
                    if (!hasProjectRef && !hasFileRef)
                    {
                        //continue; // We can't do anything with this Broken Reference

                        // Don't want to continue in this case. Since it has a broken reference
                        // we need to show it to the user!
                        ProjectFixDefinition pfd = null;
                        if (!_proposedReferenceFixes.ContainsKey(project.Key))
                        {
                            pfd = new ProjectFixDefinition
                            {
                                project = _dicProjects[project.Key]
                            };
                            _proposedReferenceFixes.Add(project.Key, pfd);
                        }
                        else
                        {
                            // We already have an entry for this project.
                            // Update the Fix Definition
                            // Since we are iterating the File References, this would not have the preferred 
                            // Project Reference fix for THIS reference!
                            pfd = _proposedReferenceFixes[project.Key];
                        }
                        if (pfd == null) continue;

                        var fd = new FixDefinition
                        {
                            orgType = FixDefinition.OrgType.Broken,
                            orgReference = reference,
                            newType = FixDefinition.NewType.NoneFound
                        };
                        pfd.FixDefinitions.Add(fd);

                        if (_missingReferences.Contains(reference.Name + ".dll")) continue;

                        _missingReferences.Add(reference.Name + ".dll");

                    }
                    // Bingo! Here is a File Reference that we CAN replace with a Project Reference

                    // 2) Have we created an entry in the File to Project xref table?
                    if (hasProjectRef)
                    {
                        var vsproject = _dicSolutionProjectReferences[reference.Name];
                        if (!_dicBrokenToProject.ContainsKey(reference.Name))
                        {
                            // 2.a Then create one
                            _dicBrokenToProject.Add(reference.Name, vsproject);
                        }
                        // 2.b Check to see if we have a proposed fix for this... 
                        // we are after all looping thru projects with file references and we found a 
                        // file reference we can replace with a project reference
                        ProjectFixDefinition pfd = null;
                        if (!_proposedReferenceFixes.ContainsKey(project.Key))
                        {
                            pfd = new ProjectFixDefinition
                            {
                                project = _dicProjects[project.Key]
                            };
                            _proposedReferenceFixes.Add(project.Key, pfd);
                        }
                        else
                        {
                            // We already have an entry for this project.
                            // Update the Fix Definition
                            // Since we are iterating the File References, this would not have the preferred 
                            // Project Reference fix for THIS reference!
                            pfd = _proposedReferenceFixes[project.Key];
                        }
                        if (pfd == null) continue;

                        var fd = new FixDefinition
                        {
                            orgType = FixDefinition.OrgType.Broken,
                            orgReference = reference,
                            newType = FixDefinition.NewType.Project,
                            newReference = vsproject
                        };
                        pfd.FixDefinitions.Add(fd);

                    }

                    if (hasFileRef)
                    {
                        var filereference = _dicSolutionFileReferences[reference.Name + ".dll"];
                        if (!_dicBrokenToFileReferencePath.ContainsKey(reference.Name))
                        {
                            // 2.a Then create one
                            _dicBrokenToFileReferencePath.Add(reference.Name, filereference);
                        }
                        // 2.b Check to see if we have a proposed fix for this... 
                        // we are after all looping thru projects with file references and we found a 
                        // file reference we can replace with a project reference
                        ProjectFixDefinition pfd = null;
                        if (!_proposedReferenceFixes.ContainsKey(project.Key))
                        {
                            pfd = new ProjectFixDefinition
                            {
                                project = _dicProjects[project.Key]
                            };
                            _proposedReferenceFixes.Add(project.Key, pfd);
                        }
                        else
                        {
                            // We already have an entry for this project.
                            // Update the Fix Definition
                            // Since we are iterating the File References, this would not have the preferred 
                            // Project Reference fix for THIS reference!
                            pfd = _proposedReferenceFixes[project.Key];
                        }
                        if (pfd == null) continue;


                        if (hasProjectRef)
                        {
                            // Then we just added it!
                            var fd = pfd.FixDefinitions[pfd.FixDefinitions.Count - 1];
                            fd.altType = FixDefinition.NewType.File;
                            fd.altReference = filereference;
                        }
                        else
                        {
                            var fd = new FixDefinition
                            {
                                orgType = FixDefinition.OrgType.Broken,
                                orgReference = reference,

                                newType = FixDefinition.NewType.File,
                                newReference = filereference
                            };

                            pfd.FixDefinitions.Add(fd);
                        }

                    }

                }

            }


        }

        /// <summary>Traverses the solution.</summary>
        /// <param name="solution">The solution.</param>
        private void TraverseSolution(Solution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (Project project in solution.Projects)
                TraverseProjects(project);
        }

        /// <summary>
        /// Traverses the projects.
        /// </summary>
        /// <param name="project">The project.</param>
        private void TraverseProjects(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (project.Kind.ToUpper() == VirtualFolder)
            {
                _output.OutputString($"\nTraversing Solution Folder - {project.Name}:\n\n");
                var innerProjects = GetSolutionFolderProjects(project);
                foreach (var innerProject in innerProjects)
                {
                    WriteReferences(innerProject);
                }
            }
            else
            {
                WriteReferences(project);
            }
        }

        /// <summary>
        /// Writes the references.
        /// </summary>
        /// <param name="project">The project.</param>
        private void WriteReferences(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var progressdialog = GetProgressDialog();

            VSProject vsProject = project.Object as VSProject;

            // These only set for debugging purposes...
            //VSProject2 vsProject = project.Object as VSProject2;
            //VSProject3 vsProject = project.Object as VSProject3;
            //VSProject4 vsProject = project.Object as VSProject4;


            if (project.Kind.ToUpper() == DatabaseProject)
            {
                _output.OutputString($"\nSkipping Database Project - {project.Name}:\n\n");
                return;
            }
            if (project.Kind.ToUpper() == TestProject)
            {
                if (vsProject != null && vsProject.References != null)
                {
                    _output.OutputString($"References for Test Project - {project.Name}:\n");
                }
                else
                {
                    _output.OutputString($"\nSkipping Test Project - {project.Name}:\n\n");
                    return;
                }
            }
            else if (project.Kind.ToUpper() == CSharpProject)
            {
                _output.OutputString($"References for C# Project - {project.Name}:\n");
            }
            else if (project.Kind.ToUpper() == VbProject)
            {
                _output.OutputString($"References for VB Project - {project.Name}:\n");
            }
            else if (vsProject == null)
            {
                _output.OutputString($"\nUNKONWN Non VSProject Kind - {project.Name} Kind {project.Kind.ToUpper()}\n\n");
                return;
            }

            References references = vsProject.References;
            if (references == null)
            {
                _output.OutputString($"\nProject Has no references - {project.Name} Kind {project.Kind.ToUpper()}\n\n");
                return;
            }

            _statusbar.Text = vsProject.Project.FullName;
            bool cancel;
            _progressDialog.UpdateProgress("Scanning Solution Folders and Projects",
                $"Scanning {project.Name}", "", 0, 0, true, out cancel);

            _dicProjects.Add(project.Name, vsProject);
            _dicProjectFileReferences.Add(project.Name, new List<Reference>());
            _dicProejctBrokenReferences.Add(project.Name, new List<Reference>());

            try
            {
                var outputType = project.Properties.Item("OutputType");
                if (outputType != null && (int)outputType.Value == 2)
                {
                    if (!_dicSolutionProjectReferences.ContainsKey(project.Name))
                        _dicSolutionProjectReferences.Add(project.Name, vsProject);
                }
            }
            catch (Exception ex)
            {
                // Not a class library project
            }

            foreach (Reference reference in references)
            {
                // NOTE::: Should i use List<Reference> for both of these? Does a broken reference give me anything except Name?

                if (reference.Path == string.Empty)
                {
                    _output.OutputString($" BROKEN REFERENCE - {reference.Name}\n");
                    _dicProejctBrokenReferences[project.Name].Add(reference);
                }
                else
                {
                    if (reference.SourceProject == null)
                    {
                        _dicProjectFileReferences[project.Name].Add(reference);
                    }

                    _output.OutputString($" {reference.Name} | {reference.Path}\n");
                }
            }
        }

        /// <summary>
        /// Gets the solution folder projects.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        private IEnumerable<Project> GetSolutionFolderProjects(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<Project> projects = new List<Project>();
            var y = (project.ProjectItems as ProjectItems).Count;
            for (var i = 1; i <= y; i++)
            {
                Project subProject = project.ProjectItems.Item(i).SubProject as Project;
                if (subProject != null)
                {
                    projects.Add(subProject);
                }
                else
                {
                    //var item = _solutionExplorer.Parent.Collection.Item(project.ProjectItems.Item(i).Name);

                    // we could be a subfolder, or possibly a solution reference in a ThirdPartyLibs type folder
                    // Among many other things, but we need to address both of these!!!
                    _output.OutputString($"UNADDRESSED File or Folder? - {project.ProjectItems.Item(i).Name} | " +
                        $"{project.ProjectItems.Item(i).Kind}\n");
                    if (project.ProjectItems.Item(i).Name.EndsWith(".dll") &&
                        !_dicSolutionFileReferences.ContainsKey(project.ProjectItems.Item(i).Name))
                        _dicSolutionFileReferences.Add(project.ProjectItems.Item(i).Name,
                            project.ProjectItems.Item(i).FileNames[1]); // Need the full path file name to create a reference
                }
            }

            return projects;
        }


        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Project {name}</exception>
        public static Project GetProject(Solution solution, string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var project = GetProject(solution.Projects.OfType<Project>(), name);

            if (project == null)
            {
                throw new Exception($"Project {name} not found in solution");
            }

            return project;
        }

        /// <summary>
        /// Gets the project.
        /// </summary>
        /// <param name="projects">The projects.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Project GetProject(IEnumerable<Project> projects, string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            foreach (Project project in projects)
            {
                var projectName = project.Name;
                if (projectName == name)
                {
                    return project;
                }
                else if (project.Kind.ToUpper() == VirtualFolder)
                {
                    var subProjects = project
                        .ProjectItems
                        .OfType<ProjectItem>()
                        .Where(item => item.SubProject != null)
                        .Select(item => item.SubProject);

                    var projectInFolder = GetProject(subProjects, name);

                    if (projectInFolder != null)
                    {
                        return projectInFolder;
                    }
                }
            }

            return null;
        }

  
        /// <summary>
        /// Creates the or find reference helper output window.
        /// </summary>
        /// <param name="outputWindowPanes">The output window panes.</param>
        /// <returns></returns>
        private OutputWindowPane CreateOrFindReferenceAnalyzerOutputWindow(OutputWindowPanes outputWindowPanes)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            for (int i = 1; i <= outputWindowPanes.Count; i++)
            {
                string name = outputWindowPanes.Item(i).Name;
                if (name == "Reference Analyzer Log")
                    return outputWindowPanes.Item(i);
            }

            return outputWindowPanes.Add("Reference Analyzer Log");
        }

        /// <summary>
        /// Gets the DTE.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        private DTE GetDte()
        {
            var task = _serviceProvider.GetServiceAsync(typeof(DTE));
            Assumes.Present(task);
            var dte = task.Result as DTE;
            return dte;
        }

        private IVsSolution GetSolution()
        {
            var task = _serviceProvider.GetServiceAsync(typeof(IVsSolution));
            Assumes.Present(task);
            var solution = task.Result as IVsSolution;
            return solution;
        }

        //// Have a datamember for this because this method did not work for some reason
        //private AnalyzerWindow GetAnalyzerWindow()
        //{
        //    var task = _serviceProvider.GetServiceAsync(typeof(AnalyzerWindow));
        //    Assumes.Present(task);
        //    var window = task.Result as AnalyzerWindow;
        //    return window;
        //}


        private IVsThreadedWaitDialog2 GetProgressDialog()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var task = _serviceProvider.GetServiceAsync(typeof(SVsThreadedWaitDialogFactory));
            Assumes.Present(task);
            var factory = task.Result as IVsThreadedWaitDialogFactory;
            IVsThreadedWaitDialog2 dialog = null;
            if (factory != null)
            {
                factory.CreateInstance(out dialog);
            }
            return dialog;
        }


    }
}
