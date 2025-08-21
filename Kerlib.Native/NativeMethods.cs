using System.Runtime.InteropServices;

namespace Kerlib.Native;

public static class NativeMethods
{
    public const int ImageBitmap = 0;
    public const int LrLoadfromfile = 0x0010;
    public const int Srccopy = 0x00CC0020;
    
    public const int WsOverlappedwindow = 0x00CF0000;
    public const int WsVisible = 0x10000000;
    public const int SwShowdefault = 10;

    public const int WmDestroy = 0x0002;
    public const int WmPaint = 0x000F;
    public const int WmSize = 0x0005;

    public const int CsHredraw = 0x0002;
    public const int CsVredraw = 0x0001;

    public const int ColorWindow = 5;
    
    public const uint WmMouseMove   = 0x0200;
    public const uint WmLButtonDown = 0x0201;
    public const uint WmLButtonUp   = 0x0202;
    public const uint WmKeyPress = 0x0102;
    
    public const uint DtCenter = 0x00000001;
    public const uint DtVcenter = 0x00000004;
    public const uint DtSingleline = 0x00000020;
    public const int DtLeft = 0x0000000;
    public const int DtRight = 0x00000002;
    
    public const int GclpHbrbackground = -10;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Wndclassexw
    {
        public uint cbSize;
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Msg
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public Point pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point { public int x; public int y; }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct Size
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect { public int left; public int top; public int right; public int bottom; }

    [StructLayout(LayoutKind.Sequential)]
    public struct Paintstruct
    {
        public IntPtr hdc;
        public int fErase;
        public Rect rcPaint;
        public int fRestore;
        public int fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Bitmap
    {
        public int bmType;
        public int bmWidth;
        public int bmHeight;
        public int bmWidthBytes;
        public ushort bmPlanes;
        public ushort bmBitsPixel;
        public IntPtr bmBits;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);
    
    [DllImport("user32.dll")]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern ushort RegisterClassExW(in Wndclassexw lpwcx);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr CreateWindowExW(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll")]
    public static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool GetMessage(out Msg lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage([In] ref Msg lpMsg);

    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage([In] ref Msg lpmsg);

    [DllImport("user32.dll")]
    public static extern void PostQuitMessage(int nExitCode);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadImage(
        IntPtr hInst,
        string name,
        uint type,
        int cx,
        int cy,
        uint fuLoad);
    
    [DllImport("user32.dll")]
    public static extern IntPtr BeginPaint(IntPtr hWnd, out Paintstruct lpPaint);

    [DllImport("user32.dll")]
    public static extern bool EndPaint(IntPtr hWnd, [In] ref Paintstruct lpPaint);
    [DllImport("user32.dll")]
    public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
    
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);
    
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool AdjustWindowRectEx(ref Rect lpRect, uint dwStyle, bool bMenu, uint dwExStyle);
    
    [DllImport("user32.dll", EntryPoint = "SetClassLongPtrW", SetLastError = true)]
    public static extern IntPtr SetClassLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("gdi32.dll")]
    public static extern bool MoveToEx(IntPtr hdc, int x, int y, IntPtr lpPoint);

    [DllImport("gdi32.dll")]
    public static extern bool LineTo(IntPtr hdc, int nXEnd, int nYEnd);

    [DllImport("gdi32.dll")]
    public static extern bool Rectangle(IntPtr hdc, int left, int top, int right, int bottom);
    
    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    public static extern bool TextOutW(IntPtr hdc, int x, int y, string lpString, int c);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateFont(
        int nHeight,
        int nWidth,
        int nEscapement,
        int nOrientation,
        int fnWeight,
        uint fdwItalic,
        uint fdwUnderline,
        uint fdwStrikeOut,
        uint fdwCharSet,
        uint fdwOutputPrecision,
        uint fdwClipPrecision,
        uint fdwQuality,
        uint fdwPitchAndFamily,
        string lpszFace
    );

    [DllImport("gdi32.dll")]
    public static extern uint SetTextColor(IntPtr hdc, uint crColor);

    [DllImport("gdi32.dll")]
    public static extern int SetBkMode(IntPtr hdc, int mode);
    
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateSolidBrush(uint crColor);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int DrawText(IntPtr hdc, string lpString, int nCount, ref Rect lpRect, uint uFormat);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

    [DllImport("gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetTextExtentPoint32(IntPtr hdc, string str, int len, out Size size);
    
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern int GetObject(IntPtr hgdiobj, int cbBuffer, out Bitmap lpvObject);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool StretchBlt(
        IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
        IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc,
        int rop);
    
    public static int GetTextWidth(IntPtr hdc, string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        GetTextExtentPoint32(hdc, text, text.Length, out Size size);
        return size.cx;
    }
    public static uint Rgb(int r, int g, int b) => (uint)(r | (g << 8) | (b << 16));
    public static uint Rgb(Color color) => Rgb(color.R, color.G, color.B);
}