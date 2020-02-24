using Newtonsoft.Json;

namespace DungeonRollAlexa.Main.GameObjects
{
    public enum DungeonDieType
    {
        Goblin, Ooze, Skeleton, Chest, Potion, Dragon
    }

    public class DungeonDie : Die
    {
        public override string Name => DungeonDieType.ToString().ToLower();

        public DungeonDieType DungeonDieType { get; set; }
[JsonIgnore]
        public bool IsMonster { get {
                return (DungeonDieType == DungeonDieType.Goblin ||
DungeonDieType == DungeonDieType.Skeleton ||
DungeonDieType == DungeonDieType.Ooze);
            } }

        public DungeonDie()
        {
            Roll();
        }

        /// <summary>
        /// Roll the dungeon die.
        /// </summary>
        public override void Roll()
        {
            int dieValue = ThreadSafeRandom.ThisThreadsRandom.Next(6);
            DungeonDieType = (DungeonDieType)dieValue;
        }
    }
}
