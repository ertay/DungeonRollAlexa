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

        public SkillResponse Welcome()
        {
            string message = "Welcome to Dungeon Roll Beta Version 4. To begin, say new game. To learn the rules, say rules. Say help at any point during the game if you need help. Say what's new to get information about this version. ";
            _lastResponseMessage = message;
            SaveData();

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse SetupNewGame()
        {            
            // user requested to start a new game, prepare hero selection
            GameState = GameState.RandomHeroPrompt;
            string message = "Do you want to play with a random hero? ";
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            
        }

        public SkillResponse DetailedHeroSelection()
        {
            GameState = GameState.DetailedHeroSelection;
            _heroSelectorIndex = 0;
            HeroType heroType = Utilities.GetHeroTypes()[_heroSelectorIndex];
            string newGameMessage = "Okay, here is more details about the first hero. ";
            string message = $"{heroType.GetDescription()}. Do you want to choose this hero? ";
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(newGameMessage + message, RepromptBuilder.Create(message), Session);
        }

        public SkillResponse BasicHeroSelection()
        {
            GameState = GameState.BasicHeroSelection;

            string message = "Alright. Here is a list of heroes to choose from: Spellsword, Mercenary, Occultist, Knight, Minstrel, or Crusader. Say a hero's name to begin. For detailed hero selection, say detailed hero selection. ";
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse SelectRandomHero()
        {
            var heroTypes = Utilities.GetHeroTypes();
            _heroSelectorIndex = ThreadSafeRandom.ThisThreadsRandom.Next(heroTypes.Count);
            return SelectHero();
        }

        public SkillResponse SelectHero(IntentRequest request)
        {
            if (GameState != GameState.BasicHeroSelection)
                return RepeatLastMessage("Invalid action. ");
            string hero = request.Intent.Slots["SelectedHero"].Value;

            switch (hero)
            {
                case "spellsword":
                    _heroSelectorIndex = 0;
                    return SelectHero();
                case "mercenary":
                    _heroSelectorIndex = 1;
                    return SelectHero();
                case "occultist":
                    _heroSelectorIndex = 2;
                    return SelectHero();
                case "knight":
                    _heroSelectorIndex = 3;
                    return SelectHero();
                case "minstrel":
                    _heroSelectorIndex = 4;
                    return SelectHero();
                case "crusader":
                    _heroSelectorIndex = 5;
                    return SelectHero();
                default:
                    return RepeatLastMessage($"{hero} is not a valid hero. ");
            }
        }

        public SkillResponse SelectHero()
        {
            // create new hero based on selection and start the game
            HeroType selectedHero = Utilities.GetHeroTypes()[_heroSelectorIndex];
            string heroMessage = "";
            switch (selectedHero)
            {
                case HeroType.SpellswordBattlemage:
                    _hero = new SpellswordBattlemageHero();
                    heroMessage = "You selected the Spellsword. ";
                    break;
                case HeroType.MercenaryCommander:
                    _hero = new MercenaryCommanderHero();
                    heroMessage = "You selected the Mercenary. ";
                    break;
                case HeroType.OccultistNecromancer:
                    _hero = new OccultistNecromancerHero();
                    heroMessage = "You selected the Occultist. ";
                    break;
                case HeroType.KnightDragonSlayer:
                    _hero = new KnightDragonSlayerHero();
                    heroMessage = "You selected the Knight. ";
                    break;
                case HeroType.MinstrelBard:
                    _hero = new MinstrelBardHero();
                    heroMessage = "You selected the Minstrel. ";
                    break;
                case HeroType.CrusaderPaladin:
                    _hero = new CrusaderPaladinHero();
                    heroMessage = "You selected the Crusader. ";
                    break;
            }

            _dungeon = new Dungeon();
            // we created the hero move to the party creation phase
            string message = InitializeDungeonDelve();
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(heroMessage + message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        private string InitializeDungeonDelve()
        {
            GameState = GameState.PartyFormation;
            string message = "";
            message = CreateParty();
            _hero.IsExhausted = false;
            message += _dungeon.CreateNewDungeon();
            // check if hero can do any actions during party formation, otherwise we move to Monster phase
            if (_hero.HasPartyFormationActions)
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
                message += "You are in the loot phase. If you want to ignore the loot and continue, say next phase. ";
                GameState = GameState.LootPhase;
            }
            else
            {
                // no monsters or loot, we go to regroup phase
                message += "You did not face any monsters or find loot in the first level. To seek glory and continue to level two, say seek glory. ";
                GameState = GameState.RegroupPhase;
            }
            return message;
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
            string message = $"Okay, here is another hero. {heroType.GetDescription()}. Do you want to choose this hero? ";
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
            Enum.TryParse(companion.FirstCharToUpper(), out CompanionType companionType);
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
            // if hero is a commander and fighter was used, let's check if there's an additional target for the fighter

            if (_dungeon.HasMonsters && _hero.CanKillAdditionalMonster(companionType))
            {
                message += $"Your {companionType} can defeat an additional monster. To use this ability, say defeat additional monster. Otherwise, say skip to ignore this ability. ";
                GameState = GameState.KillAdditionalMonster;
            }
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            
        }

        public SkillResponse DefeatAdditionalMonster(IntentRequest request)
        {
            if (GameState != GameState.KillAdditionalMonster)
                return RepeatLastMessage("Invalid action. ");
            string message = "";
            string monster = request.Intent.Slots["SelectedMonster"].Value;
            if (!_dungeon.IsMonsterInDungeon(monster))
            {
                // monster is not present in dungeon
                message = $"{monster} is not a valid target to defeat. Try saying defeat additional monster again and provide a different monster name. If you want to skip this ability, say skip. ";
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }

            // valid additional monster selected let's kill it
            var targetList = new List<DungeonDieType>();
            if(_hero.CompanionThatKillsAdditionalMonster == CompanionType.Fighter)
                targetList.Add(DungeonDieType.Goblin);
            message += $"Your {_hero.CompanionThatKillsAdditionalMonster.Value} additionally defeated ";
            message += $"{_dungeon.DefeatMonsters(monster, targetList)}. ";

            message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse DrinkPotions(IntentRequest request)
        {
            if (GameState != GameState.LootPhase)
                return ResponseBuilder.Ask("Potions can only be quaffed during the loot phase. ", RepromptBuilder.Create(_lastResponseMessage), Session);

            string message = "";
            // let's check companion first
            string companion = request.Intent.Slots["SelectedCompanion"].Value;
            if (!_hero.IsCompanionInParty(companion, true))
            {
                message = $"{companion} is not in your party. You can quaff potions with a companion present in your party. ";
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            Enum.TryParse(companion.FirstCharToUpper(), out CompanionType companionType);
            // we have valid companion, let's check number of potions
            int numberOfPotions = Utilities.ParseInt(request.Intent.Slots["NumberOfPotions"].Value);
            // plus one to graveyard because the die used to quaff potion can also be revived with a new face, unless the die is from treasure item
            var partyDie = _hero.PartyDice.First(d =>d.Companion == companionType);
            int addToGraveyard = partyDie.IsStandardPartyDie ? 1 : 0;
            message = _dungeon.ValidateNumberOfPotions(numberOfPotions, _hero.Graveyard + addToGraveyard);
            if (!string.IsNullOrEmpty(message))
            {
                // validation failed, send the error message to user
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }

            // we have valid number and companion, save the revive counter and put the companion in the graveyard
            
            message = _hero.DrinkPotions(numberOfPotions, companionType);
            _dungeon.DrinkPotions(numberOfPotions);
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

            message = _hero.ReviveCompanion(companionType);
            if (_hero.RevivalsRemaining < 1)
                message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
            _lastResponseMessage = message;
            SaveData();

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse OpenChestAction(IntentRequest request)
        {
            if (GameState != GameState.LootPhase)
                return ResponseBuilder.Ask("Chests can only be opened during the loot phase. ", RepromptBuilder.Create(_lastResponseMessage), Session);
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
            else if (GameState == GameState.KillAdditionalMonster)
            {
                // player decided to skip the fighter ability to kill additional monster
                
                message = UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
                _lastResponseMessage = message;
                SaveData();
            }
            else
                message = "You cannot skip this phase. ";
            // TODO: Use a switch statement to give helpful messages to the player when they try to skip an unskippable phase

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse RetireDelve()
        {
            if (GameState != GameState.RegroupPhase)
                return RepeatLastMessage("You can retire when you complete a level. ");
            string message = $"You retired on level {_dungeon.Level}. ";
            // lets award experience points 
            message += _hero.GainExperiencePoints(_dungeon.Level);

            
            bool gameOver = false;
            if (_dungeon.NumberOfDelves > 2)
            {
                gameOver = true;
                message += CalculateFinalScore();
                GameState = GameState.MainMenu;
            }
                
            // start a new dungeon delve if game is not over
            if(!gameOver)
                message += InitializeDungeonDelve();
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        private string CalculateFinalScore()
        {
            // third delve complete let's calculate score and show rank
            int score = _hero.Experience;
            _hero.Inventory.ForEach(i => score += i.ExperiencePoints); ;
            // each dragon scale pair worth an additional two points
            int dragonScaleParis = _hero.Inventory.Count(i => i.TreasureType == TreasureType.DragonScales) / 2;
            score += dragonScaleParis * 2;

            string rank, message;

            if (score < 16)
                rank = "Dragon Fodder";
            else if (score < 24)
                rank = "Village Hero";
            else if (score < 30)
                rank = "Seasoned Explorer";
            else if (score < 35)
                rank = "Champion";
            else
                rank = "Hero of Ages";

            message = $"Your final score is {score}. Congratulations, you are now known as {rank}! To start a new game, say new game. ";
            
            return message;
        }

        public SkillResponse FleeDungeon()
        {
            if (GameState == GameState.RegroupPhase)
                return RetireDelve();

            if (GameState == GameState.MainMenu || GameState == GameState.PartyFormation)
                return RepeatLastMessage("That is not a valid action. ");

            string message = $"You fled and concluded your delve on level {_dungeon.Level} without earning any experience points. ";

            bool gameOver = false;
            if (_dungeon.NumberOfDelves > 2)
            {
                gameOver = true;
                message += CalculateFinalScore();
                GameState = GameState.MainMenu;
            }

            // create new delve
            if(!gameOver)
                message += InitializeDungeonDelve();
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse SeekGlory()
        {
            if (GameState != GameState.RegroupPhase)
                return RepeatLastMessage("You can seek glory after finishing a level in the dungeon. ");
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
            if (GameState == newGameState && newGameState != GameState.RegroupPhase)
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
                    return $"You are now facing a dragon. You need to select {_hero.DefeatDragonCompanionCount} companions of different types to defeat the dragon and continue. ";
                    break;
                case GameState.RegroupPhase:
                    if (_dungeon.Level == 10)
                    {
                        string message = "You cleared the dungeon! ";
                        message += _hero.GainExperiencePoints(_dungeon.Level);
                        if (_dungeon.NumberOfDelves > 2)
                        {
                            message += CalculateFinalScore();
                            GameState = GameState.MainMenu;
                        }
                        else
                        {
                            message += InitializeDungeonDelve();
                        }
                        return message;
                    }
                    else
                    {
                        GameState = newGameState;
                        // check if we need to discard transformed monsters
                        string removedTransformedMonsters = _hero.RemoveMonsterCompanions();
                        
                        return $"You completed level {_dungeon.Level}. {removedTransformedMonsters}Say seek glory to continue to the next level. Say retire to end the dungeon delve and collect experience points. ";
                    }
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
            GameState = GameState.StandardDiceSelection;

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
            if (distinctCompanionCount > _hero.DefeatDragonCompanionCount - 1)
            {
                message = $"You have already selected {_hero.DefeatDragonCompanionCount} companions of different type. To clear the selection and start over, say clear dice selection. ";
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            var validSlots = request.Intent.Slots.Where(s => s.Value.Value != null);
            bool selectionApplied = false;
            

            foreach (var item in validSlots)
            {
                // if we already have selected 3, stop selection
                if (distinctCompanionCount > _hero.DefeatDragonCompanionCount - 1)
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
                if (distinctCompanionCount > _hero.DefeatDragonCompanionCount - 1)
                    message = $"The current selection is:{selection}. To defeat the dragon, say defeat dragon! ";
                else
                    message = $"The current selection is:{selection}. Select {_hero.DefeatDragonCompanionCount - distinctCompanionCount} more to defeat the dragon. ";
                _lastResponseMessage = message;
                SaveData();
            }
            else
                message = $"Invalid dice selection. You need to select {_hero.DefeatDragonCompanionCount} different companions in your party to defeat the dragon. ";

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);

        }

        public SkillResponse DefeatDragon()
        {
            if (GameState != GameState.DragonPhase)
                return ResponseBuilder.Ask("You currently do not see a dragon you can attack. ", RepromptBuilder.Create(_lastResponseMessage), Session);
            // player wants to defeat dragon, let's check if they have 3 distinct dice
            int distinctCompanionCount = _hero.PartyDice.Count(d => d.IsSelected);
            if(distinctCompanionCount < _hero.DefeatDragonCompanionCount)
                return ResponseBuilder.Ask($"You need to select {_hero.DefeatDragonCompanionCount} distinct companions to defeat the dragon. ", RepromptBuilder.Create(_lastResponseMessage), Session);

            // OK we have 3 party dice selected, let's kill the dragon
            var treasure = _dungeon.DefeatDragon();
            string message = _hero.DefeatDragon(treasure);
            message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
            _lastResponseMessage = message;
            SaveData();

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);

            
        }

        public SkillResponse UseTreasureItem(IntentRequest request)
        {
            // player wants to use item from inventory

            if(GameState != GameState.MonsterPhase && GameState != GameState.LootPhase && GameState != GameState.DragonPhase)
            {
                // cannot use item in this phase
                return ResponseBuilder.Ask("Items can only be used when you are fighting monsters, looting, or fighting the dragon. ", RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            // let's check if we have item in inventory
            string selectedItem = request.Intent.Slots["SelectedTreasureItem"].Value;
            TreasureItem item = _hero.GetTreasureFromInventory(selectedItem);
            if (item == null)
            {
                // item is not in inventory
                return ResponseBuilder.Ask($"{selectedItem} is not in your inventory. Say inventory to check your inventory. ", RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            // ok the item is present let's use it
            string message = "";
            switch (item.TreasureType)
            {
                case TreasureType.VorpalSword:
                case TreasureType.Talisman:
                case TreasureType.ScepterOfPower:
                case TreasureType.ThievesTools:
                case TreasureType.Scroll:
                    message += _hero.UseCompanionTreasure(item);
                    _dungeon.ReturnUsedTreasure(item);
                    break;
                case TreasureType.RingOfInvisibility:
                    message += UseRingOfInvisibilityItem(item);
                    break;
                case TreasureType.DragonScales:
                    message += "Each pair of dragon scales gives you two points at the end of the game. They do not have any special abilities. ";
                    break;
                case TreasureType.Elixir:
                    message += UseElixirTreasureItem(item);
                    break;
                case TreasureType.DragonBait:
                    message += UseDragonBaitTreasureItem(item);
                    break;
                case TreasureType.TownPortal:
                    message += UseTownPortalTreasureItem(item);
                    break;
            }
            _lastResponseMessage = message;
            SaveData();

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        private string UseTownPortalTreasureItem(TreasureItem item)
        {
            if (GameState != GameState.MonsterPhase && GameState != GameState.LootPhase && GameState != GameState.DragonPhase)
                return "You cannot use a town portal at the moment. You can use it in the monster phase, loot phase, or dragon phase. ";

            _hero.Inventory.Remove(item);
            _dungeon.ReturnUsedTreasure(item);
            // town portal used award experience points
            string message = _hero.GainExperiencePoints(_dungeon.Level);

            bool gameOver = false;
            if(_dungeon.NumberOfDelves > 2)
            {
                gameOver = true;
                message += CalculateFinalScore();
                GameState = GameState.MainMenu;
            }

            if (!gameOver)
                message += InitializeDungeonDelve();

            return message;
        }

        private string UseDragonBaitTreasureItem(TreasureItem item)
        {
            if (GameState != GameState.MonsterPhase)
                return "Dragon bait can only be used during the monster phase. ";
            _hero.Inventory.Remove(item);
            _dungeon.ReturnUsedTreasure(item);
            int monsterCount= _dungeon.DungeonDice.Count(d => d.IsMonster);
            _dungeon.DragonsLair += monsterCount;
            _dungeon.DungeonDice.RemoveAll(d => d.IsMonster);
            string pluralS= monsterCount == 1 ? "" : "s";
            string message = $"You used dragon bait to transform{monsterCount} monster{pluralS} to dragon face{pluralS}. The number of dragon dice in the Dragon's Lair is {_dungeon.DragonsLair}. ";
            message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
            return message;
        }

        private string UseElixirTreasureItem(TreasureItem item)
        {
            if (_hero.Graveyard < 1)
                return "The graveyard is empty. Use an elixir when you want to revive a dead companion. ";
            _hero.Inventory.Remove(item);
            _dungeon.ReturnUsedTreasure(item);
            GameState = GameState.RevivingCompanions;

            return "You used an elixir. Say revive to revive a companion. ";
        }

        private string UseRingOfInvisibilityItem(TreasureItem item)
        {
            if (_dungeon.DragonsLair < 1)
                return "There are no dragon dice in the Dragon's Lair. You don't need to use it. ";
            _hero.Inventory.Remove(item);
            _dungeon.ReturnUsedTreasure(item);
            _dungeon.DragonsLair = 0;
            string message = "You used a ring of invisibility. All dragon dice from the Dragon's Lair were discarded. ";
            message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());

            return message;
        }

        /// <summary>
        /// Handles dice selection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SkillResponse SelectDice(IntentRequest request)
        {
            if (GameState != GameState.StandardDiceSelection && GameState != GameState.PartyFormation && GameState != GameState.DragonPhase && GameState != GameState.DiceSelectionForCalculatedStrike)
            {
                // we are not in a dice selection phase
                string errorMessage = "Invalid action. Dice selection can be done after using a scroll, when you are fighting the dragon, or using certain hero abilities. ";
                return ResponseBuilder.Ask(errorMessage, RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            // check if we are in dragon phase, we need a special selection for that
            if(GameState == GameState.DragonPhase)
            {
                return DragonDiceSelection(request);
            }

            if (GameState == GameState.DiceSelectionForCalculatedStrike)
                return CalculatedStrikeSelectDice(request);
            
            SkillResponse response = null;
            string message = "";
            var dieList = new List<Die>();
            dieList.AddRange(_hero.PartyDice.Where(d => d.IsStandardPartyDie));
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
                message = "Invalid selection. You can only select dice from your party or  the dungeon dice. Also, you cannot select party dice that were added from a treasure or dice that are already selected. ";
            _lastResponseMessage = message;
            SaveData();
            response = ResponseBuilder.Ask(message, RepromptBuilder.Create(message), Session);
            return response;
        }

        public SkillResponse ClearDiceSelection()
        {
            if (GameState != GameState.StandardDiceSelection && GameState != GameState.PartyFormation && GameState != GameState.DragonPhase && GameState != GameState.DiceSelectionForCalculatedStrike)
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
            if (GameState != GameState.StandardDiceSelection && GameState != GameState.PartyFormation)
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

        public SkillResponse TransformCompanion(IntentRequest request)
        {
            // used for the hero specialties that can transform companions
            if(GameState != GameState.MonsterPhase && GameState != GameState.LootPhase && GameState != GameState.DragonPhase)
            {
                // cannot use transform
                return ResponseBuilder.Ask("You can't transform your companions at this time. ", RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            string message = "";
            // check if source companion is in party
            string companion = request.Intent.Slots["SourceCompanion"].Value;
            if (!_hero.IsCompanionInParty(companion, true))
            {
                message = $"{companion} is not in your party. You need to select an active party member to transform. ";
                return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
            }
            Enum.TryParse(companion.FirstCharToUpper(), out CompanionType companionType);
            // let's check if the selected hero has a transform ability
            message = _hero.TransformCompanion(companionType);

            SaveData();

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse ActivateUltimate(HeroUltimates ultimate, IntentRequest request = null)
        {
            // check if this hero can use the chosen ability
            string errormessage = ValidateHeroAbility(ultimate);
            if (!string.IsNullOrEmpty(errormessage))
                return ResponseBuilder.Ask(errormessage, RepromptBuilder.Create(_lastResponseMessage), Session);
                            string message = "";
            switch (ultimate)
            {
                case HeroUltimates.ArcaneBlade:
                    string companion = request.Intent.Slots["SelectedCompanion"].Value;
                    bool parseComplete = Enum.TryParse(companion.FirstCharToUpper(), out CompanionType ct);
                    CompanionType? companionType = null;
                    if (parseComplete)
                        companionType = ct;
                    message = _hero.ActivateLevelOneUltimate(companionType);
                    break;
                case HeroUltimates.ArcaneFury:
                    if(GameState != GameState.MonsterPhase && GameState != GameState.LootPhase && GameState != GameState.DragonPhase)
                    {
                        message += "Arcane Fury can only be used during the monster phase, loot phase, or dragon phase. ";
                        break;
                    }
                    message += _hero.ActivateLevelTwoUltimate(null, _dungeon);
                    message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
                    break;
                case HeroUltimates.CalculatedStrike:
                    if(GameState != GameState.MonsterPhase)
                    {
                        message = "Calculated strike can only be used when you are fighting monsters. ";
                        break;
                    }
                    message = _hero.ActivateLevelOneUltimate();
                    if(string.IsNullOrEmpty(message))
                    {
                        message = "You are preparing to do a calculated strike. Select up to two monsters to defeat. For example, say select goblin and skeleton. ";
                        GameState = GameState.DiceSelectionForCalculatedStrike;
                    }
                    
                    break;
                case HeroUltimates.BattlefieldPresence:
                    if (GameState != GameState.MonsterPhase)
                    {
                        message = "Battlefield Presence can only be used when you are fighting monsters. ";
                        break;
                    }
                    message += _hero.ActivateLevelTwoUltimate();
                    if(string.IsNullOrEmpty(message))
                    {
                        message += "You used Battlefield Presence. Select any number of party and dungeon dice to reroll. ";
                        GameState = GameState.StandardDiceSelection;
                    }
                    break;
                case HeroUltimates.AnimateDead:
                    if(GameState != GameState.MonsterPhase)
                    {
                        message = "Animate Dead can only be used when you are fighting monsters. ";
                        break;
                    }

                    message = _hero.ActivateLevelOneUltimate(_dungeon);
                    message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
                    break;
                case HeroUltimates.CommandDead:
                    if (GameState != GameState.MonsterPhase)
                    {
                        message = "Command Dead can only be used when you are fighting monsters. ";
                        break;
                    }
                    message = _hero.ActivateLevelTwoUltimate(_dungeon);
                    message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
                    break;
                case HeroUltimates.Battlecry:
                    if (GameState != GameState.MonsterPhase)
                    {
                        message = "Battlecry can only be used when you are fighting monsters. ";
                        break;
                    }
                    message = _hero.ActivateLevelOneUltimate(_dungeon);
                    message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
                    break;
                case HeroUltimates.BardsSong:
                    message = _hero.ActivateLevelOneUltimate(_dungeon);
                    message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
                    break;
                case HeroUltimates.HolyStrike:
                    string crusaderCompanion = request.Intent.Slots["SelectedCompanion"].Value;
                    bool parseComplete1 = Enum.TryParse(crusaderCompanion.FirstCharToUpper(), out CompanionType ct1);
                    CompanionType? companionType1 = null;
                    if (parseComplete1)
                        companionType1 = ct1;
                    message = _hero.ActivateLevelOneUltimate(companionType1);
                    break;
                case HeroUltimates.DivineIntervention:
                    if (GameState != GameState.MonsterPhase && GameState != GameState.LootPhase && GameState != GameState.DragonPhase)
                    {
                        message += "Divine Intervention can only be used during the monster phase, loot phase, or dragon phase. ";
                        break;
                    }
                    // load the treasure and validate
                    string selectedTreasure = request.Intent.Slots["SelectedTreasure"].Value;
                    var treasureItem = _hero.GetTreasureFromInventory(selectedTreasure);
                    if(treasureItem == null)
                    {
                        message += $"{selectedTreasure} is not in your inventory. You need to use a treasure item from your inventory to cast Divine Intervention. Say inventory to check what you have in your inventory. ";
                        break;
                    }
                    message += _hero.ActivateLevelTwoUltimate(treasureItem.TreasureType, _dungeon);
                    // if no potions quaffed, we check dungeon's state, otherwise go to revive state
                    if (_hero.RevivalsRemaining < 1)
                        message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());
                    else
                        GameState = GameState.RevivingCompanions;
                    break;
            }
            
            _lastResponseMessage = message;
            SaveData();

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);

        }

            public SkillResponse CalculatedStrikeSelectDice(IntentRequest request)
        {
            var dice = _dungeon.DungeonDice;
            int selectedCount = dice.Count(d => d.IsSelected && d.IsMonster);
            if (selectedCount > 1)
                return ResponseBuilder.Ask("You have already selected two monsters. Say calculated strike to defeat them, or clear dice selection to start over. ", RepromptBuilder.Create(_lastResponseMessage), Session);
            var validSlots = request.Intent.Slots.Where(s => s.Value.Value != null);
            bool selectionApplied = false;
            foreach (var item in validSlots)
            {
                if (selectedCount > 1)
                    break;

                string dieFace = item.Value.Value.ToString().ToLower();
                var dieToSelect = dice.FirstOrDefault(d => d.Name == dieFace && !d.IsSelected && d.IsMonster);
                if (dieToSelect != null)
                {
                    dieToSelect.IsSelected = true;
                    selectedCount++;
                    selectionApplied = true;
                }
            }

            if (!selectionApplied)
                return ResponseBuilder.Ask("Invalid monster selection. You can only select monsters that are present in the dungeon and are not already selected. ", RepromptBuilder.Create(_lastResponseMessage), Session);
            string message = "";
            if (selectedCount == 1)
                message = $"You selected {dice.First(d => d.IsSelected).Name}. You can select one more monster or say calculated strike to complete your action. ";
            else
                message = $"You selected {dice.First(d => d.IsSelected).Name} and {dice.Last(d => d.IsSelected).Name}. Say calculated strike to defeat them. ";
            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);

        }

        public SkillResponse PerformCalculatedStrike()
        {
            if (GameState != GameState.DiceSelectionForCalculatedStrike)
                return RepeatLastMessage();

            int monsterCount = _dungeon.DungeonDice.Count(d =>d.IsSelected);
            if (monsterCount < 1)
                return ResponseBuilder.Ask("You need to select up to two monsters to complete your calculated strike. Say select followed by a monster name. ", RepromptBuilder.Create(_lastResponseMessage), Session);
            // kill the selected monsters
            var dice = _dungeon.DungeonDice;
            string message = monsterCount > 1 ? $"You performed a calculated strike and defeated {dice.First(d => d.IsSelected).Name} and {dice.Last(d => d.IsSelected).Name}. " : $"You performed a calculated strike and defeated {dice.First(d => d.IsSelected).Name}. ";
            dice.RemoveAll(d => d.IsSelected);
            message += UpdatePhaseIfNeeded(_dungeon.DetermineDungeonPhase());

            _lastResponseMessage = message;
            SaveData();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        private string ValidateHeroAbility(HeroUltimates ultimate)
        {
            // returns empty string  if hero can use the ultimate
            switch (_hero.HeroType)
            {
                case HeroType.SpellswordBattlemage:
                    if (ultimate == HeroUltimates.ArcaneBlade || ultimate == HeroUltimates.ArcaneFury)
                        return string.Empty;
                    break;
                case HeroType.MercenaryCommander:
                    if (ultimate == HeroUltimates.CalculatedStrike || ultimate == HeroUltimates.BattlefieldPresence)
                        return string.Empty;
                        break;
                case HeroType.OccultistNecromancer:
                    if (ultimate == HeroUltimates.AnimateDead || ultimate == HeroUltimates.CommandDead)
                        return string.Empty;
                    break;
                case HeroType.KnightDragonSlayer:
                    if (ultimate == HeroUltimates.Battlecry )
                        return string.Empty;
                    break;
                case HeroType.MinstrelBard:
                    if (ultimate == HeroUltimates.BardsSong)
                        return string.Empty;
                    break;
                case HeroType.CrusaderPaladin:
                    if (ultimate == HeroUltimates.HolyStrike || ultimate == HeroUltimates.DivineIntervention)
                        return string.Empty;
                    break;
            }

            return "Your hero cannot use that ability. ";
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
                case GameState.DetailedHeroSelection:
                response = SelectHero();
                    break;
                case GameState.RandomHeroPrompt:
                    response = SelectRandomHero();
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
                case GameState.DetailedHeroSelection:
                response = NextHero(); 
                    break;
                case GameState.RandomHeroPrompt:
                    response = BasicHeroSelection();
                    break;
                default:
                    response = RepeatLastMessage("That was not a valid command. ");
                    break;
            }

            return response;
        }

        public SkillResponse GetDungeonStatus()
        {
            if (GameState == GameState.MainMenu)
                return RepeatLastMessage();
            string message = _dungeon.GetDungeonStatus();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse GetPartyStatus()
        {
            if (GameState == GameState.MainMenu)
                return RepeatLastMessage();
            string message = _hero.GetPartyStatus();
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse GetInventoryStatus()
        {
            if (GameState == GameState.MainMenu)
                return RepeatLastMessage();
            string message = _hero.GetInventoryStatus();
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
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                attributes.Add("hero", JsonConvert.SerializeObject(_hero, settings));
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

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            _hero = JsonConvert.DeserializeObject<Hero>(attributes["hero"].ToString(), settings);
            _dungeon = JsonConvert.DeserializeObject<Dungeon>(attributes["dungeon"].ToString());

        }

        public SkillResponse GetSpecialtyInformation()
        {
            string message = "";
            if (_hero == null)
                return ResponseBuilder.Ask("You haven't selected a hero yet. Say new game to start a new game. ", RepromptBuilder.Create(_lastResponseMessage), Session);


            message = _hero.SpecialtyInformation;
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse GetUltimateInformation()
        {
            string message = "";
            if (_hero == null)
                return ResponseBuilder.Ask("You haven't selected a hero yet. Say new game to start a new game. ", RepromptBuilder.Create(_lastResponseMessage), Session);


            message = _hero.UltimateInformation;
            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse GetItemInformation(IntentRequest request)
        {
            string message = "";
            string selectedItem = request.Intent.Slots["SelectedItem"].Value;

            switch (selectedItem)
            {
                case "vorpal sword":
                    message = "When you use a vorpal sword, it adds a fighter to your party. It gives you one experience point at the end of the game if you still have it in your inventory. ";
                    break;
                case "talisman":
                    message = "When you use a talisman, it adds a cleric to your party. It gives you one experience point at the end of the game if you still have it in your inventory. ";
                    break;
                case "scepter of power":
                    message = "When you use a scepter of power, it adds a mage to your party. It gives you one experience point at the end of the game if you still have it in your inventory. ";
                    break;
                case "thieves tools":
                    message = "When you use thieves tools, it adds a thief to your party. It gives you one experience point at the end of the game if you still have it in your inventory. ";
                    break;
                case "scroll":
                    message = "When you use a scroll treasure item, it adds a scroll to your party. It gives you one experience point at the end of the game if you still have it in your inventory. ";
                    break;
                case "ring of invisibility":
                    message = "When you use a ring of invisibility, all dragon dice from the Dragon's Lair are discarded and you can skip a dragon fight. It gives you one experience point at the end of the game if you still have it in your inventory. ";
                    break;
                case "dragon scales":
                    message = "Dragon scales gives you one experience point at the end of the game. Additionally, for each pair you get two points. So, if you have three dragon scales, you get five points at the end of the game. ";
                    break;
                case "elixir":
                    message = "When you use an elixir, you can revive a party member from the graveyard. It gives you one experience point at the end of the game if you still have it in your inventory. ";
                    break;
                case "dragon bait":
                    message = "When you use dragon bait, all monsters are transformed to dragon dice and moved to the Dragon's Lair. It gives you one experience point at the end of the game if you still have it in your inventory. ";
                    break;
                case "town portal":
                    message = "When you use a town portal, you can retire and end a delve at any phase of the dungeon. It gives you two experience point at the end of the game if you still have it in your inventory. ";
                    break;
                default:
                    message = $"{selectedItem} is not a valid item. Valid items are: vorpal sword, talisman, scepter of power, thieves tools, scroll, ring of invisibility, dragon scales, elixir, dragon bait, and town portal. ";
                    break;
            }

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse GetHelp()
        {
            string message = "";

            switch (GameState)
            {
                case GameState.MainMenu:
                    message = "You are in the main menu. To start a new game, say new game. To listen to the game rules, say rules. Say help at any point in the game to learn about valid commands. ";
                    break;
                case GameState.DetailedHeroSelection:
                    message = "You are in the hero selection phase. If you want to pick the hero that is presented, say yes. If you want to go to the next hero, say no. ";
                    break;
                case GameState.PartyFormation:
                    message = "You are in the party formation phase and about to start a new dungeon delve. Your hero can select dice in your party to reroll before you start. Say select followed by the party die name to select dice. Say party status to get information about your party. ";
                    break;
                case GameState.MonsterPhase:
                    message = "You are in the monster phase and you need to attack monsters to defeat them. Valid commands in this phase are the following: 1. Attack - for example say fighter attack goblin. A single fighter can defeat all goblins, a single mage can defeat all oozes, a single cleric defeats all skeletons, and a champion defeats all monsters of the selected type. 2. Use scroll - scrolls let you reroll any party or dungeon dice. 3. Use item - use a treasure item from your inventory. 4. Flee - if you cannot pass this phase you need to flee and end the delve. 5. Specialty - get information about how you can use your hero specialty. 6. Ultimate ability - get information about how you can use your ultimate ability. 7. Party status - get information about your party. 8. Dungeon status - get information about the current state of the dungeon. 9. Inventory - get information about your inventory. 10. Item information - get information about a specific treasure item. ";
                    break;
                case GameState.LootPhase:
                    message = "In the loot phase you can open chests to receive treasure items, or quaff potions to revive party members from the graveyard. Valid commands in this phase are the following: 1. open chest - a thief or champion can open all chests at once, while other companions can open a single chest when used. 2. Quaff potion - any party member can be used to quaff as many potions that are available. Scrolls can also quaff potions. 3. Use item - use a treasure item from your inventory 4. Next Phase - ignore  remaining loot and continue to  next phase. 5. Specialty - get information about how you can use your hero specialty. 6. Ultimate ability - get information about how you can use your ultimate ability. 7. Party status - get information about your party. 8. Dungeon status - get information about the current state of the dungeon. 9. Inventory - get information about your inventory. 10. Item information - get information about a specific treasure item. ";
                    break;
                case GameState.DragonPhase:
                    message = "In the dragon phase you need to select different companion types to defeat the dragon. Valid commands in this phase are the following: 1. Select - For example, say select fighter, mage, cleric. 2. Clear dice selection - clear dice selection to start over 3. Defeat dragon - defeat dragon using the selected dice. 4. Use item - use a treasure item from your inventory 5. Flee - if you cannot pass this phase you need to flee and end the delve. 6. Specialty - get information about how you can use your hero specialty. 7. Ultimate ability - get information about how you can use your ultimate ability. 8. Party status - get information about your party. 9. Dungeon status - get information about the current state of the dungeon. 10. Inventory - get information about your inventory. 11. Item information - get information about a specific treasure item. ";
                    break;
                case GameState.RegroupPhase:
                    message = "You completed a level in the dungeon. Valid commands in this phase are the following: 1. Seek Glory - start the next level in the dungeon. 2. Retire - End your delve and collect experience points equal to the level number. 3. Specialty - get information about how you can use your hero specialty. 4. Ultimate ability - get information about how you can use your ultimate ability. 5. Party status - get information about your party. 6. Dungeon status - get information about the current state of the dungeon. 7. Inventory - get information about your inventory. 8. Item information - get information about a specific treasure item. ";
                    break;
                case GameState.RevivingCompanions:
                    message = "You need to revive companions from the graveyard. For example, say revive champion. ";
                    break;
                case GameState.StandardDiceSelection:
                    message += "You need to select party or dungeon dice you would like to roll. For example, say select champion, skeleton, goblin. When you are ready, say roll dice, or clear dice selection to start over. Say dungeon status or party status to get information about which dice are available for selection. ";
                    break;
                case GameState.DiceSelectionForCalculatedStrike:
                    message = "You used a hero ability and are required to select dice. Use the select keyword to select dice. Say dungeon status to get information about which dice are available. ";
                    break;
                case GameState.KillAdditionalMonster:
                    message = "You can say defeat additional monster to use the hero specialty, or you can say skip, to ignore  this ability. ";
                    break;
                case GameState.RandomHeroPrompt:
                    message = "Say yes to start a new game with a random hero. Say no to choose a hero. ";
                    break;
                case GameState.BasicHeroSelection:
                    message = "Say a hero's name to start a new game with that hero. Say detailed hero selection to learn more about each hero's abilities. ";
                    break;
            }

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse ReadRules()
        {
            string rules = "The Dungeon lies before you; you’ve assembled your party of hearty adventurers and have a few tricks up your sleeve. How far will you go to seek glory and fame? Will you risk losing everything? In Dungeon Roll your goal is to collect the most experience points by defeating monsters, battling the dragon, and amassing treasure. You select a Hero avatar, such as a Mercenary, Half-Goblin, or Enchantress, which provides you with unique powers. You assemble your party by rolling seven Party Dice, while Alexa, that would be me,  serves as the Dungeon Lord and rolls a number of Dungeon Dice based on how far you have progressed through the dungeon. You use Champion, Fighter, Cleric, Mage, Thief, and Scroll faces on the Party Dice to defeat monsters such as oozes and skeletons, to claim treasure inside chests, and to revive downed companions with potions. All this fighting in the dungeon is certain to attract the attention of the boss: The Dragon! When three or more Dragon faces appear on the Dungeon Dice, the Adventurer must battle the Dragon. Defeating the dragon is a team effort, requiring three different companion types. After three rounds, you add up your experience points and retire to the inn to celebrate your exploits and to plan your next foray into the next deadly dungeon! You can say help at any point during the game to get information about valid commands. Say new game to start a new game. ";

            return ResponseBuilder.Ask(rules, RepromptBuilder.Create(_lastResponseMessage), Session);
        }

        public SkillResponse ChangeLog()
        {
            string message = "Dungeon Roll Beta Version 4 Change log: Added Crusader as a new hero. Say new game to start a new game, say rules for the rules, say help if you need help. ";

            return ResponseBuilder.Ask(message, RepromptBuilder.Create(_lastResponseMessage), Session);
        }
    }
}