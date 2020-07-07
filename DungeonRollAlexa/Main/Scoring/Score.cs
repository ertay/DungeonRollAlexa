using DungeonRollAlexa.Main.GameObjects;
using Newtonsoft.Json;

namespace DungeonRollAlexa.Main.Scoring
{
    public class Score
    {
        public HeroType HeroType { get; set; }
        public int Points { get; set; }

        [JsonIgnore]
        public string Rank
        {
            get
            {
                if (Points < 16)
                    return "Dragon Fodder";
                else if (Points < 24)
                    return "Village Hero";
                else if (Points < 30)
                    return "Seasoned Explorer";
                else if (Points < 35)
                    return "Champion";
                else
                    return "Hero of Ages";
            }
        }

    }
}
