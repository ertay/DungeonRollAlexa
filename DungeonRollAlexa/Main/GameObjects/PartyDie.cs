using Newtonsoft.Json;
using System.Collections.Generic;

namespace DungeonRollAlexa.Main.GameObjects
{
    public enum CompanionType
    {
        Fighter, Mage, Cleric, Thief, Champion, Scroll
    }

    public class PartyDie : Die 
    {
        public override string Name => Companion.ToString().ToLower();

        public bool IsInGraveyard { get; set; }

        public CompanionType Companion { get; set; }

        public bool IsFromTreasureItem { get; set; }

        /// <summary>
        /// Contains objects that this die type can interact with. For example, a fighter would contain goblins and potions as one fighter can kill all goblins or drink all potions.
        /// </summary>
        [JsonIgnore]
        public List<DungeonDieType> TargetList
        {
            get
            {
                var list = new List<DungeonDieType>();
                switch (Companion)
                {
                    case CompanionType.Fighter:
                        list.Add(DungeonDieType.Goblin);
                        break;
                    case CompanionType.Mage:
                        list.Add(DungeonDieType.Ooze);
                        break;
                    case CompanionType.Cleric:
                        list.Add(DungeonDieType.Skeleton);
                        break;
                    case CompanionType.Thief:
                        break;
                    case CompanionType.Champion:
                        list.Add(DungeonDieType.Goblin);
                        list.Add(DungeonDieType.Ooze);
                        list.Add(DungeonDieType.Skeleton);
                        break;
                    case CompanionType.Scroll:
                        break;
                }
                return list;
            }
        }

                public PartyDie()
        {
            IsInGraveyard = false;
            Roll();
        }
        /// <summary>
        /// Roll the party die.
        /// </summary>
        public override void Roll()
        {
            int dieValue = ThreadSafeRandom.ThisThreadsRandom.Next(6);
            Companion = (CompanionType)dieValue;
        }
    }
}
