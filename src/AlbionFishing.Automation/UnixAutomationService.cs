using System;
using System.Diagnostics;
using System.Threading;

namespace AlbionFishing.Automation;

/// <summary>
/// Implementação de automação para Mac/Linux usando xdotool
/// </summary>
public class UnixAutomationService : IAutomationService
{
    private readonly string _xdotoolPath;

    public UnixAutomationService()
    {
        _xdotoolPath = FindXdotool();
    }

    public void MoveTo(int x, int y)
    {
        ExecuteXdotool($"mousemove {x} {y}");
    }

    public void MoveTo(int x, int y, int durationMs)
    {
        if (durationMs <= 0)
        {
            MoveTo(x, y);
            return;
        }

        // xdotool não suporta animação nativa, então simulamos
        var currentPos = GetCurrentPosition();
        int startX = currentPos.x;
        int startY = currentPos.y;
        
        int steps = durationMs / 10; // 10ms por passo
        for (int i = 0; i <= steps; i++)
        {
            double progress = (double)i / steps;
            int newX = (int)(startX + (x - startX) * progress);
            int newY = (int)(startY + (y - startY) * progress);
            ExecuteXdotool($"mousemove {newX} {newY}");
            Thread.Sleep(10);
        }
    }

    public void Click(string button = "left")
    {
        ExecuteXdotool($"click {button}");
    }

    public void ClickAt(int x, int y, string button = "left")
    {
        ExecuteXdotool($"mousemove {x} {y} click {button}");
    }

    public (int x, int y) GetCurrentPosition()
    {
        var output = ExecuteXdotool("getmouselocation --shell");
        var lines = output.Split('\n');
        
        int x = 0, y = 0;
        foreach (var line in lines)
        {
            if (line.StartsWith("X="))
                int.TryParse(line.Substring(2), out x);
            else if (line.StartsWith("Y="))
                int.TryParse(line.Substring(2), out y);
        }
        
        return (x, y);
    }

    public void MouseDown(string button = "left")
    {
        ExecuteXdotool($"mousedown {button}");
    }

    public void MouseUp(string button = "left")
    {
        ExecuteXdotool($"mouseup {button}");
    }

    public void SendKey(string key)
    {
        ExecuteXdotool($"key {key}");
    }

    public void SendKeyCombination(params string[] keys)
    {
        var keyString = string.Join("+", keys);
        ExecuteXdotool($"key {keyString}");
    }

    public bool IsSupported()
    {
        return Environment.OSVersion.Platform == PlatformID.Unix && 
               !string.IsNullOrEmpty(_xdotoolPath);
    }

    private string FindXdotool()
    {
        var possiblePaths = new[]
        {
            "/usr/bin/xdotool",
            "/usr/local/bin/xdotool",
            "xdotool" // Se estiver no PATH
        };

        foreach (var path in possiblePaths)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return path;
                }
            }
            catch
            {
                // Ignora erros e continua procurando
            }
        }

        return string.Empty;
    }

    private string ExecuteXdotool(string arguments)
    {
        if (string.IsNullOrEmpty(_xdotoolPath))
        {
            throw new InvalidOperationException("xdotool não encontrado. Instale com: sudo apt-get install xdotool");
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _xdotoolPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"xdotool falhou: {error}");
        }

        return output;
    }
}