using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.macOS.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private string _greeting = "Welcome to Avalonia!";
    private bool _errorBarVisibility = false;
    private string _errorBarText = string.Empty;
    private bool _warningBarVisibility = false;
    private string _warningBarText = string.Empty;
    private bool _informationBarVisibility = false;
    private string _informationBarText = string.Empty;

    public string Greeting
    {
        get => _greeting;
        set => SetProperty(ref _greeting, value);
    }

    public bool ErrorBarVisibility
    {
        get => _errorBarVisibility;
        set => SetProperty(ref _errorBarVisibility, value);
    }

    public string ErrorBarText
    {
        get => _errorBarText;
        set => SetProperty(ref _errorBarText, value);
    }

    public bool WarningBarVisibility
    {
        get => _warningBarVisibility;
        set => SetProperty(ref _warningBarVisibility, value);
    }

    public string WarningBarText
    {
        get => _warningBarText;
        set => SetProperty(ref _warningBarText, value);
    }

    public bool InformationBarVisibility
    {
        get => _informationBarVisibility;
        set => SetProperty(ref _informationBarVisibility, value);
    }

    public string InformationBarText
    {
        get => _informationBarText;
        set => SetProperty(ref _informationBarText, value);
    }

    public void ShowError(string message)
    {
        ErrorBarText = message;
        ErrorBarVisibility = true;
    }

    public void ShowWarning(string message)
    {
        WarningBarText = message;
        WarningBarVisibility = true;
    }

    public void ShowInformation(string message)
    {
        InformationBarText = message;
        InformationBarVisibility = true;
    }

    public void HideAllBars()
    {
        ErrorBarVisibility = false;
        WarningBarVisibility = false;
        InformationBarVisibility = false;
    }
}
