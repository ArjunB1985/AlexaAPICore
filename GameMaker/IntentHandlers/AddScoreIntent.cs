using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using GameMaker.Implementations;
using GameMaker.Interfaces;
using GameMaker.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaker.IntentHandlers
{
    class AddScoreIntent : IIntentHandler
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
                    response.Response = Helpers.GetPlainTextResponseBody("Sorry, no active game found. Please create a game first", true, "Sorry, no active game found");
                    response.Response.ShouldEndSession = true;
                    //say no game active
                }
                else
                {
                    //if a score is passed
                    if (intentRequest.Intent.Slots["score"].Value != null)
                    {
                        foreach (var p in players)
                        {
                            if (p.PlayerBookmark == 'P')
                            {
                                //save it
                                p.PlayerScoreUncommited = Int32.Parse(intentRequest.Intent.Slots["score"].Value);
                                p.PlayerBookmark = 'C';//completed
                                gameMaker.SetGameScore(new List<Player>() { p }, userId, sessionId);
                            }
                        }
                    }

                    Player playerToSetScore = null;
                    //find next player to name
                    foreach (var p in players)
                    {
                        if (p.PlayerState == PlayerState.Active && (p.PlayerBookmark== char.MinValue||p.PlayerBookmark=='P' )) // handle pending from last session
                        {
                            playerToSetScore = p;
                            p.PlayerBookmark = 'P';//picked
                            gameMaker.SetGameScore(new List<Player>() { p }, userId, sessionId); // just update the flag
                            break;

                        }
                    }
                    if (playerToSetScore != null)
                    {
                        response.Response = Helpers.GetPlainTextResponseBody("Tell me the score for " + playerToSetScore.PlayerAlias, true, "Add scores");
                        response.Response.Directives.Add(new DialogElicitSlot("score"));
                        response.Response.ShouldEndSession = false;
                    }
                    else
                    {
                        //no more players to get score of now update scores
                        response.Response = Helpers.GetPlainTextResponseBody("Player scores added.", true, "Add scores");
                        foreach(var p in players)
                        {
                            p.PlayerScore += p.PlayerScoreUncommited;
                            p.PlayerBookmark = char.MinValue;
                        }
                        gameMaker.SetGameScore(players, userId, sessionId);
                        response.Response.ShouldEndSession = true;
                    }

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
