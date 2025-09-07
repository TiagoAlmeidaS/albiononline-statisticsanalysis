using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.macOS.Views;

/// <summary>
/// Loading window for displaying progress during tool initialization
/// </summary>
public partial class ToolLoadingWindow : Window, INotifyPropertyChanged
{
    private double _progressValue = 0;
    private string _loadingText = "Loading...";

    public ToolLoadingWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public ToolLoadingWindow(string loadingText) : this()
    {
        LoadingText = loadingText;
    }

    public double ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    public string LoadingText
    {
        get => _loadingText;
        set => SetProperty(ref _loadingText, value);
    }

    public void UpdateProgress(double value, string? text = null)
    {
        ProgressValue = Math.Max(0, Math.Min(100, value));
        if (!string.IsNullOrEmpty(text))
        {
            LoadingText = text;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
