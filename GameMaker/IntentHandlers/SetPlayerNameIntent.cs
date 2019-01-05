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
    public class SetPlayerNameIntent : IIntentHandler
    {
        public SkillResponse HandleIntent(IGameController gameMaker, IntentRequest intentRequest, string userId, string sessionId, Logger logger)
        {
            SkillResponse response = new SkillResponse();
            response.Version = "1.0";

            try {
                List<Player> players = gameMaker.GetGameScore(userId, null);
                if (players == null)
                {
                    response.Response = Helpers.GetPlainTextResponseBody("Sorry, no active game found", true, "Sorry, no active game found");
                    response.Response.ShouldEndSession = true;
                    //say no game active
                }
                else
                {
                    //first check the request and save any updates
                    foreach (var p in players)
                    {
                        if (intentRequest.Intent.Slots[Helpers.PlayerNameToSlotName(p.PlayerName)].Value != null && p.PlayerAlias != intentRequest.Intent.Slots[Helpers.PlayerNameToSlotName(p.PlayerName)].Value)
                        {
                            //save it
                            p.PlayerAlias = intentRequest.Intent.Slots[Helpers.PlayerNameToSlotName(p.PlayerName)].Value;
                            gameMaker.SetPlayerAlias(userId, sessionId, p);
                        }
                    }


                    Player playerToRename = null;
                    //find next player to name
                    foreach (var p in players)
                    {
                        if (p.PlayerState == PlayerState.Active && p.PlayerAlias == p.PlayerName)
                        {
                            playerToRename = p;
                            break;

                        }
                    }
                    if (playerToRename != null)
                    {
                        string slotName = Helpers.PlayerNameToSlotName(playerToRename.PlayerName);
                        response.Response = Helpers.GetPlainTextResponseBody("Tell me the name for " + playerToRename.PlayerName, true, "Set player names");
                        response.Response.Directives.Add(new DialogElicitSlot(slotName));
                        response.Response.ShouldEndSession = false;
                    }
                    else
                    {
                        response.Response = Helpers.GetPlainTextResponseBody("Player names are set.", true, "Set player names");
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
    } }
