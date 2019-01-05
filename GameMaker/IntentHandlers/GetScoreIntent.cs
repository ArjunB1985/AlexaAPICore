using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using GameMaker.Implementations;
using GameMaker.Interfaces;
using GameMaker.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaker.IntentHandlers
{
    public class GetScoreIntent : IIntentHandler
    {
        public SkillResponse HandleIntent(IGameController gameMaker, IntentRequest intentRequest, string userId, string sessionId, Logger logger)
        {
            SkillResponse response = new SkillResponse();
            response.Version = "1.0";
            try
            {
                List<Player> players = gameMaker.GetGameScore(userId, null);
                if (players == null)
                {
                    response.Response = Helpers.GetPlainTextResponseBody("Sorry, no active game found", true, "Game score", "Sorry, no active game found");
                    response.Response.ShouldEndSession = true;
                    //say no game active
                }
                else
                {
                    //make score statement
                    string statement = string.Empty;
                    foreach (var p in players)
                    {
                        string aliastext=(p.PlayerAlias != p.PlayerName) ? " is " + p.PlayerAlias:"" ;
                        statement += p.PlayerName + aliastext + " on " + p.PlayerScore + ". ";
                    }
                    string content = "Your current game is a " + players.Count + " player game. Here are the scores. " + statement;
                    response.Response = Helpers.GetPlainTextResponseBody(content, true, "Game score");

                   
                }
            }
            catch (Exception e)
            {
                logger.Debug(e.StackTrace);
                response.Response = Helpers.GetPlainTextResponseBody("Sorry I encountered an error! Please try again", true, "Please try again");
            }
            return response;
        }
    }
}
