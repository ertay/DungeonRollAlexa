using System.ComponentModel;

namespace DungeonRollAlexa.Main.GameObjects
{
    public enum TreasureType
    {
        [Description("Vorpal Sword")]
        VorpalSword,
        Talisman,

        [Description("Scepter of Power")]
        ScepterOfPower,
        [Description("Thieves Tools")]
        ThievesTools,
        Scroll,
        [Description("Ring of Invisibility")]
        RingOfInvisibility,
        [Description("Dragon Scales")]
        DragonScales,
        Elixir,
        [Description("Dragon Bait")]
        DragonBait,
        [Description("Town Portal")]
        TownPortal
    }

    /// <summary>
    /// This class describes the treasure items.
    /// </summary>
    public class TreasureItem
    {
        public int ExperiencePoints { get; set; }

        public TreasureType TreasureType { get; set; }
    }
}
