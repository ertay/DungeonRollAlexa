namespace DungeonRollAlexa.Main.GameObjects
{
    public class AlchemistThaumaturgeHero : Hero
    {
        public override string SpecialtyInformation => "All chests become potions. ";

        public override string UltimateInformation => IsLeveledUp ? "Your ultimate ability is Transformation Potion which rolls 2 Party dice from the Graveyard and adds them to your Party. Say Transformation Potion to use it. " : "Your ultimate ability is Healing Salve which rolls 1 Party die from the Graveyard and adds it to your Party. Say Healing Salve to use it. ";

        public override string LevelUpMessage => "Your hero leveled up and became a Thaumaturge. You can now use a new ultimate ability by saying Transformation Potion which rolls 2 Party dice from the Graveyard and adds them to your Party. ";

        public AlchemistThaumaturgeHero() : base()
        {
            HeroType = HeroType.AlchemistThaumaturge;
        }

        public override string ActivateLevelOneUltimate()
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (IsLeveledUp)
                return "You are a Thaumaturge now. To use your new ultimate ability say Transformation Potion. ";
            // check if graveyard is empty
            if (Graveyard < 1)
                return "There are no party dice in the graveyard. Use Healing Salve when you have at least one party die in the graveyard. ";
            // we have dice in the graveyard, let's add it to our party
            PartyDie die = new PartyDie();
            PartyDice.Add(die);
            Graveyard--;
            IsExhausted = true;
            return $"You cast Healing Salve and added one {die.Companion} to your party. ";
        }

        public override string ActivateLevelTwoUltimate()
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (!IsLeveledUp)
                return "Your hero is still an Alchemist and cannot use the Transformation Potion ultimate. Try saying Healing Salve instead. ";
            // check if graveyard is empty
            if (Graveyard < 1)
                return "There are no party dice in the graveyard. Use Transformation Potion when you have at least one party die in the graveyard. ";
            // we have dice in the graveyard, check if we can revive two or just one
            int reviveCount = Graveyard == 1 ? 1 : 2;
            string message = "";
            if(reviveCount == 1)
            {
                PartyDie die = new PartyDie();
                PartyDice.Add(die);
                message = $"You used a Transformation Potion and added one {die.Companion} to your party. ";
            }
            else
            {
                // we need to revive two party dice
                Graveyard -= reviveCount;
                PartyDie dieOne = new PartyDie();
                PartyDie dieTwo = new PartyDie();
                PartyDice.Add(dieOne);
                PartyDice.Add(dieTwo);
                message = "You used a Transformation Potion and added ";
                if(dieOne.Companion == dieTwo.Companion)
                {
                    message += dieOne.Companion == CompanionType.Thief ? "two thieves to your party. " : $"two {dieOne.Companion}s to your party. ";
                }
                else
                {
                    message += $"one {dieOne.Companion}, and one {dieTwo.Companion} to your party. ";
                }
            }
            
            IsExhausted = true;
            return message;
        }
    }
}
