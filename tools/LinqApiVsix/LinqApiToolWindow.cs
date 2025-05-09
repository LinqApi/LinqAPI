using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace LinqApiVsix
{
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
    [Guid("c180d4c7-20e2-4d99-ab7e-ad0567392d4c")]
    public class LinqApiToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqApiToolWindow"/> class.
        /// </summary>
        public LinqApiToolWindow() : base(null)
        {
            this.Caption = "LinqApiToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new LinqApiToolWindowControl();
        }
    }
}
