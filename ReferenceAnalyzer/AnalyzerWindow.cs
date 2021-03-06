﻿namespace ReferenceAnalyzerTool
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using static ReferenceAnalyzerTool.ReferenceAnalyzer;

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
        public List<AnalyzerWindowControl.ReferenceView> ProjectReferences
        {
            set
            {
                ((AnalyzerWindowControl)Content).ProjectReferences = value;
            }
        }
        public List<AnalyzerWindowControl.ReferenceView> FileReferences
        {
            set
            {
                ((AnalyzerWindowControl)Content).FileReferences = value;
            }
        }

        public List<string> MissingReferences
        {
            set
            {
                ((AnalyzerWindowControl)Content).MissingReferences = value;
            }
        }

        public Dictionary<string, ProjectFixDefinition> ProposedFixes
        {
            set
            {
                ((AnalyzerWindowControl)Content).ProposedFixes = value;
            }
        }

        public SelectedType SelectedType
        {
            set
            {
                ((AnalyzerWindowControl)Content).SelectedType = value;
            }
        }

        public string SelectedNode
        {
            set
            {
                ((AnalyzerWindowControl)Content).SelectedNode = value;
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
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            var win = this.Content as AnalyzerWindowControl;
            // Not sure where i was going with this...
        }

        public AnalyzerWindowControl AWControl
        {
            get
            { 
                return (AnalyzerWindowControl)this.Content;
            }
        }
    }
}
