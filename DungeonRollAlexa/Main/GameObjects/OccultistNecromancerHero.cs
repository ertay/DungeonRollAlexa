using DungeonRollAlexa.Helpers;
using System.Linq;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class OccultistNecromancerHero : Hero 
    {
        public override string SpecialtyInformation => "Your clerics may be used as mages, and mages may be used as clericss. To use this specialty, say transform cleric, or say transform mage. ";

        public override string UltimateInformation => IsLeveledUp ? "Your ultimate ability is Command Dead which lets you transform two skeletons into fighters that are discarded when you complete a level. Say Command Dead to use it. " : "Your ultimate ability is Animate Dead which transforms one skeleton into a fighter that is discarded when you complete a level. Say Animate Dead to use it. ";

        public override string LevelUpMessage => "Your hero leveled up and became a Necromancer. You can now use a new ultimate ability by saying Command Dead which transforms two skeletons into fighters that are discarded when  you complete a level. ";

        public OccultistNecromancerHero() : base()
        {
            HeroType = HeroType.OccultistNecromancer;
        }

        public override string TransformCompanion(CompanionType companion)
        {
            if (companion != CompanionType.Cleric && companion != CompanionType.Mage)
                return $"You cannot transform a {companion}. Try saying transform cleric or transform mage instead. ";
            if (companion == CompanionType.Cleric)
            {
                PartyDice.First(d => d.Companion == companion).Companion = CompanionType.Mage;
                return "You transformed a cleric into a mage. ";
            }

            PartyDice.First(d => d.Companion == companion).Companion = CompanionType.Cleric;
            return "You transformed a mage into a cleric. ";
        }

        public override string ActivateLevelOneUltimate(Dungeon dungeon)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (Experience > 4)
                return "You are a Necromancer now. To use your new ultimate ability say Command Dead. ";
            // check if skeleton exists
            var skeleton = dungeon.DungeonDice.FirstOrDefault(d => d.DungeonDieType == DungeonDieType.Skeleton);
            if (skeleton == null)
                return "There are no skeletons in this level. Use Animate Dead when you encounter skeletons. ";
            // skeleton detected let's remove it and add a fighter to our party
            dungeon.DungeonDice.Remove(skeleton);
            PartyDie fighter = new PartyDie();
            fighter.Companion = CompanionType.Fighter;
            fighter.IsFromMonster = true;
            PartyDice.Insert(0, fighter);
            IsExhausted = true;
            return "You cast Animate Dead and transformed a skeleton into a fighter. This fighter will be discarded when you complete a level. ";
        }

        public override string ActivateLevelTwoUltimate(Dungeon dungeon)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (Experience < 5)
                return "Your hero is still an Occultist and cannot use the Command Dead ultimate. Try saying Animate Dead instead. ";
            // check how many skeletons we have
            int skeletonCount = dungeon.DungeonDice.Count(d => d.DungeonDieType == DungeonDieType.Skeleton);
            if (skeletonCount < 1)
                return "There are no skeletons in this level. Use Command Dead when you encounter skeletons. ";
            // skeletons are present let's remove up to two and add  up to two fighters to our party
            int skeletonsToTransform = skeletonCount > 2 ? 2 : skeletonCount;
            string plural = skeletonsToTransform == 2 ? "s" : "";
            for (int i = 0; i < skeletonsToTransform; i++)
            {
                dungeon.DungeonDice.RemoveFirst(d => d.DungeonDieType == DungeonDieType.Skeleton);
                PartyDie fighter = new PartyDie();
                fighter.Companion = CompanionType.Fighter;
                fighter.IsFromMonster = true;
                PartyDice.Insert(0, fighter);
            }
                
            
            IsExhausted = true;
            return $"You cast Command Dead and transformed {skeletonsToTransform} skeleton{plural} into fighter{plural}. If not used, {skeletonsToTransform} fighter{plural} will be discarded when you complete the level. ";
        }


    }
}
