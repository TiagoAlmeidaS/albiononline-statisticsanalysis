using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using StatisticsAnalysisTool.Core.Interfaces;
using StatisticsAnalysisTool.macOS.Platform;

namespace StatisticsAnalysisTool.macOS.Controls
{
    public partial class DungeonControl : UserControl
    {
        private readonly IDialogService _dialogService;

        public DungeonControl()
        {
            InitializeComponent();
            _dialogService = new MacOSDialogService();
            LoadDungeonData();
        }

        private void LoadDungeonData()
        {
            // Simular dados de dungeon
            UpdateDungeonStatistics();
            UpdateLastUpdateTime();
        }

        private void UpdateDungeonStatistics()
        {
            // Atualizar estatísticas (simulado)
            if (TotalDungeonsText != null)
                TotalDungeonsText.Text = "24";
            
            if (TodayDungeonsText != null)
                TodayDungeonsText.Text = "3";
            
            if (TotalFameText != null)
                TotalFameText.Text = "125,000";
            
            if (CompletedDungeonsText != null)
                CompletedDungeonsText.Text = "18";
            
            if (FailedDungeonsText != null)
                FailedDungeonsText.Text = "6";
            
            if (SuccessRateText != null)
                SuccessRateText.Text = "75%";
            
            if (AvgFameText != null)
                AvgFameText.Text = "5,200";
            
            if (BestRunText != null)
                BestRunText.Text = "25,800";
            
            if (TotalTimeText != null)
                TotalTimeText.Text = "12h 30m";
        }

        private void UpdateLastUpdateTime()
        {
            if (LastUpdateText != null)
                LastUpdateText.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void ResetDungeons_Click(object sender, RoutedEventArgs e)
        {
            // Implementar reset de dungeons
            // TODO: Implement proper confirmation dialog
            var result = true; // Simplified for now

            if (result)
            {
                // Resetar dados
                LoadDungeonData();
            }
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            // Implementar exclusão de dungeons selecionados
            if (DungeonListBox?.SelectedItems?.Count > 0)
            {
                // TODO: Implement proper confirmation dialog
                var result = true; // Simplified for now

                if (result)
                {
                    // Deletar itens selecionados
                    UpdateSelectedCount();
                }
            }
            else
            {
                _dialogService.ShowMessageBox("No Selection", "Please select one or more dungeons to delete.");
            }
        }

        private void ViewAnalytics_Click(object sender, RoutedEventArgs e)
        {
            // Implementar visualização de analytics
            _dialogService.ShowMessageBox("Analytics", "Dungeon analytics feature coming soon!");
        }

        private void FameChart_Click(object sender, RoutedEventArgs e)
        {
            // Implementar gráfico de fama
            _dialogService.ShowMessageBox("Fame Chart", "Fame chart feature coming soon!");
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            // Implementar configurações de dungeon
            _dialogService.ShowMessageBox("Dungeon Settings", "Dungeon settings feature coming soon!");
        }

        private void UpdateSelectedCount()
        {
            if (SelectedCountText != null && DungeonListBox != null)
            {
                var selectedCount = DungeonListBox.SelectedItems?.Count ?? 0;
                SelectedCountText.Text = selectedCount.ToString();
            }
        }

        private void DungeonListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedCount();
        }
    }
}
