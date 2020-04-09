using Alexa.NET.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonRollAlexa.Extensions
{
    public static class RepromptBuilder
    {
        public static Reprompt Create(string text) => Create(new SsmlOutputSpeech{ Ssml = $"<speak> {text} </speak>" });

        public static Reprompt Create(IOutputSpeech speech) => new Reprompt { OutputSpeech = speech };
    }
}
