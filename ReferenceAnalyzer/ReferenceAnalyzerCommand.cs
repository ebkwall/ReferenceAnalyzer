using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ReferenceAnalyzerTool
{
    /// <summary>
    /// Command handler
    /// </summary>
    public sealed class ReferenceAnalyzerCommand

    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int SolutionCommandId = 0x0101;
        public const int FolderCommandId = 0x0102;
        public const int ProjectCommandId = 0x0103;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("0b1b04b4-d599-4f86-9843-2d3e1b7a1b86");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceAnalyzerCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ReferenceAnalyzerCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuSolutionCommandID = new CommandID(CommandSet, SolutionCommandId);
            var menuSolutionItem = new MenuCommand(this.Execute, menuSolutionCommandID);
            commandService.AddCommand(menuSolutionItem);

            var menuFolderCommandID = new CommandID(CommandSet, FolderCommandId);
            var menuFolderItem = new MenuCommand(this.Execute, menuFolderCommandID);
            commandService.AddCommand(menuFolderItem);

            var menuProjectCommandID = new CommandID(CommandSet, ProjectCommandId);
            var menuProjectItem = new MenuCommand(this.Execute, menuProjectCommandID);
            commandService.AddCommand(menuProjectItem);

            //Common.AnalyzerProvider = ServiceProvider;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ReferenceAnalyzerCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ReferenceAnalyzer's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new ReferenceAnalyzerCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        public void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = package.FindToolWindow(typeof(AnalyzerWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

            // Execute it!
            Common.Worker.Execute();
        }
    }
}
