using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;

namespace AlbionFishing.Vision.Reporting;

/// <summary>
/// Reporter em CSV/JSONL para o comportamento do Bobber-In-Water.
/// 
/// Padrão do projeto:
/// - Sem dependência de GUI; apenas IO de arquivos.
/// - Cria diretório "analysis_output" sob a pasta do projeto executável.
/// - Nomeia arquivos com timestamp para facilitar inspeção.
/// </summary>
public class CsvBobberVisionReporter : IBobberVisionReporter
{
    private readonly object _lockObj = new();
    private string _sessionId = string.Empty;
    private string _baseDir = string.Empty;
    private string _csvPath = string.Empty;
    private string _jsonlPath = string.Empty;
    private FileStream? _csvStream;
    private FileStream? _jsonlStream;
    private StreamWriter? _csvWriter;
    private StreamWriter? _jsonlWriter;
    private int _framesSinceLastFlush;
    private DateTime _startUtc;
    private int _frames;
    private int _detections;
    private readonly List<double> _scores = new();
    private double _sumScores;
    private double _sumSqScores;
    private string? _templatePath;
    private Rectangle _region;
    private double _confidence;

    public void StartSession(string? sessionName = null, string? templatePath = null, Rectangle? detectionRegion = null, double? confidenceThreshold = null)
    {
        lock (_lockObj)
        {
            // Garantir que writers anteriores estejam fechados
            CloseWriters_NoLock();

            _sessionId = string.IsNullOrWhiteSpace(sessionName)
                ? $"biw_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
                : sessionName!;
            _startUtc = DateTime.UtcNow;
            _templatePath = templatePath;
            _region = detectionRegion ?? Rectangle.Empty;
            _confidence = confidenceThreshold ?? 0.5;

            _baseDir = Path.Combine(AppContext.BaseDirectory, "analysis_output");
            Directory.CreateDirectory(_baseDir);
            _csvPath = Path.Combine(_baseDir, $"{_sessionId}_frames.csv");
            _jsonlPath = Path.Combine(_baseDir, $"{_sessionId}_frames.jsonl");

            try
            {
                Console.WriteLine($"[BiwReporter] Session '{_sessionId}' started. Output: '{_baseDir}'\n  CSV: {_csvPath}\n  JSONL: {_jsonlPath}");
            }
            catch { /* ignore console errors in some hosts */ }

            // Abrir streams com compartilhamento para leitores (ex.: Excel) e escrever header
            OpenWriters_NoLock();
            var header = string.Join(',', new[]
            {
                "ts", "detected", "score", "posX", "posY", "patchW", "patchH",
                "bboxX", "bboxY", "templateW", "templateH",
                "patchStdDev", "templateOnly",
                "zDy", "zRipple", "dyPixels", "rippleEnergy", "jerk", "flow", "diff", "votes", "microMove",
                "hookDetected", "scaleUsed", "templateId", "colorGateOk", "v1", "v2", "maskOnRatio", "method", "usedMask", "reason"
            });
            if (_csvWriter != null)
            {
                WriteLineWithRetry_NoLock(_csvWriter, header);
                _csvWriter.Flush();
            }

            // jsonl priming: no header needed
            _frames = 0;
            _detections = 0;
            _scores.Clear();
            _sumScores = 0;
            _sumSqScores = 0;
            _framesSinceLastFlush = 0;
        }
    }

