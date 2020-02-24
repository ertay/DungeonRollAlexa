using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class SpellswordBattlemageHero : Hero
    {
        public SpellswordBattlemageHero() : base()
        {
            HeroType = HeroType.SpellswordBattlemage;
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
