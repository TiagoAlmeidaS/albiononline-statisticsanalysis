using AlbionFishing.Core;
using Microsoft.Extensions.Logging;

namespace AlbionFishing.Automation;

public class AutomationManager : IAutomationManager
{
    private readonly IAutomationService _automationService;
    private readonly ILogger<AutomationManager>? _logger;
    private readonly Random _random;

    public AutomationManager(ILogger<AutomationManager>? logger = null)
    {
        _logger = logger;
        _random = new Random();
        
        try
        {
            _automationService = AutomationServiceFactory.Create();
            _logger?.LogInformation("Automação inicializada para plataforma: {Platform}", 
                AutomationServiceFactory.GetPlatformInfo());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Erro ao inicializar automação");
            throw;
        }
    }

    public void ClickWithDelay(double minDelay = 0.2, double maxDelay = 0.4, string button = "left")
    {
        double delay = _random.NextDouble() * (maxDelay - minDelay) + minDelay;
        Thread.Sleep((int)(delay * 1000));
        _automationService.Click(button);
    }

    public void ClickAtWithDelay(int x, int y, double minDelay = 0.2, double maxDelay = 0.4, string button = "left")
    {
        double delay = _random.NextDouble() * (maxDelay - minDelay) + minDelay;
        Thread.Sleep((int)(delay * 1000));
        _automationService.ClickAt(x, y, button);
    }

    public void ClickFishingArea(BotConfig config)
    {
        if (config.Fishing.Area.Count < 4) return;

        int x = config.Fishing.Area[0] + config.Fishing.Area[2] / 2;
        int y = config.Fishing.Area[1] + config.Fishing.Area[3] / 2;

        // Usar delays fixos já que não temos mais Cast_Delay_Min/Max
        double minDelay = 0.1; // 100ms
        double maxDelay = 0.2; // 200ms

        ClickAtWithDelay(x, y, minDelay, maxDelay, "left");
    }

    public void MoveTo(int x, int y, int durationMs = 0)
    {
        _automationService.MoveTo(x, y, durationMs);
    }

    public void PressKey(string key)
    {
        _automationService.SendKey(key);
    }

    public (int x, int y) GetMousePosition()
    {
        return _automationService.GetCurrentPosition();
    }

    public void MouseDown(string button = "left")
    {
        _automationService.MouseDown(button);
    }

    public void MouseUp(string button = "left")
    {
        _automationService.MouseUp(button);
    }
    
     public void ClickFishingAreaToCastHook(BotConfig config, double minDelay = 0.2, double maxDelay = 0.4)
    {
        try
        {
            // ✅ VALIDAÇÃO: Garantir que a configuração é válida
            if (config?.Fishing?.Area == null || config.Fishing.Area.Count < 4)
            {
                _logger?.LogError("❌ Configuração de área de pesca inválida");
                throw new ArgumentException("Configuração de área de pesca inválida");
            }

            // ✅ VALIDAÇÃO: Garantir que os delays são válidos
            if (minDelay < 0 || maxDelay < 0 || minDelay > maxDelay)
            {
                _logger?.LogWarning("⚠️ Delays inválidos: min={MinDelay}, max={MaxDelay}. Usando valores padrão.", minDelay, maxDelay);
                minDelay = 0.2;
                maxDelay = 0.4;
            }

            // ✅ MELHORIA: Gerar posição aleatória dentro da área de pesca
            var area = config.Fishing.Area;
            int x = _random.Next(area[0], area[0] + area[2]);
            int y = _random.Next(area[1], area[1] + area[3]);

            // ✅ MELHORIA: Gerar delay aleatório para pressionar o botão
            var pressToCastHook = _random.NextDouble() * (maxDelay - minDelay) + minDelay;

            _logger?.LogDebug("🎣 ClickFishingAreaToCastHook - Área: [{Area}], Posição: ({X}, {Y}), PressDelay: {PressDelay:F3}s", 
                string.Join(", ", area), x, y, pressToCastHook);

            // ✅ MELHORIA: Mover para a posição antes de clicar
            _automationService.MoveTo(x, y, 100); // Movimento suave de 100ms
            Thread.Sleep(50); // Pequena pausa para estabilizar

            _automationService.MouseDown("left");
            // ✅ CORRIGIDO: Converter segundos para milissegundos
            Thread.Sleep((int)(pressToCastHook * 1000));
            _automationService.MouseUp("left");

            _logger?.LogDebug("✅ ClickFishingAreaToCastHook executado com sucesso");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "❌ Erro em ClickFishingAreaToCastHook");
            throw;
        }
    }
} 