namespace ReferenceAnalyzerTool
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("6aa84643-b8de-4227-b2ca-efe71b2bccfa")]
    public class AnalyzerWindow : ToolWindowPane
    {
        private AnalyzerWindowControl _awc;

        public ReferenceAnalyzer Analyzer { set { _awc.Analyzer = value; } }
        public List<AnalyzerWindowControl.ReferenceView> ProjectReferences
        {
            set
            {
                _awc.ProjectReferences = value;
            }
        }
        public List<AnalyzerWindowControl.ReferenceView> FileReferences
        {
            set
            {
                _awc.FileReferences = value;
            }
        }
        
        public List<string> MissingReferences
        {
            set
            {
                _awc.MissingReferences = value;
            }
        }

        public Dictionary<string, ReferenceAnalyzer.ProjectFixDefinition> ProposedFixes
        {
            set
            {
                _awc.ProposedFixes = value;
            }
        }

        public ReferenceAnalyzer.SelectedType SelectedType
        {
            set
            {
                _awc.SelectedType = value;
            }
        }

        public string SelectedNode
        {
            set
            {
                _awc.SelectedNode = value;
            }
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzerWindow"/> class.
        /// </summary>
        public AnalyzerWindow() : base(null)
        {
            this.Caption = "Reference Analyzer";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new AnalyzerWindowControl();
            _awc = (AnalyzerWindowControl)this.Content;
        }
    }
}
