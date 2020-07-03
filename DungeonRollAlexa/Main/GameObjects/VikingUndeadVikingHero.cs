namespace DungeonRollAlexa.Main.GameObjects
{
    public class VikingUndeadVikingHero : Hero
    {
        public override string SpecialtyInformation => IsLeveledUp ? "When Forming the Party, 2 party dice are removed from the game and the remaining five become champions without being rolled. All skeletons become potions. " : "When Forming the Party, 2 party dice are removed from the game and the remaining five become champions without being rolled. ";

        public override string UltimateInformation => "Your ultimate ability is Viking Fury which discards all dice in the Dragon's Lair. Say Viking Fury to use it. ";

        public override string LevelUpMessage => "Your hero leveled up and became an Undead Viking. Now all skeletons become potions. ";

        public VikingUndeadVikingHero() : base()
        {
            HeroType = HeroType.VikingUndeadViking;
        }

        public override string RollPartyDice(Dungeon dungeon, int partySize = 7)
        {
            // this hero plays with 5 party dice and starts with 5 champions
            string message = "Your party consists of: ";
            for (int i = 0; i < 5; i++)
            {
                PartyDie die = new PartyDie();
                die.Companion = CompanionType.Champion;
                PartyDice.Add(die);
            }
            message += $"{GetPartyAsString()}. ";

            return message;
        }

        public override string ActivateLevelOneUltimate(Dungeon dungeon)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";
            if (dungeon.DragonsLair == 0)
                return "There are no dice in the Dragon's lair to discard. You can use this ability when there are dice in the lair. ";

            dungeon.DragonsLair = 0;
            IsExhausted = true;

            return "You enter a state of fury and discard all dice in the Dragon's Lair. ";

        }

        public override string ActivateLevelTwoUltimate(Dungeon dungeon)
        {
            return ActivateLevelOneUltimate(dungeon);
        }

        public override void LevelUp(Dungeon dungeon)
        {
            base.LevelUp(dungeon);
            // mark the boolean that all skeletons need to be transformed to potions now
            if(dungeon is DungeonForViking dv)
            {
                dv.SkeletonsToPotions = true;
            }
        }
    }
}   
