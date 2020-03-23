using DungeonRollAlexa.Helpers;
using System.Linq;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class HalfGoblinChieftainHero : Hero
    {
        public override string SpecialtyInformation => "You may open Chests and quaff Potions at any time during the Monsters phase. ";

        public override string UltimateInformation => IsLeveledUp ? "Your ultimate ability is Pull Rank which lets you transform two goblins into thieves that are discarded when you complete a level. Say Pull Rank to use it. " : "Your ultimate ability is Plea For Help which transforms one goblin into a thief that is discarded when you complete a level. Say Plea For Help to use it. ";

        public override string LevelUpMessage => "Your hero leveled up and became a Chieftain. You can now use a new ultimate ability by saying Pull Rank which transforms two goblins into thieves that are discarded when  you complete a level. ";

        public HalfGoblinChieftainHero() : base()
        {
            HeroType = HeroType.HalfGoblinChieftain;
        }

        public override string ActivateLevelOneUltimate(Dungeon dungeon)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (IsLeveledUp)
                return "You are a Chieftain now. To use your new ultimate ability say Pull Rank. ";
            // check if goblin exists
            var goblin = dungeon.DungeonDice.FirstOrDefault(d => d.DungeonDieType == DungeonDieType.Goblin);
            if (goblin == null)
                return "There are no goblins in this level. Use Plea For Help when you encounter goblins. ";
            // goblin detected let's remove it and add a fighter to our party
            dungeon.DungeonDice.Remove(goblin);
            PartyDie thief = new PartyDie();
            thief.Companion = CompanionType.Thief;
            thief.IsFromMonster = true;
            PartyDice.Insert(0, thief);
            IsExhausted = true;
            return "You Plea For Help and transformed a goblin into a thief. This thief will be discarded when you complete a level. ";
        }

        public override string ActivateLevelTwoUltimate(Dungeon dungeon)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (!IsLeveledUp)
                return "Your hero is still a Half-Goblin and cannot use the Pull Rank ultimate. Try saying Plea For Help instead. ";
            // check how many goblins we have
            int goblinCount = dungeon.DungeonDice.Count(d => d.DungeonDieType == DungeonDieType.Goblin);
            if (goblinCount < 1)
                return "There are no goblins in this level. Use Pull Rank when you encounter goblins. ";
            // goblins are present let's remove up to two and add  up to two thieves to our party
            int goblinsToTransform = goblinCount > 2 ? 2 : goblinCount;
            string plural = goblinsToTransform == 2 ? "s" : "";
            string pluralThief = goblinsToTransform == 2 ? "thieves" : "a thief";
            for (int i = 0; i < goblinsToTransform; i++)
            {
                dungeon.DungeonDice.RemoveFirst(d => d.DungeonDieType == DungeonDieType.Goblin);
                PartyDie thief = new PartyDie();
                thief.Companion = CompanionType.Thief;
                thief.IsFromMonster = true;
                PartyDice.Insert(0, thief);
            }


            IsExhausted = true;
            return $"You Pull Rank and transformed {goblinsToTransform} goblin{plural} into {pluralThief}, which will be discarded when you complete this level. ";
        }
    }
}
