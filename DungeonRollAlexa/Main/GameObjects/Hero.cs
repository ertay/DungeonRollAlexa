using DungeonRollAlexa.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DungeonRollAlexa.Helpers;

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

        public int Graveyard { get; set; }

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
            Graveyard =0;
            Inventory = new List<TreasureItem>();
            UsedItems = new List<TreasureItem>();
        }

        public void ClearParty()
        {
            PartyDice.Clear();
            Graveyard = 0;
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

        public void UsePartyDie(CompanionType companion)
        {
            // TODO: Consider using this method everywhere Dice need to be moved from party to graveyard

            // always try to remove companion created from treasure item first
            
            bool companionFromTreasureRemoved = PartyDice.RemoveFirst(d => d.IsFromTreasureItem && d.Companion == companion);
            if (companionFromTreasureRemoved)
                return;
            // player didnt have a companion of this type that came from treasure, let's remove a normal die
                var partyDie = PartyDice.First(d => d.Companion == companion);
            PartyDice.Remove(partyDie);
            Graveyard++;
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

        public string DefeatDragon(TreasureItem treasureItem)
        {
            // player defeated dragon, let's remove the dice selection and move companions to graveyard
            string selectedCompanions= string.Join(", ", PartyDice.Where(d => d.IsSelected).Select(d => d.Name).ToList());
            UsePartyDie(PartyDice.First(d => d.IsSelected).Companion);
            UsePartyDie(PartyDice.First(d => d.IsSelected).Companion);
            UsePartyDie(PartyDice.First(d => d.IsSelected).Companion);

            // TODO: Defeating dragon can be with two companions, fix this

            Inventory.Add(treasureItem);
            string message = $"You used your {selectedCompanions} to defeat the dragon. You acquired {treasureItem.TreasureType.GetDescription()}. ";
            return message;
        }

        public string AcquireTreasureItems(CompanionType companion, List<TreasureItem> treasureItems)
        {
            // remove the companion from our party dice and add it to the graveyard
            var companionDie = PartyDice.First(d => d.Companion == companion);
            UsePartyDie(companion);


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
            UsePartyDie(companion);

            // now add treasure
            Inventory.Add(item);

            string message = $"You used a {companionDie.Name} to open a chest and received {item.TreasureType.GetDescription()}. ";

            return message;
        }

        public TreasureItem GetTreasureFromInventory(string treasureName)
        {
            return Inventory.FirstOrDefault(i => i.TreasureType.GetDescription().ToLower() == treasureName);
        }

        public string UseCompanionTreasure(TreasureItem treasure)
        {
            // we used a treasure item, remove it from inventory
            Inventory.Remove(treasure);
            string message = "";
            PartyDie partyDie = new PartyDie();
            partyDie.IsFromTreasureItem = true;

            switch (treasure.TreasureType)
            {
                case TreasureType.VorpalSword:
                    message = "You used your vorpal sword and added a fighter to your party. ";
                    partyDie.Companion = CompanionType.Fighter;
                    break;
                case TreasureType.Talisman:
                    message = "You used your talisman and added a cleric to your party. ";
                    partyDie.Companion = CompanionType.Cleric;
                    break;
                case TreasureType.ScepterOfPower:
                    message = "You used your scepter of power and added a mage to your party. ";
                    partyDie.Companion = CompanionType.Mage;
                    break;
                case TreasureType.ThievesTools:
                    message = "You used your thieves tools and added a thief to your party. ";
                    partyDie.Companion = CompanionType.Thief;
                    break;
                case TreasureType.Scroll:
                    message = "You used your scroll treasure item and added a scroll to your party. ";
                    partyDie.Companion = CompanionType.Scroll;
                    break;
            }
// insert the new companion in the beginning of the party dice
            PartyDice.Insert(0, partyDie);
            return message;
        }

        public List<DungeonDieType> UseCompanionToAttack(string companion)
        {
            var partyDie = PartyDice.First(d => d.Name == companion);
            // add this companion to the graveyard and remove from party
            UsePartyDie(partyDie.Companion);                

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
            UsePartyDie(companion);

            return $"You used a {companion} to quaff potions. The number of remaining companion revivals is {RevivalsRemaining}. Say revive followed by the companion name to begin reviving. ";
        }

        public string ReviveCompanion(CompanionType companion)
        {
            // move a party die from graveyard to party dice then set the companion value
            Graveyard--;
            var partyDie = new PartyDie();
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
            message += Graveyard > 0 ? $"The number of dice in the graveyard is {Graveyard}. " : "";

            return message;

        }

        public string GetInventoryStatus()
        {
            if (Inventory.Count < 1)
                return "There are no items in your inventory. ";
            string items = string.Join(", ", Inventory.Select(i => i.TreasureType.GetDescription()).ToList());
            return $"Your inventory contains: {items}. ";
        }

        public virtual void ActivateSpecialty() { }

        public virtual void ActivateUltimate() { }

        public virtual void LevelUp() { }
    }
}   
