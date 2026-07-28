using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NavigationRunState", menuName = "TRS/Navigation/Run State")]
public class NavigationRunState : ScriptableObject
{
    [Serializable]
    public class ArtifactEntry
    {
        public string ArtifactId;
        public int ZoneIndex;
        public bool IsCompleted;
        public bool IsClaimed;
        public int RequiredKills;
        public int CurrentKills;
    }

    [Serializable]
    private class ZoneProgress
    {
        public int ZoneIndex = -1;
        public int RequiredCores;
        public int AvailableCoreIslands;
        public int PlacedCores;
        public bool IntroPlayed;
        public List<ArtifactEntry> Artifacts = new();
    }

    private static NavigationRunState _runtimeInstance;

    [SerializeField] private int worldSeed;
    [SerializeField] private int currentZoneIndex;
    [SerializeField] private int currentArtifactZoneIndex = -1;
    [SerializeField] private string currentArtifactIslandId = string.Empty;
    [SerializeField] private List<ZoneProgress> zones = new();

    public static NavigationRunState Instance
    {
        get
        {
            if (_runtimeInstance == null)
            {
                _runtimeInstance = CreateInstance<NavigationRunState>();
                _runtimeInstance.name = "Runtime NavigationRunState";
                _runtimeInstance.hideFlags = HideFlags.HideAndDontSave;
            }

            return _runtimeInstance;
        }
    }

    public event Action<int, int, int> ZoneProgressChanged;

    public int CurrentWorldSeed => worldSeed;
    public int CurrentZoneIndex => currentZoneIndex;
    public int CurrentArtifactZoneIndex => currentArtifactZoneIndex;
    public string CurrentArtifactIslandId => currentArtifactIslandId;
    public bool HasCurrentArtifactIsland => !string.IsNullOrWhiteSpace(currentArtifactIslandId);

    private void OnEnable()
    {
        if (_runtimeInstance == null || !_runtimeInstance.hideFlags.HasFlag(HideFlags.HideAndDontSave))
            _runtimeInstance = this;
    }

    public int GetOrCreateWorldSeed(int configuredSeed)
    {
        worldSeed = WorldRunSeed.Resolve(configuredSeed);
        return worldSeed;
    }

    public void StartZone(int zoneIndex, int requiredCores, int availableCoreIslands)
    {
        ZoneProgress zone = GetOrCreateZone(zoneIndex);
        currentZoneIndex = zoneIndex;

        int resolvedRequiredCores = ClampRequiredCores(requiredCores);
        int resolvedAvailableCoreIslands = Mathf.Max(resolvedRequiredCores, availableCoreIslands);

        if (availableCoreIslands < resolvedRequiredCores)
            Debug.LogError($"[NavigationRunState] Zona {zoneIndex} tiene menos islas de artefacto que nucleos requeridos.");

        if (zone.PlacedCores <= 0)
        {
            zone.RequiredCores = resolvedRequiredCores;
            zone.AvailableCoreIslands = resolvedAvailableCoreIslands;
        }
        else if (zone.AvailableCoreIslands < zone.RequiredCores)
        {
            zone.AvailableCoreIslands = Mathf.Max(zone.RequiredCores, resolvedAvailableCoreIslands);
        }

        zone.PlacedCores = Mathf.Clamp(zone.PlacedCores, 0, zone.RequiredCores);
        RaiseZoneProgressChanged(zone);
    }

    public void RegisterArtifactIsland(int zoneIndex, string artifactId)
    {
        if (string.IsNullOrWhiteSpace(artifactId)) return;

        ZoneProgress zone = GetOrCreateZone(zoneIndex);
        ArtifactEntry entry = FindArtifact(zone, artifactId);
        if (entry != null) return;

        zone.Artifacts.Add(new ArtifactEntry
        {
            ArtifactId = artifactId,
            ZoneIndex = zoneIndex
        });
    }

