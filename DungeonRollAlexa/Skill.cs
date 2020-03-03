using Alexa.NET;
using Alexa.NET.LocaleSpeech;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonRollAlexa.Extensions;
using DungeonRollAlexa.Main;
using DungeonRollAlexa.Main.GameObjects;

namespace DungeonRollAlexa
{
    public static class Skill
    {
        private static GameSession _gameSession;

        [FunctionName("DungeonRollAlexa")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            var json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

            // Verifies that the request is indeed coming from Alexa.
            var isValid = await skillRequest.ValidateRequestAsync(req, log);
            if (!isValid)
            {
                return new BadRequestResult();
            }

            // setup game session
            _gameSession = new GameSession(skillRequest.Session);

            // Setup language resources.
            var store = SetupLanguageResources();
            var locale = skillRequest.CreateLocale(store);

            var request = skillRequest.Request;
            SkillResponse response = null;

            try
            {
                if (request is LaunchRequest launchRequest)
                {
                    log.LogInformation("Session started");

                    var welcomeMessage = await locale.Get(LanguageKeys.Welcome, null);
                    var welcomeRepromptMessage = await locale.Get(LanguageKeys.WelcomeReprompt, null);
                    response = ResponseBuilder.Ask(welcomeMessage, RepromptBuilder.Create(welcomeRepromptMessage), _gameSession.Session);
                }
                else if (request is IntentRequest intentRequest)
                {
                    // Checks whether to handle system messages defined by Amazon.
                    var systemIntentResponse = await HandleSystemIntentsAsync(intentRequest, locale);
                    if (systemIntentResponse.IsHandled)
                    {
                        response = systemIntentResponse.Response;
                    }
                    else
                    {
                        // Processes request according to intentRequest.Intent.Name...
                        response = HandleUserIntents(intentRequest);
                    }
                }
                else if (request is SessionEndedRequest sessionEndedRequest)
                {
                    log.LogInformation("Session ended");
                    response = ResponseBuilder.Empty();
                }
            }
            catch(Exception ex)
            {
                var message = await locale.Get(LanguageKeys.Error, null);
                response = ResponseBuilder.Tell(ex.Message);
                response.Response.ShouldEndSession = false;
            }

            return new OkObjectResult(response);
        }

        private static async Task<(bool IsHandled, SkillResponse Response)> HandleSystemIntentsAsync(IntentRequest request, ILocaleSpeech locale)
        {
            SkillResponse response = null;

            switch (request.Intent.Name)
            {
                case BuiltInIntent.Fallback:
                    response = _gameSession.RepeatLastMessage("That is not a valid action. ");
                    break;
                case BuiltInIntent.Repeat:
                    response = _gameSession.RepeatLastMessage();
                    break;
                case BuiltInIntent.Yes:
                    response = _gameSession.ResolveYesIntent();
                    break;
                case BuiltInIntent.No:
                    response = _gameSession.ResolveNoIntent();
                    break;
                case BuiltInIntent.Cancel:
                    {
                        var message = await locale.Get(LanguageKeys.Cancel, null);
                        response = ResponseBuilder.Tell(message);
                        break;
                    }

                case BuiltInIntent.Help:
                    {
                        var message = await locale.Get(LanguageKeys.Help, null);
                        response = ResponseBuilder.Ask(message, RepromptBuilder.Create(message));
                        break;
                    }

                case BuiltInIntent.Stop:
                    {
                        var message = await locale.Get(LanguageKeys.Stop, null);
                        response = ResponseBuilder.Tell(message);
                        break;
                    }
            }

            return (response != null, response);
        }
        
