using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class SpellswordBattlemageHero : Hero
    {
        
        public override string SpecialtyInformation => "Your fighters may be used as mages, and mages may be used as fighters. To use this specialty, say transform fighter, or say transform mage. ";

        public override string UltimateInformation => IsLeveledUp ? "Your ultimate ability is Arcane Fury which lets you discard all monsters, chests, potions, and dice in the dragon's lair. Say Arcane Fury to use it. " : "Your ultimate ability is Arcane Blade and when activated lets you use the hero as a fighter or mage. Say Arcane Blade to use it. ";
        
        public override string LevelUpMessage
        {
            get
            {
                
                return "Your hero leveled up and became a Battlemage. You can now use a new ultimate ability by saying Arcane Fury which removes all dungeon dice and dragon dice from the Dragon's Lair. ";
            }
        }

        public SpellswordBattlemageHero() : base()
        {
            HeroType = HeroType.SpellswordBattlemage;
        }

        public override string TransformCompanion(CompanionType companion)
        {
            if (companion != CompanionType.Fighter && companion != CompanionType.Mage)
                return $"You cannot transform a {companion}. Try saying transform fighter or transform mage instead. ";
            if(companion == CompanionType.Fighter)
            {
                PartyDice.First(d => d.Companion == companion).Companion = CompanionType.Mage;
                return "You transformed a fighter into a mage. ";
            }

            PartyDice.First(d => d.Companion == companion).Companion = CompanionType.Fighter;
            return "You transformed a mage into a fighter. ";
        }


        public override string ActivateLevelOneUltimate(CompanionType? companion = null, Dungeon dungeon = null)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";
            if (Experience > 4)
                return "You are a Battlemage now. To use your new ultimate ability say Arcane Fury. ";
            // lets check if valid companion is provided
            if (companion == null || companion.Value != CompanionType.Fighter && companion.Value != CompanionType.Mage)
                return "Invalid action. To use your hero as a fighter say arcane blade fighter. To use your hero as a mage, say arcane blade mage. ";

            PartyDie die = new PartyDie();
            die.Companion = companion.Value;
            die.IsFromTreasureOrHeroAbility = true;
            PartyDice.Insert(0, die);
            IsExhausted = true;
            return $"You used your Arcane Blade. Your hero has been added to your party as a {companion.Value}. ";
            
        }

        public override string ActivateLevelTwoUltimate(CompanionType? companion = null, Dungeon dungeon = null)
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (Experience < 5)
                return "Your hero is still a Spellsword and cannot use the Arcane Fury ultimate. Try saying arcane blade instead. ";
            dungeon.DungeonDice.Clear();
            dungeon.DragonsLair = 0;
            IsExhausted = true;
            return "You cast Arcane Fury and discarded all dungeon dice from this level. ";
        }



        
    }
}
