using Avalonia.Controls;
using Avalonia.Interactivity;
using StatisticsAnalysisTool.macOS.ViewModels;

namespace StatisticsAnalysisTool.macOS.Views;

public partial class GameDataPreparationWindow : Window
{
    private readonly GameDataPreparationWindowViewModel _viewModel;

    public GameDataPreparationWindow()
    {
        InitializeComponent();
        _viewModel = new GameDataPreparationWindowViewModel();
        DataContext = _viewModel;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private async void SelectGameFolder_Click(object? sender, RoutedEventArgs e)
    {
        await _viewModel.OpenPathSelectionCommand.ExecuteAsync(null);
    }

    private async void ConfirmButton_Click(object? sender, RoutedEventArgs e)
    {
        await _viewModel.ConfirmSelectionCommand.ExecuteAsync(null);
        Close(true);
    }
}
