using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using DungeonRollAlexa.Extensions;
using DungeonRollAlexa.Helpers;
using DungeonRollAlexa.Main.GameObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonRollAlexa.Main
{
    /// <summary>
    /// This is the main class that runs the game.
    /// </summary>
    public class GameSession
    {
        public Session Session { get; set; }
        private string _lastResponseMessage = string.Empty;
        public GameState GameState { get; set; }

        private Hero _hero;
        private Dungeon _dungeon;
        private int _heroSelectorIndex;

        /// <summary>
        /// Constructor to initialize the session
        /// </summary>
        /// <param name="session"></param>
        public GameSession(Session session)
        {
            Session = session;
            
            // initialize attributes
            if(Session.Attributes == null)
            {
                // new session, attributes are null, create them
                Session.Attributes = new Dictionary<string, object>();
                GameState = GameState.MainMenu;
            }
            else
            {
                // not a new session, we need to load data to populate our objects
                LoadData();
            }
        }

        public SkillResponse SetupNewGame()
        {            
            // user requested to start a new game, prepare hero selection
            GameState = GameState.HeroSelection;
            _heroSelectorIndex = 0;
            HeroType heroType = Utilities.GetHeroTypes()[_heroSelectorIndex];
            string newGameMessage = "Okay, let's pick your hero. ";
            string message = $"Do you want to pick the following hero: {heroType.GetDescription()}. ";
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(newGameMessage+ message, RepromptBuilder.Create(message), Session);
        }

        public SkillResponse SelectHero()
        {
            // create new hero based on selection and start the game
            HeroType selectedHero = Utilities.GetHeroTypes()[_heroSelectorIndex];
            switch (selectedHero)
            {
                case HeroType.SpellswordBattlemage:
                    _hero = new SpellswordBattlemageHero();
                    break;
                case HeroType.MercenaryCommander:
                    _hero = new MercenaryCommanderHero();
                    break;
            }

            // we created the hero move to the party creation phase
            GameState = GameState.PartyFormation;
            string message = "";
            message = CreateParty();
            _dungeon = new Dungeon();
            message += _dungeon.CreateNewDungeon();
            // check if hero can do any actions during party formation, otherwise we move to Monster phase
            if(_hero.HasPartyFormationActions)
            {
                message += $"{_hero.PartyFormationActionMessage}";
            }
            else if (_dungeon.HasMonsters)
            {
                message += "You are now in the monster phase. You need to defeat the monster to continue. ";
                GameState = GameState.MonsterPhase;
            }
            else if (_dungeon.HasLoot)
            {
                // no monsters we go to loot phase
                message += "You are now in the loot phase. When you are ready, say next phase. ";
                GameState = GameState.LootPhase;
            }
            else
            {
                // no monsters or loot, we go to regroup phase
                message += "You did not face any monsters or find loot in the first level. To seek glory and continue to level two, say seek glory. ";
                GameState = GameState.RegroupPhase;
            }
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create("Reprompt message."), Session);
        }

        public SkillResponse NextHero()
        {
            // user answered with No to the offered hero, present the next hero in the list
            var heroList = Utilities.GetHeroTypes();
            if(++_heroSelectorIndex >= heroList.Count)
            {
                // we showed all the heroes, return to first hero
                _heroSelectorIndex = 0;
            }
            HeroType heroType = heroList[_heroSelectorIndex];
            string message = $"Alright. Do you want to pick the following hero: {heroType.GetDescription()}. ";
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(message), Session);
        }

        private string CreateParty()
        {
            string message = "";

            // reset the hero party / graveyard
            _hero.ClearParty();
            message += _hero.RollPartyDice();

            return message;
        }

        public SkillResponse AttackMonster(IntentRequest request)
        {
            string message = "";
            string companion = request.Intent.Slots["SelectedCompanion"].Value;
            if(!_hero.IsCompanionInParty(companion))
            {
                if (companion == "scroll")
                    message = "You throw a scroll at the monster but nothing happens. Uh oh! ";
                else
                    message = $"{companion} is not in your party. You need to attack with a companion present in your party. ";
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            // we got a valid companion let's check the monster
            string monster = request.Intent.Slots["SelectedMonster"].Value;
            if (!_dungeon.IsMonsterInDungeon(monster))
            {
                // monster is not present in dungeon
                message = $"{monster} is not a valid target to attack. ";
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }

            // we have a valid companion and monster, let's resolve the fight
            // we use the companion and get the target list of monsters it can kill (whole group)
            var targetList = _hero.UseCompanionToAttack(companion);
            message += $"You used your {companion} to defeat "; 
            message += $"{_dungeon.DefeatMonsters(monster, targetList)}. ";


            message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());

            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            
        }

        public SkillResponse DrinkPotions(IntentRequest request)
        {
            // drink potion requested let's check if number is valid
            int numberOfPotions =Utilities.ParseInt(request.Intent.Slots["NumberOfPotions"].Value);
            // plus one to graveyard because the die used to quaff potion can also be revived with a new face
            string message = _dungeon.ValidateNumberOfPotions(numberOfPotions, _hero.Graveyard.Count + 1);
            if(!string.IsNullOrEmpty(message))
            {
                // validation failed, send the error message to user
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            // we have a valid potion number let's check companion
            string companion = request.Intent.Slots["SelectedCompanion"].Value;
            if (!_hero.IsCompanionInParty(companion, true))
            {
                message = $"{companion} is not in your party. You can quaff potions with a companion present in your party. ";
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }

            // we have valid number and companion, save the revive counter and put the companion in the graveyard
            Enum.TryParse(companion.FirstCharToUpper(), out CompanionType companionType);
            message = _hero.DrinkPotions(numberOfPotions, companionType);
            // set game state to reviving companions, we can't do anything else until this is finished
            GameState = GameState.RevivingCompanions;
            _lastResponseMessage = message;
            SaveData();

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);

        }

        public SkillResponse ReviveCompanion(IntentRequest request)
        {
            // revive requested check game state first
            string message = "";
            if (GameState != GameState.RevivingCompanions)
                return RepeatLastMessage("You can only revive companions after quaffing potions or elixirs. ");
            // check if valid companion
            string companion = request.Intent.Slots["SelectedCompanion"].Value;
            bool isCompanionValid = Enum.TryParse(companion.FirstCharToUpper(), out CompanionType companionType);
            if (!isCompanionValid)
                return RepeatLastMessage($"{companion} is not a valid companion. You can revive fighter, cleric, mage, thief, champion, or scroll. ");
            // we have a valid companion let's revive it
            // first remove potion from dungeon
            _dungeon.DrinkPotion();
            message = _hero.ReviveCompanion(companionType);
            if (_hero.RevivalsRemaining < 1)
                message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
            _lastResponseMessage = message;
            SaveData();

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse OpenChestAction(IntentRequest request)
        {
            // user wants to open chest
            string message = "";
            string companion = request.Intent.Slots["SelectedCompanion"].Value;
            if (!_hero.IsCompanionInParty(companion))
            {
                if (companion == "scroll")
                    message = "You cannot open a chest with a scroll. Use a different companion. ";
                else
                    message = $"{companion} is not in your party. You need to open chests with a companion present in your party. ";
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            // we got a valid companion proceed to check if there's a valid chest
if(!_dungeon.HasChest)
            {
                // no chest found
                message = "There are no chests to open. ";
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }

            // chest available let's open
            
            Enum.TryParse(companion.FirstCharToUpper(), out CompanionType companionType);
            // if we have a thief or champion we will open all chests
            bool openAllChests = (companionType == CompanionType.Thief || companionType == CompanionType.Champion) ? true : false;
            if(openAllChests)
            {
                var treasureItems = _dungeon.OpenAllChests();
                message = _hero.AcquireTreasureItems(companionType, treasureItems);
            }
            else
            {
                var treasureItem = _dungeon.OpenChest();
                message = _hero.AcquireSingleTreasureItem(companionType, treasureItem);
            }

            // check if game phase needs to change
            message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());

            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse GoToNextPhase()
        {
            string message = "";
            if (GameState == GameState.PartyFormation)
            {
                message = UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
                _lastResponseMessage = message;
                SaveData();
            }
            else if (GameState == GameState.LootPhase)
            {
                // player decided to skip loot phase, clear the dice
                _dungeon.DungeonDice.Clear();
                message = UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
                _lastResponseMessage = message;
                SaveData();
            }
            else
                message = "You cannot skip this phase. ";
            // TODO: Use a switch statement to give helpful messages to the player when they try to skip an unskippable phase

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse SeekGlory()
        {
            // user wants to continue to next level
            string message = "You decided to seek glory and challenge the next level of the dungeon! ";
            message += _dungeon.CreateNextDungeonLevel();
            message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        private string UpdatePhaseIfNeeded(GameState newGameState)
        {
            // checks if we need to move to a new phase and returns the appropriate message
            if (GameState == newGameState)
                return ""; 
            // return empty string if we're still in the same state
            switch (newGameState)
            {
                case GameState.MonsterPhase:
                    GameState = newGameState;
                    return "You are now in the monster phase. Defeat all monsters to continue. ";
                    break;
                case GameState.LootPhase:
                    GameState = newGameState;
                    return "You are now in the loot phase. In this phase you can use party dice to open chests or quaff potions. Say next phase if you want to ignore some of  the loot and continue. ";
                    break;
                case GameState.DragonPhase:
                    GameState = newGameState;
                    return "You are now facing a dragon. You need to select three companions of different types to defeat the dragon and continue. ";
                    break;
                case GameState.RegroupPhase:
                    GameState = newGameState;
                    return $"You completed level {_dungeon.Level}. Say seek glory to continue to the next level. Say retire to end the dungeon delve and collect experience points. ";
                    break;
            }

            return "";
        }

        public SkillResponse UseScroll()
        {
            string message = "";

            if (GameState != GameState.MonsterPhase)
                return ResponseBuilder.Ask("Scrolls can only be used during the monster phase.", RepromptBuilder.Create(_lastResponseMessage), Session);

            // we are in Monster phase let's check if we have a scroll
            if (!_hero.IsCompanionInParty("scroll", true))
            {
                // scroll not in party
                message = "You don't have a scroll in your party.";
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session); 
            }
            // we have a scroll, let'let's set the game state to dice selection phase
            // first move scroll to graveyard
            _hero.UsePartyDie(CompanionType.Scroll);
            GameState = GameState.DiceSelectionForScroll;

            message = "You used a scroll. Say select followed by the dungeon or party dice names you want to select for rolling. ";
            _lastResponseMessage = message;
            SaveData();

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse DragonDiceSelection(IntentRequest request)
        {
            // handles party dice selection when fighting a dragon
            string message = "";
            // first check if we already have selected 3 distinct companions
            int distinctCompanionCount = _hero.PartyDice.Count(d => d.IsSelected == true);
            if (distinctCompanionCount > 2)
            {
                message = "You have already selected three companions of different type. To clear the selection and start over, say clear dice selection. ";
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            var validSlots = request.Intent.Slots.Where(s => s.Value.Value != null);
            bool selectionApplied = false;
            

            foreach (var item in validSlots)
            {
                // if we already have selected 3, stop selection
                if (distinctCompanionCount > 2)
                    break;

                bool isValidCompanion = Enum.TryParse(item.Value.Value.FirstCharToUpper(), out CompanionType companion);
                if (!isValidCompanion || companion == CompanionType.Scroll)
                {
                    // invalid companion chosen, continue to next item
                    continue;
                }
                // companion is valid, lets check if we have a companion like it that is selected already
                var partyDie = _hero.PartyDice.FirstOrDefault(d => d.Companion == companion);
                if (partyDie == null || partyDie.IsSelected)
                    continue;
                // if we are here, we have a valid companion that is not selected, let's select it
                partyDie.IsSelected = true;
                distinctCompanionCount++;
                selectionApplied = true;
                
            }

            if (selectionApplied)
            {
                string selection = string.Join(", ", _hero.PartyDice.Where(d => d.IsSelected).Select(d => d.Name).ToList());
                if (distinctCompanionCount > 2)
                    message = $"The current selection is:{selection}. To defeat the dragon, say defeat dragon! ";
                else
                    message = $"The current selection is:{selection}. Select {3 - distinctCompanionCount} more to defeat the dragon. ";
                _lastResponseMessage = message;
                SaveData();
            }
            else
                message = "Invalid dice selection. You need to select three different companions in your party to defeat the dragon. ";

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);

        }

        public SkillResponse DefeatDragon()
        {
            if (GameState != GameState.DragonPhase)
                return ResponseBuilder.Ask("You currently do not see a dragon you can attack. ", RepromptBuilder.Create(_lastResponseMessage), Session);
            // player wants to defeat dragon, let's check if they have 3 distinct dice
            int distinctCompanionCount = _hero.PartyDice.Count(d => d.IsSelected);
            if(distinctCompanionCount < 3)
                return ResponseBuilder.Ask("You need to select three distinct companions to defeat the dragon. ", RepromptBuilder.Create(_lastResponseMessage), Session);

            // OK we have 3 party dice selected, let's kill the dragon
            var treasure = _dungeon.DefeatDragon();
            string message = _hero.DefeatDragon(treasure);
            message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
            _lastResponseMessage = message;
            SaveData();

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);

            
        }

        /// <summary>
        /// Handles dice selection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SkillResponse SelectDice(IntentRequest request)
        {
            if (GameState != GameState.DiceSelectionForScroll && GameState != GameState.PartyFormation && GameState != GameState.DragonPhase)
            {
                // we are not in a dice selection phase
                string errorMessage = "Invalid action. Dice selection can be done after using a scroll or when you are fighting the dragon.";
                return ResponseBuilder.Ask(errorMessage, RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            // check if we are in dragon phase, we need a special selection for that
            if(GameState == GameState.DragonPhase)
            {
                return DragonDiceSelection(request);
            }
            
            SkillResponse response = null;
            string message = "";
            var dieList = new List<Die>();
            dieList.AddRange(_hero.PartyDice);
            // dungeon dice can't be selected during party formation phase
            if (GameState != GameState.PartyFormation)
                dieList.AddRange(_dungeon.DungeonDice);

            var validSlots = request.Intent.Slots.Where(s => s.Value.Value != null);
            bool selectionApplied = false;
            foreach (var item in validSlots)
            {
                string dieFace = item.Value.Value.ToString().ToLower();
                var dieToSelect = dieList.FirstOrDefault(d => d.Name ==dieFace && !d.IsSelected);
                if(dieToSelect != null)
                {
                    dieToSelect.IsSelected = true;
                    selectionApplied = true;
                }
            }

            if (selectionApplied)
            {
                string selection = string.Join(", ", dieList.Where(d => d.IsSelected).Select(d => d.Name).ToList());
                message = $"The current selection is:{selection}. You can select more dice, or say roll selected dice. You can say clear dice selection to start over. ";
            }
            else
                if (GameState == GameState.PartyFormation)
                message = "Invalid dice selection. You can only select dice from your party, and dice that are not selected already. ";
            else    
                message = "Invalid selection. You can only select dice from your party or  the dungeon dice. Also, you cannot select dice that are already selected. ";
            _lastResponseMessage = message;
            SaveData();
            response = ResponseBuilder.Ask(message, RepromptBuilder.Create(message), Session);
            return response;
        }

        public SkillResponse ClearDiceSelection()
        {
            if (GameState != GameState.DiceSelectionForScroll && GameState != GameState.PartyFormation && GameState != GameState.DragonPhase)
            {
                string errorMessage = "Invalid action. Dice selection can be done after using a scroll or when you are fighting the dragon.";
                return ResponseBuilder.Ask(errorMessage, RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            var diceList = new List<Die>();
            diceList.AddRange(_hero.PartyDice);
                diceList.AddRange(_dungeon.DungeonDice);
            diceList.ForEach(d => d.IsSelected = false); ;

            string message = "Dice selection was cleared. ";
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(message), Session);
        }

        public SkillResponse RollSelectedDice()
        {
            if (GameState != GameState.DiceSelectionForScroll && GameState != GameState.PartyFormation)
            {
                string errorMessage = "Invalid action. You can roll dice after using a scroll and selecting the dice you want to roll.";
                return ResponseBuilder.Ask(errorMessage, RepromptBuilder.Create(_lastResponseMessage), Session);
            }

            string message = "";
            message += _hero.RollSelectedDice();
            message += _dungeon.RollSelectedDice();
            bool diceRolled = true;
            if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message))
            {
                message = "You need to select dice before rolling. For example, say select fighter. ";
                diceRolled = false;
            }
            if (diceRolled)
                message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(message), Session);
        }
        /// <summary>
        /// This function will handle all the Yes intent requests depending on the state of the game.
        /// </summary>
        /// <returns></returns>
        public SkillResponse ResolveYesIntent()
        {
            SkillResponse response = null;

            switch (GameState)
            {
                case GameState.HeroSelection:
                response = SelectHero();
                    break;
                default:
                    response= RepeatLastMessage("That was not a valid command. ");
                    break;
            }

            return response;
        }

        /// <summary>
        /// This function will resolve all the No intents depending on the game state.
        /// </summary>
        /// <returns></returns>
        public SkillResponse ResolveNoIntent()
        {
            SkillResponse response = null;

            switch (GameState)
            {
                case GameState.HeroSelection:
                response = NextHero(); 
                    break;
                default:
                    response = RepeatLastMessage("That was not a valid command. ");
                    break;
            }

            return response;
        }

        public SkillResponse GetDungeonStatus()
        {
            string message = _dungeon.GetDungeonStatus();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse GetPartyStatus()
        {
            string message = _hero.GetPartyStatus();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse RepeatLastMessage(string message = "")
        {
            return ResponseBuilder.Ask(message + _lastResponseMessage, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        private void SaveData()
        {
            if (Session == null || Session.Attributes == null)
            {
                throw new NullReferenceException("Session or session attributes not initialized.");
            }

            var attributes = new Dictionary<string, object>();

            attributes.Add("gameState", (int)GameState);
            attributes.Add("lastResponseMessage", _lastResponseMessage);
            attributes.Add("heroSelectorIndex", _heroSelectorIndex);
            if (_hero != null)
            {
                attributes.Add("hero", JsonConvert.SerializeObject(_hero));
            }
            else
            {
                attributes.Add("hero", string.Empty);
            }

            if (_dungeon != null)
            {
                attributes.Add("dungeon", JsonConvert.SerializeObject(_dungeon));
            }
            else
            {
                attributes.Add("dungeon", string.Empty);
            }

            Session.Attributes = attributes;
        }

        private void LoadData()
        {
            if (Session == null)
            {
                throw new NullReferenceException("Session not initialized. Cannot load data.");
            }

            Dictionary<string, object> attributes = Session.Attributes;

            GameState = (GameState)Utilities.ParseInt(attributes["gameState"]);
            _lastResponseMessage = attributes["lastResponseMessage"].ToString();
            _heroSelectorIndex= Utilities.ParseInt(attributes["heroSelectorIndex"]);
            _hero = JsonConvert.DeserializeObject<Hero>(attributes["hero"].ToString());
            _dungeon = JsonConvert.DeserializeObject<Dungeon>(attributes["dungeon"].ToString());

        }
    }
}