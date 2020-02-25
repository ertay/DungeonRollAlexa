using DungeonRollAlexa.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DungeonRollAlexa.Main.GameObjects
{
    public enum HeroType
    {
        [Description("Spellsword - Specialty: Fighters may be used as mages, and mages may be used as fighters. The ultimate ability is Arcane Blade and when activated lets you use the hero as a fighter or mage. Turns into Battlemage when leveled up and the ultimate ability is replaced with Arcane Fury which lets you discard all monsters, chests, potions, and dice in the dragon's lair. ")]
        SpellswordBattlemage,
        [Description("Mercenary - Specialty: When forming the party, you may reroll any number of party dice. The ultimate ability is Calculated Strike which lets you defeat any two monsters. Turns into a Commander when leveled up and gets a new specialty that lets your fighters defeat an additional monster of any type, while the new ultimate ability is Battlefield Presence and lets you reroll any number of party or dungeon dice. ")]
        MercenaryCommander
    }

    /// <summary>
    /// Parent hero class. All heroes should inherit from this class.
    /// </summary>
    public class Hero
    {
        public HeroType HeroType { get; set; }
        public bool IsExhausted { get; set; }

        public int Experience { get; set; }

        public List<PartyDie> PartyDice { get; set; }

        public List<PartyDie> Graveyard { get; set; }

        public int RevivalsRemaining { get; set; }

        public List<TreasureItem> Inventory { get; set; }

        public List<TreasureItem> UsedItems { get; set; }
        
        public bool HasPartyFormationActions { get; set; }
        [JsonIgnore]
        public virtual string PartyFormationActionMessage { get;  }
        public  Hero()
        {
            IsExhausted = false;
            HasPartyFormationActions = false;
            Experience = 0;
            PartyDice = new List<PartyDie>();
            Graveyard = new List<PartyDie>();
            Inventory = new List<TreasureItem>();
            UsedItems = new List<TreasureItem>();
        }

        public void ClearParty()
        {
            PartyDice.Clear();
            Graveyard.Clear();
        }

        public string RollPartyDice(int partySize = 7)
        {
            // roll party dice to create your party
            // use the size to determine the size of party as some heroes may have a larger party
            string message = "Rolling the party dice. Your party consists of: ";
            for (int i = 0; i <partySize ; i++)
            {
                PartyDie die = new PartyDie();
                PartyDice.Add(die);
                message += (i == partySize - 1) ? $"and {die.Companion}. " : $"{die.Companion}, ";

            }
            return message;
        }

        public string RollSelectedDice()
        {
            bool partyDiceRolled = false;
            foreach (var item in PartyDice)
            {
                if (item.IsSelected)
                {
                    item.Roll();
                    item.IsSelected = false;
                    partyDiceRolled = true;
                }
            }

            string message = partyDiceRolled ? $"Your party now consists of: {GetPartyAsString()}. " : "";
            return message;
        }

            /// <summary>
            /// Rolls one die that currently has the specified companion.
            /// </summary>
            /// <param name="companion"></param>
            /// <returns></returns>
            public string RollPartyDie(CompanionType companion)
        {
            string message = "";
            var die = PartyDice.FirstOrDefault(d => d.Companion == companion);
            if (die == null)
            {
                message = $"You do not have a {companion} in your party.Choose another companion to roll.  ";
            }
            else
            {
                die.Roll();
                message = $"You rolled your {companion} and now got a {die.Companion} in your party. ";
            }
            return message;
        }

        public string AcquireTreasureItems(CompanionType companion, List<TreasureItem> treasureItems)
        {
            // remove the companion from our party dice and add it to the graveyard
            var companionDie = PartyDice.First(d => d.Companion == companion);
            PartyDice.Remove(companionDie);
            Graveyard.Add(companionDie);

            // add the treasure items to our inventory
            Inventory.AddRange(treasureItems);

            string message = treasureItems.Count > 1 ? $"You used a {companionDie.Name} to open {treasureItems.Count} chests and received: ": $"You used a {companionDie.Name} to open {treasureItems.Count} chest and received: ";

            List<string> stringItemList = new List<string>();

            treasureItems.ForEach(t => stringItemList.Add(t.TreasureType.GetDescription()));

            message += $"{string.Join(", ", stringItemList)}. ";

            return message;
        }

        public string AcquireSingleTreasureItem(CompanionType companion, TreasureItem item)
        {
            // acquire a single treasure item
            // first remove companion from party and add to graveyard
            var companionDie = PartyDice.First(d => d.Companion == companion);
            PartyDice.Remove(companionDie);
            Graveyard.Add(companionDie);

            // now add treasure
            Inventory.Add(item);

            string message = $"You used a {companionDie.Name} to open a chest and received {item.TreasureType.GetDescription()}. ";

            return message;
        }

        public List<DungeonDieType> UseCompanionToAttack(string companion)
        {
            var partyDie = PartyDice.First(d => d.Name == companion);
                // add this companion to the graveyard and remove from party
                Graveyard.Add(partyDie);
            PartyDice.Remove(partyDie);
            return partyDie.TargetList;
        }

        public string GetPartyAsString()
        {
            string message = "";
            if (PartyDice.Count == 1)
            {
                message = PartyDice[0].Name;
            }
            else if (PartyDice.Count == 2)
            {
                message = $"{PartyDice[0].Name} and {PartyDice[1].Name}";
            }
            else if (PartyDice.Count > 2)
            {
                for (int i = 0; i < PartyDice.Count; i++)
                {
                    message += (i == PartyDice.Count - 1) ? $"and {PartyDice[i].Name}" : $"{PartyDice[i].Name}, ";
                }
            }
            else
                message = "You do not have any party dice";

            return message;
        }

        public bool IsCompanionInParty(string companionName, bool includeScroll = false)
        {
            // checks if the companion is present in the party
            if(includeScroll)
                return PartyDice.Any(d => d.Name == companionName);
            // don't include scrolls
            return PartyDice.Any(d => d.Companion != CompanionType.Scroll && d.Name == companionName);
        }

        public string DrinkPotions(int numberOfPotions, CompanionType companion)
        {
            // puts the companion in the graveyard and saves the number of potions quaffed to track the revivals
            RevivalsRemaining = numberOfPotions;
            var partyDie = PartyDice.First(d => d.Companion == companion);
            PartyDice.Remove(partyDie);
            Graveyard.Add(partyDie);

            return $"You used a {companion} to quaff potions. The number of remaining companion revivals is {RevivalsRemaining}. Say revive followed by the companion name to begin reviving. ";
        }

        public string ReviveCompanion(CompanionType companion)
        {
            // move a party die from graveyard to party dice then set the companion value
            var partyDie = Graveyard[0];
            Graveyard.RemoveAt(0);
            partyDie.Companion = companion;
            PartyDice.Add(partyDie);
            RevivalsRemaining--;
            if (RevivalsRemaining > 0)
                return $"You added a {companion} to your party. Say revive to revive another companion. ";

            // no more revivals
            return $"You added a {companion} to your party. ";
        }
        

        public string GetPartyStatus()
        {
            string message = $"Your party consists of: {GetPartyAsString()}. ";
            message += Graveyard.Count > 0 ? $"The number of dice in the graveyard is {Graveyard.Count}. " : "";

            return message;

        }

        public virtual void ActivateSpecialty() { }

        public virtual void ActivateUltimate() { }

        public virtual void LevelUp() { }
    }
}   
