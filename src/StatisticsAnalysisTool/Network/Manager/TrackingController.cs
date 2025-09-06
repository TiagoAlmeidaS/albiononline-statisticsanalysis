using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Core;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Guild;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.PacketProviders;
using StatisticsAnalysisTool.Party;
using StatisticsAnalysisTool.StorageHistory;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Network.Operations;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using StatisticsAnalysisTool.Diagnostics;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Manager;

public class TrackingController : ITrackingController
{
    private const int MaxNotifications = 4000;

    private NetworkManager _networkManager;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public readonly LiveStatsTracker LiveStatsTracker;
    public readonly CombatController CombatController;
    public readonly DungeonController DungeonController;
    public readonly ClusterController ClusterController;
    public readonly EntityController EntityController;
    public readonly LootController LootController;
    public readonly StatisticController StatisticController;
    public readonly TreasureController TreasureController;
    public readonly MailController MailController;
    public readonly MarketController MarketController;
    public readonly TradeController TradeController;
    public readonly VaultController VaultController;
    public readonly GatheringController GatheringController;
    public readonly PartyController PartyController;
    public readonly GuildController GuildController;
    private readonly List<LoggingFilterType> _notificationTypesFilters = [];

    public TrackingController(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        ClusterController = new ClusterController(this, mainWindowViewModel);
        EntityController = new EntityController(this, mainWindowViewModel);
        DungeonController = new DungeonController(this, mainWindowViewModel);
        CombatController = new CombatController(this, mainWindowViewModel);
        LootController = new LootController(this, mainWindowViewModel);
        StatisticController = new StatisticController(this, mainWindowViewModel);
        TreasureController = new TreasureController(this, mainWindowViewModel);
        MailController = new MailController(this, mainWindowViewModel);
        MarketController = new MarketController(this, mainWindowViewModel);
        TradeController = new TradeController(this, mainWindowViewModel);
        VaultController = new VaultController(mainWindowViewModel);
        GatheringController = new GatheringController(this, mainWindowViewModel);
        PartyController = new PartyController(this, mainWindowViewModel);
        GuildController = new GuildController(this, mainWindowViewModel);
        LiveStatsTracker = new LiveStatsTracker(this, mainWindowViewModel);

        _ = InitTrackingAsync();
    }

    #region Tracking

    public async Task InitTrackingAsync()
    {
        await StartTrackingAsync();

        _mainWindowViewModel.IsDamageMeterTrackingActive = SettingsController.CurrentSettings.IsDamageMeterTrackingActive;
        _mainWindowViewModel.IsTrackingPartyLootOnly = SettingsController.CurrentSettings.IsTrackingPartyLootOnly;
        _mainWindowViewModel.LoggingBindings.IsTrackingSilver = SettingsController.CurrentSettings.IsTrackingSilver;
        _mainWindowViewModel.LoggingBindings.IsTrackingFame = SettingsController.CurrentSettings.IsTrackingFame;
        _mainWindowViewModel.LoggingBindings.IsTrackingMobLoot = SettingsController.CurrentSettings.IsTrackingMobLoot;

        _mainWindowViewModel.LoggingBindings.GameLoggingCollectionView = CollectionViewSource.GetDefaultView(_mainWindowViewModel.LoggingBindings.TrackingNotifications) as ListCollectionView;
        if (_mainWindowViewModel.LoggingBindings?.GameLoggingCollectionView != null)
        {
            _mainWindowViewModel.LoggingBindings.GameLoggingCollectionView.IsLiveSorting = true;
            _mainWindowViewModel.LoggingBindings.GameLoggingCollectionView.IsLiveFiltering = true;
            _mainWindowViewModel.LoggingBindings.GameLoggingCollectionView.SortDescriptions.Add(new SortDescription(nameof(DateTime), ListSortDirection.Descending));
        }
    }

