﻿using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using GameMaker.Interfaces;
using GameMaker.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaker.IntentHandlers
{
    public class AMAZON_FallbackIntent : IIntentHandler
    {
        public SkillResponse HandleIntent(IGameController gameMaker, IntentRequest intentRequest, string userId, string sessionId, Logger logger)
        {
            
            SkillResponse response = new SkillResponse();
            response.Version = "1.0";
            try
            {
                response.Response = Helpers.GetPlainTextResponseBody("Sorry, I did not get that, please try again!", true, "Please try again.");
                response.Response.ShouldEndSession = true;
            }catch(Exception e)
            {
                logger.Debug(e.StackTrace);
                response.Response = Helpers.GetPlainTextResponseBody("Sorry I encountered an error! Please try again", true, "Please try again");
            }
            return response;
        }
    }
}
