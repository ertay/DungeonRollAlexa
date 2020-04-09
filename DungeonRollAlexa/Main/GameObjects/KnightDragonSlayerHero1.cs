using DungeonRollAlexa.Helpers;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class KnightDragonSlayerHero : Hero
    {
        public override string SpecialtyInformation => IsLeveledUp ? "When forming the party, all scrolls become champions. You can defeat a dragon with two different companions instead of three. " : "When forming the party, all scrolls become champions. ";

        public override string UltimateInformation => "Your ultimate ability is Battlecry which transforms all monsters into dragon faces and moves them to the Dragon's Lair. Say Battlecry to use it. ";

        public override string LevelUpMessage => "Your hero leveled up and became a Dragon Slayer. Now you can defeat a dragon with two different companions instead of three. ";

        public KnightDragonSlayerHero() : base()
        {
            HeroType = HeroType.KnightDragonSlayer;
        }

        public override string RollPartyDice(int partySize = 7)
        {
            // roll party dice to create your party
            // transform scrolls to champions for this hero
            string message = $"Rolling the party dice. {SoundManager.DiceRollSound(true)} Your party consists of: ";
            int scrollCount = 0;
            for (int i = 0; i < partySize; i++)
            {
                PartyDie die = new PartyDie();
                if (die.Companion == CompanionType.Scroll)
                {
                    die.Companion = CompanionType.Champion;
                    scrollCount++;
                }
                PartyDice.Add(die);
                

            }
            message += $"{GetPartyAsString()}. ";

            if (scrollCount == 1)
                message += "One scroll was transformed to a champion. ";
            else if (scrollCount > 1)
                message += $"{scrollCount} scrolls were transformed to champions. ";
            return message;
        }

        public override string ActivateLevelOneUltimate(Dungeon dungeon)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            int monsterCount = dungeon.DungeonDice.RemoveAll(d => d.IsMonster);
            dungeon.DragonsLair += monsterCount;
            
            IsExhausted = true;
            return $"You shouted a Battlecry and transformed all monsters into dragon faces. The number of dragons in the Dragon's Lair is {dungeon.DragonsLair}. ";
        }

        public override string ActivateLevelTwoUltimate(Dungeon dungeon)
        {
            return ActivateLevelOneUltimate(dungeon);
        }

        public override void LevelUp()
        {
            base.LevelUp();
            DefeatDragonCompanionCount = 2;
        }
    }
}   
