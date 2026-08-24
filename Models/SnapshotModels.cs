using System;
using System.Collections.Generic;
using System.Linq;

namespace Dispeller.Models;

public sealed class DresserSnapshotStore
{
    public DresserSnapshot? Current { get; set; }
    public DresserSnapshot? Previous { get; set; }
}

public sealed class DresserSnapshot
{
    public ulong ContentId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public List<DresserSnapshotItem> Items { get; set; } = [];

    public List<DresserItem> ToDresserItems() =>
        Items.Select(item => new DresserItem(
            item.Slot,
            item.ItemId,
            0,
            item.Stain1,
            item.Stain2)).ToList();
}

public sealed class DresserSnapshotItem
{
    public uint Slot { get; set; }
    public uint ItemId { get; set; }
    public byte Stain1 { get; set; }
    public byte Stain2 { get; set; }
}
