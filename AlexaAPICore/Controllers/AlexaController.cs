using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using GameMaker.Implementations;
using GameMaker.Interfaces;
using Alexa.NET.Response.Directive;
using GameMaker.Utils;
using NLog.Config;
using NLog.Targets;
using NLog;
using Newtonsoft.Json;
using GameMaker.IntentHandlers;

namespace AlexaAPICore.Controllers
{
   
    [ApiController]
    public class AlexaController : ControllerBase

    {
        LoggingConfiguration config = new LoggingConfiguration();
        Logger logger = null;
        public AlexaController()
        {
            
            var fileTarget = new FileTarget("target2")
            {
                FileName = "${basedir}/file.txt",
                Layout = "${longdate} ${level} ${message}  ${exception}"
            };
            config.AddTarget(fileTarget);
            config.AddRuleForOneLevel(LogLevel.Debug, fileTarget); // only errors to file
            LogManager.Configuration = config;
            logger = LogManager.GetLogger("AlexaAPILog");
           

        }
        [HttpPost, Route("api/alexa")]
        public SkillResponse EntryPoint(SkillRequest alexaRequestInput)
        {
            SkillResponse response = new SkillResponse();
            //start logging
            try
            {
                logger.Debug("Request:" +JsonConvert.SerializeObject(alexaRequestInput.Request));
                
                string userId = alexaRequestInput.Context.System.User.UserId;
                string sessionId = alexaRequestInput.Session.SessionId;
                response.Version = "1.0";
                //get last active game for user
                IGameController gameMaker = new ScoreKeepingGameController();
                List<Player> players = gameMaker.GetGameScore(alexaRequestInput.Context.System.User.UserId, null);
                switch (alexaRequestInput.Request.Type)
                {

                    case "LaunchRequest":

                        logger.Debug("Launch request in");
                       response.Response = new ResponseBody();
                        response.Response.Card = new SimpleCard()
                        {
                            //                        Content = "Hello! Enjoy your game while I keep the scores. You can tell me to start a game or ask for the score of your current game.",
                            Content = "Start a game?",

                            Title = "Welcome!!"
                        };
                        response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Start a game or get score?" };
                        response.Response.ShouldEndSession = false;

                        logger.Debug("Launch request out");
                        break;

                    case "IntentRequest":

                        logger.Debug("intent request in");
                        IntentRequest intentRequest = (IntentRequest)(alexaRequestInput.Request);
                        switch (intentRequest.Intent.Name)
                        {
                            case "playercountintent":

                                logger.Debug("player count request in");

                                PlayerCountIntent intent = new PlayerCountIntent();
                                return intent.HandleIntent(gameMaker,intentRequest,userId,sessionId,logger);

                            case "setplayernameintent":
                                if (players == null)
                                {
                                    response.Response = new ResponseBody();
                                    response.Response.Card = new SimpleCard()
                                    {
                                        Content = "Sorry, no active game found",
                                        Title = "Game not found!"
                                    };

                                    response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Sorry, no active game found" };
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
                                        if (p.PlayerState == PlayerState.Active && p.PlayerAlias==p.PlayerName)
                                        {
                                            playerToRename = p;
                                            break;

                                        }
                                    }
                                    if (playerToRename != null)
                                    {
                                        string slotName = Helpers.PlayerNameToSlotName(playerToRename.PlayerName);
                                        response.Response = new ResponseBody();
                                        response.Response.Card = new SimpleCard()
                                        {
                                            Content = "Tell me the name for " + playerToRename.PlayerName,
                                            Title = "Rename players!"
                                        };
                                        response.Response.Directives.Add(new DialogElicitSlot(slotName));
                                        response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Tell me the name of " + playerToRename.PlayerName };
                                        response.Response.ShouldEndSession = false;
                                    }
                                    else
                                    {
                                        response.Response = new ResponseBody();
                                        response.Response.Card = new SimpleCard()
                                        {
                                            Content = "Player names are set.",
                                            Title = "Player names set!"
                                        };

                                        response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Player names set. You can now add scores as you play." };
                                        response.Response.ShouldEndSession = true;
                                    }

                                }
                                break;
                            case "scoreintent":
                                logger.Debug("in score intent");
                                if (players == null)
                                {
                                    response.Response = new ResponseBody();
                                    response.Response.Card = new SimpleCard()
                                    {
                                        Content = "Sorry, no active game found",
                                        Title = "Game not found!"
                                    };

                                    response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Sorry, no active game found" };
                                    response.Response.ShouldEndSession = true;
                                    //say no game active
                                }
                                else
                                {
                                    //make score statement
                                    string statement = string.Empty;
                                    foreach (var p in players)
                                    {
                                        statement += p.PlayerName + ": " + p.PlayerScore + ". ";
                                    }
                                    response.Response = new ResponseBody();
                                    response.Response.Card = new SimpleCard()
                                    {
                                        Content = "Your current game is a " + players.Count + " player game. Here are the scores. " + statement
                                        ,
                                        Title = "Scores!"
                                    };

                                    response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Your current game is a " + players.Count + " player game. Here are the scores. " + statement };
                                    response.Response.ShouldEndSession = true;
                                    //say score
                                }
                                break;
                            case "playintent":
                                logger.Debug("in play intent");
                                if (intentRequest.Intent.Slots.Count != 1)
                                { //error
                                }
                                else
                                {
                                    response.Response = new ResponseBody();
                                    response.Response.Card = new SimpleCard()
                                    {
                                        Content = "I heard " + intentRequest.Intent.Slots["players"].Value,
                                        Title = "Players"
                                    };
                                    response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "I heard " + intentRequest.Intent.Slots["players"].Value };

                                }

                                break;
                        }
                        break;
                    case "SessionEndedRequest":
                        response.Response = new ResponseBody();
                        response.Response.Card = new SimpleCard()
                        {
                            Content = "Thanks! You can now add scores!",
                            Title = "Players"
                        };
                        response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Thanks! You can now add scores" };

                        break;
                    case "CanFulfillIntentRequest":
                        logger.Debug("in can fulfill"); break;
                    case "FallbackIntent":
                        response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Didn't catch that. Please try again." };
                        break;
                    default:

                        break;
                        //error

                }
                logger.Debug("Response: "+JsonConvert.SerializeObject(response.Response));
                return response;
            }
            catch (Exception e)
            {
                logger.Debug(e.StackTrace);
                response.Response = new ResponseBody();
                response.Response.Card = new SimpleCard()
                {
                    Content = e.Message,
                    Title = "Players"
                };
                response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text=e.Message};

                return response;
            }
        }
    }
}