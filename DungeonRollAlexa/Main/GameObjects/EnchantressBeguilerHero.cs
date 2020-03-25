using System.Linq;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class EnchantressBeguilerHero : Hero
    {
        public override string SpecialtyInformation => "Scrolls may be transformed to any other companion. Say transform scroll to use this ability. ";

        public override string UltimateInformation => IsLeveledUp ? "Your ultimate ability is Mesmerize which lets you transform two monsters into one potion. Say Mesmerize to use it. " : "Your ultimate ability is Charm Monster which transforms one monster into a potion. Say Charm Monster to use it. ";

        public override string LevelUpMessage => "Your hero leveled up and became a Beguiler. You can now use a new ultimate ability by saying Mesmerize which transforms two monsters into one potion. ";

        public EnchantressBeguilerHero() : base()
        {
            HeroType = HeroType.EnchantressBeguiler;
        }

        public override string TransformCompanion(CompanionType companion)
        {
            if (companion == CompanionType.Scroll)
                return $"You don't need to transform a scroll into a scroll. Try saying transform scroll to champion instead.  ";
            
            PartyDice.First(d => d.Companion == CompanionType.Scroll).Companion = companion;
            return $"You transformed a scroll into a {companion}. ";
        }

        public override string ActivateLevelOneUltimate()
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (IsLeveledUp)
                return "You are a Beguiler now. Try saying Mesmerize to activate your new ultimate ability. ";

            IsExhausted = true;
            // TODO: Refactor this this is a bad solution
            return string.Empty;

        }

        public override string ActivateLevelTwoUltimate()
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (!IsLeveledUp)
                return "Your hero is still an Enchantress and cannot use the Mesmerize ultimate. Try saying Charm Monster instead. ";

            IsExhausted = true;
            // TODO: Refactor this this is a bad solution
            return string.Empty;

        }

    }
}
