using System.Runtime.InteropServices;

namespace AlbionFishing.Automation;

public class KeyboardController
{
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern byte VkKeyScan(char ch);

    private const uint KEYEVENTF_KEYDOWN = 0x0000;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    public void PressKey(byte virtualKeyCode)
    {
        keybd_event(virtualKeyCode, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        Thread.Sleep(10);
        keybd_event(virtualKeyCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    public void PressKey(char character)
    {
        byte vk = VkKeyScan(character);
        PressKey(vk);
    }

    public void PressKey(string key)
    {
        byte vk = GetVirtualKeyCode(key);
        PressKey(vk);
    }

    public void HoldKey(byte virtualKeyCode)
    {
        keybd_event(virtualKeyCode, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
    }

    public void ReleaseKey(byte virtualKeyCode)
    {
        keybd_event(virtualKeyCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    private byte GetVirtualKeyCode(string key)
    {
        return key.ToLower() switch
        {
            "space" => 0x20,
            "enter" => 0x0D,
            "tab" => 0x09,
            "escape" => 0x1B,
            "f1" => 0x70,
            "f2" => 0x71,
            "f3" => 0x72,
            "f4" => 0x73,
            "f5" => 0x74,
            "f6" => 0x75,
            "f7" => 0x76,
            "f8" => 0x77,
            "f9" => 0x78,
            "f10" => 0x79,
            "f11" => 0x7A,
            "f12" => 0x7B,
            _ => 0x00 // Tecla não reconhecida
        };
    }
} 