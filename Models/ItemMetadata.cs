namespace Dispeller.Models;

public sealed record ItemMetadata(
    uint ItemId,
    string Name,
    uint IconId,
    GearSlot Slot,
    ModelSignature Model,
    byte DyeCount,
    bool CanGoInArmoire);
