using System;
using System.Runtime.InteropServices;
using UnityEngine;

//source: https://www.youtube.com/watch?v=RqgsGaMPZTw
public class TransparentWindow : MonoBehaviour
{

    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();

    public struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("Dwmapi.dll")]
    public static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll")]
    public static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);


    [DllImport("user32.dll")]
    public static extern uint SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);



    [DllImport("user32.dll", SetLastError = true)]
    private static extern System.IntPtr FindWindow(String lpClassName, String lpWindowName);


    const int GWL_EXSTYLE = -20;
    const int WS_EX_LAYERED = 0x00080000;
    const int WS_EX_TRANSPARENT = 0x00000020;

    private void Start()
    {
#if !UNITY_EDITOR
        SetupTransparency();
#endif
    }

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    void SetupTransparency()
    {
        Application.runInBackground = true;

        var hWnd = GetActiveWindow();
        MARGINS m = new MARGINS() { cxLeftWidth = -1 };
        //apply transparency
        DwmExtendFrameIntoClientArea(hWnd, ref m);
        //let clicks through the app
        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        //bring window back up after clicking through it
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }
}