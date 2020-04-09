using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;

namespace DungeonRollAlexa.Extensions
{
    public static class ResponseCreator
    {
        public static SkillResponse Ask(string message, Reprompt reprompt, Session session)
        {
            // converts plain message into ssmls
            var speech = new SsmlOutputSpeech();
            speech.Ssml = $"<speak> {message} </speak>";
            
            return ResponseBuilder.Ask(speech, reprompt, session);
        }
    }
}
