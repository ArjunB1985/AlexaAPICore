using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using GameMaker.Implementations;
using GameMaker.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace AlexaAPICore.Controllers
{
   
    [ApiController]
    public class PrescreenController : ControllerBase
    {
        LoggingConfiguration config = new LoggingConfiguration();
        Logger logger = null;
        public PrescreenController()
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

        [HttpPost, Route("api/alexa_elig")]
        public SkillResponse Prescreen(SkillRequest alexaRequestInput)
        {
            SkillResponse response = new SkillResponse();
            response.Version = "1.0";
            logger.Debug("Request:" + JsonConvert.SerializeObject(alexaRequestInput.Request));

            switch (alexaRequestInput.Request.Type)
            {

                case "LaunchRequest":

                    logger.Debug("Launch request in");
                    Helpers.caseInfo = null;
                    response.Response = new ResponseBody();
                    response.Response.Card = new SimpleCard()
                    {
                        //                        Content = "Hello! Enjoy your game while I keep the scores. You can tell me to start a game or ask for the score of your current game.",
                        Content = "Hello!! Welcome to Childcare pre-screening!",

                        Title = "Welcome!!"
                    };
                    response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Hello!! welcome to Childcare pre-screening! You can now say, check my eligibility" };
                    response.Response.ShouldEndSession = false;

                    logger.Debug("Launch request out");
                    break;
                case "IntentRequest":
                    IntentRequest intentRequest = (IntentRequest)alexaRequestInput.Request;
                    if (intentRequest.Intent.Name == "PreScreen")
                    {
                        if (intentRequest.Intent.Slots["expense"].Value != null)
                        {
                            //mandatory feilds done.
                            Helpers.LoadModel(intentRequest);
                            int finalize = 0;
                            if (Helpers.caseInfo.AdditionalQuestions == null)
                            {
                                Helpers.caseInfo.AdditionalQuestions = new List<AdditionalQuestions>();
                            }
                            if (Helpers.caseInfo.FatherAge < 20 && !Helpers.caseInfo.FatherWorking && !Helpers.caseInfo.AdditionalQuestions.Exists(p => p.SlotName == "father_in_school"))
                            {

                                Helpers.caseInfo.AdditionalQuestions.Add(new AdditionalQuestions() { SlotName = "father_in_school" });
                                Helpers.caseInfo.AdditionalQuestions.First(p => p.SlotName == "father_in_school").Status = true;
                                response.Response = new ResponseBody();
                                response.Response = Helpers.GetPlainTextResponseBody("You indicated father of kid is teenager, is " + Helpers.caseInfo.ChildName + "'s" + " father in school?", false);
                                response.Response.Directives.Add(new DialogElicitSlot("father_in_school"));
                                response.Response.ShouldEndSession = false;
                                return response;
                            }
                            else
                            {
                                finalize++;

                            }
                            if (Helpers.caseInfo != null && Helpers.caseInfo.MotherAge < 20 && !Helpers.caseInfo.MotherWorking && !Helpers.caseInfo.AdditionalQuestions.Exists(p => p.SlotName == "mother_in_school"))
                            {

                                Helpers.caseInfo.AdditionalQuestions.Add(new AdditionalQuestions() { SlotName = "mother_in_school" });
                                Helpers.caseInfo.AdditionalQuestions.First(p => p.SlotName == "mother_in_school").Status = true;
                                response.Response = new ResponseBody();
                                response.Response = Helpers.GetPlainTextResponseBody("You indicated mother of kid is teenager, is " + Helpers.caseInfo.ChildName + "'s" + " mother in school?", false);
                                response.Response.Directives.Add(new DialogElicitSlot("mother_in_school"));
                                response.Response.ShouldEndSession = false;
                                return response;
                            }
                            else
                            {
                                if (finalize == 1)
                                {
                                    Helpers.caseInfo.ModelComplete = true;
                                }
                            }
                            //load model
                            //if (Helpers.caseInfo == null)
                            //{

                            //}
                            //else
                            //{ // add additional params

                            //    int i = 0;

                            //    if(intentRequest.Intent.Slots["father_in_school"].Value!=null)
                            //    {
                            //        Helpers.caseInfo.FatherInSchool = intentRequest.Intent.Slots["father_in_school"].Value.ToLower() == "yes";
                            //        i++;
                            //    }
                            //    if (intentRequest.Intent.Slots["mother_in_school"].Value != null)
                            //    {
                            //        Helpers.caseInfo.MotherInSchool = intentRequest.Intent.Slots["mother_in_school"].Value.ToLower() == "yes";
                            //        i++;
                            //    }
                            //    if (i == 2)
                            //    {
                            //        Helpers.caseInfo.ModelComplete = true;
                            //    }

                            //}
                            if (Helpers.caseInfo.ModelComplete)
                            {
                                //all needed data captured
                                var result = Helpers.EvaluateEligibility();
                                if (result.Pass)
                                {
                                    response.Response = Helpers.GetPlainTextResponseBody("Congratulations! based on the data you provided you may be eligible for child care. Visit an office to apply!", true, "Prescreening Result", "Potentially eligible.");
                                    response.Response.ShouldEndSession = true;
                                }
                                else
                                {
                                    response.Response = Helpers.GetPlainTextResponseBody("Sorry, based on the data you provided you don't seem to be eligible for child care. " + result.FailReason + " However you can always visit an office to talk to a worker.", true, "Prescreening Result", "Ineligible due to: " + result.FailReason);
                                    response.Response.ShouldEndSession = true;
                                }
                                Helpers.caseInfo = null;
                            }
                           

                        }
                        else
                        {
                            //delegate
                            response.Response = new ResponseBody();
                            response.Response.Directives.Add(new DialogDelegate());
                            response.Response.ShouldEndSession = false;
                        }
                    }

                    break;
            }
            logger.Debug("Request:" + JsonConvert.SerializeObject(response.Response));

            return response;
        }
    }
}