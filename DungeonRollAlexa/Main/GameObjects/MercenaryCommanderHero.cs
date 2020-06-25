using System;

namespace DungeonRollAlexa.Main.GameObjects
{
    public class MercenaryCommanderHero : Hero
    {

        public override string PartyFormationActionMessage => "You can reroll any of your party dice before continuing. For example, say select fighter, thief, to select the dice. Say skip to start the delve with your current party. ";

        public override string SpecialtyInformation => IsLeveledUp? "Your fighters defeat an additional monster of any type. " : "When forming the party, you may reroll any number of party dice. ";

        public override string UltimateInformation => IsLeveledUp ? "Your ultimate ability is Battlefield Presence and lets you reroll any number of party or dungeon dice. Say Battlefield Presence to use it. " : "Your ultimate ability is Calculated Strike which lets you defeat any two monsters. Say Calculated Strike to use it. ";

        public override string LevelUpMessage { get
            {
                
                
                return "Your hero leveled up and became a Commander. Your fighters can now defeat an additional monster and Battlefield Presence is your new ultimate ability which lets you roll any dungeon and party dice. ";
            } }

        public override CompanionType? CompanionThatKillsAdditionalMonster => CompanionType.Fighter;

        public MercenaryCommanderHero() : base()
        {
            HeroType = HeroType.MercenaryCommander;
            HasPartyFormationActions = true;
        }

        public override string ActivateLevelOneUltimate()
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (Experience > 4)
                return "You are a Commander now. Try saying Battlefield Presence to activate your new ultimate ability. ";

            IsExhausted = true;
            // TODO: Refactor this this is a bad solution
            return string.Empty;

        }

        public override string ActivateLevelTwoUltimate()
        {
            if (IsExhausted)
                return "Your hero is exhausted and cannot use the ultimate ability in this dungeon delve. ";

            if (!IsLeveledUp)
                return "Your hero is still a Mercenary and cannot use the Battlefield Presence ultimate. Try saying Calculated Strike instead. ";

            IsExhausted = true;
            
            return string.Empty;
        }

        public override void LevelUp()
        {
            base.LevelUp();
            HasPartyFormationActions = false;
        }

        public override bool CanKillAdditionalMonster(CompanionType companion)
        {
            if (IsLeveledUp && companion == CompanionThatKillsAdditionalMonster.Value)
                return true;
            return false;
        }
    }
}