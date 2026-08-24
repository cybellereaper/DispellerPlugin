namespace Dispeller.Models;

public sealed record DresserItem(
    uint Slot,
    uint ItemId,
    uint IconId,
    byte Stain1,
    byte Stain2);
