using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WhiteRoom
{
    public partial class Win9xProgressBar : ProgressBar
    {
        public IntPtr InternalHandle = IntPtr.Zero;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetWindowTheme(this.Handle, "", "");
            CustomBorder.SetElementNoBorder(this.Handle);
            InternalHandle = this.Handle;
        }
        [DllImport("uxtheme.dll")]
        private static extern int SetWindowTheme(IntPtr hWnd, string appname, string idlist);
    }
    public class CustomBorder
    {
        // modified from http://stackoverflow.com/a/4031202/883015

        #region Style values
        private const uint WS_OVERLAPPED = 0x00000000;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_CHILD = 0x40000000;
        private const uint WS_MINIMIZE = 0x20000000;
        private const uint WS_VISIBLE = 0x10000000;
        private const uint WS_DISABLED = 0x08000000;
        private const uint WS_CLIPSIBLINGS = 0x04000000;
        private const uint WS_CLIPCHILDREN = 0x02000000;
        private const uint WS_MAXIMIZE = 0x01000000;
        private const uint WS_CAPTION = 0x00C00000;
        private const uint WS_BORDER = 0x00800000;
        private const uint WS_DLGFRAME = 0x00400000;
        private const uint WS_VSCROLL = 0x00200000;
        private const uint WS_HSCROLL = 0x00100000;
        private const uint WS_SYSMENU = 0x00080000;
        private const uint WS_THICKFRAME = 0x00040000;
        private const uint WS_GROUP = 0x00020000;
        private const uint WS_TABSTOP = 0x00010000;

        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;

        private const uint WS_EX_DLGMODALFRAME = 0x00000001;
        private const uint WS_EX_NOPARENTNOTIFY = 0x00000004;
        private const uint WS_EX_TOPMOST = 0x00000008;
        private const uint WS_EX_ACCEPTFILES = 0x00000010;
        private const uint WS_EX_TRANSPARENT = 0x00000020;
        private const uint WS_EX_MDICHILD = 0x00000040;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;
        private const uint WS_EX_WINDOWEDGE = 0x00000100;
        private const uint WS_EX_CLIENTEDGE = 0x00000200;
        private const uint WS_EX_CONTEXTHELP = 0x00000400;
        public const uint WS_EX_STATICEDGE = 0x00020000;

        private const int WS_EX_NOANIMATION = 0x04000000;
        public const int GWL_EX_STYLE = -20;
        public const int GWL_STYLE = (-16);
        #endregion

        public static void SetElementNoBorder(IntPtr hWnd)
        {
            //RemoveElementStyle(hWnd, GWL_STYLE, (int)(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZE | WS_MAXIMIZE | WS_SYSMENU));
            RemoveElementStyle(hWnd, GWL_EX_STYLE, (int)(WS_EX_DLGMODALFRAME | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE));
        }

        public static void RemoveElementStyle(IntPtr hWnd, int modifier, int style)
        {
            int currStyle = GetWindowLong(hWnd, GWL_EX_STYLE);
            currStyle &= ~style;
            SetWindowLong(hWnd, modifier, currStyle);
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    }
}