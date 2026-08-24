namespace Dispeller.Models;

public enum GearSlot
{
    All = 0,
    MainHand = 1,
    OffHand = 2,
    Head = 3,
    Body = 4,
    Gloves = 5,
    Legs = 6,
    Feet = 7,
    Ears = 8,
    Neck = 9,
    Wrists = 10,
    Ring = 11,
    Unknown = 99,
}

public static class GearSlotExtensions
{
    public static string DisplayName(this GearSlot slot) => slot switch
    {
        GearSlot.MainHand => "Main Hand",
        GearSlot.OffHand => "Off Hand",
        GearSlot.Head => "Head",
        GearSlot.Body => "Body",
        GearSlot.Gloves => "Gloves",
        GearSlot.Legs => "Legs",
        GearSlot.Feet => "Feet",
        GearSlot.Ears => "Ears",
        GearSlot.Neck => "Neck",
        GearSlot.Wrists => "Wrists",
        GearSlot.Ring => "Ring",
        GearSlot.All => "All slots",
        _ => "Unknown",
    };

    public static bool IsWeapon(this GearSlot slot) =>
        slot is GearSlot.MainHand or GearSlot.OffHand;

    public static bool IsArmor(this GearSlot slot) =>
        slot is GearSlot.Head or GearSlot.Body or GearSlot.Gloves or GearSlot.Legs or GearSlot.Feet;

    public static bool IsAccessory(this GearSlot slot) =>
        slot is GearSlot.Ears or GearSlot.Neck or GearSlot.Wrists or GearSlot.Ring;
}
