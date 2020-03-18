using System.ComponentModel;

namespace DungeonRollAlexa.Main.GameObjects
{
    public enum HeroType
    {
        [Description("Spellsword - Specialty: Fighters may be used as mages, and mages may be used as fighters. The ultimate ability is Arcane Blade and when activated lets you use the hero as a fighter or mage. Turns into Battlemage when leveled up and the ultimate ability is replaced with Arcane Fury which lets you discard all monsters, chests, potions, and dice in the dragon's lair. ")]
        SpellswordBattlemage,
        [Description("Mercenary - Specialty: When forming the party, you may reroll any number of party dice. The ultimate ability is Calculated Strike which lets you defeat any two monsters. Turns into a Commander when leveled up and gets a new specialty that lets your fighters defeat an additional monster of any type, while the new ultimate ability is Battlefield Presence and lets you reroll any number of party or dungeon dice. ")]
        MercenaryCommander,
        [Description("Occultist - Specialty: Clerics may be used as mages, and mages may be used as clerics. The ultimate ability is Animate Dead which transforms a skeleton into a fighter that gets discarded when you complete a level. Turns into Necromancer when leveled up and the ultimate ability is replaced with Command Dead which transforms two skeletons to fighters that get discarded when you complete a level. ")]
        OccultistNecromancer,
        [Description("Knight - Specialty: When forming the party, all scrolls become champions. The ultimate ability is Battlecry which transforms all monsters into dragon faces and moves them to the Dragon's Lair. Turns into Dragon Slayer when leveled up and can defeat a dragon with two different companions instead of three. ")]
        KnightDragonSlayer,

    }
}   
