using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using AlexaApiCoreLibs.Validators;
using GameMaker.Implementations;
using GameMaker.IntentHandlers;
using GameMaker.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace AlexaAPICore.Controllers
{
    
    [ApiController]
    public class BenefindController : ControllerBase
    {
        LoggingConfiguration config = new LoggingConfiguration();
        Logger logger = null;
   

        public BenefindController()
        {
            var fileTarget = new FileTarget("target2")
            {
                FileName = "${basedir}/file.txt",
                Layout = "${longdate} ${level} ${message}  ${exception}"
            };
            config.AddTarget(fileTarget);
            config.AddRuleForOneLevel(NLog.LogLevel.Debug, fileTarget); // only errors to file
            
            LogManager.Configuration = config;
            logger = LogManager.GetLogger("AlexaAPILog");
        }
        [HttpPost, Route("api/benefind_demo")]
        public SkillResponse HandleResponse(SkillRequest alexaRequestInput)
        {
            AlexaRequestValidationService validator = new AlexaRequestValidationService();
            SpeechletRequestValidationResult validationResult = validator.ValidateAlexaRequest(alexaRequestInput);
            
            if (validationResult != SpeechletRequestValidationResult.OK)
            {
                logger.Debug("validation error: " + validationResult.ToString());
                new Exception("Invalid Request");
            }
            SkillResponse response = new SkillResponse();
            response.Version = "1.0";
            logger.Debug("Request:" + JsonConvert.SerializeObject(alexaRequestInput.Request));
            CaseInfo caseInfo = Helpers.GetCaseInfo(alexaRequestInput.Context.System.User.UserId);
            if (caseInfo.profile == null)
            {
                caseInfo.profile = Helpers.GetUserProfile(alexaRequestInput);
            }
            switch (alexaRequestInput.Request.Type)
            {

                case "LaunchRequest":

                    logger.Debug("Launch request in");

                    response.Response = new ResponseBody();
                    response.Response.Card = new SimpleCard()
                    {
                        Content = "Hello "+ caseInfo.profile.name + ". Welcome to your Benefind dashboard. Say: \"Check case summary\", \"Check my pending documents\" or \"Schedule an appointment\"",

                        Title = "Benifind Dashboard"
                    };
                    response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Hello "+ caseInfo.profile.name + ". Welcome to your Beni-find dashboard. You don't have any new notifications. You can now say, Check case summary, or  Check my pending documents, Or say Schedule an appointment  \"" };
                    //response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Hello!! say, \"I am ready!\"" };

                    response.Response.ShouldEndSession = false;

                    logger.Debug("Launch request out");
                    break;
                case "SessionEndedRequest":
                    response.Response = new ResponseBody();
                    response.Response.Card = new SimpleCard()
                    {
                        Content = "Goodbye, have a good day!",

                        Title = "Welcome!!"
                    };
                    response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Goodbye, have a good day!" };
                    response.Response.ShouldEndSession = true;
                    return response;
                case "IntentRequest":
                    try
                    {
                        IntentRequest intentRequest = (IntentRequest)(alexaRequestInput.Request);
                        if (intentRequest.Intent.Name == "casesummary")
                        {
                            string text = "Hello <Name>, I see your SNAP case is active and ongoing. The SNAP case will be up for renewal for March 31st 2019. Your child care case is pending for an RFI. Please say \"Check my RFI\" to know more.";
                            response.Response = Helpers.GetPlainTextResponseBody(text.Replace("<Name>", caseInfo.profile.name), true, "Case Summary");
                            response.Response.ShouldEndSession = false; ;
                        }
                        if (intentRequest.Intent.Name == "rfi")
                        {

                            string text = "Hello <Name>, you have a household composition RFI, due by January 31st 2019. You can upload documents on our self service portal or return documents to our offices. P.A.F.S 76 is a supported document for this RFI.";
                            response.Response = Helpers.GetPlainTextResponseBody(text.Replace("<Name>", caseInfo.profile.name), true, "RFI Details");
                            response.Response.ShouldEndSession = false; ;
                        }
                        if (intentRequest.Intent.Name == "schedule")
                        {
                            string text = "";
                            if (intentRequest.Intent.ConfirmationStatus == "CONFIRMED")
                            {


                                var date = intentRequest.Intent.Slots["date"].Value;
                                var time = intentRequest.Intent.Slots["time"].Value;


                                text = "All set, your appointment is scheduled for the selected time. I have also sent you this information on your email.";
                                Helpers.SendEmailAsync(caseInfo.profile.email, caseInfo.profile.name, date, time);

                            }
                            else
                            {
                                text = "Ok, Roger that!! Request cancelled!";
                            }
                            response.Response = Helpers.GetPlainTextResponseBody(text.Replace("<Name>", caseInfo.profile.name), true, "Appointments");
                            response.Response.ShouldEndSession = true; ;

                        }

                        if (intentRequest.Intent.Name == "AMAZON.StopIntent")
                        {
                            var stophandler = new AMAZON_StopIntent();
                            var skillresponse = stophandler.HandleIntent(null, null, null, null, logger);
                            skillresponse.Version = "1.0";
                            return skillresponse;
                        }
                        if (intentRequest.Intent.Name == "AMAZON.FallbackIntent")
                        {
                            var fallbackhandler = new AMAZON_FallbackIntent();
                            var fallbackresponse = fallbackhandler.HandleIntent(null, null, null, null, logger);
                            fallbackresponse.Version = "1.0";
                            return fallbackresponse;
                        }
                        if (intentRequest.Intent.Name == "AMAZON.CancelIntent")
                        {
                            var cancelhandler = new AMAZON_CancelIntent();
                            var cancellresponse = cancelhandler.HandleIntent(null, null, null, null, logger);
                            cancellresponse.Version = "1.0";
                            return cancellresponse;
                        }
                        //if (intentRequest.Intent.Name == "AMAZON.HelpIntent")
                        //{
                        //    var helphandler = new AMAZON_HelpIntent();
                        //    var helplresponse = helphandler.HandleIntent(null, null, null, null, logger);
                        //    helplresponse.Version = "1.0";
                        //    helplresponse.Response.ShouldEndSession = false;
                        //    return helplresponse;
                        //}
                        break;
                    }
                    catch (Exception e)
                    {

                        response.Response = Helpers.GetPlainTextResponseBody("Aaargh, the application encountered an error. Please try again later. Sorry for the inconvenience", true, "Error", e.Message);
                        response.Response.ShouldEndSession = true;
                        logger.Debug(e.StackTrace);
                    }
                    break;


            }
            logger.Debug("Response:" + JsonConvert.SerializeObject(response.Response));

            return response;
        }

       
    }
}