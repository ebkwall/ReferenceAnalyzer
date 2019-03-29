using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using System.Collections.ObjectModel;
using static ReferenceAnalyzerTool.AnalyzerWindowControl;

namespace ReferenceAnalyzerTool
{
    public static class Common
    {
        // These are required to be set before the Worker can be used
        //public static Microsoft.VisualStudio.Shell.IAsyncServiceProvider AnalyzerProvider { set; get; }
        //public static Microsoft.VisualStudio.Shell.IAsyncServiceProvider WindowProvider { set; get; }

        private static ObservableCollection<ReferenceView> _projectReferencesObservableCollection =
            new ObservableCollection<ReferenceView>();
        public static ObservableCollection<ReferenceView> ProjectReferencesObservableCollection
        {
            get { return _projectReferencesObservableCollection; }
            set
            {
                _projectReferencesObservableCollection = value;
            }
        }

        internal static ReferenceAnalyzer Worker { set; get; }
        internal static SolutionChangeHandler SolutionEventHandler { set; get; }

        public static AnalyzerWindow analyzerWindow;
        private static Microsoft.VisualStudio.Shell.IAsyncServiceProvider _serviceProvider;
        public static Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return _serviceProvider;
            }
            set
            {
                _serviceProvider = value;
                _dte = GetDte();
            }
        }

        private static DTE _dte;
        public static DTE dte
        {
            get { return _dte; }
            private set { _dte = value; }
        }

        private static DTE GetDte()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (ServiceProvider == null) return null;

            var task = ServiceProvider.GetServiceAsync(typeof(DTE));
            Assumes.Present(task);
            var dte = task.Result as DTE;
            return dte;
        }
    }
}
