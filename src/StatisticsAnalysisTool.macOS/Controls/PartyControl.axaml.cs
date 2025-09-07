using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.macOS.Controls;

/// <summary>
/// Party control for managing party members
/// </summary>
public partial class PartyControl : UserControl, INotifyPropertyChanged
{
    private ObservableCollection<PartyMember> _partyMembers;
    private PartyMember? _selectedPartyMember;
    private int _partyMemberCount;

    public PartyControl()
    {
        InitializeComponent();
        _partyMembers = new ObservableCollection<PartyMember>();
        DataContext = this;
        
        // Initialize with sample data
        InitializeSampleData();
    }

    public ObservableCollection<PartyMember> PartyMembers
    {
        get => _partyMembers;
        set => SetProperty(ref _partyMembers, value);
    }

    public PartyMember? SelectedPartyMember
    {
        get => _selectedPartyMember;
        set => SetProperty(ref _selectedPartyMember, value);
    }

    public int PartyMemberCount
    {
        get => _partyMemberCount;
        set => SetProperty(ref _partyMemberCount, value);
    }

    public bool HasSelectedMember => SelectedPartyMember != null;

    private void InitializeSampleData()
    {
        // Add sample party members
        PartyMembers.Add(new PartyMember
        {
            PlayerName = "Player1",
            GuildName = "Test Guild",
            CurrentMap = "Lymhurst",
            StatusColor = "#4CAF50" // Green for online
        });
        
        PartyMembers.Add(new PartyMember
        {
            PlayerName = "Player2",
            GuildName = "Test Guild",
            CurrentMap = "Bridgewatch",
            StatusColor = "#F39C12" // Orange for away
        });
        
        PartyMembers.Add(new PartyMember
        {
            PlayerName = "Player3",
            GuildName = "Another Guild",
            CurrentMap = "Martlock",
            StatusColor = "#E74C3C" // Red for offline
        });
        
        UpdatePartyMemberCount();
    }

    private void UpdatePartyMemberCount()
    {
        PartyMemberCount = PartyMembers.Count;
    }

    private void SearchButton_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement search functionality
        System.Diagnostics.Debug.WriteLine("Search button clicked");
    }

    private void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement refresh functionality
        System.Diagnostics.Debug.WriteLine("Refresh button clicked");
    }

    private void CopyPartyInfo_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement copy to clipboard functionality
        System.Diagnostics.Debug.WriteLine("Copy party info clicked");
    }

    private void AddMember_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement add member functionality
        System.Diagnostics.Debug.WriteLine("Add member clicked");
    }

    private void RemoveSelected_Click(object? sender, RoutedEventArgs e)
    {
        if (SelectedPartyMember != null)
        {
            PartyMembers.Remove(SelectedPartyMember);
            UpdatePartyMemberCount();
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

/// <summary>
/// Party member data model
/// </summary>
public class PartyMember : INotifyPropertyChanged
{
    private string _playerName = string.Empty;
    private string _guildName = string.Empty;
    private string _currentMap = string.Empty;
    private string _statusColor = "#4CAF50";

    public string PlayerName
    {
        get => _playerName;
        set => SetProperty(ref _playerName, value);
    }

    public string GuildName
    {
        get => _guildName;
        set => SetProperty(ref _guildName, value);
    }

    public string CurrentMap
    {
        get => _currentMap;
        set => SetProperty(ref _currentMap, value);
    }

    public string StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
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
