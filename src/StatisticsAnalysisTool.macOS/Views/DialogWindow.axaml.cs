using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.macOS.Views;

/// <summary>
/// Dialog window for displaying messages and getting user input
/// </summary>
public partial class DialogWindow : Window, INotifyPropertyChanged
{
    private string _title = "Dialog";
    private string _message = "Message";
    private string _yesButtonText = "Yes";
    private string _noButtonText = "No";
    private string _okButtonText = "OK";
    private bool _showYesNoButtons = true;
    private bool _showOkButton = false;
    private bool _result = false;

    public DialogWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public DialogWindow(string title, string message, bool showYesNo = true) : this()
    {
        Title = title;
        Message = message;
        ShowYesNoButtons = showYesNo;
        ShowOkButton = !showYesNo;
    }

    public new string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string YesButtonText
    {
        get => _yesButtonText;
        set => SetProperty(ref _yesButtonText, value);
    }

    public string NoButtonText
    {
        get => _noButtonText;
        set => SetProperty(ref _noButtonText, value);
    }

    public string OkButtonText
    {
        get => _okButtonText;
        set => SetProperty(ref _okButtonText, value);
    }

    public bool ShowYesNoButtons
    {
        get => _showYesNoButtons;
        set => SetProperty(ref _showYesNoButtons, value);
    }

    public bool ShowOkButton
    {
        get => _showOkButton;
        set => SetProperty(ref _showOkButton, value);
    }

    public bool Result
    {
        get => _result;
        private set => SetProperty(ref _result, value);
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }

    private void YesButton_Click(object? sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
    }

    private void NoButton_Click(object? sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
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
