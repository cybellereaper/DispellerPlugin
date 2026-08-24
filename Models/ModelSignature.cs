namespace Dispeller.Models;

public readonly record struct ModelSignature(
    ushort Primary,
    ushort Secondary,
    ushort Variant,
    ushort Extra)
{
    public static ModelSignature FromRaw(ulong raw)
    {
        var primary = (ushort)(raw & 0xFFFF);
        var secondary = (ushort)((raw >> 16) & 0xFFFF);
        var variant = (ushort)((raw >> 32) & 0xFFFF);
        var extra = (ushort)((raw >> 48) & 0xFFFF);

        // Equipment models only need the primary key. Weapons use all four fields.
        return variant != 0
            ? new ModelSignature(primary, secondary, variant, extra)
            : new ModelSignature(primary, 0, 0, 0);
    }

    public bool IsEmpty => Primary == 0 && Secondary == 0 && Variant == 0 && Extra == 0;

    public override string ToString() =>
        $"{Primary:D4}-{Secondary:D4}-{Variant:D4}-{Extra:D4}";
}
