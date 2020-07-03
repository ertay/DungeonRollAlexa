using DungeonRollAlexa.Helpers;
using System;
using System.Collections.Generic;

namespace DungeonRollAlexa.Main.GameObjects
{
    /// <summary>
    /// Special dungeon for Viking. When Viking is leveled up, all skeletons become potions.
    /// </summary>
    public class DungeonForViking : Dungeon
    {
        public bool SkeletonsToPotions { get; set; }

        public override string CreateNewDungeon()
        {
            // create default dungeon if we don't need to transform skeletons yet
            if(!SkeletonsToPotions)
                return base.CreateNewDungeon();

            string message = "";

            NumberOfDelves++;
            Level = 1;
            DungeonDice = new List<DungeonDie>();
            DragonsLair = 0;

            switch (NumberOfDelves)
            {
                case 1:
                    message += "Get ready for your first dungeon delve! ";
                    break;
                case 2:
                    message += "Get ready for your second dungeon delve! ";
                    break;
                case 3:
                    message += "Get ready for your final dungeon delve! ";
                    break;
            }

            DungeonDie die = new DungeonDie();
            if (die.DungeonDieType == DungeonDieType.Dragon)
            {
                DragonsLair++;
                message += "The first level of the dungeon is empty, but there is one dragon die in the Dragon's Lair. ";
            }
            else if (die.DungeonDieType == DungeonDieType.Skeleton)
            {
                die.DungeonDieType = DungeonDieType.Potion;
                DungeonDice.Add(die);
                message += $"The first level in the dungeon has one {DungeonDice[0].DungeonDieType} which was transformed from a skeleton. ";
            }
            else
            {
                DungeonDice.Add(die);
                message += $"The first level in the dungeon has one {DungeonDice[0].DungeonDieType}. ";
            }

            return message;

        }

        public override string CreateNextDungeonLevel()
        {
            // use default level creation if we dont need to transform  yet
            if(!SkeletonsToPotions)
                return base.CreateNextDungeonLevel();

            Level++;
            // get the number of dice we can roll
            int dungeonDiceToRoll = Math.Min(Level, TOTAL_DUNGEON_DICE - DragonsLair);
            int transformedSkeletons = 0;
            for (int i = 0; i < dungeonDiceToRoll; i++)
            {
                var die = new DungeonDie();
                // if dragon add it to dragon's lair and continue
                if (die.DungeonDieType == DungeonDieType.Dragon)
                {
                    DragonsLair++;
                    // we got a dragon continue with a new die
                    continue;
                }
                // transform skeleton into potion if die is a skeleton
                if (die.DungeonDieType == DungeonDieType.Skeleton)
                {
                    die.DungeonDieType = DungeonDieType.Potion;
                    transformedSkeletons++;
                }
                // at it to dungeon dice
                DungeonDice.Add(die);
            }
            string dragons = DragonsLair > 0 ? $"The number of dragon dice in the Dragon's lair is {DragonsLair}. " : "";
            string skeletonsToPotionsMessage = "";
            if (transformedSkeletons == 1)
                skeletonsToPotionsMessage = "One skeleton was transformed to a potion. ";
            if (transformedSkeletons > 1)
                skeletonsToPotionsMessage = $"{transformedSkeletons} skeletons were transformed to potions. ";
            string message = $"{SoundManager.DiceRollSound(true)} {dragons}{skeletonsToPotionsMessage}";
            return message;
        }

        public override string RollSelectedDice()
        {
            // use default rolling if we don't need to transform skeletons to potions
            if(!SkeletonsToPotions)
                return base.RollSelectedDice();

            bool dungeonDiceRolled = false;
            int transformedSkeletons = 0;
            foreach (var item in DungeonDice)
            {
                if (item.IsSelected)
                {
                    item.Roll();
                    item.IsSelected = false;
                    dungeonDiceRolled = true;
                    if (item.DungeonDieType == DungeonDieType.Skeleton)
                    {
                        item.DungeonDieType = DungeonDieType.Potion;
                        transformedSkeletons++;
                    }
                }
            }
            // we may have rolled dragons, let's make sure
            CheckForDragons();
            string skeletonsToPotionsMessage = "";
            if (transformedSkeletons == 1)
                skeletonsToPotionsMessage = "One skeleton was transformed to a potion. ";
            if (transformedSkeletons > 1)
                skeletonsToPotionsMessage = $"{transformedSkeletons} skeletons were transformed to potions. ";
            string message = dungeonDiceRolled ? $"{skeletonsToPotionsMessage}Dungeon now has {GetDungeonDiceAsString()}. The number of dragon dice in the Dragon's lair is {DragonsLair}. " : "";

            return message;
        }
    }
}
