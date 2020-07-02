using Alexa.NET.Request.Type;
using DungeonRollAlexa.Main.GameObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DungeonRollAlexa.Helpers
{
    public static class Utilities
    {
        public static List<HeroType> GetHeroTypes()
        {
            return new List<HeroType>() { HeroType.SpellswordBattlemage, HeroType.MercenaryCommander, HeroType.OccultistNecromancer, HeroType.KnightDragonSlayer, HeroType.MinstrelBard, HeroType.CrusaderPaladin, HeroType.HalfGoblinChieftain, HeroType.EnchantressBeguiler, HeroType.AlchemistThaumaturge, HeroType.TrackerRanger, HeroType.ArchaeologistTombRaider};
                }

        public static List<TreasureItem> GenerateTreasureItems()
        {
            List<TreasureItem> list = new List<TreasureItem>()
            {
                {new TreasureItem(){ TreasureType = TreasureType.VorpalSword, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.VorpalSword, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.VorpalSword, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.Talisman, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.Talisman, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.Talisman, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.ScepterOfPower, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.ScepterOfPower, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.ScepterOfPower, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.ThievesTools, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.ThievesTools, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.ThievesTools, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.Scroll, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.Scroll, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.Scroll, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.RingOfInvisibility, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.RingOfInvisibility, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.RingOfInvisibility, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.RingOfInvisibility, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.DragonScales, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.DragonScales, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.DragonScales, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.DragonScales, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.DragonScales, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.DragonScales, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.Elixir, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.Elixir, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.Elixir, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.DragonBait, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.DragonBait, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.DragonBait, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.DragonBait, ExperiencePoints = 1 } },
                {new TreasureItem(){ TreasureType = TreasureType.TownPortal, ExperiencePoints = 2 } },
                {new TreasureItem(){ TreasureType = TreasureType.TownPortal, ExperiencePoints = 2 } },
                {new TreasureItem(){ TreasureType = TreasureType.TownPortal, ExperiencePoints = 2 } },
                {new TreasureItem(){ TreasureType = TreasureType.TownPortal, ExperiencePoints = 2 } },
            };
            list.Shuffle();
            return list;
        }
        public const int NUMBER_OF_RULES = 24;

        public static string GetRule(int ruleNumber)
        {
            string rule = "";
            switch (ruleNumber)
            {
                case 0:
                    rule = "Great! Don't worry, this won't take long! You can say Next to proceed to the next step, back to go back to the previous step, and repeat if you want me to repeat the current rule. Say new game at any time to start a new game. Now, say next to continue.";
                    break;
                case 1:
                    rule = "The game consists of three dungeon delves. The following three things happen when you start a delve: 1. I roll seven party dice to form your party for the delve. 2. Your hero is refreshed and ready to use the ultimate ability. 3. I roll one dungeon die to populate the dungeon. Say next to continue.";
                    break;
                case 2:
                    rule = "When I roll dungeon dice, one of the results can be a dragon. Anytime a Dragon face is rolled on a Dungeon die, it moves to the Dragon's Lair where it remains until you have defeated the dragon, or the Delve is over. Dice in the Dragon's Lair may not be re-rolled by Scrolls or abilities! Say next to continue.";
                    break;
                case 3:
                    rule = "A dungeon delve is divided into four phases: 1. Monster phase. 2. Loot phase. 3. Dragon phase. 4. Regroup phase. Say next to continue.";
                    break;
                case 4:
                    rule = "In the monster phase, you need to use your companions to attack goblins, skeletons, or ooze monsters. When you use a party member to do an action, that member is moved to the graveyard, and then the effects are resolved. If you are unable to defeat the monsters, you need to flee and complete your delve without earning any experience points. The following actions can be performed in any order in this phase: 1. Attack monsters. 2. Use hero abilities. 3. Use a scroll. Say next to continue.";
                    break;
                case 5:
                    rule = "Certain Companions are more adept at dispatching certain types of monsters then others. Using your companions efficiently is key. TheFighter defeats one Skeleton, one Ooze, or any number of Goblins. The Cleric defeats one Goblin, one Ooze, or any number of Skeletons. The Mage defeats one Goblin, one Skeleton, or any number of Oozes. The Thief defeats one Goblin, one Skeleton or one Ooze. The Champion may be used to defeat any number of Goblins, any number of Skeletons or any number of Oozes. To attack a monster, you can simply say attack and I will guide you to complete your action. You can also give me a full command by saying Attack goblin with fighter, or use champion to attack skeleton. Say next to continue.";
                    break;
                case 6:
                    rule = "During the monster phase, you can also use your hero's specialty as many times as you wish. To learn how to use the specialty, simply say specialty during a dungeon delve. Your hero also has an ultimate ability which can be used once per delve. To learn how to use your ultimate ability, say ultimate ability during a dungeon delve. Say next to continue.";
                    break;
                case 7:
                    rule = "Sometimes you will have scrolls in your party. When you use a scroll, you can re-roll any number of your party and dungeon dice. To use a scroll, simply say use scroll during the monster phase. Say next to continue.";
                    break;
                case 8:
                    rule = "When you defeat all monsters, if there are any potions or chests in the current level, you move to the loot phase. In the loot phase you can do the following: 1. Open chests to acquire treasure items. 2. Quaff potions to revive party members from the graveyard. Say next to continue.";
                    break;
                case 9:
                    rule = "During the loot phase, to open a chest, simply say open chest. One Thief or Champion may be used to open all available Chests in the level. Any other Companion may be used to open one Chest. Keep in mind that companions that are used to open chests are moved to the graveyard. Say next to continue.";
                    break;
                case 10:
                    rule = "During the loot phase, say quaff potion, to use a party member and quaff potions. Any Party die, including Scrolls, can be used to Quaff any number of Potions. For each Potion quaffed, you can revive a party member from the graveyard and choose its type. Say next to continue.";
                    break;
                case 11:
                    rule = "Keep in mind that when you quaff potions, the party member that you chose to quaff potions is first moved to the graveyard. For example, if you have one party member in the graveyard, and two available potions, you can use a party member to quaff both potions and revive the party member that was already in the graveyard plus the one that you just used to quaff potions. Say next to continue.";
                    break;
                case 12:
                    rule = "We are halfway there, just a few more things to know! All this fighting is certain to attract the attention of the Dragon! If there are three or more dice in the Dragon's Lair, the Dragon arrives and you must defeat it to proceed! ! Say next to continue.";
                    break;
                case 13:
                    rule = "When fighting the dragon, you must select three different companions to defeat it. Use the select command to select your companions. If you are unable to defeat the dragon, you must use the flee command to complete your delve without earning any experience points. Say next to continue.";
                    break;
                case 14:
                    rule = "When you defeat a dragon, you gain a treasure item, and one experience point. All dragon dice from the Dragon's Lair are removed and added to the available pool of dungeon dice. Say next to continue.";
                    break;
                case 15:
                    rule = "A dungeon delve consists of 10 levels. When you complete a level you enter the regroup phase. In this phase you can do one of the following: 1. Seek glory by challenging the next level of the dungeon. 2. Retire to the tavern to complete your dungeon delve and gain experience points equal to the current level of the dungeon. Say next to continue.";
                    break;
                case 16:
                    rule = "When you decide to seek glory and challenge the next level, I will roll dungeon dice equal to the level to populate the dungeon. For example, in level three, I will roll three dungeon dice. Keep in mind that the available pool of dungeon dice is seven, and dice that are in the Dragon's Lair are not rolled. So, for example if you are in level seven, and there are two dragon faces in the Dragon's Lairr, I will roll five dungeon dice. Say next to continue.";
                    break;
                case 17:
                    rule = "Beware! Once the Dungeon dice are rolled for a new level, you must defeat all monsters (and possibly the dragon), or you must Flee, gaining no Experience for this delve. There is no turning back once the Dungeon dice are cast! Say next to continue.";
                    break;
                case 18:
                    rule = "The second action  you can take during the regroup phase is retire. When you retire to the tavern, you complete your delve and gain experience points equal to the current level. If you complete level ten, the retire action is automatically resolved and you gain ten experience points. Say next to continue.";
                    break;
                case 19:
                    rule = "Throughout the game, you will accumulate Treasure by opening Chests and defeating Dragons. Each Treasure item provides you with a one-time ability that may be used at any time during a delve. To use a treasure item, simply say use item. To get more information about what an item does, you can say item information. To check your inventory, you can say inventory. Say next to continue.";
                    break;
                case 20:
                    rule = "Used Treasure items are shuffled into the available pool of treasures. Unused Treasure items contribute to your final score at the end of the game. Say next to continue.";
                    break;
                case 21:
                    rule = "<amazon:emotion name=\"excited\" intensity=\"low\">We are almost done!</amazon:emotion> When you gain five or more experience points, your hero levels up. When this happens, depending on your hero, you gain a new specialty or ultimate ability. You can say specialty, or say ultimate ability, during a dungeon delve to learn how to use them. Say next to continue.";
                    break;
                case 22:
                    rule = "If at any point during a dungeon delve you need to check what is available in your party, you can simply say party status. You can also check the dungeon status by saying dungeon status. Say next to continue.";
                    break;
                case 23:
                    rule = "When you complete your third dungeon delve, I will calculate your final score. Your final score is equal to your experience points plus points received from unused treasure items. The town portal is worth two points, and all other items are worth one point. Additionally, for each pair of dragon scales, you gain two extra points! Say next to continue to the final step.";
                    break;
                case 24:
                    rule = "You are now ready to begin! If you ever need help during a dungeon delve, just say help to get information about valid commands. <amazon:emotion name=\"excited\" intensity=\"medium\">Good luck adventurer! Say new game to start a new game.</amazon:emotion>";
                    break;
            }
            return rule;
        }

        public static string GetSlotValue(string slotName, IntentRequest request)
        {

            var authority = request.Intent.Slots[slotName].Resolution.Authorities.FirstOrDefault();
            if (authority == null || authority.Values == null)
                return request.Intent.Slots[slotName].Value;
            return authority.Values.First().Value.Name;

        }

        public static int ParseInt(object o)
        {
            int result = -1;
            try
            {
                result = Convert.ToInt16(o);
            }
            catch (Exception ex)
            {
                // invalid number
            }

            return result;
        }

        public static string GetDescription<T>(this T enumerationValue)
    where T : struct
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            //If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString();
        }

        public static bool RemoveFirst<T>(this IList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    list.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Shuffle extension for IList lists.
        /// Grabbed from https://stackoverflow.com/a/1262619/3646421
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
    }
}

