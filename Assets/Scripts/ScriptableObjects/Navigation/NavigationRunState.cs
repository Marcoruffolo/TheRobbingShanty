using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NavigationRunState", menuName = "TRS/Navigation/Run State")]
public class NavigationRunState : ScriptableObject
{
    [Serializable]
    private class ZoneProgress
    {
        public int ZoneIndex = -1;
        public int RequiredCores;
        public int AvailableCoreIslands;
        public int LinkedCores;
        public bool IntroPlayed;
        public List<string> AvailableArtifactIds = new();
        public List<string> CompletedArtifactIds = new();
        public List<string> ClaimedArtifactIds = new();
        public List<string> LinkedArtifactIds = new();
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

    public int CurrentZoneIndex => currentZoneIndex;
    public int CurrentArtifactZoneIndex => currentArtifactZoneIndex;
    public string CurrentArtifactIslandId => currentArtifactIslandId;
    public bool HasCurrentArtifactIsland => !string.IsNullOrWhiteSpace(currentArtifactIslandId);

    public int GetOrCreateWorldSeed(int configuredSeed)
    {
        if (configuredSeed != 0)
        {
            worldSeed = configuredSeed;
            return worldSeed;
        }

        if (worldSeed == 0)
            worldSeed = UnityEngine.Random.Range(1, int.MaxValue);

        return worldSeed;
    }

    public void EnsureZone(int zoneIndex, int requiredCores, int availableCoreIslands)
    {
        ZoneProgress zone = GetOrCreateZone(zoneIndex);

        if (zone.RequiredCores <= 0)
            zone.RequiredCores = Mathf.Max(0, requiredCores);

        if (zone.AvailableCoreIslands <= 0)
            zone.AvailableCoreIslands = Mathf.Max(0, availableCoreIslands);
        RaiseZoneProgressChanged(zone);
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

    public int GetLinkedCores(int zoneIndex)
    {
        ZoneProgress zone = FindZone(zoneIndex);
        return zone != null ? zone.LinkedCores : 0;
    }

    public bool HasLinkedRequiredCores(int zoneIndex)
    {
        ZoneProgress zone = FindZone(zoneIndex);
        return zone != null && zone.RequiredCores > 0 && zone.LinkedCores >= zone.RequiredCores;
    }

    public void RegisterArtifactIsland(int zoneIndex, string artifactId)
    {
        if (string.IsNullOrWhiteSpace(artifactId)) return;

        ZoneProgress zone = GetOrCreateZone(zoneIndex);
        AddUnique(zone.AvailableArtifactIds, artifactId);
    }

    public void SetCurrentArtifactIsland(int zoneIndex, string artifactId)
    {
        currentZoneIndex = zoneIndex;
        currentArtifactZoneIndex = zoneIndex;
        currentArtifactIslandId = artifactId ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(currentArtifactIslandId))
            RegisterArtifactIsland(zoneIndex, currentArtifactIslandId);
    }

    public bool IsArtifactCompleted(string artifactId)
    {
        ZoneProgress zone = FindZoneForArtifact(artifactId);
        return zone != null && zone.CompletedArtifactIds.Contains(artifactId);
    }

    public bool IsCoreClaimed(string artifactId)
    {
        ZoneProgress zone = FindZoneForArtifact(artifactId);
        return zone != null && zone.ClaimedArtifactIds.Contains(artifactId);
    }

    public bool IsCoreLinked(string artifactId)
    {
        ZoneProgress zone = FindZoneForArtifact(artifactId);
        return zone != null && zone.LinkedArtifactIds.Contains(artifactId);
    }

    public void MarkArtifactCompleted(string artifactId = null)
    {
        string id = ResolveArtifactId(artifactId);
        if (string.IsNullOrWhiteSpace(id)) return;

        ZoneProgress zone = GetOrCreateZoneForArtifact(id);
        AddUnique(zone.CompletedArtifactIds, id);
        RaiseZoneProgressChanged(zone);
    }

