using System.Linq;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class MinstrelBardHero : Hero
    {
        public override string SpecialtyInformation => IsLeveledUp ? "Your champions can defeat an additional monster. Your thieves may be used as mages, and mages may be used as thieves. To use this specialty, say transform thief, or say transform mage. " : "Your thieves may be used as mages, and mages may be used as thieves. To use this specialty, say transform thief, or say transform mage. ";

        public override string UltimateInformation => "Your ultimate ability is Bard's Song which discards all dragon dice from the Dragon's Lair. Say Bard's Song to use it. ";

        public override string LevelUpMessage => "Your hero leveled up and became a Bard. Now, your champions can defeat an additional monster. ";

        public override CompanionType? CompanionThatKillsAdditionalMonster => CompanionType.Champion;

        public MinstrelBardHero() : base()
        {
            HeroType = HeroType.MinstrelBard;
        }

        public override string TransformCompanion(CompanionType companion)
        {
            if (companion != CompanionType.Thief && companion != CompanionType.Mage)
                return $"You cannot transform a {companion}. Try saying transform thief or transform mage instead. ";
            if (companion == CompanionType.Thief)
            {
                PartyDice.First(d => d.Companion == companion).Companion = CompanionType.Mage;
                return "You transformed a thief into a mage. ";
            }

            PartyDice.First(d => d.Companion == companion).Companion = CompanionType.Thief;
            return "You transformed a mage into a thief. ";
        }

        public override string ActivateLevelOneUltimate(Dungeon dungeon)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";
            if (dungeon.DragonsLair == 0)
                return "There are no dice in the Dragon's lair to discard. You can use this ability when there are dice in the lair. ";

            dungeon.DragonsLair = 0;
            IsExhausted = true;

            return "You sing a Bard's song to discard all dice from the Dragon's Lair. ";

        }

        public override string ActivateLevelTwoUltimate(Dungeon dungeon)
        {
            return ActivateLevelOneUltimate(dungeon);
        }

        public override bool CanKillAdditionalMonster(CompanionType companion)
        {
            if (IsLeveledUp && companion == CompanionThatKillsAdditionalMonster.Value)
                return true;
            return false;
        }
    }
}   
