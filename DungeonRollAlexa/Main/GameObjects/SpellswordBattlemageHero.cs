using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class SpellswordBattlemageHero : Hero
    {
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

        public override void ActivateSpecialty()
        {
            throw new NotImplementedException();
        }

        public override void ActivateUltimate()
        {
            throw new NotImplementedException();
        }

        public override void LevelUp()
        {
            throw new NotImplementedException();
        }
    }
}
