using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceAnalyzerTool
{
    public static class Common
    {
        // These are required to be set before the Worker can be used
        //public static Microsoft.VisualStudio.Shell.IAsyncServiceProvider AnalyzerProvider { set; get; }
        //public static Microsoft.VisualStudio.Shell.IAsyncServiceProvider WindowProvider { set; get; }

               
        public static ReferenceAnalyzerWorker Worker { set; get; }
    }
}