        private static SkillResponse HandleUserIntents(IntentRequest request)
        {
            SkillResponse response = null;
            
            switch (request.Intent.Name)
            {
                case "NewGameIntent":
                    //start a new game
                    response = _gameSession.SetupNewGame();
                    break;
                case "ScrollDieIntent":
                    response = _gameSession.UseScroll();
                    break;
                    case "SelectDiceIntent":
                    response = _gameSession.SelectDice(request);
                    break;
                case "ClearDiceSelectionIntent":
                    response = _gameSession.ClearDiceSelection();
                    break;
                case "RollSelectedDiceIntent":
                    response = _gameSession.RollSelectedDice();
                    break;
                case "AttackMonsterIntent":
                    response = _gameSession.AttackMonster(request);
                    break;
                case "DefeatAdditionalMonsterIntent":
                    response = _gameSession.DefeatAdditionalMonster(request);
                    break;
                case "OpenChestIntent":
                    response = _gameSession.OpenChestAction(request);
                    break;
                case "DrinkPotionIntent":
                    response = _gameSession.DrinkPotions(request);
                    break;
                case "ReviveCompanionIntent":
                    response = _gameSession.ReviveCompanion(request);
                    break;
                case "DefeatDragonIntent":
                    response = _gameSession.DefeatDragon();
                    break;
                case "UseTreasureItemIntent":
                    response = _gameSession.UseTreasureItem(request);
                    break;
                case "TransformCompanionIntent":
                    response = _gameSession.TransformCompanion(request);
                    break;
                case "SeekGloryIntent":
                    response = _gameSession.SeekGlory();
                    break;
                case "NextPhaseIntent":
                    response = _gameSession.GoToNextPhase();
                    break;
                case "FleeIntent":
                    response = _gameSession.FleeDungeon();
                    break;
                case "RetireIntent":
                    response = _gameSession.RetireDelve();
                    break;
                case "PartyStatusIntent":
                    response = _gameSession.GetPartyStatus();
                    break;
                case "DungeonStatusIntent":
                    response = _gameSession.GetDungeonStatus();
                    break;
                case "InventoryIntent":
                    response = _gameSession.GetInventoryStatus();
                    break;
                // ultimate ability intents
                case "ArcaneBladeIntent":
                    response = _gameSession.ActivateUltimate(HeroUltimates.ArcaneBlade, request);
                    break;
                case "ArcaneFuryIntent":
                    response = _gameSession.ActivateUltimate(HeroUltimates.ArcaneFury, request);
                    break;
                case "CalculatedStrikeIntent":
                    if (_gameSession.GameState != GameState.DiceSelectionForCalculatedStrike)
                        response = _gameSession.ActivateUltimate(HeroUltimates.CalculatedStrike, request);
                    else
                        response = _gameSession.PerformCalculatedStrike();
                    break;
                case "BattlefieldPresenceIntent":
                    response = _gameSession.ActivateUltimate(HeroUltimates.BattlefieldPresence, request);
                    break;
            }

            return response;
        }

        private static DictionaryLocaleSpeechStore SetupLanguageResources()
        {
            // Creates the locale speech store for each supported languages.
            var store = new DictionaryLocaleSpeechStore();

            store.AddLanguage("en", new Dictionary<string, object>
            {
                [LanguageKeys.Welcome] = "Welcome to the skill!",
                [LanguageKeys.WelcomeReprompt] = "You can ask help if you need instructions on how to interact with the skill",
                [LanguageKeys.Response] = "This is just a sample answer",
                [LanguageKeys.Cancel] = "Canceling...",
                [LanguageKeys.Help] = "Help...",
                [LanguageKeys.Stop] = "Bye bye!",
                [LanguageKeys.Error] = "I'm sorry, there was an unexpected error. Please, try again later."
            });

            store.AddLanguage("it", new Dictionary<string, object>
            {
                [LanguageKeys.Welcome] = "Benvenuto nella skill!",
                [LanguageKeys.WelcomeReprompt] = "Se vuoi informazioni sulle mie funzionalit�, prova a chiedermi aiuto",
                [LanguageKeys.Response] = "Questa � solo una risposta di prova",
                [LanguageKeys.Cancel] = "Sto annullando...",
                [LanguageKeys.Help] = "Aiuto...",
                [LanguageKeys.Stop] = "A presto!",
                [LanguageKeys.Error] = "Mi dispiace, si � verificato un errore imprevisto. Per favore, riprova di nuovo in seguito."
            });

            return store;
        }
    }
}
