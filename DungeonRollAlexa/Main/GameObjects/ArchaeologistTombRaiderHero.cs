using DungeonRollAlexa.Helpers;
using System;
using System.Linq;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class ArchaeologistTombRaiderHero : Hero
    {
        public override string SpecialtyInformation => "Acquire two treasure items when forming a party, but six least valuable treasure items are discarded at game end. ";

        public override string UltimateInformation => IsLeveledUp ? "Your ultimate ability is Treasure Seeker which lets you draw two treasure items, and then discard one treasure item. Say Treasure Seeker to use it. " : "Your ultimate ability is Treasure Seeker which lets you draw two treasure items, and then discard two treasure items. Say Treasure Seeker to use it. ";

        public override string LevelUpMessage => "Your hero leveled up and became a Tomb Raider. Now, your Treasure Seeker ultimate ability lets you draw two treasure items, and then discard one treasure item. ";

        public int RemainingTreasureDiscards { get; set; }

        public ArchaeologistTombRaiderHero() : base()
        {
            HeroType = HeroType.ArchaeologistTombRaider;
        }

        public override string RollPartyDice(Dungeon dungeon, int partySize = 7)
        {
            string message = base.RollPartyDice(dungeon, partySize);
            // lets draw two treasure items from the dungeon and add to inventory
            var firstItem = dungeon.TreasureItems.First();
            dungeon.TreasureItems.Remove(firstItem);
            var secondItem = dungeon.TreasureItems.First();
            dungeon.TreasureItems.Remove(secondItem);
            Inventory.Add(firstItem);
            Inventory.Add(secondItem);
            message += $"You acquired two treasures, {firstItem.TreasureType.GetDescription()}, and {secondItem.TreasureType.GetDescription()}. ";
            return message;
        }

        public override string ActivateLevelOneUltimate(Dungeon dungeon)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            // draw two treasure items and add to inventory
            var firstItem = dungeon.TreasureItems.First();
            dungeon.TreasureItems.Remove(firstItem);
            var secondItem= dungeon.TreasureItems.First();
            dungeon.TreasureItems.Remove(secondItem);
            Inventory.Add(firstItem);
            Inventory.Add(secondItem);

            RemainingTreasureDiscards = 2;
            IsExhausted = true;
            return $"You acquired two treasure items, {firstItem.TreasureType.GetDescription()}, and {secondItem.TreasureType.GetDescription()}. You now need to discard two treasure items from your inventory. {GetInventoryStatus()}. Say discard treasure to discard your first treasure item. ";
        }

        public override string ActivateLevelTwoUltimate(Dungeon dungeon)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            // draw two treasure items and add to inventory
            var firstItem = dungeon.TreasureItems.First();
            dungeon.TreasureItems.Remove(firstItem);
            var secondItem = dungeon.TreasureItems.First();
            dungeon.TreasureItems.Remove(secondItem);
            Inventory.Add(firstItem);
            Inventory.Add(secondItem);

            RemainingTreasureDiscards = 1;
            IsExhausted = true;
            return $"You acquired two treasure items, {firstItem.TreasureType.GetDescription()}, and {secondItem.TreasureType.GetDescription()}. You now need to discard one treasure item from your inventory. {GetInventoryStatus()}. Say discard treasure to discard a treasure item. ";
        }

        public void DiscardTreasuresAtGameEnd()
        {
            // discard everything if there are less than 7 items
            if (Inventory.Count < 7)
            {
                Inventory.Clear();
                return;
            }
            // we have more than 7 items, let's discard items that cost 1 XP and not dragon scales first
            int discardCount = 0;
            var oneXPItems = Inventory.Where(i => i.ExperiencePoints == 1 && i.TreasureType != TreasureType.DragonScales).ToList();
            if(oneXPItems.Count < 6)
            {
                // less than 6 items, we will need to discard additional treasures
                while(oneXPItems.Count > 0)
                {
                    var item = oneXPItems.First();
                    Inventory.Remove(item);
                    oneXPItems.Remove(item);
                    discardCount++;
                }
            }
            else
            {
                // we have 6 or more treasures that give use 1 XP, discard those and return
                for (int i = 0; i < 6; i++)
                {
                    var treasure = oneXPItems.First();
                    Inventory.Remove(treasure);
                    oneXPItems.Remove(treasure);
                }
                return;
            }
            // we need to discard more treasures, let's check if we have dragon scales that are not paired
            int dragonScalesRemainder = Inventory.Count(i => i.TreasureType == TreasureType.DragonScales) % 2;
            if(dragonScalesRemainder == 1)
            {
                // we have one dragon scales that is not paired, remove it
                Inventory.RemoveFirst(i => i.TreasureType == TreasureType.DragonScales);
                discardCount++;
                // check if we reached 6 discards
                if (discardCount == 6)
                    return;
            }

            // if we're here, we need to remove more items, let's start with 2 XP ones, otherwise start removing dragon scales
            while(discardCount < 6)
            {
                var twoXpItem = Inventory.FirstOrDefault(i => i.ExperiencePoints == 2);
                if (twoXpItem == null)
                    Inventory.RemoveAt(0);
                else
                    Inventory.Remove(twoXpItem);
                discardCount++;
            }
            // we're done with removal, we should have removed a total of 6 treasures
            
        }

    }
}
