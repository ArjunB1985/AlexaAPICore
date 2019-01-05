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
            response.Version = "1.0";
            //start logging
            try
            {
                logger.Debug("Request:" +JsonConvert.SerializeObject(alexaRequestInput.Request));
                
                string userId = alexaRequestInput.Context.System.User.UserId;
                string sessionId = alexaRequestInput.Session.SessionId;
               
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
                       
                        //switch (intentRequest.Intent.Name)
                        //{
                            var type = Type.GetType("GameMaker.IntentHandlers." + intentRequest.Intent.Name.Replace('.','_') +", GameMaker, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null");
                        IIntentHandler handler = (IIntentHandler)Activator.CreateInstance(type);
                        response= handler.HandleIntent(gameMaker, intentRequest, userId, sessionId, logger);
                        break;
                    //case "PlayerCountIntent":

                    //            logger.Debug("player count request in");

                    //            handler = new PlayerCountIntent();
                    //            return handler.HandleIntent(gameMaker,intentRequest,userId,sessionId,logger);

                    //        case "SetPlayerNameIntent":
                    //            handler = new SetPlayerNameIntent();
                    //            return handler.HandleIntent(gameMaker, intentRequest, userId, sessionId, logger);

                    //        case "GetScoreIntent":
                    //            logger.Debug("in score intent");
                    //            handler = new GetScoreIntent();
                    //            return handler.HandleIntent(gameMaker, intentRequest, userId, sessionId, logger);
                    //            //say score
                        
                    //        case "playintent":
                               
                    //            break;
                    //    }
                       
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