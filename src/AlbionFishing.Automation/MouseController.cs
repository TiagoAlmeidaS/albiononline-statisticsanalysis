using System.Runtime.InteropServices;

namespace AlbionFishing.Automation;

public class MouseController
{
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

    public void MoveTo(int x, int y)
    {
        SetCursorPos(x, y);
    }

    public void MoveTo(int x, int y, int durationMs = 0)
    {
        if (durationMs <= 0)
        {
            SetCursorPos(x, y);
            return;
        }

        GetCursorPos(out POINT currentPos);
        int startX = currentPos.X;
        int startY = currentPos.Y;
        
        int steps = durationMs / 10; // 10ms por passo
        for (int i = 0; i <= steps; i++)
        {
            double progress = (double)i / steps;
            int newX = (int)(startX + (x - startX) * progress);
            int newY = (int)(startY + (y - startY) * progress);
            SetCursorPos(newX, newY);
            Thread.Sleep(10);
        }
    }

    public void Click(string button = "left")
    {
        uint downFlag, upFlag;
        
        switch (button.ToLower())
        {
            case "right":
                downFlag = MOUSEEVENTF_RIGHTDOWN;
                upFlag = MOUSEEVENTF_RIGHTUP;
                break;
            case "left":
            default:
                downFlag = MOUSEEVENTF_LEFTDOWN;
                upFlag = MOUSEEVENTF_LEFTUP;
                break;
        }

        mouse_event(downFlag, 0, 0, 0, UIntPtr.Zero);
        Thread.Sleep(10);
        mouse_event(upFlag, 0, 0, 0, UIntPtr.Zero);
    }

    public void ClickAt(int x, int y, string button = "left")
    {
        MoveTo(x, y);
        Thread.Sleep(50); // Pequena pausa para estabilizar
        Click(button);
    }

    public (int x, int y) GetCurrentPosition()
    {
        GetCursorPos(out POINT point);
        return (point.X, point.Y);
    }

    public void MouseDown(string button = "left")
    {
        uint downFlag;
        
        switch (button.ToLower())
        {
            case "right":
                downFlag = MOUSEEVENTF_RIGHTDOWN;
                break;
            case "left":
            default:
                downFlag = MOUSEEVENTF_LEFTDOWN;
                break;
        }

        mouse_event(downFlag, 0, 0, 0, UIntPtr.Zero);
    }

    public void MouseUp(string button = "left")
    {
        uint upFlag;
        
        switch (button.ToLower())
        {
            case "right":
                upFlag = MOUSEEVENTF_RIGHTUP;
                break;
            case "left":
            default:
                upFlag = MOUSEEVENTF_LEFTUP;
                break;
        }

        mouse_event(upFlag, 0, 0, 0, UIntPtr.Zero);
    }
} 