    public async Task StartTrackingAsync()
    {
        if (_networkManager?.IsAnySocketActive() ?? false)
        {
            return;
        }

        _networkManager = new NetworkManager(this);

        var provider = SettingsController.CurrentSettings.PacketProvider;

        if (provider == PacketProviderKind.Sockets && !ApplicationCore.IsAppStartedAsAdministrator())
        {
            _mainWindowViewModel.SetErrorBar(Visibility.Visible, LocalizationController.Translation("START_APPLICATION_AS_ADMINISTRATOR"));
            return;
        }

        try
        {
            await LoadDataAsync();

            ClusterController?.RegisterEvents();
            LootController?.RegisterEvents();
            TreasureController?.RegisterEvents();

            LiveStatsTracker.Start();

            _mainWindowViewModel.DungeonBindings.DungeonStatsFilter =
                new DungeonStatsFilter(_mainWindowViewModel.DungeonBindings);

            _networkManager.Start();
            _mainWindowViewModel.IsTrackingActive = true;
        }
        catch (Exception ex)
        {
            string userMsg = GetTrackingStartErrorMessage(ex, provider);

            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(ex, "StartTracking failed | provider={Provider} | admin={IsAdmin} | msg={UserMsg}", provider, ApplicationCore.IsAppStartedAsAdministrator(), userMsg);

            _mainWindowViewModel.SetErrorBar(Visibility.Visible, userMsg);

            try
            {
                StopTracking();
            }
            catch
            {
                // ignored
            }

            _mainWindowViewModel.IsTrackingActive = false;
        }
    }

    private static string GetTrackingStartErrorMessage(Exception ex, PacketProviderKind provider)
    {
        if (ex is NoListeningAdaptersException)
        {
            return LocalizationController.Translation("NO_LISTENING_ADAPTERS");
        }

        if (ex is SocketException se)
        {
            return string.Format(LocalizationController.Translation("ERR_SOCKET_FAILED_WITH_CODE"), se.SocketErrorCode);
        }

        if (ex is UnauthorizedAccessException)
        {
            return LocalizationController.Translation("START_APPLICATION_AS_ADMINISTRATOR");
        }

        if (ex is DllNotFoundException d && (d.Message.Contains("wpcap", StringComparison.OrdinalIgnoreCase) || d.Message.Contains("npcap", StringComparison.OrdinalIgnoreCase)))
        {
            return LocalizationController.Translation("ERR_NPCAP_DLL_MISSING");
        }

        if (ex is TypeInitializationException { InnerException: DllNotFoundException inner } &&
            (inner.Message.Contains("wpcap", StringComparison.OrdinalIgnoreCase) ||
             inner.Message.Contains("npcap", StringComparison.OrdinalIgnoreCase)))
        {
            return LocalizationController.Translation("ERR_NPCAP_DLL_MISSING");
        }

        if (ex.GetType().Name.Equals("PcapException", StringComparison.OrdinalIgnoreCase))
        {
            return LocalizationController.Translation("ERR_NPCAP_OPEN_FAILED");
        }

        if (ex is InvalidOperationException)
        {
            return LocalizationController.Translation("ERR_CAPTURE_START_INVALID_OPERATION");
        }

        return LocalizationController.Translation("PACKET_HANDLER_ERROR_MESSAGE");
    }

    public void StopTracking()
    {
        if (!_mainWindowViewModel.IsTrackingActive)
        {
            return;
        }

        _networkManager.Stop();

        LiveStatsTracker?.Stop();

        TreasureController.UnregisterEvents();
        LootController.UnregisterEvents();
        ClusterController.UnregisterEvents();

        _mainWindowViewModel.IsTrackingActive = false;

        Debug.Print("Stopped tracking");
    }

    public async Task SaveDataAsync()
    {
        await Task.WhenAll(
            VaultController.SaveInFileAsync(),
            TradeController.SaveInFileAsync(),
            TreasureController.SaveInFileAsync(),
            StatisticController.SaveInFileAsync(),
            DungeonController.SaveInFileAsync(),
            GatheringController.SaveInFileAsync(true),
            GuildController.SaveInFileAsync(),
            CombatController.SaveInFileAsync(),
            MarketController.SaveInFileAsync(),
            EstimatedMarketValueController.SaveInFileAsync()
        );
    }

    private async Task LoadDataAsync()
    {
        await Task.WhenAll(
            EstimatedMarketValueController.LoadFromFileAsync(),
            StatisticController.LoadFromFileAsync(),
            TradeController.LoadFromFileAsync(),
            TreasureController.LoadFromFileAsync(),
            DungeonController.LoadDungeonFromFileAsync(),
            GatheringController.LoadFromFileAsync(),
            VaultController.LoadFromFileAsync(),
            GuildController.LoadFromFileAsync(),
            CombatController.LoadFromFileAsync(),
            MarketController.LoadFromFileAsync()
        );
    }

    public bool ExistIndispensableInfos => ClusterController.CurrentCluster != null && EntityController.ExistLocalEntity();

