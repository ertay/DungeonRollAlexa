using DungeonRollAlexa.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class TrackerRangerHero : Hero
    {
        public override string SpecialtyInformation => "Once per level, you may reroll one goblin. Say reroll goblin to use this ability. ";

        public override string UltimateInformation => IsLeveledUp ? "Your ultimate ability is Flurry Of Arrows which discards one monster of each type. Say Flurry Of Arrows to use it. " : "Your ultimate ability is Called Shot which lets you discard one monster of any type. Say Called Shot to use it. ";

        public override string LevelUpMessage => "Your hero leveled up and became a Rangerr. You can now use a new ultimate ability by saying Flurry Of Arrows which discards one monster of each type. ";

        public bool GoblinRerolled { get; set; }

        public TrackerRangerHero() : base()
        {
            HeroType = HeroType.TrackerRanger;
        }

        public override string ActivateSpecialty(Dungeon dungeon = null)
        {
            // check if we have already used this ability in t his level
            if (GoblinRerolled)
                return "You have already rerolled a goblin in this level. You can reroll a goblin once per level. ";
            // check if there is a goblin present
            var goblin = dungeon.DungeonDice.FirstOrDefault(d => d.DungeonDieType == DungeonDieType.Goblin);
            if (goblin == null)
                return "There are no goblins in this level. Use the reroll goblin ability when you encounter a goblin. ";
            // goblin is present, roll it and return the new result
            goblin.Roll();
            GoblinRerolled = true;
            // we may have rolled a dragon, add it to the lair in that case
            if (goblin.DungeonDieType == DungeonDieType.Dragon)
                dungeon.CheckForDragons();
            return $"Rolling a goblin die. {SoundManager.DiceRollSound(true)} The result is {goblin.Name}. ";
        }

        public override string ActivateLevelOneUltimate()
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (IsLeveledUp)
                return "You are a Ranger now. Try saying Flurry Of Arrows to activate your new ultimate ability. ";

            IsExhausted = true;
            // TODO: Refactor this this is a bad solution
            return string.Empty;

        }

        public override string ActivateLevelTwoUltimate(Dungeon dungeon)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (!IsLeveledUp)
                return "Your hero is still a Tracker and cannot use the Flurry Of Arrows ultimate. Try saying Called Shot instead. ";

            if (!dungeon.HasMonsters)
                return "There are no monsters at the moment. Use Flurry of Arrows when you encounter goblins, oozes, or skeletons. ";
            /*
            var goblin = dungeon.DungeonDice.FirstOrDefault(d => d.DungeonDieType == DungeonDieType.Goblin);
            var ooze = dungeon.DungeonDice.FirstOrDefault(d => d.DungeonDieType == DungeonDieType.Ooze);
            var skeleton = dungeon.DungeonDice.FirstOrDefault(d => d.DungeonDieType == DungeonDieType.Skeleton);
            var monsters = new List<DungeonDie>();
            if(goblin != null)
                monsters.Add(goblin);
            if (ooze!= null)
                monsters.Add(ooze);
            if (skeleton!= null)
                monsters.Add(skeleton);
                */
                // select at most one monster of each type
            var monsters = dungeon.DungeonDice.Where(d => d.IsMonster).GroupBy(d => d.DungeonDieType).Select(g => g.First()).ToList();
            foreach (var item in monsters)
            {
                dungeon.DungeonDice.Remove(item);
            }
            string message = "";
            if (monsters.Count == 1)
                message = $"Your Flurry of Arrows defeated one {monsters.First().Name}. ";
            else if (monsters.Count == 2)
                message = $"Your Flurry of Arrows defeated one {monsters[0].Name}, and one {monsters[1].Name}. ";
            else
                message = "Your Flurry of Arrows defeated one goblin, one ooze, and one skeleton. ";
            monsters.Clear();
            IsExhausted = true;

            return message;

        }

        public override void ResetOnDungeonLevelChange()
        {
            GoblinRerolled = false;
        }
    }
}
