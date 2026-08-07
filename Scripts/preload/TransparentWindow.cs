// Remember to include System and System.Runtime.InteropServices


// thank you ZeadenTheBirb for adding linux support to this
using Godot;
using System;
using System.Runtime.InteropServices;

public partial class TransparentWindow : Node
{ // Autoloaded

    // SetWindowLong() modifies a specific flag value associated with a window.
    // We pass the window handle, the index of the property, and the flags the property will have
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    // macOS AppKit interop: Godot can reset the window level / "hides on
    // deactivate" behaviour when the window style is re-evaluated (e.g. when
    // the window is moved to another monitor), so we re-assert it ourselves.
    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, long arg);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, int arg);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern void objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    // This is the index of the property we want to modify
    private const int GwlExStyle = -20;

    // The flags we want to set
    private const int WsExLayered = 0x80000;         // Makes the window "layered"
    private const int WsExTransparent = 0x20;       // Makes the window "clickable through"
                                                    // check https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles 
                                                    // This is the variable containing the window handle
    private IntPtr _hWnd;
    //  private bool isGb;

    private bool _isWindows;
    private bool _isMacOS;
    private IntPtr _nsWindow = IntPtr.Zero;
    private float _macTick = 0f;

    public override void _Ready()
    {
        _isWindows = OperatingSystem.IsWindows();
        _isMacOS = OperatingSystem.IsMacOS();
        if (_isWindows)
        {
            // We store the window handle
            _hWnd = (IntPtr)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, GetWindow().GetWindowId());

            // We can set the properties already from here
            SetWindowLong(_hWnd, GwlExStyle, WsExLayered);

            SetClickThrough(true);
        }
        else
        {
            GetWindow().Transparent = true;
            GetWindow().TransparentBg = true;
            GetWindow().MousePassthrough = true;
            Engine.MaxFps = 45;

            if (_isMacOS)
            {
                _nsWindow = (IntPtr)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, GetWindow().GetWindowId());
                MacForceFloating();
            }
        }
    }

    public override void _Process(double delta)
    {
        if (!_isMacOS || _nsWindow == IntPtr.Zero)
        {
            return;
        }
        // Godot re-evaluates the window style (level + hidesOnDeactivate) whenever
        // the window is resized/moved (e.g. the setMonitor command), so re-assert
        // every second to keep the character visible on every monitor.
        _macTick += (float)delta;
        if (_macTick >= 1.0f)
        {
            _macTick = 0f;
            MacForceFloating();
        }
    }

    private void MacForceFloating()
    {
        // Keep the window floating above normal windows of other apps.
        objc_msgSend(_nsWindow, sel_registerName("setLevel:"), 3L); // NSFloatingWindowLevel
        // Never hide the window when the app is deactivated.
        objc_msgSend(_nsWindow, sel_registerName("setHidesOnDeactivate:"), 0);
        // If it was hidden anyway, bring it back to the front.
        if (objc_msgSend_bool(_nsWindow, sel_registerName("isHidden")))
        {
            objc_msgSend(_nsWindow, sel_registerName("orderFrontRegardless"));
        }
    }

    // This function sets the property of being clickable or not, we will call this function from the mouse detection 
    public void SetClickThrough(bool clickthrough)
    {
        _isWindows = OperatingSystem.IsWindows();
        if (_isWindows)
        {
            if (clickthrough)
            {
                // We set the window as layered and click-through
                SetWindowLong(_hWnd, GwlExStyle, WsExLayered | WsExTransparent);
                Engine.MaxFps = 45;
            }
            else
            {
                // We only set the window as layered, so it will be clickable
                SetWindowLong(_hWnd, GwlExStyle, WsExLayered);
                Engine.MaxFps = 60;
            }
        }
        else
        {
            GetWindow().MousePassthrough = clickthrough;
            Engine.MaxFps = clickthrough ? 45 : 60;
        }
    }

    /* What is a layered window? 
	 * In the Windows API, a layered window is a special type of window that offers several
	 * advantages over standard windows:
	 * 
	 * Transparency: Layered windows can be partially transparent, allowing the content of underlying windows
	 * to show through. This can be achieved using either color keying, where a specific color in the window
	 * is transparent, or alpha blending, where the window's opacity is specified for each pixel.
	 *
	 * Complex Shapes: Layered windows can have complex shapes that are not limited by rectangular regions.
	 * This is achieved by defining a custom region, allowing for more visually appealing or functional window designs.
	 *
	 * Animation: Layered windows can be animated smoothly without the visual artifacts
	 * that can occur with standard windows due to region updates. This is because the system automatically manages
	 * the composition of layered windows with underlying elements.
	 */
}