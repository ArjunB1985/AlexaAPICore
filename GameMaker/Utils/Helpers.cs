using Alexa.NET.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaker.Utils
{
    public static class Helpers
    {
        public static string PlayerNameToSlotName(string playerName)
        {
            string number = playerName.Trim().Substring(7);
            switch (number)
            {
                case "1":
                    return "playerone";
                case "2":
                    return "playertwo";
                case "3":
                    return "playerthree";
                case "4":
                    return "playerfour";
                case "5":
                    return "playerfive";
                case "6":
                    return "playersix";
                case "7":
                    return "playerseven";
                case "8":
                    return "playereight";
                case "9":
                    return "playernine";
                case "10":
                    return "playerten";
                default:
                    throw new Exception("Invalid player name");
            }
        }

        public static ResponseBody GetPlainTextResponseBody( string text,bool needCard, string title=null, string cardText=null)
        {
            ResponseBody body = new ResponseBody();
            if (needCard)
            {
                if (cardText == null)
                {
                    cardText = text;
                }
                body.Card = new SimpleCard()
                {
                    Content = cardText,
                    Title ="title"
                };
            }

            body.OutputSpeech = new PlainTextOutputSpeech() { Text = text };
            return body;

        }
    }
}
