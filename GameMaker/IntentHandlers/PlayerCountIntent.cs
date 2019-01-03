using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using GameMaker.Interfaces;
using GameMaker.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaker.IntentHandlers
{
    public class PlayerCountIntent : IIntentHandler
    {
        public SkillResponse HandleIntent(IGameController gameMaker, IntentRequest intentRequest, String userId, string sessionId, Logger logger)
        {
            SkillResponse response = new SkillResponse();
            try {
               
                if (intentRequest.DialogState == DialogState.Completed)
                {
                    //ask if players need to be named
                    logger.Debug("in dialog complete");
                    var x = gameMaker.StartGame(userId, sessionId, Int32.Parse(intentRequest.Intent.Slots["players"].Value), intentRequest.Intent.Slots["gametype"].Resolution.Authorities[0].Values[0].Value.Id);
                    response.Response=Helpers.GetPlainTextResponseBody( "Game created for " + intentRequest.Intent.Slots["players"].Value + " players", true, "Game Created");
                    response.Response.ShouldEndSession = true;
                    logger.Debug("out dialog complete");
                }
                else
                {

                    logger.Debug("in dialog incomplete");
                    //keep delegating until all slots are full

                    if (intentRequest.Intent.ConfirmationStatus == "DENIED")
                    {//if completed but not confirmed. cancel the game creation.
                        response.Response = Helpers.
                            GetPlainTextResponseBody( "No worries! Game creation cancelled! Please try again.", true, "Game Creation Cancelled", "Game creation cancelled! Please try again.");
                        response.Response.ShouldEndSession = true;
                        
                    }
                    else
                    {
                        response.Response.Card = new SimpleCard()
                        {
                            Content = "Number of players and who wins the game, high scorer or low?",
                            Title = "Game Details"
                        };
                        response.Response.Directives.Add(new DialogDelegate() { UpdatedIntent = intentRequest.Intent });
                        response.Response.ShouldEndSession = false;

                    }
                    logger.Debug("out dialog incomplete");
                }
            }
            catch ( Exception e)
            {
                logger.Debug(e.StackTrace);
                response.Response= Helpers.GetPlainTextResponseBody("Sorry I encountered an error! Please try again", true, "Please try again");
            }
            return response;
        }
    }
}
