using DungeonRollAlexa.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonRollAlexa.Main.GameObjects
{
    /// <summary>
    /// This class manages the dungeon dice, dragon, dungeon level.
    /// </summary>
    public class Dungeon
    {
        private const int TOTAL_DUNGEON_DICE = 7;
        public int NumberOfDelves { get; set; }
        public int Level { get; set; }
        /// <summary>
        /// Holds all the treasure items that can be acquired when opening a chest.
        /// </summary>
        public List<TreasureItem> TreasureItems { get; set; }

        public List<DungeonDie> DungeonDice { get; set; }

        public int DragonsLair { get; set; }

        [JsonIgnore]
        public bool HasMonsters => DungeonDice.Any(d => d.IsMonster);

        [JsonIgnore]
        public bool HasLoot => DungeonDice.Any(d => d.DungeonDieType == DungeonDieType.Potion || d.DungeonDieType == DungeonDieType.Chest);
        [JsonIgnore]
        public bool HasChest => DungeonDice.Any(d => d.DungeonDieType == DungeonDieType.Chest);

        
        public GameState DetermineDungeonPhase()
        {// this method checks the status of the dungeon and returns a game state enum to determine which phase we should be in
            // first check if there are any monsters
            if (DungeonDice.Any(d => d.IsMonster))
            {
                // we have a monster we should be in omnster phase
                return GameState.MonsterPhase;
            }
            // no monsters, let's check if there is loot
            if (HasLoot)
            {
                // we found loot, should be in loot phase
                return GameState.LootPhase;
            }
            // lets check if we should go to dragon phase
            if (DragonsLair > 2)
            {
                // there are 3 or more dragons in the lair, we should fight the dragon
                return GameState.DragonPhase;
            }
            // TODO: Consider checking if level 10 is cleared to mark the end of a delve and award points
            // if we are here, dungeon level is clear and we should be in the regroup phase
            return GameState.RegroupPhase;

        }

        public string IgnoreLoot()
        {
            // player decided to ignore loot, so we need to remove it from dungeon dice
            DungeonDice.Clear();
            return "You decided to ignore the remaining loot and continue. ";
        }
        
        public string CreateNewDungeon()
        {
            string message;

            if (TreasureItems == null)
                TreasureItems = Utilities.GenerateTreasureItems();
            Level = 1;
            DungeonDice = new List<DungeonDie>();
            DragonsLair = 0;
            
            DungeonDie die = new DungeonDie();
            if (die.DungeonDieType == DungeonDieType.Dragon)
            {
                DragonsLair++;
                message = "The first level of the dungeon is empty, but there is one dragon die in the Dragon's Lair. ";
            }
            else
            {
                DungeonDice.Add(die);
                message = $"The first level in the dungeon has one {DungeonDice[0].DungeonDieType}. ";
            }
            message += $"";
            return message;

        }

        public string CreateNextDungeonLevel()
        {
            // creates the next dungeon level
            
            Level++;
            // get the number of dice we can roll
            int dungeonDiceToRoll = Math.Min(Level, TOTAL_DUNGEON_DICE - DragonsLair);

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
                // not a dragon, at it to dungeon dice
                DungeonDice.Add(die);
            }

            string message = $"Level {Level} of the dungeon has {GetDungeonDiceAsString()}. The number of dragon dice in the Dragon's lair is {DragonsLair}. ";
            return message;

        }

        public string RollSelectedDice()
        {
            bool dungeonDiceRolled = false;
            foreach (var item in DungeonDice)
            {
                if (item.IsSelected)
                {
                    item.Roll();
                    item.IsSelected = false;
                    dungeonDiceRolled = true;
                }
            }
            // we may have rolled dragons, let's make sure
            CheckForDragons();
            string message = dungeonDiceRolled ? $"Dungeon now has {GetDungeonDiceAsString()}. The number of dragon dice in the Dragon's lair is {DragonsLair}. " : "";

            return message;
        }

        public string GetDungeonDiceAsString()
        {
            // formatted string to describe the dungeon
            string message = "";

            if(DungeonDice.Count == 1)
            {
                message = $"{DungeonDice[0].Name}";
            }
            else if (DungeonDice.Count == 2)
            {
                message = $"{DungeonDice[0].Name} and {DungeonDice[1].Name}";
            }
            else if (DungeonDice.Count > 2)
            {
                for (int i = 0; i < DungeonDice.Count; i++)
                {
                    message += (i == DungeonDice.Count - 1) ? $"and {DungeonDice[i].Name}" : $"{DungeonDice[i].Name}, ";
                }
            }
            else
                message = "no monsters or loot in this level";

            return message;
        }

        private void CheckForDragons()
        {
            // Checks if there are dragons in the dungeon dice and increments the lair size
            for (int i = DungeonDice.Count - 1; i >= 0; i--)
            {
                if(DungeonDice[i].DungeonDieType == DungeonDieType.Dragon)
                {
                    DragonsLair++;
                    DungeonDice.RemoveAt(i);
                }
            }
        }

        public bool IsMonsterInDungeon(string monsterName)
        {
            // checks if the monster is present in the dungeon
            return DungeonDice.Any(d => d.Name == monsterName && d.IsMonster);
        }

        public string DefeatMonsters(string monster, List<DungeonDieType> targetList)
        {
            // player attacked a monster, remove all monsters of that type if it is in the target list, otherwise remove single monster
            var monsterDie = DungeonDice.First(d => d.Name == monster);
            bool removeAllMonstersOfThisType = targetList.Any(m => m == monsterDie.DungeonDieType);
            if (!removeAllMonstersOfThisType)
            {
                // we do not need to kill all monsters, just  remove a single one
                DungeonDice.Remove(monsterDie);
                return $"one {monsterDie.Name}";
            }
            int monstersKilled = 0;
            for (int i = DungeonDice.Count - 1; i >= 0; i--)
            {
                if(DungeonDice[i].DungeonDieType == monsterDie.DungeonDieType)
                {
                    monstersKilled++;
                    DungeonDice.RemoveAt(i);
                }
            }

            return monstersKilled > 1 ?  $"{monstersKilled} {monsterDie.Name} monsters" : $"{monstersKilled} {monsterDie.Name}"; ;


        }

        public TreasureItem DefeatDragon()
        {
            DragonsLair = 0;
            var treasure = TreasureItems[0];
            TreasureItems.RemoveAt(0);
            return treasure;
        }

        public TreasureItem OpenChest()
        {
            // check if there are chests available to open
            if(!HasChest)
            {
                // no chests to open
                return null;
            }
            // we have a chest, remove it from the list of dungeon dice
            var chest = DungeonDice.First(d => d.DungeonDieType == DungeonDieType.Chest);
            DungeonDice.Remove(chest);

            // now return a treasure item and remove from treasure items
            var treasure = TreasureItems[0];
            TreasureItems.RemoveAt(0);

            return treasure;
        }

        public List<TreasureItem> OpenAllChests()
        {
            // opens all chests and returns multiple treasure items
            // if no chests return null
            if (!HasChest)
                return null;
            List<TreasureItem> items = new List<TreasureItem>();
            int numberOfChests = DungeonDice.Count(d => d.DungeonDieType == DungeonDieType.Chest);
            // remove all chests
            DungeonDice.RemoveAll(d => d.DungeonDieType == DungeonDieType.Chest);
            // now lets add the treasure items to the list we will return
            for (int i = 0; i < numberOfChests; i++)
            {
                items.Add(TreasureItems[0]);
                TreasureItems.RemoveAt(0);
            }

            return items;
        }

        public void ReturnUsedTreasure(TreasureItem treasure)
        {
            // used treasure items  are shuffled into the pool
            int randomIndex = ThreadSafeRandom.ThisThreadsRandom.Next(TreasureItems.Count);
            TreasureItems.Insert(randomIndex, treasure);
        }

        public void DrinkPotion()
        {
            // remove a potion from the dungeon dice
            var potion = DungeonDice.First(d => d.DungeonDieType == DungeonDieType.Potion);
            DungeonDice.Remove(potion);
        }

        public string ValidateNumberOfPotions(int requestedNumberOfPotions, int numberOfCompanionsInGraveyard)
        {
            // return empty string if number is valid, otherwise returns the error message
            int availablePotions = DungeonDice.Count(d => d.DungeonDieType == DungeonDieType.Potion);

            if (availablePotions < 1)
                return "There are no potions available. ";
            else if (requestedNumberOfPotions < 1)
                return $"You provided an invalid number of potions. The number of potions that are currently available is {availablePotions}. ";
            else if (availablePotions < requestedNumberOfPotions)
                return $"The number of potions currently available is {availablePotions}. Try the quaff potion command again. ";
            else if (requestedNumberOfPotions > numberOfCompanionsInGraveyard)
                return $"The number of potions you can quaff can't be higher than the companions in your graveyard. ";

            // no errors found proceed
            return string.Empty;
        }

        public string GetDungeonStatus()
        {
             string message = $"You are in level {Level}. The dungeon has {GetDungeonDiceAsString()}. The number of dragon dice in the Dragon's lair is {DragonsLair}. ";
            return message;
        }

    }
}
