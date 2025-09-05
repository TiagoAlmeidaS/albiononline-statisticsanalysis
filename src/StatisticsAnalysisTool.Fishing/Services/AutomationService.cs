using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace StatisticsAnalysisTool.Fishing.Services
{
    /// <summary>
    /// Implementação do serviço de automação usando APIs do Windows
    /// </summary>
    public class AutomationService : IAutomationService
    {
        private readonly ILogger<AutomationService> _logger;
        
        public event EventHandler<MouseActionEventArgs> MouseActionExecuted;
        public event EventHandler<KeyboardActionEventArgs> KeyboardActionExecuted;
        
        public bool IsAvailable => true; // Assumindo que sempre está disponível no Windows
        
        public AutomationService(ILogger<AutomationService> logger)
        {
            _logger = logger;
        }
        
        public void MouseDown(string button)
        {
            try
            {
                var buttonCode = GetMouseButtonCode(button);
                if (buttonCode == 0) return;
                
                var input = new INPUT
                {
                    type = INPUT_MOUSE,
                    union = new INPUTUNION
                    {
                        mi = new MOUSEINPUT
                    {
                        dwFlags = buttonCode,
                        dx = 0,
                        dy = 0,
                        dwExtraInfo = IntPtr.Zero
                        }
                    }
                };
                
                SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
                
                _logger.LogDebug("MouseDown executado: {Button}", button);
                MouseActionExecuted?.Invoke(this, new MouseActionEventArgs
                {
                    Action = "MouseDown",
                    Button = button,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar MouseDown: {Button}", button);
            }
        }
        
        public void MouseUp(string button)
        {
            try
            {
                var buttonCode = GetMouseButtonUpCode(button);
                if (buttonCode == 0) return;
                
                var input = new INPUT
                {
                    type = INPUT_MOUSE,
                    union = new INPUTUNION
                    {
                        mi = new MOUSEINPUT
                    {
                        dwFlags = buttonCode,
                        dx = 0,
                        dy = 0,
                        dwExtraInfo = IntPtr.Zero
                        }
                    }
                };
                
                SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
                
                _logger.LogDebug("MouseUp executado: {Button}", button);
                MouseActionExecuted?.Invoke(this, new MouseActionEventArgs
                {
                    Action = "MouseUp",
                    Button = button,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar MouseUp: {Button}", button);
            }
        }
        
        public void MouseClick(string button)
        {
            MouseDown(button);
            System.Threading.Thread.Sleep(50); // Pequena pausa
            MouseUp(button);
        }
        
        public void MouseMove(int x, int y)
        {
            try
            {
                var input = new INPUT
                {
                    type = INPUT_MOUSE,
                    union = new INPUTUNION
                    {
                        mi = new MOUSEINPUT
                    {
                        dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE,
                        dx = x * (65535 / GetSystemMetrics(0)), // Convert to absolute coordinates
                        dy = y * (65535 / GetSystemMetrics(1)), // Convert to absolute coordinates
                        dwExtraInfo = IntPtr.Zero
                        }
                    }
                };
                
                SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
                
                _logger.LogDebug("MouseMove executado: ({X}, {Y})", x, y);
                MouseActionExecuted?.Invoke(this, new MouseActionEventArgs
                {
                    Action = "MouseMove",
                    Position = new Point(x, y),
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar MouseMove: ({X}, {Y})", x, y);
            }
        }
        
        public void KeyDown(int key)
        {
            try
            {
                var input = new INPUT
                {
                    type = INPUT_KEYBOARD,
                    union = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                    {
                        wVk = (ushort)key,
                        wScan = 0,
                        dwFlags = 0,
                        dwExtraInfo = IntPtr.Zero
                        }
                    }
                };
                
                SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
                
                _logger.LogDebug("KeyDown executado: {Key}", key);
                KeyboardActionExecuted?.Invoke(this, new KeyboardActionEventArgs
                {
                    Action = "KeyDown",
                    KeyCode = key,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar KeyDown: {Key}", key);
            }
        }
        
        public void KeyUp(int key)
        {
            try
            {
                var input = new INPUT
                {
                    type = INPUT_KEYBOARD,
                    union = new INPUTUNION
                    {
                        ki = new KEYBDINPUT
                    {
                        wVk = (ushort)key,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        dwExtraInfo = IntPtr.Zero
                        }
                    }
                };
                
                SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
                
                _logger.LogDebug("KeyUp executado: {Key}", key);
                KeyboardActionExecuted?.Invoke(this, new KeyboardActionEventArgs
                {
                    Action = "KeyUp",
                    KeyCode = key,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar KeyUp: {Key}", key);
            }
        }
        
        public void KeyPress(int key)
        {
            KeyDown(key);
            System.Threading.Thread.Sleep(50); // Pequena pausa
            KeyUp(key);
        }
        
        private uint GetMouseButtonCode(string button)
        {
            return button.ToLower() switch
            {
                "left" => MOUSEEVENTF_LEFTDOWN,
                "right" => MOUSEEVENTF_RIGHTDOWN,
                "middle" => MOUSEEVENTF_MIDDLEDOWN,
                _ => 0
            };
        }
        
        private uint GetMouseButtonUpCode(string button)
        {
            return button.ToLower() switch
            {
                "left" => MOUSEEVENTF_LEFTUP,
                "right" => MOUSEEVENTF_RIGHTUP,
                "middle" => MOUSEEVENTF_MIDDLEUP,
                _ => 0
            };
        }
        
        // P/Invoke declarations
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
        
        private const uint INPUT_MOUSE = 0;
        private const uint INPUT_KEYBOARD = 1;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUTUNION union;
        }
        
        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}