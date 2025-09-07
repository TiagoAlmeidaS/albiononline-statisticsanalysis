using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.macOS.Controls
{
    public partial class DungeonControl : UserControl
    {
        public DungeonControl()
        {
            InitializeComponent();
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
            var result = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Reset Dungeons", 
                    "Are you sure you want to reset all dungeon data? This action cannot be undone.",
                    MessageBox.Avalonia.Enums.ButtonEnum.YesNo,
                    MessageBox.Avalonia.Enums.Icon.Question)
                .Show();

            if (result == MessageBox.Avalonia.Enums.ButtonResult.Yes)
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
                var result = MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Delete Dungeons", 
                        $"Are you sure you want to delete {DungeonListBox.SelectedItems.Count} selected dungeon(s)?",
                        MessageBox.Avalonia.Enums.ButtonEnum.YesNo,
                        MessageBox.Avalonia.Enums.Icon.Warning)
                    .Show();

                if (result == MessageBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    // Deletar itens selecionados
                    UpdateSelectedCount();
                }
            }
            else
            {
                MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("No Selection", 
                        "Please select one or more dungeons to delete.",
                        MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                        MessageBox.Avalonia.Enums.Icon.Information)
                    .Show();
            }
        }

        private void ViewAnalytics_Click(object sender, RoutedEventArgs e)
        {
            // Implementar visualização de analytics
            MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Analytics", 
                    "Dungeon analytics feature coming soon!",
                    MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    MessageBox.Avalonia.Enums.Icon.Information)
                .Show();
        }

        private void FameChart_Click(object sender, RoutedEventArgs e)
        {
            // Implementar gráfico de fama
            MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Fame Chart", 
                    "Fame chart feature coming soon!",
                    MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    MessageBox.Avalonia.Enums.Icon.Information)
                .Show();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            // Implementar configurações de dungeon
            MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Dungeon Settings", 
                    "Dungeon settings feature coming soon!",
                    MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    MessageBox.Avalonia.Enums.Icon.Information)
                .Show();
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
