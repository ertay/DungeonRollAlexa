using DungeonRollAlexa.Helpers;
using System.Linq;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class CrusaderPaladinHero : Hero
    {
        public override string SpecialtyInformation => "Your fighters may be used as clerics, and clerics may be used as fighters. To use this specialty, say transform fighter, or say transform cleric. ";

        public override string UltimateInformation => IsLeveledUp ? "Your ultimate ability is Divine Intervention which lets you discard one treasure item to open all chests, quaff all potions, discard all monsters, and discard all dragon dice in the Dragon's Lair. Say Divine Intervention to use it. " : "Your ultimate ability is Holy Strike and when activated lets you use the hero as a fighter or cleric. Say Holy Strike to use it. ";

        public override string LevelUpMessage => "Your hero leveled up and became a Paladin. You can now use a new ultimate ability by saying Divine Intervention which lets you discard one treasure item to open all chests, quaff all potions, discard all monsters, and discard all dragon dice in the Dragon's Lair. ";

        public CrusaderPaladinHero() : base()
        {
            HeroType = HeroType.CrusaderPaladin;
        }

        public override string TransformCompanion(CompanionType companion)
        {
            if (companion != CompanionType.Fighter && companion != CompanionType.Cleric)
                return $"You cannot transform a {companion}. Try saying transform fighter or transform cleric instead. ";
            if (companion == CompanionType.Fighter)
            {
                PartyDice.First(d => d.Companion == companion).Companion = CompanionType.Cleric;
                return "You transformed a fighter into a cleric. ";
            }

            PartyDice.First(d => d.Companion == companion).Companion = CompanionType.Fighter;
            return "You transformed a cleric into a fighter. ";
        }

        public override string ActivateLevelOneUltimate(CompanionType? companion = null, Dungeon dungeon = null)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";
            if (IsLeveledUp)
                return "You are a Paladin now. To use your new ultimate ability say Divine Intervention. ";
            // lets check if valid companion is provided
            if (companion == null || companion.Value != CompanionType.Fighter && companion.Value != CompanionType.Cleric)
                return "Invalid action. To use your hero as a fighter say Holy Strike fighter. To use your hero as a cleric, say Holy Strike cleric. ";

            PartyDie die = new PartyDie();
            die.Companion = companion.Value;
            die.IsFromTreasureOrHeroAbility = true;
            PartyDice.Insert(0, die);
            IsExhausted = true;
            return $"You used Holy Strike. Your hero has been added to your party as a {companion.Value}. ";

        }

        public override string ActivateLevelTwoUltimate(TreasureType treasure, Dungeon dungeon)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (!IsLeveledUp)
                return "Your hero is still a Crusader and cannot use the Divine Intervention ultimate. Try saying Holy Strike instead. ";

            if (dungeon.DungeonDice.Count < 1 && dungeon.DragonsLair < 1)
                return "The dungeon is empty. There is no need to cast Divine Intervention. ";

            // check if the provided treasure is in our inventory
            if (!Inventory.Any(t => t.TreasureType == treasure))
                return $"{treasure.GetDescription()} is not in your inventory. You need to use a treasure item from your inventory to cast Divine Intervention. Say inventory to check what you have in your inventory. ";
            // we have the item in our inventory, remove it and return it to the dungeon
            var treasureItem = Inventory.First(t => t.TreasureType == treasure);
            Inventory.Remove(treasureItem);
            dungeon.ReturnUsedTreasure(treasureItem);
            string message = $"You used {treasure.GetDescription()} to cast Divine Intervention. ";
            // check if there are monsters and discard if so
            if(dungeon.HasMonsters)
            {
                dungeon.DungeonDice.RemoveAll(d => d.IsMonster);
                message += "All monsters have been discarded. ";
            }
            if(dungeon.DragonsLair > 0)
            {
                dungeon.DragonsLair = 0;
                message += "Dragons in the Dragon's Lair have been discarded. ";
            }

            if (dungeon.HasChest)
            {
                message += AcquireTreasureItems(null, dungeon.OpenAllChests());
            }

            if (Graveyard > 0 && dungeon.DungeonDice.Any(d => d.DungeonDieType == DungeonDieType.Potion))
            {
                // make sure we drink no more than what is in the graveyard
                int potionCount = dungeon.DungeonDice.Count(d => d.DungeonDieType == DungeonDieType.Potion);
                RevivalsRemaining = Graveyard >= potionCount ? potionCount : Graveyard;
                if(RevivalsRemaining > 0)
                {
                    string plural = RevivalsRemaining > 1 ? "s" : "";
                    message += $"You quaffed {RevivalsRemaining} potion{plural}. Say revive followed by the companion name to begin reviving. ";
                    dungeon.DungeonDice.Clear();
                }
            }

            IsExhausted = true;
            return message;
        }
    }
}