    public void LogFrame(BobberVisionFrame f)
    {
        lock (_lockObj)
        {
            _frames++;
            if (f.Detected) _detections++;
            _scores.Add(f.Score);
            _sumScores += f.Score;
            _sumSqScores += f.Score * f.Score;

            // Garantir writers abertos
            if (_csvWriter == null || _jsonlWriter == null)
            {
                OpenWriters_NoLock();
            }

            // CSV line
            var csvValues = new List<string>
            {
                f.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                Bool(f.Detected),
                Num(f.Score),
                Num(f.PosX),
                Num(f.PosY),
                Int(f.PatchW),
                Int(f.PatchH),
                OptInt(f.BBoxX),
                OptInt(f.BBoxY),
                OptInt(f.TemplateW),
                OptInt(f.TemplateH),
                OptNum(f.PatchStdDev),
                OptBool(f.TemplateOnly),
                Num(f.ZDy),
                Num(f.ZRipple),
                Num(f.DyPixels),
                Num(f.RippleEnergy),
                Bool(f.Jerk),
                Bool(f.Flow),
                Bool(f.Diff),
                Int(f.Votes),
                Bool(f.MicroMove),
                Bool(f.HookDetected),
                OptNum(f.ScaleUsed),
                Quote(f.TemplateId),
                OptBool(f.ColorGateOk),
                OptNum(f.MatchV1),
                OptNum(f.MatchV2),
                OptNum(f.MaskOnRatio),
                Quote(f.Method),
                OptBool(f.UsedMask),
                Quote(f.Reason)
            };
            var csvLine = string.Join(',', csvValues);
            if (_csvWriter != null)
            {
                WriteLineWithRetry_NoLock(_csvWriter, csvLine);
            }

            // JSONL line
            var json = "{" +
                       $"\"ts\":\"{f.TimestampUtc:O}\"," +
                       $"\"detected\":{f.Detected.ToString().ToLowerInvariant()}," +
                       $"\"score\":{f.Score.ToString(CultureInfo.InvariantCulture)}," +
                       $"\"posX\":{f.PosX.ToString(CultureInfo.InvariantCulture)}," +
                       $"\"posY\":{f.PosY.ToString(CultureInfo.InvariantCulture)}," +
                       $"\"patchW\":{f.PatchW}," +
                       $"\"patchH\":{f.PatchH}," +
                       (f.BBoxX.HasValue ? $"\"bboxX\":{f.BBoxX.Value}," : string.Empty) +
                       (f.BBoxY.HasValue ? $"\"bboxY\":{f.BBoxY.Value}," : string.Empty) +
                       (f.TemplateW.HasValue ? $"\"templateW\":{f.TemplateW.Value}," : string.Empty) +
                       (f.TemplateH.HasValue ? $"\"templateH\":{f.TemplateH.Value}," : string.Empty) +
                       (f.PatchStdDev.HasValue ? $"\"patchStdDev\":{f.PatchStdDev.Value.ToString(CultureInfo.InvariantCulture)}," : string.Empty) +
                       (f.TemplateOnly.HasValue ? $"\"templateOnly\":{(f.TemplateOnly.Value ? "true" : "false")}," : string.Empty) +
                       $"\"zDy\":{f.ZDy.ToString(CultureInfo.InvariantCulture)}," +
                       $"\"zRipple\":{f.ZRipple.ToString(CultureInfo.InvariantCulture)}," +
                       $"\"dyPixels\":{f.DyPixels.ToString(CultureInfo.InvariantCulture)}," +
                       $"\"rippleEnergy\":{f.RippleEnergy.ToString(CultureInfo.InvariantCulture)}," +
                       $"\"jerk\":{f.Jerk.ToString().ToLowerInvariant()}," +
                       $"\"flow\":{f.Flow.ToString().ToLowerInvariant()}," +
                       $"\"diff\":{f.Diff.ToString().ToLowerInvariant()}," +
                       $"\"votes\":{f.Votes}," +
                       $"\"microMove\":{f.MicroMove.ToString().ToLowerInvariant()}," +
                       $"\"hookDetected\":{f.HookDetected.ToString().ToLowerInvariant()}," +
                       (f.ScaleUsed.HasValue ? $"\"scaleUsed\":{f.ScaleUsed.Value.ToString(CultureInfo.InvariantCulture)}," : string.Empty) +
                       (f.TemplateId != null ? $"\"templateId\":\"{Escape(f.TemplateId)}\"," : string.Empty) +
                       (f.ColorGateOk.HasValue ? $"\"colorGateOk\":{(f.ColorGateOk.Value ? "true" : "false")}," : string.Empty) +
                       (f.MatchV1.HasValue ? $"\"v1\":{f.MatchV1.Value.ToString(CultureInfo.InvariantCulture)}," : string.Empty) +
                       (f.MatchV2.HasValue ? $"\"v2\":{f.MatchV2.Value.ToString(CultureInfo.InvariantCulture)}," : string.Empty) +
                       (f.MaskOnRatio.HasValue ? $"\"maskOnRatio\":{f.MaskOnRatio.Value.ToString(CultureInfo.InvariantCulture)}," : string.Empty) +
                       (f.Method != null ? $"\"method\":\"{Escape(f.Method)}\"," : string.Empty) +
                       (f.UsedMask.HasValue ? $"\"usedMask\":{(f.UsedMask.Value ? "true" : "false")}," : string.Empty) +
                       (f.Reason != null ? $"\"reason\":\"{Escape(f.Reason)}\"," : string.Empty);
            if (json.EndsWith(',')) json = json.Substring(0, json.Length - 1);
            json += "}";
            if (_jsonlWriter != null)
            {
                WriteLineWithRetry_NoLock(_jsonlWriter, json);
            }

            // Flush periódico para reduzir risco de perda em caso de falhas
            _framesSinceLastFlush++;
            if (_framesSinceLastFlush >= 25)
            {
                try { _csvWriter?.Flush(); } catch {}
                try { _jsonlWriter?.Flush(); } catch {}
                _framesSinceLastFlush = 0;
            }
        }
    }

