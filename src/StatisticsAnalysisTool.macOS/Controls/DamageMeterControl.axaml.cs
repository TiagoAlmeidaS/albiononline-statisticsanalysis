using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.macOS.Controls;

/// <summary>
/// Damage meter control with Albion Online visual style for tracking combat damage
/// </summary>
public partial class DamageMeterControl : UserControl
{
    private readonly ObservableCollection<DamageEntry> _damageEntries = new();
    private bool _isActive = false;
    private DateTime _fightStartTime;
    private long _totalDamage = 0;

    public DamageMeterControl()
    {
        InitializeComponent();
        SetupDamageMeter();
    }

    private void SetupDamageMeter()
    {
        // Initialize damage meter control
        DamageListBox.ItemsSource = _damageEntries;
        
        // Add some sample data
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        _damageEntries.Add(new DamageEntry
        {
            PlayerName = "PlayerName",
            Weapon = "Sword T4.2",
            Damage = 15420,
            Percentage = 45.2,
            Icon = "👤",
            Color = "#4CAF50"
        });

        _damageEntries.Add(new DamageEntry
        {
            PlayerName = "MagePlayer",
            Weapon = "Staff T3.1",
            Damage = 8750,
            Percentage = 25.8,
            Icon = "🧙",
            Color = "#2196F3"
        });

        _damageEntries.Add(new DamageEntry
        {
            PlayerName = "ArcherPlayer",
            Weapon = "Bow T5.3",
            Damage = 6230,
            Percentage = 18.4,
            Icon = "🏹",
            Color = "#FF9800"
        });

        _damageEntries.Add(new DamageEntry
        {
            PlayerName = "HealerPlayer",
            Weapon = "Holy T2.4",
            Damage = 2600,
            Percentage = 7.7,
            Icon = "🔮",
            Color = "#9C27B0"
        });

        UpdateStatistics();
    }

    #region Event Handlers

    private void ResetDamageMeter_Click(object? sender, RoutedEventArgs e)
    {
        ResetDamageMeter();
    }

    private void OpenDamageMeterWindow_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement damage meter window opening
        Console.WriteLine("Open Damage Meter Window clicked");
    }

    private void CopyToClipboard_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement copy to clipboard functionality
        Console.WriteLine("Copy to Clipboard clicked");
    }

    private void TakeSnapshot_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement snapshot functionality
        Console.WriteLine("Take Snapshot clicked");
    }

    private void SaveReport_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement save report functionality
        Console.WriteLine("Save Report clicked");
    }

    #endregion

    #region Private Methods

    private void ResetDamageMeter()
    {
        _damageEntries.Clear();
        _totalDamage = 0;
        _isActive = false;
        
        UpdateStatus(false);
        UpdateStatistics();
        
        Console.WriteLine("Damage meter reset");
    }

    private void UpdateStatus(bool isActive)
    {
        if (isActive)
        {
            StatusText.Text = "Active";
            StatusDescriptionText.Text = "Tracking damage dealt to enemies";
        }
        else
        {
            StatusText.Text = "Inactive";
            StatusDescriptionText.Text = "Damage meter is not tracking";
        }
    }

    private void UpdateStatistics()
    {
        _totalDamage = 0;
        foreach (var entry in _damageEntries)
        {
            _totalDamage += entry.Damage;
        }

        TotalDamageText.Text = _totalDamage.ToString("N0");
        ParticipantsText.Text = _damageEntries.Count.ToString();

        if (_isActive && _fightStartTime != DateTime.MinValue)
        {
            var duration = DateTime.Now - _fightStartTime;
            FightDurationText.Text = duration.ToString(@"mm\:ss");
            
            if (duration.TotalSeconds > 0)
            {
                var dps = _totalDamage / duration.TotalSeconds;
                DpsText.Text = ((long)dps).ToString("N0");
            }
        }
        else
        {
            FightDurationText.Text = "00:00";
            DpsText.Text = "0";
        }

        LastUpdateText.Text = DateTime.Now.ToString("HH:mm:ss");
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts the damage meter tracking
    /// </summary>
    public void StartTracking()
    {
        _isActive = true;
        _fightStartTime = DateTime.Now;
        UpdateStatus(true);
    }

    /// <summary>
    /// Stops the damage meter tracking
    /// </summary>
    public void StopTracking()
    {
        _isActive = false;
        UpdateStatus(false);
    }

    /// <summary>
    /// Adds a damage entry
    /// </summary>
    /// <param name="playerName">Name of the player</param>
    /// <param name="weapon">Weapon used</param>
    /// <param name="damage">Damage amount</param>
    /// <param name="icon">Player icon</param>
    /// <param name="color">Player color</param>
    public void AddDamageEntry(string playerName, string weapon, long damage, string icon, string color)
    {
        // Check if player already exists
        var existingEntry = _damageEntries.FirstOrDefault(e => e.PlayerName == playerName);
        if (existingEntry != null)
        {
            existingEntry.Damage += damage;
        }
        else
        {
            _damageEntries.Add(new DamageEntry
            {
                PlayerName = playerName,
                Weapon = weapon,
                Damage = damage,
                Icon = icon,
                Color = color
            });
        }

        // Recalculate percentages
        RecalculatePercentages();
        UpdateStatistics();
    }

    /// <summary>
    /// Recalculates damage percentages for all entries
    /// </summary>
    private void RecalculatePercentages()
    {
        if (_totalDamage == 0) return;

        foreach (var entry in _damageEntries)
        {
            entry.Percentage = (_totalDamage > 0) ? (entry.Damage * 100.0) / _totalDamage : 0;
        }
    }

    /// <summary>
    /// Gets the current damage meter statistics
    /// </summary>
    /// <returns>Damage meter statistics</returns>
    public DamageMeterStats GetStats()
    {
        return new DamageMeterStats
        {
            TotalDamage = _totalDamage,
            Participants = _damageEntries.Count,
            IsActive = _isActive,
            FightDuration = _isActive ? DateTime.Now - _fightStartTime : TimeSpan.Zero,
            Entries = _damageEntries.ToList()
        };
    }

    #endregion
}

/// <summary>
/// Represents a damage entry
/// </summary>
public class DamageEntry
{
    public string PlayerName { get; set; } = string.Empty;
    public string Weapon { get; set; } = string.Empty;
    public long Damage { get; set; }
    public double Percentage { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// Represents damage meter statistics
/// </summary>
public class DamageMeterStats
{
    public long TotalDamage { get; set; }
    public int Participants { get; set; }
    public bool IsActive { get; set; }
    public TimeSpan FightDuration { get; set; }
    public List<DamageEntry> Entries { get; set; } = new();
}
