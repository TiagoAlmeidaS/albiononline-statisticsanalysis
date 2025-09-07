using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.macOS.Controls;

/// <summary>
/// Guild control with Albion Online visual style for guild information and management
/// </summary>
public partial class GuildControl : UserControl
{
    private readonly ObservableCollection<GuildMember> _guildMembers = new();
    private GuildInfo _guildInfo = new();

    public GuildControl()
    {
        InitializeComponent();
        SetupGuildControl();
    }

    private void SetupGuildControl()
    {
        // Initialize guild control
        GuildMembersListBox.ItemsSource = _guildMembers;
        
        // Add some sample data
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        _guildInfo = new GuildInfo
        {
            Name = "Dark Knights",
            Alliance = "Shadow Empire",
            MemberCount = 47,
            OnlineMembers = 23,
            TotalFame = 1200000,
            GuildLevel = 15,
            Territories = 3,
            KillFame = 850000,
            GatheringFame = 350000,
            GuildId = "DK-001"
        };

        _guildMembers.Add(new GuildMember
        {
            Name = "GuildMaster",
            Role = "Guild Master",
            Tier = "T8.3",
            FamePerHour = 1250,
            IsOnline = true,
            Icon = "👑",
            Color = "#4CAF50"
        });

        _guildMembers.Add(new GuildMember
        {
            Name = "WarriorPlayer",
            Role = "Officer",
            Tier = "T7.2",
            FamePerHour = 890,
            IsOnline = true,
            Icon = "⚔️",
            Color = "#2196F3"
        });

        _guildMembers.Add(new GuildMember
        {
            Name = "MagePlayer",
            Role = "Member",
            Tier = "T6.1",
            FamePerHour = 650,
            IsOnline = false,
            Icon = "🧙",
            Color = "#FF9800"
        });

        _guildMembers.Add(new GuildMember
        {
            Name = "ArcherPlayer",
            Role = "Member",
            Tier = "T5.4",
            FamePerHour = 420,
            IsOnline = true,
            Icon = "🏹",
            Color = "#9C27B0"
        });

        UpdateGuildInfo();
    }

    #region Event Handlers

    private void RefreshGuild_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement guild data refresh
        Console.WriteLine("Refresh Guild clicked");
        UpdateGuildInfo();
    }

    private void ViewStatistics_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement statistics viewing
        Console.WriteLine("View Statistics clicked");
    }

    private void ManageMembers_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement member management
        Console.WriteLine("Manage Members clicked");
    }

    private void GuildWars_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement guild wars viewing
        Console.WriteLine("Guild Wars clicked");
    }

    private void DetailedStats_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement detailed statistics
        Console.WriteLine("Detailed Stats clicked");
    }

    #endregion

    #region Private Methods

    private void UpdateGuildInfo()
    {
        GuildNameText.Text = _guildInfo.Name;
        AllianceNameText.Text = _guildInfo.Alliance;
        MemberCountText.Text = _guildInfo.MemberCount.ToString();
        OnlineMembersText.Text = _guildInfo.OnlineMembers.ToString();
        TotalFameText.Text = FormatNumber(_guildInfo.TotalFame);
        GuildLevelText.Text = _guildInfo.GuildLevel.ToString();
        TerritoriesText.Text = _guildInfo.Territories.ToString();
        KillFameText.Text = FormatNumber(_guildInfo.KillFame);
        GatheringFameText.Text = FormatNumber(_guildInfo.GatheringFame);
        GuildIdText.Text = _guildInfo.GuildId;
        LastUpdateText.Text = DateTime.Now.ToString("HH:mm:ss");
    }

    private string FormatNumber(long number)
    {
        if (number >= 1000000)
            return $"{number / 1000000.0:F1}M";
        if (number >= 1000)
            return $"{number / 1000.0:F1}K";
        return number.ToString();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the guild information
    /// </summary>
    /// <param name="guildInfo">New guild information</param>
    public void UpdateGuildInfo(GuildInfo guildInfo)
    {
        _guildInfo = guildInfo;
        UpdateGuildInfo();
    }

    /// <summary>
    /// Adds a guild member
    /// </summary>
    /// <param name="member">Guild member to add</param>
    public void AddGuildMember(GuildMember member)
    {
        _guildMembers.Add(member);
    }

    /// <summary>
    /// Removes a guild member
    /// </summary>
    /// <param name="memberName">Name of member to remove</param>
    public void RemoveGuildMember(string memberName)
    {
        var member = _guildMembers.FirstOrDefault(m => m.Name == memberName);
        if (member != null)
        {
            _guildMembers.Remove(member);
        }
    }

    /// <summary>
    /// Updates a guild member's information
    /// </summary>
    /// <param name="memberName">Name of member to update</param>
    /// <param name="updatedMember">Updated member information</param>
    public void UpdateGuildMember(string memberName, GuildMember updatedMember)
    {
        var member = _guildMembers.FirstOrDefault(m => m.Name == memberName);
        if (member != null)
        {
            var index = _guildMembers.IndexOf(member);
            _guildMembers[index] = updatedMember;
        }
    }

    /// <summary>
    /// Gets the current guild information
    /// </summary>
    /// <returns>Current guild information</returns>
    public GuildInfo GetGuildInfo()
    {
        return _guildInfo;
    }

    /// <summary>
    /// Gets all guild members
    /// </summary>
    /// <returns>List of guild members</returns>
    public List<GuildMember> GetGuildMembers()
    {
        return _guildMembers.ToList();
    }

    #endregion
}

/// <summary>
/// Represents guild information
/// </summary>
public class GuildInfo
{
    public string Name { get; set; } = string.Empty;
    public string Alliance { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int OnlineMembers { get; set; }
    public long TotalFame { get; set; }
    public int GuildLevel { get; set; }
    public int Territories { get; set; }
    public long KillFame { get; set; }
    public long GatheringFame { get; set; }
    public string GuildId { get; set; } = string.Empty;
}

/// <summary>
/// Represents a guild member
/// </summary>
public class GuildMember
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public int FamePerHour { get; set; }
    public bool IsOnline { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
