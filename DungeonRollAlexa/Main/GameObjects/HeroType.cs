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
        [Description("Minstrel - Specialty: Thieves may be used as mages, and mages may be used as thieves. The ultimate ability is Bard's Song which discards all dragon dice from the Dragon's Lair. Turns into Bard when leveled up and gets an additional specialty which makes your champions defeat an additional monster. ")]
        MinstrelBard,
        [Description("Crusader - Specialty: Fighters may be used as clerics, and clerics may be used as fighters. The ultimate ability is Holy Strike and when activated lets you use the hero as a fighter or cleric. Turns into Paladin when leveled up and the ultimate ability is replaced with Divine Intervention which lets you discard one treasure item to open all chests, quaff all potions, discard all monsters, and discard all dragon dice in the Dragon's Lair. ")]
        CrusaderPaladin,
        [Description("Half-Goblin - Specialty: You may open Chests and quaff Potions at any time during the Monsters phase. The ultimate ability is Plea For Help which transforms a goblin into a thief that gets discarded when you complete a level. Turns into Chieftain when leveled up and the ultimate ability is replaced with Pull Rank which transforms two goblins to fighters that get discarded when you complete a level. ")]
        HalfGoblinChieftain,
        [Description("Enchantress - Specialty: Scrolls may be transformed to any other companion. The ultimate ability is Charm Monster which transforms a monster into a potion. Turns into Beguiler when leveled up and the ultimate ability is replaced with Mesmerize which transforms two monsters into one potion. ")]
        EnchantressBeguiler,
        [Description("Alchemist - Specialty: All chests become potions. The ultimate ability is Healing Salve which rolls 1 Party die from the Graveyard and adds it to your Party. Turns into Thaumaturge when leveled up and the ultimate ability is replaced with Transformation Potion which rolls 2 Party dice from the Graveyard and adds them to your Party. ")]
        AlchemistThaumaturge,
        [Description("Tracker - Specialty: Once per level, you may reroll 1 Goblin. The ultimate ability is Called Shot which lets you discard one monster of any type. Turns into Ranger when leveled up and the ultimate ability is replaced with Flurry Of Arrows which discards one monster of each type. ")]
        TrackerRanger,
        [Description("Archaeologist - Specialty: Acquire two treasure items when forming a party, but discard six treasure items at game end. The ultimate ability is Treasure Seeker which lets you draw two treasure items and then discard two treasure items. Turns into Tomb Raider when leveled up and the Treasure Seeker ability lets you draw two treasure items, and then discard one treasure item. ")]
        ArchaeologistTombRaider,
        [Description("Viking - Specialty: When Forming the Party, 2 party dice are removed from the game and the remaining five become champions without being rolled. The ultimate ability is Viking Fury which discards all dice in the Dragon's Lair. Turns into Undead Viking when leveled up and gets a new specialty which turns all skeletons into potions. ")]
        VikingUndeadViking,

    }
}   