    public void EndSession()
    {
        lock (_lockObj)
        {
            if (_frames == 0)
            {
                WriteSummary(new List<double>());
                CloseWriters_NoLock();
                return;
            }
            WriteSummary(_scores);
            CloseWriters_NoLock();
        }
    }

    private void WriteSummary(List<double> scores)
    {
        var endUtc = DateTime.UtcNow;
        var durSec = (endUtc - _startUtc).TotalSeconds;
        var mean = scores.Any() ? scores.Average() : 0.0;
        var p95 = scores.Any() ? Percentile(scores, 95) : 0.0;
        var summary = new StringBuilder();
        summary.AppendLine("=== BOBBER VISION SUMMARY ===");
        summary.AppendLine($"session: {_sessionId}");
        summary.AppendLine($"start_utc: {_startUtc:O}");
        summary.AppendLine($"end_utc: {endUtc:O}");
        summary.AppendLine($"duration_sec: {durSec:F2}");
        summary.AppendLine($"frames: {_frames}");
        summary.AppendLine($"detections: {_detections}");
        summary.AppendLine($"meanScore: {mean:F4}");
        summary.AppendLine($"p95Score: {p95:F4}");
        summary.AppendLine($"confidenceThreshold: {_confidence:F3}");
        if (!string.IsNullOrWhiteSpace(_templatePath)) summary.AppendLine($"template: {_templatePath}");
        if (_region != Rectangle.Empty) summary.AppendLine($"region: {_region.X},{_region.Y},{_region.Width},{_region.Height}");

        var path = Path.Combine(_baseDir, $"{_sessionId}_summary.txt");
        try
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            sw.Write(summary.ToString());
        }
        catch
        {
            // último recurso
            try { File.WriteAllText(path, summary.ToString(), Encoding.UTF8); } catch {}
        }
    }

    private static string Bool(bool v) => v ? "true" : "false";
    private static string OptBool(bool? v) => v.HasValue ? (v.Value ? "true" : "false") : string.Empty;
    private static string Num(double v) => v.ToString(CultureInfo.InvariantCulture);
    private static string Num(float v) => v.ToString(CultureInfo.InvariantCulture);
    private static string Int(int v) => v.ToString(CultureInfo.InvariantCulture);
    private static string OptNum(double? v) => v.HasValue ? v.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
    private static string OptInt(int? v) => v.HasValue ? v.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
    private static string Quote(string? s) => s == null ? string.Empty : $"\"{Escape(s)}\"";
    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static double Percentile(List<double> sequence, double percentile)
    {
        if (sequence.Count == 0) return 0;
        var sorted = sequence.OrderBy(x => x).ToList();
        var position = (percentile / 100.0) * (sorted.Count + 1);
        var index = (int)Math.Floor(position) - 1;
        var fraction = position - Math.Floor(position);
        if (index < 0) return sorted.First();
        if (index >= sorted.Count - 1) return sorted.Last();
        return sorted[index] + fraction * (sorted[index + 1] - sorted[index]);
    }

    // --- IO helpers (não expostos) ---
    private void OpenWriters_NoLock()
    {
        try
        {
            _csvStream = new FileStream(_csvPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            _csvWriter = new StreamWriter(_csvStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = false,
                NewLine = Environment.NewLine
            };
        }
        catch
        {
            // fallback para escrita direta
            _csvStream = null;
            _csvWriter = null;
        }

        try
        {
            _jsonlStream = new FileStream(_jsonlPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            _jsonlWriter = new StreamWriter(_jsonlStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = false,
                NewLine = Environment.NewLine
            };
        }
        catch
        {
            _jsonlStream = null;
            _jsonlWriter = null;
        }
    }

    private void CloseWriters_NoLock()
    {
        try { _csvWriter?.Flush(); } catch {}
        try { _jsonlWriter?.Flush(); } catch {}
        try { _csvWriter?.Dispose(); } catch {}
        try { _jsonlWriter?.Dispose(); } catch {}
        try { _csvStream?.Dispose(); } catch {}
        try { _jsonlStream?.Dispose(); } catch {}
        _csvWriter = null;
        _jsonlWriter = null;
        _csvStream = null;
        _jsonlStream = null;
    }

    private void WriteLineWithRetry_NoLock(StreamWriter writer, string line, int maxRetries = 3, int backoffMs = 50)
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                writer.WriteLine(line);
                return;
            }
            catch (IOException)
            {
                if (attempt == maxRetries) throw;
                try { Thread.Sleep(backoffMs * (attempt + 1)); } catch {}
                continue;
            }
            catch
            {
                // Em qualquer outra exceção, não propagar para não interromper detecção
                return;
            }
        }
    }
}


