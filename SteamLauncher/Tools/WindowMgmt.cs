using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace SteamLauncher.Tools
{
    public class WindowMgmt
    {
        #region Wrapper/Helper Functions

        /// <summary>
        /// Toggles whether a window is set as 'TopMost'.
        /// </summary>
        /// <param name="windowHandle"></param>
        /// <param name="isTopMost"></param>
        /// <returns></returns>
        public static bool SetWindowTopMost(IntPtr windowHandle, bool isTopMost)
        {
            return SetWindowPos(windowHandle,
                                isTopMost ? HWND_TOPMOST : HWND_NOTOPMOST,
                                0, 0, 0, 0,
                                SetWindowPosFlags.SWP_NOMOVE | 
                                SetWindowPosFlags.SWP_NOSIZE |
                                SetWindowPosFlags.SWP_SHOWWINDOW);
        }

        #endregion

        public enum Lwa : uint
        {
            LWA_COLORKEY = 0x1,
            LWA_ALPHA = 0x2
        }

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        /// <summary>
        /// Retrieves a handle to a window whose class name and window name match the specified strings. The function
        /// searches child windows, beginning with the one following the specified child window. This function does not
        /// perform a case-sensitive search.
        /// </summary>
        /// <param name="hwndParent">A handle to the parent window whose child windows are to be searched. If
        /// hwndParent is NULL, the function uses the desktop window as the parent window.The function searches among
        /// windows that are child windows of the desktop. If hwndParent is HWND_MESSAGE, the function searches all
        /// message-only windows.</param>
        /// <param name="hwndChildAfter">A handle to a child window. The search begins with the next child window in
        /// the Z order. The child window must be a direct child window of hwndParent, not just a descendant window. If
        /// hwndChildAfter is NULL, the search begins with the first child window of hwndParent. Note that if both
        /// hwndParent and hwndChildAfter are NULL, the function searches all top-level and message-only
        /// windows.</param>
        /// <param name="lpClassName">The class name or a class atom created by a previous call to the RegisterClass or
        /// RegisterClassEx function. The atom must be placed in the low-order word of lpszClass; the high-order word
        /// must be zero. If lpszClass is a string, it specifies the window class name. The class name can be any name
        /// registered with RegisterClass or RegisterClassEx, or any of the predefined control-class names, or it can
        /// be MAKEINTATOM(0x8000). In this latter case, 0x8000 is the atom for a menu class. For more information, see
        /// the Remarks section of this topic.</param>
        /// <param name="lpWindowName">The window name (the window's title). If this parameter is NULL, all window
        /// names match.</param>
        /// <returns>If the function succeeds, the return value is a handle to the window that has the specified class
        /// and window names. If the function fails, the return value is NULL. To get extended error information, call
        /// GetLastError.</returns>
        /// <remarks>If the lpszWindow parameter is not NULL, FindWindowEx calls the GetWindowText function to retrieve
        /// the window name for comparison. For a description of a potential problem that can arise, see the Remarks
        /// section of GetWindowText. An application can call this function in the following way. <br/>
        /// <example>FindWindowEx( NULL, NULL, MAKEINTATOM(0x8000), NULL );</example> <br/>
        /// Note that 0x8000 is the atom for a menu class. When an application calls this function, the function checks
        /// whether a context menu is being displayed that the application created.</remarks>
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, 
                                                 IntPtr hwndChildAfter, 
                                                 string lpClassName, 
                                                 string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        #region GetWindowLong

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Retrieves information about the specified window. The function also retrieves the value at a specified
        /// offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved. Valid values are in the range zero
        /// through the number of bytes of extra window memory, minus the size of a LONG_PTR. To retrieve any other
        /// value, use one of the provided constant values.</param>
        /// <returns>If the function succeeds, the return value is the requested value. If the function fails, the
        /// return value is zero. To get extended error information, call GetLastError.</returns>
        /// <remarks>Reserve extra window memory by specifying a nonzero value in the cbWndExtra member of the
        /// WNDCLASSEX structure used with the RegisterClassEx function.</remarks>
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, GwlIndex nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex.Value);

            return GetWindowLongPtr32(hWnd, nIndex.Value);
        }

        #endregion

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        #region LockSetForegroundWindow

        /// <summary>
        /// Used with <see cref="WindowMgmt.LockSetForegroundWindow"/> to specify whether to enable or disable calls to
        /// SetForegroundWindow.
        /// </summary>
        public enum Lsfw : uint
        {
            /// <summary>
            /// Disable calls to SetForegroundWindow
            /// </summary>
            LSFW_LOCK = 1,

            /// <summary>
            /// Enable calls to SetForegroundWindow
            /// </summary>
            LSFW_UNLOCK = 2
        }

        /// <summary>
        /// Foreground processes can call this function to disable calls to the 'SetForegroundWindow' function.
        /// </summary>
        /// <param name="uLockCode">Specifies whether to enable or disable calls to 'SetForegroundWindow'.</param>
        /// <returns><c>true</c> or nonzero if the function succeeds, <c>false</c> or zero otherwise or if function
        /// fails.</returns>
        /// <remarks>
        /// <para>
        /// The system automatically enables calls to SetForegroundWindow if the user presses the ALT key or takes some
        /// action that causes the system itself to change the foreground window (for example, clicking a background
        /// window).
        /// </para>
        /// <para>
        /// This function is provided so applications can prevent other applications from making a foreground change
        /// that can interrupt its interaction with the user.
        /// </para>
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool LockSetForegroundWindow(Lsfw uLockCode);

        #endregion

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, Lwa dwFlags);

        #region SetWindowLong

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// Changes an attribute of the specified window. The function also sets a value at the specified offset in the
        /// extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs. The
        /// SetWindowLongPtr function fails if the process that owns the window specified by the hWnd parameter is at a
        /// higher process privilege in the UIPI hierarchy than the process the calling thread resides in.</param>
        /// <param name="nIndex">The zero-based offset to the value to be set. Valid values are in the range zero
        /// through the number of bytes of extra window memory, minus the size of a LONG_PTR. To set any other value,
        /// use one of the provided constant values.</param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>If the function succeeds, the return value is the previous value of the specified offset. If the
        /// function fails, the return value is zero. To get extended error information, call GetLastError. If the
        /// previous value is zero and the function succeeds, the return value is zero, but the function does not clear
        /// the last error information. To determine success or failure, clear the last error information by calling
        /// SetLastError with 0, then call SetWindowLongPtr. Function failure will be indicated by a return value of
        /// zero and a GetLastError result that is nonzero.</returns>
        public static IntPtr SetWindowLongPtr(IntPtr hWnd, GwlIndex nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex.Value, dwNewLong);

            return new IntPtr(SetWindowLong32(hWnd, nIndex.Value, dwNewLong.ToInt32()));
        }

        #endregion

        #region SetWindowPos

        /// <summary>
        ///     Changes the size, position, and Z order of a child, pop-up, or top-level window. These windows are
        ///     ordered according to their appearance on the screen. The topmost window receives the highest rank and
        ///     is the first window in the Z order.
        ///     <para>See https://msdn.microsoft.com/en-us/library/windows/desktop/ms633545%28v=vs.85%29.aspx for more
        ///     information.</para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br />A handle to the window.</param>
        /// <param name="hWndInsertAfter">
        ///     C++ ( hWndInsertAfter [in, optional]. Type: HWND )<br />A handle to the window to precede the
        ///     positioned window in the Z order. This parameter must be a window handle or one of the following
        ///     values.
        ///     <list type="table">
        ///     <itemheader>
        ///         <term>HWND placement</term><description>Window to precede placement</description>
        ///     </itemheader>
        ///     <item>
        ///         <term>HWND_BOTTOM ((HWND)1)</term>
        ///         <description>
        ///         Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window,
        ///         the window loses its topmost status and is placed at the bottom of all other windows.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_NOTOPMOST ((HWND)-2)</term>
        ///         <description>
        ///         Places the window above all non-topmost windows (that is, behind all topmost windows). This flag
        ///         has no effect if the window is already a non-topmost window.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_TOP ((HWND)0)</term><description>Places the window at the top of the Z
        ///         order.</description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_TOPMOST ((HWND)-1)</term>
        ///         <description>
        ///         Places the window above all non-topmost windows. The window maintains its topmost position even
        ///         when it is deactivated.
        ///         </description>
        ///     </item>
        ///     </list>
        ///     <para>For more information about how this parameter is used, see the following Remarks section.</para>
        /// </param>
        /// <param name="X">C++ ( X [in]. Type: int )<br />The new position of the left side of the window, in client
        /// coordinates.</param>
        /// <param name="Y">C++ ( Y [in]. Type: int )<br />The new position of the top of the window, in client
        /// coordinates.</param>
        /// <param name="cx">C++ ( cx [in]. Type: int )<br />The new width of the window, in pixels.</param>
        /// <param name="cy">C++ ( cy [in]. Type: int )<br />The new height of the window, in pixels.</param>
        /// <param name="uFlags">
        ///     C++ ( uFlags [in]. Type: UINT )<br />The window sizing and positioning flags. This parameter can be a
        ///     combination of the following values.
        ///     <list type="table">
        ///     <itemheader>
        ///         <term>HWND sizing and positioning flags</term>
        ///         <description>Where to place and size window. Can be a combination of any</description>
        ///     </itemheader>
        ///     <item>
        ///         <term>SWP_ASYNCWINDOWPOS (0x4000)</term>
        ///         <description>
        ///         If the calling thread and the thread that owns the window are attached to different input queues,
        ///         the system posts the request to the thread that owns the window. This prevents the calling thread
        ///         from blocking its execution while other threads process the request.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_DEFERERASE (0x2000)</term>
        ///         <description>Prevents generation of the WM_SYNCPAINT message. </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_DRAWFRAME (0x0020)</term>
        ///         <description>Draws a frame (defined in the window's class description) around the
        ///         window.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_FRAMECHANGED (0x0020)</term>
        ///         <description>
        ///         Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the
        ///         window, even if the window's size is not being changed. If this flag is not specified,
        ///         WM_NCCALCSIZE is sent only when the window's size is being changed
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_HIDEWINDOW (0x0080)</term><description>Hides the window.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOACTIVATE (0x0010)</term>
        ///         <description>
        ///         Does not activate the window. If this flag is not set, the window is activated and moved to the top
        ///         of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
        ///         parameter).
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOCOPYBITS (0x0100)</term>
        ///         <description>
        ///         Discards the entire contents of the client area. If this flag is not specified, the valid contents
        ///         of the client area are saved and copied back into the client area after the window is sized or
        ///         repositioned.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOMOVE (0x0002)</term>
        ///         <description>Retains the current position (ignores X and Y parameters).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOOWNERZORDER (0x0200)</term>
        ///         <description>Does not change the owner window's position in the Z order.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOREDRAW (0x0008)</term>
        ///         <description>
        ///         Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the
        ///         client area, the nonclient area (including the title bar and scroll bars), and any part of the
        ///         parent window uncovered as a result of the window being moved. When this flag is set, the
        ///         application must explicitly invalidate or redraw any parts of the window and parent window that
        ///         need redrawing.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOREPOSITION (0x0200)</term><description>Same as the SWP_NOOWNERZORDER
        ///         flag.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOSENDCHANGING (0x0400)</term>
        ///         <description>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOSIZE (0x0001)</term>
        ///         <description>Retains the current size (ignores the cx and cy parameters).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOZORDER (0x0004)</term>
        ///         <description>Retains the current Z order (ignores the hWndInsertAfter parameter).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_SHOWWINDOW (0x0040)</term><description>Displays the window.</description>
        ///     </item>
        ///     </list>
        /// </param>
        /// <returns><c>true</c> or nonzero if the function succeeds, <c>false</c> or zero otherwise or if function
        /// fails.</returns>
        /// <remarks>
        ///     <para>
        ///     As part of the Vista re-architecture, all services were moved off the interactive desktop into Session
        ///     0. hwnd and window manager operations are only effective inside a session and cross-session attempts to
        ///     manipulate the hwnd will fail. For more information, see The Windows Vista Developer Story: Application
        ///     Compatibility Cookbook.
        ///     </para>
        ///     <para>
        ///     If you have changed certain window data using SetWindowLong, you must call SetWindowPos for the changes
        ///     to take effect. Use the following combination for uFlags: SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER |
        ///     SWP_FRAMECHANGED.
        ///     </para>
        ///     <para>
        ///     A window can be made a topmost window either by setting the hWndInsertAfter parameter to HWND_TOPMOST
        ///     and ensuring that the SWP_NOZORDER flag is not set, or by setting a window's position in the Z order so
        ///     that it is above any existing topmost windows. When a non-topmost window is made topmost, its owned
        ///     windows are also made topmost. Its owners, however, are not changed.
        ///     </para>
        ///     <para>
        ///     If neither the SWP_NOACTIVATE nor SWP_NOZORDER flag is specified (that is, when the application
        ///     requests that a window be simultaneously activated and its position in the Z order changed), the value
        ///     specified in hWndInsertAfter is used only in the following circumstances.
        ///     </para>
        ///     <list type="bullet">
        ///     <item>Neither the HWND_TOPMOST nor HWND_NOTOPMOST flag is specified in hWndInsertAfter. </item>
        ///     <item>The window identified by hWnd is not the active window. </item>
        ///     </list>
        ///     <para>
        ///     An application cannot activate an inactive window without also bringing it to the top of the Z order.
        ///     Applications can change an activated window's position in the Z order without restrictions, or it can
        ///     activate a window and then move it to the top of the topmost or non-topmost windows.
        ///     </para>
        ///     <para>
        ///     If a topmost window is repositioned to the bottom (HWND_BOTTOM) of the Z order or after any non-topmost
        ///     window, it is no longer topmost. When a topmost window is made non-topmost, its owners and its owned
        ///     windows are also made non-topmost windows.
        ///     </para>
        ///     <para>
        ///     A non-topmost window can own a topmost window, but the reverse cannot occur. Any window (for example, a
        ///     dialog box) owned by a topmost window is itself made a topmost window, to ensure that all owned windows
        ///     stay above their owner.
        ///     </para>
        ///     <para>
        ///     If an application is not in the foreground, and should be in the foreground, it must call the
        ///     SetForegroundWindow function.
        ///     </para>
        ///     <para>
        ///     To use SetWindowPos to bring a window to the top, the process that owns the window must have
        ///     SetForegroundWindow permission.
        ///     </para>
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        /// <summary>
        /// Places the window above all non-topmost windows. The window maintains its topmost position even when it
        /// is deactivated.
        /// </summary>
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        /// <summary>
        /// Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no
        /// effect if the window is already a non-topmost window.
        /// </summary>
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        /// <summary>
        /// Places the window at the top of the Z order.
        /// </summary>
        public static readonly IntPtr HWND_TOP = new IntPtr(0);

        /// <summary>
        /// Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the
        /// window loses its topmost status and is placed at the bottom of all other windows.
        /// </summary>
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        [Flags]
        public enum SetWindowPosFlags : uint
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            ///     If the calling thread and the thread that owns the window are attached to different input queues,
            ///     the system posts the request to the thread that owns the window. This prevents the calling thread
            ///     from blocking its execution while other threads process the request.
            /// </summary>
            SWP_ASYNCWINDOWPOS = 0x4000,

            /// <summary>
            ///     Prevents generation of the WM_SYNCPAINT message.
            /// </summary>
            SWP_DEFERERASE = 0x2000,

            /// <summary>
            ///     Draws a frame (defined in the window's class description) around the window.
            /// </summary>
            SWP_DRAWFRAME = 0x0020,

            /// <summary>
            ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the
            ///     window, even if the window's size is not being changed. If this flag is not specified,
            ///     WM_NCCALCSIZE is sent only when the window's size is being changed.
            /// </summary>
            SWP_FRAMECHANGED = 0x0020,

            /// <summary>
            ///     Hides the window.
            /// </summary>
            SWP_HIDEWINDOW = 0x0080,

            /// <summary>
            ///     Does not activate the window. If this flag is not set, the window is activated and moved to the top
            ///     of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
            ///     parameter).
            /// </summary>
            SWP_NOACTIVATE = 0x0010,

            /// <summary>
            ///     Discards the entire contents of the client area. If this flag is not specified, the valid contents
            ///     of the client area are saved and copied back into the client area after the window is sized or
            ///     repositioned.
            /// </summary>
            SWP_NOCOPYBITS = 0x0100,

            /// <summary>
            ///     Retains the current position (ignores X and Y parameters).
            /// </summary>
            SWP_NOMOVE = 0x0002,

            /// <summary>
            ///     Does not change the owner window's position in the Z order.
            /// </summary>
            SWP_NOOWNERZORDER = 0x0200,

            /// <summary>
            ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the
            ///     client area, the nonclient area (including the title bar and scroll bars), and any part of the
            ///     parent window uncovered as a result of the window being moved. When this flag is set, the
            ///     application must explicitly invalidate or redraw any parts of the window and parent window that
            ///     need redrawing.
            /// </summary>
            SWP_NOREDRAW = 0x0008,

            /// <summary>
            ///     Same as the SWP_NOOWNERZORDER flag.
            /// </summary>
            SWP_NOREPOSITION = 0x0200,

            /// <summary>
            ///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
            /// </summary>
            SWP_NOSENDCHANGING = 0x0400,

            /// <summary>
            ///     Retains the current size (ignores the cx and cy parameters).
            /// </summary>
            SWP_NOSIZE = 0x0001,

            /// <summary>
            ///     Retains the current Z order (ignores the hWndInsertAfter parameter).
            /// </summary>
            SWP_NOZORDER = 0x0004,

            /// <summary>
            ///     Displays the window.
            /// </summary>
            SWP_SHOWWINDOW = 0x0040,
        }
        // ReSharper restore InconsistentNaming
        #endregion

        #region ShowWindow

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        public static bool ShowWindow(IntPtr handle, ShowWindowCmd nCmdShow)
        {
            return ShowWindow(handle, nCmdShow.Value);
        }

        #endregion

        public static string GetClassName(IntPtr windowHandle)
        {
            var condition = new PropertyCondition(AutomationElementIdentifiers.NativeWindowHandleProperty, windowHandle.ToInt32());
            var element = AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            return element?.Current.ClassName;
        }

        public static string GetWindowTitle(IntPtr hwnd)
        {
            var buff = new StringBuilder(500);
            GetWindowText(hwnd, buff, buff.Capacity);
            return buff.ToString();
        }
    }

    /// <summary>
    /// Provides constant values to be used with functions like <see cref="WindowMgmt.GetWindowLongPtr"/> and <see
    /// cref="WindowMgmt.SetWindowLongPtr"/>. 
    /// </summary>
    /// <remarks>The zero-based offset to the value to be set/retrieved. Valid values are in the range zero through the
    /// number of bytes of extra window memory, minus the size of a LONG_PTR. To set/retrieve any other value, use one
    /// the provided constant values.</remarks>
    public class GwlIndex : Enumeration
    {
        /// <summary>
        /// Retrieves the address of the window procedure, or a handle representing the address of the window
        /// procedure. You must use the CallWindowProc function to call the window procedure.
        /// </summary>
        public static readonly GwlIndex GWL_WNDPROC = new GwlIndex(-4, nameof(GWL_WNDPROC));

        /// <summary>
        /// Retrieves a handle to the application instance.
        /// </summary>
        public static readonly GwlIndex GWL_HINSTANCE = new GwlIndex(-6, nameof(GWL_HINSTANCE));

        /// <summary>
        /// Retrieves a handle to the parent window, if any.
        /// </summary>
        public static readonly GwlIndex GWL_HWNDPARENT = new GwlIndex(-8, nameof(GWL_HWNDPARENT));

        /// <summary>
        /// Retrieves the identifier of the window.
        /// </summary>
        public static readonly GwlIndex GWL_ID = new GwlIndex(-12, nameof(GWL_ID));

        /// <summary>
        /// Retrieves the window styles.
        /// </summary>
        public static readonly GwlIndex GWL_STYLE = new GwlIndex(-16, nameof(GWL_STYLE));

        /// <summary>
        /// Retrieves the extended window styles. 
        /// </summary>
        public static readonly GwlIndex GWL_EXSTYLE = new GwlIndex(-20, nameof(GWL_EXSTYLE));

        /// <summary>
        /// Retrieves the user data associated with the window. This data is intended for use by the application that
        /// created the window. Its value is initially zero.
        /// </summary>
        public static readonly GwlIndex GWL_USERDATA = new GwlIndex(-21, nameof(GWL_USERDATA));

        private GwlIndex(int value, string displayName) : base(value, displayName) { }

        /// <summary>
        /// In special cases, a custom index value, that does not correspond to one of the preexisting constant values,
        /// can be provided to GetWindowLong. This method allows you to create a new <see cref="GwlIndex"/> instance
        /// using a custom value.
        /// </summary>
        /// <param name="value">Custom index value.</param>
        /// <param name="displayName">Name of custom value. Mostly irrelevant and can be left blank.</param>
        /// <returns>A new <see cref="GwlIndex"/> instance containing the custom value provided.</returns>
        public static GwlIndex CreateCustomValue(int value, string displayName = "")
        {
            return new GwlIndex(value, displayName);
        }
    }

    /// <summary>
    /// Provides constant values to be used for the 2nd parameter (nCmdShow) of <see cref="WindowMgmt.ShowWindow"/>.
    /// </summary>
    /// <remarks>Controls how the window is to be shown. This parameter is ignored the first time an application calls
    /// ShowWindow, if the program that launched the application provides a STARTUPINFO structure. Otherwise, the first
    /// time ShowWindow is called, the value should be the value obtained by the WinMain function in its nCmdShow
    /// parameter. In subsequent calls, this parameter can be one of the following values.</remarks>
    public class ShowWindowCmd : Enumeration
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        public static readonly ShowWindowCmd SW_HIDE = new ShowWindowCmd(0, nameof(SW_HIDE));

        /// <summary>
        /// Activates and displays a window. If the window is minimized or maximized, the system restores it to its
        /// original size and position. An application should specify this flag when displaying the window for the
        /// first time.
        /// </summary>
        public static readonly ShowWindowCmd SW_SHOWNORMAL = new ShowWindowCmd(1, nameof(SW_SHOWNORMAL));

        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        public static readonly ShowWindowCmd SW_SHOWMINIMIZED = new ShowWindowCmd(2, nameof(SW_SHOWMINIMIZED));

        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>
        public static readonly ShowWindowCmd SW_SHOWMAXIMIZED = new ShowWindowCmd(3, nameof(SW_SHOWMAXIMIZED));

        /// <summary>
        /// Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that
        /// the window is not activated.
        /// </summary>
        public static readonly ShowWindowCmd SW_SHOWNOACTIVATE = new ShowWindowCmd(4, nameof(SW_SHOWNOACTIVATE));

        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        public static readonly ShowWindowCmd SW_SHOW = new ShowWindowCmd(5, nameof(SW_SHOW));

        /// <summary>
        /// Minimizes the specified window and activates the next top-level window in the Z order.
        /// </summary>
        public static readonly ShowWindowCmd SW_MINIMIZE = new ShowWindowCmd(6, nameof(SW_MINIMIZE));

        /// <summary>
        /// Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is
        /// not activated.
        /// </summary>
        public static readonly ShowWindowCmd SW_SHOWMINNOACTIVE = new ShowWindowCmd(7, nameof(SW_SHOWMINNOACTIVE));

        /// <summary>
        /// Displays the window in its current size and position. This value is similar to SW_SHOW, except that the
        /// window is not activated.
        /// </summary>
        public static readonly ShowWindowCmd SW_SHOWNA = new ShowWindowCmd(8, nameof(SW_SHOWNA));

        /// <summary>
        /// Activates and displays the window. If the window is minimized or maximized, the system restores it to its
        /// original size and position. An application should specify this flag when restoring a minimized window.
        /// </summary>
        public static readonly ShowWindowCmd SW_RESTORE = new ShowWindowCmd(9, nameof(SW_RESTORE));

        /// <summary>
        /// Sets the show state based on the SW_ value specified in the STARTUPINFO structure passed to the
        /// CreateProcess function by the program that started the application.
        /// </summary>
        public static readonly ShowWindowCmd SW_SHOWDEFAULT = new ShowWindowCmd(10, nameof(SW_SHOWDEFAULT));

        /// <summary>
        /// Minimizes a window, even if the thread that owns the window is not responding. This flag should only be
        /// used when minimizing windows from a different thread.
        /// </summary>
        public static readonly ShowWindowCmd SW_FORCEMINIMIZE = new ShowWindowCmd(11, nameof(SW_FORCEMINIMIZE));

        private ShowWindowCmd(int value, string displayName) : base(value, displayName) { }
    }

    /// <summary>
    /// Provides constant values to be used with functions such as <see cref="WindowMgmt.SetWindowLongPtr"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "StaticMemberInitializerReferesToMemberBelow")]
    public class WindowStylesExtended : Enumeration
    {
        /// <summary>
        /// The window accepts drag-drop files.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_ACCEPTFILES =
            new WindowStylesExtended(0x00000010, nameof(WS_EX_ACCEPTFILES));

        /// <summary>
        /// Forces a top-level window onto the taskbar when the window is visible.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_APPWINDOW =
            new WindowStylesExtended(0x00040000, nameof(WS_EX_APPWINDOW));

        /// <summary>
        /// The window has a border with a sunken edge.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_CLIENTEDGE =
            new WindowStylesExtended(0x00000200, nameof(WS_EX_CLIENTEDGE));

        /// <summary>
        /// Paints all descendants of a window in bottom-to-top painting order using double-buffering. Bottom-to-top
        /// painting order allows a descendent window to have translucency (alpha) and transparency (color-key)
        /// effects, but only if the descendent window also has the WS_EX_TRANSPARENT bit set. Double-buffering allows
        /// the window and its descendents to be painted without flicker. This cannot be used if the window has a class
        /// style of either CS_OWNDC or CS_CLASSDC. Windows 2000: This style is not supported.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_COMPOSITED =
            new WindowStylesExtended(0x02000000, nameof(WS_EX_COMPOSITED));

        /// <summary>
        /// The title bar of the window includes a question mark. When the user clicks the question mark, the cursor
        /// changes to a question mark with a pointer. If the user then clicks a child window, the child receives a
        /// WM_HELP message. The child window should pass the message to the parent window procedure, which should call
        /// the WinHelp function using the HELP_WM_HELP command. The Help application displays a pop-up window that
        /// typically contains help for the child window. WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or
        /// WS_MINIMIZEBOX styles.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_CONTEXTHELP =
            new WindowStylesExtended(0x00000400, nameof(WS_EX_CONTEXTHELP));

        /// <summary>
        /// The window itself contains child windows that should take part in dialog box navigation. If this style is
        /// specified, the dialog manager recurses into children of this window when performing navigation operations
        /// such as handling the TAB key, an arrow key, or a keyboard mnemonic.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_CONTROLPARENT =
            new WindowStylesExtended(0x00010000, nameof(WS_EX_CONTROLPARENT));

        /// <summary>
        /// The window has a double border; the window can, optionally, be created with a title bar by specifying the
        /// WS_CAPTION style in the dwStyle parameter.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_DLGMODALFRAME =
            new WindowStylesExtended(0x00000001, nameof(WS_EX_DLGMODALFRAME));

        /// <summary>
        /// The window is a layered window. This style cannot be used if the window has a class style of either
        /// CS_OWNDC or CS_CLASSDC. Windows 8: The WS_EX_LAYERED style is supported for top-level windows and child
        /// windows.Previous Windows versions support WS_EX_LAYERED only for top-level windows.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_LAYERED =
            new WindowStylesExtended(0x00080000, nameof(WS_EX_LAYERED));

        /// <summary>
        /// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the
        /// horizontal origin of the window is on the right edge. Increasing horizontal values advance to the left. 
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_LAYOUTRTL =
            new WindowStylesExtended(0x00400000, nameof(WS_EX_LAYOUTRTL));

        /// <summary>
        /// The window has generic left-aligned properties. This is the default.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_LEFT =
            new WindowStylesExtended(0x00000000, nameof(WS_EX_LEFT));

        /// <summary>
        /// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the
        /// vertical scroll bar (if present) is to the left of the client area. For other languages, the style is
        /// ignored.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_LEFTSCROLLBAR =
            new WindowStylesExtended(0x00004000, nameof(WS_EX_LEFTSCROLLBAR));

        /// <summary>
        /// The window text is displayed using left-to-right reading-order properties. This is the default.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_LTRREADING =
            new WindowStylesExtended(0x00000000, nameof(WS_EX_LTRREADING));

        /// <summary>
        /// The window is a MDI child window.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_MDICHILD =
            new WindowStylesExtended(0x00000040, nameof(WS_EX_MDICHILD));

        /// <summary>
        /// A top-level window created with this style does not become the foreground window when the user clicks it.
        /// The system does not bring this window to the foreground when the user minimizes or closes the foreground
        /// window. The window should not be activated through programmatic access or via keyboard navigation by
        /// accessible technology, such as Narrator. To activate the window, use the SetActiveWindow or
        /// SetForegroundWindow function. The window does not appear on the taskbar by default. To force the window to
        /// appear on the taskbar, use the WS_EX_APPWINDOW style.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_NOACTIVATE =
            new WindowStylesExtended(0x08000000, nameof(WS_EX_NOACTIVATE));

        /// <summary>
        /// The window does not pass its window layout to its child windows.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_NOINHERITLAYOUT =
            new WindowStylesExtended(0x00100000, nameof(WS_EX_NOINHERITLAYOUT));

        /// <summary>
        /// The child window created with this style does not send the WM_PARENTNOTIFY message to its parent window
        /// when it is created or destroyed.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_NOPARENTNOTIFY =
            new WindowStylesExtended(0x00000004, nameof(WS_EX_NOPARENTNOTIFY));

        /// <summary>
        /// The window does not render to a redirection surface. This is for windows that do not have visible content
        /// or that use mechanisms other than surfaces to provide their visual.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_NOREDIRECTIONBITMAP =
            new WindowStylesExtended(0x00200000, nameof(WS_EX_NOREDIRECTIONBITMAP));

        /// <summary>
        /// The window is an overlapped window.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_OVERLAPPEDWINDOW =
            new WindowStylesExtended(WS_EX_WINDOWEDGE.Value | WS_EX_CLIENTEDGE.Value, nameof(WS_EX_OVERLAPPEDWINDOW));

        /// <summary>
        /// The window is palette window, which is a modeless dialog box that presents an array of commands.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_PALETTEWINDOW =
            new WindowStylesExtended(WS_EX_WINDOWEDGE.Value | WS_EX_TOOLWINDOW.Value | WS_EX_TOPMOST.Value, 
                                     nameof(WS_EX_PALETTEWINDOW));

        /// <summary>
        /// The window has generic "right-aligned" properties. This depends on the window class. This style has an
        /// effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order
        /// alignment; otherwise, the style is ignored. Using the WS_EX_RIGHT style for static or edit controls has the
        /// same effect as using the SS_RIGHT or ES_RIGHT style, respectively.Using this style with button controls has
        /// the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_RIGHT =
            new WindowStylesExtended(0x00001000, nameof(WS_EX_RIGHT));

        /// <summary>
        /// The vertical scroll bar (if present) is to the right of the client area. This is the default.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_RIGHTSCROLLBAR =
            new WindowStylesExtended(0x00000000, nameof(WS_EX_RIGHTSCROLLBAR));

        /// <summary>
        /// If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the
        /// window text is displayed using right-to-left reading-order properties. For other languages, the style is
        /// ignored.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_RTLREADING =
            new WindowStylesExtended(0x00002000, nameof(WS_EX_RTLREADING));

        /// <summary>
        /// The window has a three-dimensional border style intended to be used for items that do not accept user input.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_STATICEDGE =
            new WindowStylesExtended(0x00020000, nameof(WS_EX_STATICEDGE));

        /// <summary>
        /// The window is intended to be used as a floating toolbar. A tool window has a title bar that is shorter than
        /// a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in
        /// the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system
        /// menu, its icon is not displayed on the title bar. However, you can display the system menu by
        /// right-clicking or by typing ALT+SPACE. 
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_TOOLWINDOW =
            new WindowStylesExtended(0x00000080, nameof(WS_EX_TOOLWINDOW));

        /// <summary>
        /// The window should be placed above all non-topmost windows and should stay above them, even when the window
        /// is deactivated. To add or remove this style, use the SetWindowPos function.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_TOPMOST =
            new WindowStylesExtended(0x00000008, nameof(WS_EX_TOPMOST));

        /// <summary>
        /// The window should not be painted until siblings beneath the window (that were created by the same thread)
        /// have been painted. The window appears transparent because the bits of underlying sibling windows have
        /// already been painted. To achieve transparency without these restrictions, use the SetWindowRgn function.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_TRANSPARENT =
            new WindowStylesExtended(0x00000020, nameof(WS_EX_TRANSPARENT));

        /// <summary>
        /// The window has a border with a raised edge.
        /// </summary>
        public static readonly WindowStylesExtended WS_EX_WINDOWEDGE =
            new WindowStylesExtended(0x00000100, nameof(WS_EX_WINDOWEDGE));

        private WindowStylesExtended(int value, string displayName) : base(value, displayName) { }

        /// <summary>
        /// Can be used to create an instance of <see cref="WindowStylesExtended"/> with a custom value in the case
        /// that a needed constant value is missing from the list of predefined values.
        /// </summary>
        /// <param name="value">The user-defined custom value.</param>
        /// <param name="displayName">Name of custom value. Mostly irrelevant and can be left blank.</param>
        /// <returns>A new <see cref="WindowStylesExtended"/> instance containing the custom value provided.</returns>
        public static WindowStylesExtended CreateCustomValue(int value, string displayName = "")
        {
            return new WindowStylesExtended(value, displayName);
        }
    }
}
