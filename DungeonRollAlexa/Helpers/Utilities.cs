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
            return new List<HeroType>() { HeroType.SpellswordBattlemage, HeroType.MercenaryCommander, HeroType.OccultistNecromancer, HeroType.KnightDragonSlayer };
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

