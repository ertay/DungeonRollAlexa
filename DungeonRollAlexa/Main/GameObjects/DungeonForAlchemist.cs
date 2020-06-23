using DungeonRollAlexa.Helpers;
using System;
using System.Collections.Generic;

namespace DungeonRollAlexa.Main.GameObjects
{
    /// <summary>
    /// The alchemist needs a modified dungeon as chests are all transformed to potions.
    /// This class overrides the methods that involve dungeon dice rolls to transform chests into potions.
    /// </summary>
    public class DungeonForAlchemist : Dungeon
    {
        public override string CreateNewDungeon()
        {
            string message = "";

            if (TreasureItems == null)
                TreasureItems = Utilities.GenerateTreasureItems();
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
            else if (die.DungeonDieType == DungeonDieType.Chest)
            {
                die.DungeonDieType = DungeonDieType.Potion;
                DungeonDice.Add(die);
                message += $"The first level in the dungeon has one {DungeonDice[0].DungeonDieType} which was transformed from a chest. ";
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
            // creates the next dungeon level

            Level++;
            // get the number of dice we can roll
            int dungeonDiceToRoll = Math.Min(Level, TOTAL_DUNGEON_DICE - DragonsLair);
            int transformedChests = 0;
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
                // transform chest into potion if die is a chest
                if (die.DungeonDieType == DungeonDieType.Chest)
                {
                    die.DungeonDieType = DungeonDieType.Potion;
                    transformedChests++;
                }
                    // at it to dungeon dice
                    DungeonDice.Add(die);
            }
            string dragons = DragonsLair > 0 ? $"The number of dragon dice in the Dragon's lair is {DragonsLair}. " : "";
            string chestsToPotions = "";
            if (transformedChests == 1)
                chestsToPotions = "One chest was transformed to a potion. ";
            if (transformedChests > 1)
                chestsToPotions = $"{transformedChests} chests were transformed to potions. ";
            string message = $"{SoundManager.DiceRollSound(true)} {dragons}{chestsToPotions}";
            return message;

        }

        public override string RollSelectedDice()
        {
            bool dungeonDiceRolled = false;
            int transformedChests = 0;
            foreach (var item in DungeonDice)
            {
                if (item.IsSelected)
                {
                    item.Roll();
                    item.IsSelected = false;
                    dungeonDiceRolled = true;
                    if(item.DungeonDieType == DungeonDieType.Chest)
                    {
                        item.DungeonDieType = DungeonDieType.Potion;
                        transformedChests++;
                    }
                }
            }
            // we may have rolled dragons, let's make sure
            CheckForDragons();
            string chestsToPotions = "";
            if (transformedChests == 1)
                chestsToPotions = "One chest was transformed to a potion. ";
            if (transformedChests > 1)
                chestsToPotions = $"{transformedChests} chests were transformed to potions. ";
            string message = dungeonDiceRolled ? $"{chestsToPotions}Dungeon now has {GetDungeonDiceAsString()}. The number of dragon dice in the Dragon's lair is {DragonsLair}. " : "";

            return message;
        }
    }
}
