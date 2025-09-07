using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StatisticsAnalysisTool.Core.Interfaces;
using StatisticsAnalysisTool.Core;
using StatisticAnalysisTool.Extractor.Enums;

namespace StatisticsAnalysisTool.macOS.ViewModels;

public partial class GameDataPreparationWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    
    [ObservableProperty]
    private string _title = "Game Data Preparation";
    
    [ObservableProperty]
    private string _selectedGameFolder = string.Empty;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    private bool _isConfirmButtonEnabled = false;
    
    [ObservableProperty]
    private string _translationSelectMainGameFolder = "SELECT ALBION ONLINE MAIN GAME FOLDER...";
    
    [ObservableProperty]
    private string _translationConfirm = "CONFIRM";
    
    [ObservableProperty]
    private string _translationSteamLauncher = "STEAM LAUNCHER";
    
    [ObservableProperty]
    private string _translationSteamLauncherMessage = "If you installed Albion Online through Steam, please select the Steam installation folder.";
    
    // Caminhos padrão do Albion Online no macOS
    private static readonly string[] DefaultGamePaths = {
        "/Applications/Albion Online.app/Contents/Resources/Data",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                    "Library/Application Support/Steam/steamapps/common/Albion Online/Albion-Online.app/Contents/Resources/Data"),
        "/Applications/Steam.app/Contents/MacOS/steamapps/common/Albion Online/Albion-Online.app/Contents/Resources/Data"
    };

    public GameDataPreparationWindowViewModel()
    {
        _dialogService = ServiceLocator.Resolve<IDialogService>();
        TryDetectGameFolder();
    }

    [RelayCommand]
    private async Task OpenPathSelection()
    {
        try
        {
            ErrorMessage = string.Empty;
            
            // Usar o serviço de diálogo nativo do macOS
            var selectedPath = _dialogService.ShowFolderDialog("Select Albion Online Game Folder");
            
            if (!string.IsNullOrEmpty(selectedPath))
            {
                SelectedGameFolder = selectedPath;
                await ValidateGameFolder();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error selecting folder: {ex.Message}";
        }
    }

    [RelayCommand]
    private Task ConfirmSelection()
    {
        if (IsValidGameFolder(SelectedGameFolder))
        {
            // Salvar a pasta selecionada nas configurações
            // TODO: Implementar salvamento nas configurações
        }
        return Task.CompletedTask;
    }

    private async Task ValidateGameFolder()
    {
        if (string.IsNullOrEmpty(SelectedGameFolder))
        {
            IsConfirmButtonEnabled = false;
            return;
        }

        try
        {
            if (IsValidGameFolder(SelectedGameFolder))
            {
                ErrorMessage = string.Empty;
                IsConfirmButtonEnabled = true;
            }
            else
            {
                ErrorMessage = "Invalid game folder. Please select a valid Albion Online installation folder.";
                IsConfirmButtonEnabled = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error validating folder: {ex.Message}";
            IsConfirmButtonEnabled = false;
        }
    }

    private void TryDetectGameFolder()
    {
        foreach (var path in DefaultGamePaths)
        {
            if (IsValidGameFolder(path))
            {
                SelectedGameFolder = path;
                IsConfirmButtonEnabled = true;
                break;
            }
        }
    }

    private static bool IsValidGameFolder(string gameFolder)
    {
        if (string.IsNullOrEmpty(gameFolder) || !Directory.Exists(gameFolder))
            return false;

        try
        {
            // Verificar se contém a estrutura de pastas do Albion Online
            var gameDataPath = Path.Combine(gameFolder, "Albion-Online_Data", "StreamingAssets", "GameData");
            
            if (!Directory.Exists(gameDataPath))
                return false;

            // Verificar arquivos essenciais
            var essentialFiles = new[]
            {
                "items.bin",
                "mobs.bin",
                "spells.bin",
                "localization.bin"
            };

            foreach (var file in essentialFiles)
            {
                if (!File.Exists(Path.Combine(gameDataPath, file)))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetServerTypeString(ServerType serverType)
    {
        return serverType switch
        {
            ServerType.Staging => "staging",
            ServerType.Playground => "playground",
            _ => "game"
        };
    }
}