    #endregion

    #region Notifications

    public async Task AddNotificationAsync(TrackingNotification item)
    {
        item.SetType();

        if (!IsTrackingAllowedByMainCharacter() && item.Type is LoggingFilterType.Fame or LoggingFilterType.Silver or LoggingFilterType.Faction)
        {
            return;
        }

        if (_mainWindowViewModel?.LoggingBindings?.TrackingNotifications == null)
        {
            return;
        }

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingFame && item.Type == LoggingFilterType.Fame)
        {
            return;
        }

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingSilver && item.Type == LoggingFilterType.Silver)
        {
            return;
        }

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingMobLoot && item.Fragment is OtherGrabbedLootNotificationFragment { IsLootedPlayerMob: true })
        {
            return;
        }

        SetNotificationFilteredVisibility(item);

        await Application.Current.Dispatcher.InvokeAsync(delegate
        {
            _mainWindowViewModel?.LoggingBindings?.TrackingNotifications.Insert(0, item);
        });

        await RemovesUnnecessaryNotificationsAsync();
        await SetNotificationTypesAsync();
    }

    private async Task RemovesUnnecessaryNotificationsAsync()
    {
        if (!IsRemovesUnnecessaryNotificationsActiveAllowed())
        {
            return;
        }

        _isRemovesUnnecessaryNotificationsActive = true;

        int? numberToBeRemoved = _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.Count - MaxNotifications;
        if (numberToBeRemoved is > 0)
        {
            var notifications = _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.ToList().OrderBy(x => x?.DateTime).Take((int) numberToBeRemoved).ToAsyncEnumerable();
            if (notifications != null)
            {
                await foreach (var notification in notifications)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        _ = _mainWindowViewModel?.LoggingBindings?.TrackingNotifications.Remove(notification);
                    });
                }
            }
        }

        _isRemovesUnnecessaryNotificationsActive = false;
    }

    public async Task ClearNotificationsAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _mainWindowViewModel?.LoggingBindings?.TrackingNotifications.Clear();
        });
    }

    public async Task NotificationUiFilteringAsync(string text = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(text))
            {
                await _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.ToAsyncEnumerable().ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Collapsed;
                })!;

                await _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.ToAsyncEnumerable().Where(x =>
                    (_notificationTypesFilters?.Contains(x.Type) ?? true)
                    &&
                    (x.Fragment is OtherGrabbedLootNotificationFragment fragment &&
                     (fragment.LootedByName.ToLower().Contains(text.ToLower())
                      || fragment.LocalizedName.ToLower().Contains(text.ToLower())
                      || fragment.LootedFromName.ToLower().Contains(text.ToLower())
                     )
                     ||
                     x.Fragment is KillNotificationFragment killFragment &&
                     (killFragment.Died.ToLower().Contains(text.ToLower())
                      || killFragment.KilledBy.ToLower().Contains(text.ToLower())
                     )
                    )
                    && (IsLootFromMobShown || x.Fragment is OtherGrabbedLootNotificationFragment { IsLootedPlayerMob: false } or not OtherGrabbedLootNotificationFragment)
                ).ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Visible;
                })!;
            }
            else
            {
                await _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.ToAsyncEnumerable().ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Collapsed;
                })!;

                await _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.Where(x =>
                    (_notificationTypesFilters?.Contains(x.Type) ?? false)
                    && (IsLootFromMobShown || x.Fragment is OtherGrabbedLootNotificationFragment { IsLootedPlayerMob: false } or not OtherGrabbedLootNotificationFragment)
                ).ToAsyncEnumerable().ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Visible;
                })!;
            }
        }
        catch (Exception)
        {
            // ignore
        }
    }

    private void SetNotificationFilteredVisibility(TrackingNotification trackingNotification)
    {
        trackingNotification.Visibility = IsNotificationFiltered(trackingNotification) ? Visibility.Collapsed : Visibility.Visible;
    }

    private bool IsNotificationFiltered(TrackingNotification trackingNotification)
    {
        return !_notificationTypesFilters?.Exists(x => x == trackingNotification.Type) ?? false;
    }

    public void UpdateFilterType(LoggingFilterType notificationType, bool isSelected)
    {
        if (notificationType == LoggingFilterType.ShowLootFromMob)
        {
            IsLootFromMobShown = isSelected;
            SettingsController.CurrentSettings.IsLootFromMobShown = isSelected;
        }
        else if (isSelected && !_notificationTypesFilters.Exists(x => x == notificationType))
        {
            _notificationTypesFilters.Add(notificationType);
        }
        else if (!isSelected && _notificationTypesFilters.Exists(x => x == notificationType))
        {
            _notificationTypesFilters.Remove(notificationType);
        }

        UpdateLoggingFilterSettings(notificationType, isSelected);
    }

    private static void UpdateLoggingFilterSettings(LoggingFilterType notificationType, bool isSelected)
    {
        switch (notificationType)
        {
            case LoggingFilterType.Fame:
                SettingsController.CurrentSettings.IsMainTrackerFilterFame = isSelected;
                break;
            case LoggingFilterType.Silver:
                SettingsController.CurrentSettings.IsMainTrackerFilterSilver = isSelected;
                break;
            case LoggingFilterType.Faction:
                SettingsController.CurrentSettings.IsMainTrackerFilterFaction = isSelected;
                break;
            case LoggingFilterType.EquipmentLoot:
                SettingsController.CurrentSettings.IsMainTrackerFilterEquipmentLoot = isSelected;
                break;
            case LoggingFilterType.ConsumableLoot:
                SettingsController.CurrentSettings.IsMainTrackerFilterConsumableLoot = isSelected;
                break;
            case LoggingFilterType.SimpleLoot:
                SettingsController.CurrentSettings.IsMainTrackerFilterSimpleLoot = isSelected;
                break;
            case LoggingFilterType.UnknownLoot:
                SettingsController.CurrentSettings.IsMainTrackerFilterUnknownLoot = isSelected;
                break;
            case LoggingFilterType.SeasonPoints:
                SettingsController.CurrentSettings.IsMainTrackerFilterSeasonPoints = isSelected;
                break;
            case LoggingFilterType.ShowLootFromMob:
                SettingsController.CurrentSettings.IsLootFromMobShown = isSelected;
                break;
            case LoggingFilterType.Kill:
                SettingsController.CurrentSettings.IsMainTrackerFilterKill = isSelected;
                break;
        }
    }

    public bool IsLootFromMobShown { get; set; }

    private async Task SetNotificationTypesAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            var notifications = _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.ToAsyncEnumerable();
            if (notifications != null)
            {
                await foreach (var notification in notifications)
                {
                    notification.SetType();
                }
            }
        });
    }

    private static bool _isRemovesUnnecessaryNotificationsActive;
    private DateTime _lastRemovesUnnecessaryNotifications;

    private bool IsRemovesUnnecessaryNotificationsActiveAllowed(int waitTimeInSeconds = 1)
    {
        var currentDateTime = DateTime.UtcNow;
        var difference = currentDateTime.Subtract(_lastRemovesUnnecessaryNotifications);
        if (difference.Seconds >= waitTimeInSeconds && !_isRemovesUnnecessaryNotificationsActive)
        {
            _lastRemovesUnnecessaryNotifications = currentDateTime;
            return true;
        }

        return false;
    }

    public async Task ResetTrackingNotificationsAsync()
    {
        var dialog = new DialogWindow(LocalizationController.Translation("RESET_TRACKING_NOTIFICATIONS"), LocalizationController.Translation("SURE_YOU_WANT_TO_RESET_TRACKING_NOTIFICATIONS"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is true)
        {
            await ClearNotificationsAsync()!;
            Application.Current.Dispatcher.Invoke(() => _mainWindowViewModel?.LoggingBindings?.TopLooters?.Clear());
            LootController?.ClearLootLogger();
        }
    }

    #endregion

    #region Movement Analysis

    private readonly List<MoveOperation> _movementHistory = new();
    private const int MaxMovementHistory = 1000;

    public void AnalyzeMovement(MoveOperation moveOperation)
    {
        if (!IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        // Adicionar à história de movimento
        _movementHistory.Add(moveOperation);

        // Manter apenas os últimos movimentos para evitar uso excessivo de memória
        if (_movementHistory.Count > MaxMovementHistory)
        {
            _movementHistory.RemoveAt(0);
        }

        // Analisar padrões de movimento
        AnalyzeMovementPatterns(moveOperation);

        // Log para debug (opcional)
        Log.Debug("Movement detected - Time: {Time}, Position: [{X}, {Y}], NewPosition: [{NewX}, {NewY}], Speed: {Speed}, Direction: {Direction}",
            moveOperation.Time,
            moveOperation.Position[0], moveOperation.Position[1],
            moveOperation.NewPosition[0], moveOperation.NewPosition[1],
            moveOperation.Speed,
            moveOperation.Direction);
    }

    private void AnalyzeMovementPatterns(MoveOperation moveOperation)
    {
        // Calcular distância percorrida
        var distance = CalculateDistance(moveOperation.Position, moveOperation.NewPosition);
        
        // Calcular velocidade real (se necessário)
        var realSpeed = CalculateRealSpeed(moveOperation, distance);
        
        // Detectar padrões suspeitos (opcional)
        DetectSuspiciousMovement(moveOperation, distance, realSpeed);
    }

    private static double CalculateDistance(float[] position1, float[] position2)
    {
        if (position1.Length < 2 || position2.Length < 2)
            return 0;

        var dx = position2[0] - position1[0];
        var dy = position2[1] - position1[1];
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static double CalculateRealSpeed(MoveOperation moveOperation, double distance)
    {
        // Calcular velocidade baseada na distância e tempo
        // Assumindo que o tempo está em milissegundos
        var timeInSeconds = moveOperation.Time / 1000.0;
        return timeInSeconds > 0 ? distance / timeInSeconds : 0;
    }

    private void DetectSuspiciousMovement(MoveOperation moveOperation, double distance, double realSpeed)
    {
        // Detectar movimentos muito rápidos ou distâncias muito grandes
        const double maxReasonableSpeed = 50.0; // Ajustar conforme necessário
        const double maxReasonableDistance = 100.0; // Ajustar conforme necessário

        if (realSpeed > maxReasonableSpeed || distance > maxReasonableDistance)
        {
            Log.Warning("Suspicious movement detected - Speed: {Speed}, Distance: {Distance}", realSpeed, distance);
        }
    }

    public List<MoveOperation> GetMovementHistory()
    {
        return new List<MoveOperation>(_movementHistory);
    }

    public void ClearMovementHistory()
    {
        _movementHistory.Clear();
    }

    #endregion

    #region Attack Analysis

    private readonly List<AttackStartOperation> _attackHistory = new();
    private const int MaxAttackHistory = 500;

    public void AnalyzeAttackStart(AttackStartOperation attackOperation)
    {
        if (!IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        // Adicionar à história de ataques
        _attackHistory.Add(attackOperation);

        // Manter apenas os últimos ataques para evitar uso excessivo de memória
        if (_attackHistory.Count > MaxAttackHistory)
        {
            _attackHistory.RemoveAt(0);
        }

        // Analisar padrões de ataque
        AnalyzeAttackPatterns(attackOperation);

        // Log para debug (opcional)
        Log.Debug("Attack started - TargetId: {TargetId}, AttackType: {AttackType}, WeaponType: {WeaponType}",
            attackOperation.TargetId,
            attackOperation.AttackType,
            attackOperation.WeaponType);
    }

    private void AnalyzeAttackPatterns(AttackStartOperation attackOperation)
    {
        // Analisar frequência de ataques
        AnalyzeAttackFrequency();
        
        // Analisar tipos de arma utilizados
        AnalyzeWeaponUsage(attackOperation.WeaponType);
        
        // Detectar padrões suspeitos
        DetectSuspiciousAttackPatterns(attackOperation);
    }

    private void AnalyzeAttackFrequency()
    {
        // Calcular ataques por minuto (últimos 5 minutos)
        var recentAttacks = _attackHistory.Where(a => 
            DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)).Ticks < DateTime.UtcNow.Ticks).Count();
        
        var attacksPerMinute = recentAttacks / 5.0;
        
        if (attacksPerMinute > 10) // Mais de 10 ataques por minuto
        {
            Log.Warning("High attack frequency detected: {AttacksPerMinute} attacks/minute", attacksPerMinute);
        }
    }

    private void AnalyzeWeaponUsage(int weaponType)
    {
        // Analisar distribuição de tipos de arma
        var weaponUsage = _attackHistory.GroupBy(a => a.WeaponType)
            .ToDictionary(g => g.Key, g => g.Count());
        
        // Log estatísticas de uso de armas
        Log.Debug("Weapon usage stats: {WeaponStats}", 
            string.Join(", ", weaponUsage.Select(kvp => $"Type {kvp.Key}: {kvp.Value}")));
    }

    private void DetectSuspiciousAttackPatterns(AttackStartOperation attackOperation)
    {
        // Detectar ataques muito rápidos consecutivos
        var recentAttacks = _attackHistory.TakeLast(10).ToList();
        if (recentAttacks.Count >= 3)
        {
            // Verificar se há ataques muito próximos no tempo
            // (Esta é uma implementação simplificada)
            Log.Debug("Recent attack pattern: {RecentCount} attacks in last 10", recentAttacks.Count);
        }
    }

    public List<AttackStartOperation> GetAttackHistory()
    {
        return new List<AttackStartOperation>(_attackHistory);
    }

    public void ClearAttackHistory()
    {
        _attackHistory.Clear();
    }

    #endregion

    #region Mob Position Analysis

    private readonly Dictionary<long, float[]> _mobPositions = new();
    private readonly Dictionary<long, float[]> _mobNewPositions = new();

    public void AnalyzeMobPosition(long? objectId, float[] position, float[] newPosition)
    {
        if (!IsTrackingAllowedByMainCharacter() || objectId == null)
        {
            return;
        }

        var mobId = (long)objectId;

        // Armazenar posições do mob
        _mobPositions[mobId] = position;
        _mobNewPositions[mobId] = newPosition;

        // Analisar movimento do mob
        AnalyzeMobMovement(mobId, position, newPosition);

        // Log para debug
        Log.Debug("Mob position - ID: {MobId}, Position: [{X}, {Y}], NewPosition: [{NewX}, {NewY}]",
            mobId,
            position[0], position[1],
            newPosition[0], newPosition[1]);
    }

    private void AnalyzeMobMovement(long mobId, float[] position, float[] newPosition)
    {
        // Calcular distância percorrida pelo mob
        var distance = CalculateMobDistance(position, newPosition);
        
        // Detectar mobs que se moveram muito (possível teleporte ou erro)
        if (distance > 50.0f) // Mais de 50 unidades
        {
            Log.Warning("Mob {MobId} moved unusually far: {Distance} units", mobId, distance);
        }

        // Analisar padrões de movimento de mobs
        if (distance > 0.1f) // Mob se moveu
        {
            Log.Debug("Mob {MobId} moved {Distance} units", mobId, distance);
        }
    }

    private static float CalculateMobDistance(float[] position1, float[] position2)
    {
        if (position1.Length < 2 || position2.Length < 2)
            return 0;

        var dx = position2[0] - position1[0];
        var dy = position2[1] - position1[1];
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    public Dictionary<long, float[]> GetMobPositions()
    {
        return new Dictionary<long, float[]>(_mobPositions);
    }

    public void ClearMobPositions()
    {
        _mobPositions.Clear();
        _mobNewPositions.Clear();
    }

    #endregion

    #region Specific character name tracking

    public bool IsTrackingAllowedByMainCharacter()
    {
        var localEntity = EntityController.GetLocalEntity();

        if (localEntity?.Value?.Name == null || string.IsNullOrEmpty(SettingsController.CurrentSettings.MainTrackingCharacterName))
        {
            return true;
        }

        if (localEntity.Value.Value.Name == SettingsController.CurrentSettings.MainTrackingCharacterName)
        {
            return true;
        }

        if (localEntity.Value.Value.Name != SettingsController.CurrentSettings.MainTrackingCharacterName)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Gear repairing

    private long _buildingObjectId = -1;
    private long _upcomingRepairCosts;

    public void RegisterBuilding(long buildingObjectId)
    {
        _buildingObjectId = buildingObjectId;
    }

    public void UnregisterBuilding(long buildingObjectId)
    {
        if (buildingObjectId != _buildingObjectId)
        {
            return;
        }

        _buildingObjectId = -1;
        _upcomingRepairCosts = 0;
    }

    public void SetUpcomingRepair(long buildingObjectId, long costs)
    {
        if (_buildingObjectId != buildingObjectId)
        {
            return;
        }

        _upcomingRepairCosts = costs;
    }

    public void RepairFinished(long userObjectId, long buildingObjectId)
    {
        if (EntityController.LocalUserData.UserObjectId != userObjectId || _upcomingRepairCosts <= 0 || _buildingObjectId != buildingObjectId)
        {
            return;
        }

        StatisticController?.AddValue(ValueType.RepairCosts, FixPoint.FromInternalValue(_upcomingRepairCosts).DoubleValue);
        StatisticController?.UpdateRepairCostsUi();
    }

    #endregion
}