    public void MarkCoreClaimed(string artifactId = null)
    {
        string id = ResolveArtifactId(artifactId);
        if (string.IsNullOrWhiteSpace(id)) return;

        ZoneProgress zone = GetOrCreateZoneForArtifact(id);
        AddUnique(zone.CompletedArtifactIds, id);
        AddUnique(zone.ClaimedArtifactIds, id);
        RaiseZoneProgressChanged(zone);
    }

    public bool HasClaimedCoreReady(int zoneIndex = -1)
    {
        ZoneProgress zone = ResolveZone(zoneIndex);
        if (zone == null || (zone.RequiredCores > 0 && zone.LinkedCores >= zone.RequiredCores))
            return false;

        foreach (string claimedId in zone.ClaimedArtifactIds)
            if (!zone.LinkedArtifactIds.Contains(claimedId))
                return true;

        return false;
    }

    public int GetUnlinkedClaimedCoreCount(int zoneIndex = -1)
    {
        ZoneProgress zone = ResolveZone(zoneIndex);
        if (zone == null) return 0;

        int count = 0;
        foreach (string claimedId in zone.ClaimedArtifactIds)
            if (!zone.LinkedArtifactIds.Contains(claimedId))
                count++;

        return count;
    }
    public bool TryLinkNextClaimedCore(out string linkedArtifactId, int zoneIndex = -1)
    {
        linkedArtifactId = string.Empty;

        ZoneProgress zone = ResolveZone(zoneIndex);
        if (zone == null || (zone.RequiredCores > 0 && zone.LinkedCores >= zone.RequiredCores))
            return false;

        foreach (string claimedId in zone.ClaimedArtifactIds)
        {
            if (zone.LinkedArtifactIds.Contains(claimedId))
                continue;

            zone.LinkedArtifactIds.Add(claimedId);
            zone.LinkedCores = zone.LinkedArtifactIds.Count;
            linkedArtifactId = claimedId;
            RaiseZoneProgressChanged(zone);
            return true;
        }

        return false;
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
        ZoneProgress zone = ResolveZone(zoneIndex);
        if (zone == null) return "Nucleos vinculados: 0/0";

        return $"Nucleos vinculados: {zone.LinkedCores}/{zone.RequiredCores}";
    }

    public void ResetRun()
    {
        worldSeed = 0;
        currentZoneIndex = 0;
        currentArtifactZoneIndex = -1;
        currentArtifactIslandId = string.Empty;
        zones.Clear();
        ZoneProgressChanged?.Invoke(0, 0, 0);
    }

    private string ResolveArtifactId(string artifactId)
    {
        return string.IsNullOrWhiteSpace(artifactId) ? currentArtifactIslandId : artifactId;
    }

    private ZoneProgress ResolveZone(int zoneIndex)
    {
        return zoneIndex >= 0 ? FindZone(zoneIndex) : FindZone(currentZoneIndex);
    }

    private ZoneProgress GetOrCreateZoneForArtifact(string artifactId)
    {
        ZoneProgress zone = FindZoneForArtifact(artifactId);
        if (zone != null) return zone;

        int fallbackZoneIndex = currentArtifactZoneIndex >= 0 ? currentArtifactZoneIndex : currentZoneIndex;
        zone = GetOrCreateZone(fallbackZoneIndex);
        AddUnique(zone.AvailableArtifactIds, artifactId);
        return zone;
    }

    private ZoneProgress FindZoneForArtifact(string artifactId)
    {
        if (string.IsNullOrWhiteSpace(artifactId)) return null;

        foreach (ZoneProgress zone in zones)
        {
            if (zone.AvailableArtifactIds.Contains(artifactId) ||
                zone.CompletedArtifactIds.Contains(artifactId) ||
                zone.ClaimedArtifactIds.Contains(artifactId) ||
                zone.LinkedArtifactIds.Contains(artifactId))
            {
                return zone;
            }
        }

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

    private void RaiseZoneProgressChanged(ZoneProgress zone)
    {
        ZoneProgressChanged?.Invoke(zone.ZoneIndex, zone.LinkedCores, zone.RequiredCores);
    }
    private static void AddUnique(List<string> list, string value)
    {
        if (!list.Contains(value))
            list.Add(value);
    }
}
