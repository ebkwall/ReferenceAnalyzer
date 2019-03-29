using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using static ReferenceAnalyzerTool.ReferenceAnalyzer;

namespace ReferenceAnalyzerTool
{
    class SolutionChangeHandler
    {

        private DTE _dte;
        private UIHierarchy _solutionExplorer;
        private SelectionEvents _dteSelectionEvents;

        public SolutionChangeHandler()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _dte = Common.dte;
            var _dte2 = _dte as DTE2;
            _solutionExplorer = _dte2.ToolWindows.SolutionExplorer;

            _dteSelectionEvents = _dte.Events.SelectionEvents;
            _dteSelectionEvents.OnChange += HandleNewFileSelection;
        }

        public void HandleNewFileSelection()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_solutionExplorer == null || _solutionExplorer.SelectedItems == null)
                return;

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

            Common.analyzerWindow.SelectedNode = selectedItem.Name;

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
            SelectedType selectedType = SelectedType.Solution;

            // if IsSolution then we need to iterate all the projects and folders...
            if (isSolution)
            {
                selectedType = SelectedType.Solution;
            }
            else
            {
                selectedProject = GetProject(solution, selectedItem.Name);
                if (selectedProject == null)
                {
                    // Nothing selected so... default to solution
                    selectedType = SelectedType.Solution;
                }
                else if (selectedProject.Kind.ToUpper() == VirtualFolder)
                {
                    selectedType = SelectedType.Folder;
                }
                else
                {
                    selectedType = SelectedType.Project;
                }
            }
            Common.analyzerWindow.SelectedType = selectedType;
        }
    }
}