    public void SetCurrentArtifactIsland(int zoneIndex, string artifactId)
    {
        currentZoneIndex = zoneIndex;
        currentArtifactZoneIndex = zoneIndex;
        currentArtifactIslandId = artifactId ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(currentArtifactIslandId))
            RegisterArtifactIsland(zoneIndex, currentArtifactIslandId);
    }

    public void ClearCurrentArtifactIsland(string artifactId = null)
    {
        if (!string.IsNullOrWhiteSpace(artifactId) && artifactId != currentArtifactIslandId)
            return;

        currentArtifactZoneIndex = -1;
        currentArtifactIslandId = string.Empty;
    }

    public void ClearCurrentIslandContext()
    {
        ClearCurrentArtifactIsland();
    }

    public int GetRequiredCores(int zoneIndex)
    {
        ZoneProgress zone = FindZone(zoneIndex);
        return zone != null ? zone.RequiredCores : 0;
    }

    public int GetAvailableCoreIslandCount(int zoneIndex)
    {
        ZoneProgress zone = FindZone(zoneIndex);
        return zone != null ? zone.AvailableCoreIslands : 0;
    }

    public int GetPlacedCores(int zoneIndex)
    {
        ZoneProgress zone = FindZone(zoneIndex);
        return zone != null ? zone.PlacedCores : 0;
    }

    public bool IsZoneReady(int zoneIndex)
    {
        ZoneProgress zone = FindZone(zoneIndex);
        return zone != null
            && zone.RequiredCores > 0
            && zone.AvailableCoreIslands >= zone.RequiredCores;
    }

    public int GetUnplacedCoreCountInInventory(PlayerInventoryHolder inventory, InventoryItemData navigationCore)
    {
        if (inventory == null || navigationCore == null) return 0;
        return inventory.GetItemCount(navigationCore);
    }

    public bool CanPlaceCore(int zoneIndex)
    {
        ZoneProgress zone = FindZone(zoneIndex);
        return zone != null && IsZoneReady(zoneIndex) && zone.PlacedCores < zone.RequiredCores;
    }

    public bool PlaceCore(int zoneIndex)
    {
        ZoneProgress zone = FindZone(zoneIndex);
        if (zone == null || !IsZoneReady(zoneIndex) || zone.PlacedCores >= zone.RequiredCores)
            return false;

        zone.PlacedCores++;
        RaiseZoneProgressChanged(zone);
        return true;
    }

    public bool HasRequiredPlacedCores(int zoneIndex)
    {
        ZoneProgress zone = FindZone(zoneIndex);
        return zone != null && IsZoneReady(zoneIndex) && zone.PlacedCores >= zone.RequiredCores;
    }

    public bool IsArtifactCompleted(string artifactId)
    {
        ArtifactEntry entry = FindArtifact(artifactId);
        return entry != null && entry.IsCompleted;
    }

    public bool IsCoreClaimed(string artifactId)
    {
        ArtifactEntry entry = FindArtifact(artifactId);
        return entry != null && entry.IsClaimed;
    }

    public void SetArtifactKillProgress(int currentKills, int requiredKills, string artifactId = null)
    {
        ArtifactEntry entry = ResolveArtifactEntry(artifactId);
        if (entry == null) return;

        entry.RequiredKills = Mathf.Max(0, requiredKills);
        entry.CurrentKills = Mathf.Clamp(currentKills, 0, entry.RequiredKills);
    }

    public int GetArtifactKillsRemaining(string artifactId)
    {
        ArtifactEntry entry = FindArtifact(artifactId);
        return entry != null ? Mathf.Max(0, entry.RequiredKills - entry.CurrentKills) : 0;
    }

    public void MarkArtifactCompleted(string artifactId = null)
    {
        ArtifactEntry entry = ResolveArtifactEntry(artifactId);
        if (entry == null) return;

        entry.IsCompleted = true;
        RaiseZoneProgressChanged(GetOrCreateZone(entry.ZoneIndex));
    }

    public void MarkCoreClaimed(string artifactId = null)
    {
        ArtifactEntry entry = ResolveArtifactEntry(artifactId);
        if (entry == null) return;

        entry.IsCompleted = true;
        entry.IsClaimed = true;
        RaiseZoneProgressChanged(GetOrCreateZone(entry.ZoneIndex));
    }

    public bool HasIntroPlayed(int zoneIndex)
    {
        ZoneProgress zone = FindZone(zoneIndex);
        return zone != null && zone.IntroPlayed;
    }

    public void MarkIntroPlayed(int zoneIndex)
    {
        ZoneProgress zone = GetOrCreateZone(zoneIndex);
        zone.IntroPlayed = true;
    }

    public string GetProgressText(int zoneIndex = -1)
    {
        ZoneProgress zone = zoneIndex >= 0 ? FindZone(zoneIndex) : FindZone(currentZoneIndex);
        if (zone == null || !IsZoneReady(zone.ZoneIndex)) return string.Empty;
        return $"Nucleos colocados: {zone.PlacedCores}/{zone.RequiredCores}";
    }

    public void ResetRun()
    {
        WorldRunSeed.Reset();
        worldSeed = 0;
        currentZoneIndex = 0;
        ClearCurrentIslandContext();
        zones.Clear();
        ZoneProgressChanged?.Invoke(0, 0, 0);
    }

    private ArtifactEntry ResolveArtifactEntry(string artifactId)
    {
        string id = string.IsNullOrWhiteSpace(artifactId) ? currentArtifactIslandId : artifactId;
        if (string.IsNullOrWhiteSpace(id)) return null;

        ArtifactEntry entry = FindArtifact(id);
        if (entry != null) return entry;

        int zoneIndex = currentArtifactZoneIndex >= 0 ? currentArtifactZoneIndex : currentZoneIndex;
        RegisterArtifactIsland(zoneIndex, id);
        return FindArtifact(id);
    }

    private ArtifactEntry FindArtifact(string artifactId)
    {
        if (string.IsNullOrWhiteSpace(artifactId)) return null;

        foreach (ZoneProgress zone in zones)
        {
            ArtifactEntry entry = FindArtifact(zone, artifactId);
            if (entry != null) return entry;
        }

        return null;
    }

    private ArtifactEntry FindArtifact(ZoneProgress zone, string artifactId)
    {
        if (zone == null) return null;
        foreach (ArtifactEntry entry in zone.Artifacts)
            if (entry.ArtifactId == artifactId)
                return entry;
        return null;
    }

    private ZoneProgress GetOrCreateZone(int zoneIndex)
    {
        ZoneProgress zone = FindZone(zoneIndex);
        if (zone != null) return zone;

        zone = new ZoneProgress { ZoneIndex = zoneIndex };
        zones.Add(zone);
        return zone;
    }

    private ZoneProgress FindZone(int zoneIndex)
    {
        foreach (ZoneProgress zone in zones)
            if (zone.ZoneIndex == zoneIndex)
                return zone;
        return null;
    }

    private int ClampRequiredCores(int value)
    {
        return Mathf.Max(1, value);
    }

    private void RaiseZoneProgressChanged(ZoneProgress zone)
    {
        ZoneProgressChanged?.Invoke(zone.ZoneIndex, zone.PlacedCores, zone.RequiredCores);
    }
}