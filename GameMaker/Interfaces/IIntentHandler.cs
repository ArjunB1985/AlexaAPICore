using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaker.Interfaces
{
    public interface IIntentHandler
    {
        SkillResponse HandleIntent(IGameController gameMaker, IntentRequest intentRequest, String userId, string sessionId, Logger logger);

    }
}
