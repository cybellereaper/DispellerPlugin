using System;
using System.Collections.Generic;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dispeller.Models;
using Lumina.Excel.Sheets;

namespace Dispeller.Services;

/// <summary>
/// Resolves game-data metadata once per item and pre-indexes the Cabinet sheet for O(1) Armoire checks.
/// </summary>
public sealed class ItemMetadataService : IDisposable
{
    private readonly IDataManager dataManager;
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly IPluginLog log;

    private readonly Dictionary<uint, ItemMetadata?> itemCache = [];
    private HashSet<uint>? armoireItemIds;
    private bool disposed;

    public ItemMetadataService(
        IDataManager dataManager,
        IDalamudPluginInterface pluginInterface,
        IPluginLog log)
    {
        this.dataManager = dataManager;
        this.pluginInterface = pluginInterface;
        this.log = log;

        pluginInterface.LanguageChanged += OnLanguageChanged;
    }

    public ItemMetadata? Get(uint itemId)
    {
        if (itemId == 0)
            return null;

        if (itemCache.TryGetValue(itemId, out var cached))
            return cached;

        var sheet = dataManager.GetExcelSheet<Item>();
        if (!sheet.TryGetRow(itemId, out var item))
        {
            itemCache[itemId] = null;
            return null;
        }

        var slot = ResolveSlot(item);
        if (slot == GearSlot.Unknown)
        {
            itemCache[itemId] = null;
            return null;
        }

        var metadata = new ItemMetadata(
            itemId,
            item.Name.ExtractText(),
            item.Icon,
            slot,
            ModelSignature.FromRaw(item.ModelMain),
            item.DyeCount,
            GetArmoireIndex().Contains(itemId));

        itemCache[itemId] = metadata;
        return metadata;
    }

    public void Warm(IEnumerable<uint> itemIds)
    {
        foreach (var itemId in itemIds)
            _ = Get(itemId);
    }

    private HashSet<uint> GetArmoireIndex()
    {
        if (armoireItemIds != null)
            return armoireItemIds;

        var result = new HashSet<uint>();
        foreach (var row in dataManager.GetExcelSheet<Cabinet>())
        {
            if (row.Item.RowId != 0)
                result.Add(row.Item.RowId);
        }

        armoireItemIds = result;
        log.Debug($"Indexed {result.Count} Armoire-eligible items.");
        return result;
    }

    private static GearSlot ResolveSlot(Item item)
    {
        if (!item.EquipSlotCategory.IsValid)
            return GearSlot.Unknown;

        var category = item.EquipSlotCategory.Value;

        if (category.MainHand > 0) return GearSlot.MainHand;
        if (category.OffHand > 0) return GearSlot.OffHand;
        if (category.Head > 0) return GearSlot.Head;
        if (category.Body > 0) return GearSlot.Body;
        if (category.Gloves > 0) return GearSlot.Gloves;
        if (category.Legs > 0) return GearSlot.Legs;
        if (category.Feet > 0) return GearSlot.Feet;
        if (category.Ears > 0) return GearSlot.Ears;
        if (category.Neck > 0) return GearSlot.Neck;
        if (category.Wrists > 0) return GearSlot.Wrists;
        if (category.FingerR > 0 || category.FingerL > 0) return GearSlot.Ring;

        return GearSlot.Unknown;
    }

    private void OnLanguageChanged(string _)
    {
        // Names are localized; model/slot data is cheap enough to repopulate lazily.
        itemCache.Clear();
    }

    public void Dispose()
    {
        if (disposed)
            return;

        pluginInterface.LanguageChanged -= OnLanguageChanged;
        disposed = true;
    }
}
