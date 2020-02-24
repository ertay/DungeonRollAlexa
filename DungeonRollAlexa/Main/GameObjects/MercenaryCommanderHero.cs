namespace DungeonRollAlexa.Main.GameObjects
{
    public class MercenaryCommanderHero : Hero
    {

        public override string PartyFormationActionMessage => "You can reroll any of your party dice before continuing. For example, say select fighter, thief, to select the dice. ";

        public MercenaryCommanderHero() : base()
        {
            HeroType = HeroType.MercenaryCommander;
            HasPartyFormationActions = true;
        }

        public override void ActivateSpecialty()
        {
            throw new System.NotImplementedException();
        }

        public override void ActivateUltimate()
        {
            throw new System.NotImplementedException();
        }

        public override void LevelUp()
        {
            throw new System.NotImplementedException();
        }
    }
}