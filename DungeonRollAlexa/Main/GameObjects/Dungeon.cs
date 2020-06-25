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
        protected const int TOTAL_DUNGEON_DICE = 7;
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
        
        public virtual string CreateNewDungeon()
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
            else
            {
                DungeonDice.Add(die);
                message += $"The first level in the dungeon has one {DungeonDice[0].DungeonDieType}. ";
            }
            
            return message;

        }

        public virtual string CreateNextDungeonLevel()
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
            string dragons = DragonsLair > 0 ? $"The number of dragon dice in the Dragon's lair is {DragonsLair}. " : "";
            string message = $"{SoundManager.DiceRollSound(true)} {dragons}";
            return message;

        }

        public virtual string RollSelectedDice()
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

            var groups = DungeonDice.GroupBy(g => g.Name).OrderByDescending(g => g.Count()).Select(g => new { Key = (g.Count() > 1) ? g.Key + "s" : g.Key, Count = g.Count() });
            int dieCount = 0;
            string dieName = "";

            if (groups.Count() == 1)
            {
                dieCount = groups.First().Count;
                dieName = groups.First().Key;
                if (dieCount > 1)
                {
                    //dieName += "s"; ;
                }
                message = $"{dieCount} {dieName}";
            }
            else if (groups.Count() == 2)
            {
                List<string> formattedGroups = new List<string>();
                foreach (var item in groups)
                {
                    dieCount = item.Count;
                    dieName = item.Key;
                    if (dieCount > 1)
                    {
                        //dieName += "s"; ;
                    }
                    formattedGroups.Add($"{dieCount} {dieName}");
                }
                message = $"{formattedGroups[0]} and {formattedGroups[1]}";
            }
            else if (groups.Count() > 2)
            {
                for (int i = 0; i < groups.Count(); i++)
                {
                    dieCount = groups.ElementAt(i).Count;
                    dieName = groups.ElementAt(i).Key;
                    if (dieCount > 1)
                    {
                        //dieName += "s"; ;
                    }
                    message += (i == groups.Count() - 1) ? $"and {dieCount} {dieName}" : $"{dieCount} {dieName}, ";
                }
            }
            else
                message = "no monsters or loot";

            return message;
        }

        public void CheckForDragons()
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

        public bool IsMonsterInDungeon(DungeonDieType monster)
        {
            // checks if the monster is present in the dungeon
            return DungeonDice.Any(d => d.DungeonDieType== monster && d.IsMonster);
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

            // remove all chests
            int numberOfChests = DungeonDice.RemoveAll(d => d.DungeonDieType == DungeonDieType.Chest);
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

        public void DrinkPotions(int numberOfPotions)
        {
            // remove potions from the dungeon dice
            for (int i = 0; i < numberOfPotions; i++)
            {
                DungeonDice.RemoveFirst(d => d.DungeonDieType == DungeonDieType.Potion);
            }

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
