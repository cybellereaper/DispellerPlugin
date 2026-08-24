using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dispeller.Models;

namespace Dispeller.Services;

/// <summary>
/// Stores the current and immediately previous glamour-dresser snapshots per character.
/// Keeping two generations provides persistent change tracking across game/plugin restarts.
/// </summary>
public sealed class SnapshotService
{
    private readonly IPlayerState playerState;
    private readonly IPluginLog log;
    private readonly string snapshotDirectory;

    private readonly Dictionary<ulong, DresserSnapshotStore> cache = [];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    public SnapshotService(
        IDalamudPluginInterface pluginInterface,
        IPlayerState playerState,
        IPluginLog log)
    {
        this.playerState = playerState;
        this.log = log;

        snapshotDirectory = Path.Combine(pluginInterface.ConfigDirectory.FullName, "snapshots");
        Directory.CreateDirectory(snapshotDirectory);
    }

    public bool Capture(IReadOnlyList<DresserItem> items)
    {
        var contentId = GetCurrentContentId();
        if (contentId == 0)
            return false;

        var store = GetStore(contentId);
        var currentItems = items
            .Where(item => item.ItemId != 0)
            .OrderBy(item => item.Slot)
            .ThenBy(item => item.ItemId)
            .Select(item => new DresserSnapshotItem
            {
                Slot = item.Slot,
                ItemId = item.ItemId,
                Stain1 = item.Stain1,
                Stain2 = item.Stain2,
            })
            .ToList();

        if (store.Current != null && Equivalent(store.Current.Items, currentItems))
            return false;

        store.Previous = store.Current;
        store.Current = new DresserSnapshot
        {
            ContentId = contentId,
            CharacterName = playerState.IsLoaded ? playerState.CharacterName : string.Empty,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            Items = currentItems,
        };

        SaveStore(contentId, store);
        return true;
    }

    public DresserSnapshot? GetCurrent()
    {
        var contentId = GetCurrentContentId();
        return contentId == 0 ? null : GetStore(contentId).Current;
    }

    public DresserSnapshot? GetPrevious()
    {
        var contentId = GetCurrentContentId();
        return contentId == 0 ? null : GetStore(contentId).Previous;
    }

    private ulong GetCurrentContentId() =>
        playerState.IsLoaded ? playerState.ContentId : 0;

    private DresserSnapshotStore GetStore(ulong contentId)
    {
        if (cache.TryGetValue(contentId, out var store))
            return store;

        var path = GetPath(contentId);
        if (!File.Exists(path))
        {
            store = new DresserSnapshotStore();
            cache[contentId] = store;
            return store;
        }

        try
        {
            store = JsonSerializer.Deserialize<DresserSnapshotStore>(
                        File.ReadAllText(path),
                        JsonOptions)
                    ?? new DresserSnapshotStore();
        }
        catch (Exception ex)
        {
            log.Warning(ex, $"Could not read dresser snapshot '{path}'. Starting a fresh snapshot history.");
            store = new DresserSnapshotStore();
        }

        cache[contentId] = store;
        return store;
    }

    private void SaveStore(ulong contentId, DresserSnapshotStore store)
    {
        try
        {
            Directory.CreateDirectory(snapshotDirectory);
            var path = GetPath(contentId);
            var tempPath = path + ".tmp";

            File.WriteAllText(tempPath, JsonSerializer.Serialize(store, JsonOptions));
            File.Move(tempPath, path, overwrite: true);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Could not persist glamour dresser snapshot.");
        }
    }

    private string GetPath(ulong contentId) =>
        Path.Combine(snapshotDirectory, $"{contentId:X16}.json");

    private static bool Equivalent(
        IReadOnlyList<DresserSnapshotItem> left,
        IReadOnlyList<DresserSnapshotItem> right)
    {
        if (left.Count != right.Count)
            return false;

        for (var i = 0; i < left.Count; i++)
        {
            var a = left[i];
            var b = right[i];

            if (a.Slot != b.Slot ||
                a.ItemId != b.ItemId ||
                a.Stain1 != b.Stain1 ||
                a.Stain2 != b.Stain2)
            {
                return false;
            }
        }

        return true;
    }
}
