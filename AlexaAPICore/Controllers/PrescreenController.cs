using System;
using System.Collections.Generic;
using System.Linq;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using AlexaApiCoreLibs.Validators;
using GameMaker.Implementations;
using GameMaker.IntentHandlers;
using GameMaker.Utils;
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
                FileName = "${basedir}/file_prescreening.txt",
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
            AlexaRequestValidationService validator = new AlexaRequestValidationService();
            SpeechletRequestValidationResult validationResult = validator.ValidateAlexaRequest(alexaRequestInput);
            if (validationResult != SpeechletRequestValidationResult.OK)
            {
                logger.Debug("validation error: " + validationResult.ToString());
                new Exception("Invalid Request");
            }
            SkillResponse response = new SkillResponse();
            response.Version = "1.0";
        //    logger.Debug("Request:" + JsonConvert.SerializeObject(alexaRequestInput.Request));

            switch (alexaRequestInput.Request.Type)
            {

                case "LaunchRequest":

                //    logger.Debug("Launch request in");
                   
                    response.Response = new ResponseBody();
                    response.Response.Card = new SimpleCard()
                    {
                        //                        Content = "Hello! Enjoy your game while I keep the scores. You can tell me to start a game or ask for the score of your current game.",
                        Content = "Hello!! Welcome to Childcare pre-screening! Please note: This is a demo skill to demonstrate voice driven pre-screening process. Outcomes have no real world significance.",

                        Title = "Welcome!!"
                    };
                    response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Hello!! welcome to Childcare pre-screening! You can now say, check my eligibility" };
                    response.Response.ShouldEndSession = false;

                  //  logger.Debug("Launch request out");
                    break;
                case "SessionEndedRequest":
                    response.Response = new ResponseBody();
                    response.Response.Card = new SimpleCard()
                    {
                        //                        Content = "Hello! Enjoy your game while I keep the scores. You can tell me to start a game or ask for the score of your current game.",
                        Content = "Goodbye, have a good day!",

                        Title = "Welcome!!"
                    };
                    response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Goodbye, have a good day!" };
                    response.Response.ShouldEndSession = true;
                    return response;
                case "IntentRequest":
                    try
                    {
                        CaseInfo caseInfo = Helpers.GetCaseInfo(alexaRequestInput.Session.SessionId);
                        IntentRequest intentRequest = (IntentRequest)alexaRequestInput.Request;
                        
                        if (intentRequest.Intent.Name == "PreScreen")
                        {
                            UpdateModel(caseInfo, intentRequest.Intent);
                            string slot = GetNextSlot(caseInfo);
                            ResponseBody body = GetResponseForSlot(slot,caseInfo.ChildName);
                            caseInfo.LastAskedSlot = slot;
                            response.Response = body;
                            if (body.ShouldEndSession==true)
                            {
                                Helpers.RemoveCaseInfo(alexaRequestInput.Session.SessionId);
                            }

                        }
                        if (intentRequest.Intent.Name == "AMAZON.StopIntent")
                        {
                            var stophandler = new AMAZON_StopIntent();
                            var skillresponse= stophandler.HandleIntent(null, null, null, null, logger);
                            skillresponse.Version = "1.0";
                            return skillresponse;
                        }
                        if (intentRequest.Intent.Name == "AMAZON.FallbackIntent")
                        {
                            var fallbackhandler = new AMAZON_FallbackIntent();
                            var fallbackresponse=fallbackhandler.HandleIntent(null, null, null, null, logger);
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
                        if (intentRequest.Intent.Name == "AMAZON.HelpIntent")
                        {
                            var helphandler = new AMAZON_HelpIntent();
                            var helplresponse = helphandler.HandleIntent(null, null, null, null, logger);
                            helplresponse.Version = "1.0";
                            helplresponse.Response.ShouldEndSession = false;
                            return helplresponse;
                        }
                        break;
                    }catch(Exception e)
                    {
                       
                        response.Response = Helpers.GetPlainTextResponseBody("Aaargh, the application encountered an error. Please try again later. Sorry for the inconvenience", true, "Error",e.Message);
                        response.Response.ShouldEndSession = true;
                        
                        logger.Debug(e.StackTrace);
                    }
                    break;
                
                   
            }
            // logger.Debug("Response:" + JsonConvert.SerializeObject(response.Response));
            response.Response.Reprompt = new Reprompt("Sorry, I didn't hear you, can you repeat that?");
            return response;
        }

        private void UpdateModel(CaseInfo caseInfo, Intent intent)
        {
            if (caseInfo.LastAskedSlot == null || caseInfo.LastAskedSlot == "")//first time
                return;
            //verify data capture integrity
            
            if (intent.Slots[caseInfo.LastAskedSlot].Resolution != null)
            {
                if (intent.Slots[caseInfo.LastAskedSlot].Resolution.Authorities[0].Status.Code == "ER_SUCCESS_MATCH")
                {
                    caseInfo.DataElements.Add(new KeyValuePair<string, string>(caseInfo.LastAskedSlot, intent.Slots[caseInfo.LastAskedSlot].Value));
                }
                else
                {
                    caseInfo.ErrorFlag = true;
                }
            }
            else
            {
                if (intent.Slots[caseInfo.LastAskedSlot].Value == "?"|| intent.Slots[caseInfo.LastAskedSlot].Value == null)
                {
                    caseInfo.ErrorFlag = true;
                }
                else
                {
                    caseInfo.DataElements.Add(new KeyValuePair<string, string>(caseInfo.LastAskedSlot, intent.Slots[caseInfo.LastAskedSlot].Value));
                }
            }

        }

        private ResponseBody GetResponseForSlot(string slot,string childName)
        {
            ResponseBody body;
            string title="";
            string cardContent="";
            string audiotext="";
            bool sessionend = false; ;
            
            switch (slot)
            {
                case "residence":
                    audiotext = cardContent = "Great! Lets get started. Are you a citizen of US and resident of Kentucky?";
                    title = "Residency";
                    
                    break;
                case "child_name":
                    audiotext = cardContent = "Give me a dummy name for the child you are applying for?";
                    title = "Child's name";
                    break;
                case "child_age":
                    audiotext = cardContent = "How old is " + childName + "?";
                    title = "Child's age";
                    break;
                case "special_needs":
                    audiotext = cardContent = "Does " + childName + " has any special needs?";
                    title = "Special needs";
                    break;
                case "assets":
                    audiotext = cardContent = "Do you have assets in excess of 1 million? ";
                    title = "Assets";
                    break;
                case "father_working":
                    audiotext = cardContent = "Does " + childName + "'s father is employed, or self employed, and works for more than twenty hours a week?";  ;
                    title = "Father's work";
                    break;
                case "father_age":
                    audiotext = cardContent = "Child care program has work requirements, for both parents. However, you may be still eligible, if parents are teens, and in school. Tell me, how old is " + childName + "'s father.";
                    title = "Father's Age";
                    break;
                case "father_in_school":
                    audiotext = cardContent = "Since " + childName + "'s father is a teen, is he enrolled in school?";
                    title = "Father's school enrollment";
                    break;
                case "mother_working":
                    audiotext = cardContent = "Does " + childName + "'s mother is employed, or self employed, and works for more than twenty hours a week?"; ;
                    title = "Mother's work";
                    break;
                case "mother_age":
                    audiotext = cardContent = "Child care program has work requirements, for both parents. However, you may be still eligible if parents are teens, and in school. Tell me, how old is " + childName + "'s mother.";
                    title = "Mother's Age";
                    break;
                case "mother_in_school":
                    audiotext = cardContent = "Since " + childName + "'s mother is a teen, is she enrolled in school?";
                    title = "Mother's school enrollment";
                    break;
                case "income_job":
                    audiotext = cardContent = "How much the parents earn in total, from employment, and self employment?";
                    title = "Income";
                    break;
                case "income_other":
                    audiotext = cardContent = "How much does the family earn from any other income sources?";
                    title = "Unearned Income";
                    break;
                case "expense":
                    audiotext = cardContent = "And, the last question. How much the household pay in child support, for any kid not in household?";
                    title = "Child Support Expense";
                    break;
                case "NE_residence":
                    audiotext = cardContent = "Sorry! You are not eligible. Child care program requires you to be a resident of Kentucky, and citizen of United States. Please note: This is a demo skill to demonstrate voice driven pre-screening process. Outcomes have no real world significance.";
                    title = "Pre-screening Results";
                    sessionend = true;

                    break;
                case "NE_child_age":
                    audiotext = cardContent = "Sorry! You are not eligible. " + childName + " is older than the allowed age for the program. Please note: This is a demo skill to demonstrate voice driven pre-screening process. Outcomes have no real world significance.";
                    title = "Pre-screening Results";
                    sessionend = true;
                    break;
                case "NE_assets":
                    audiotext = cardContent = "Sorry! You are not eligible. You have too much assets to qualify. Please note: This is a demo skill to demonstrate voice driven pre-screening process. Outcomes have no real world significance.";
                    title = "Pre-screening Results";
                    sessionend = true;
                    break;
                case "NE_work":
                    audiotext = cardContent = "Sorry! You are not eligible. You do not meet work requirements. Please note: This is a demo skill to demonstrate voice driven pre-screening process. Outcomes have no real world significance.";
                    title = "Pre-screening Results";
                    sessionend = true;
                    break;
                case "NE_income":
                    audiotext = cardContent = "Sorry! You are not eligible. Your household income is above the limit allowed. Please note: This is a demo skill to demonstrate voice driven pre-screening process. Outcomes have no real world significance.";
                    title = "Pre-screening Results";
                    sessionend = true;
                    break;
                case "EG":
                    audiotext = cardContent = "Awesome, we are all set. And we have some great news for you! You might be eligible for child care program. Please note, pre-screening results does not guarantee actual eligibility. Please visit your nearest office to apply!! Thank you!! Please note: This is a demo skill to demonstrate voice driven pre-screening process. Outcomes have no real world significance.";
                    title = "Pre-screening Results";
                    sessionend = true;
                    break;
                case "ERR":
                    audiotext = cardContent = "Aaargh, the application encountered an error. Please try again later. Sorry for the inconvenience";
                    title = "Error";
                    sessionend = true;
                    break;
                default:
                    audiotext = cardContent = "Aaargh, the application encountered an error. Please try again later. Sorry for the inconvenience";
                    title = "Error";
                    sessionend = true;
                    break;

            }
            body = Helpers.GetPlainTextResponseBody(audiotext, true, title);
            body.ShouldEndSession = sessionend;
            if (!sessionend)
            {
                body.Directives.Add(new DialogElicitSlot(slot));
            }
           
            return body;
        }

        private string GetNextSlot(CaseInfo caseInfo)
        {
            if (caseInfo.ErrorFlag == true)
            {
                return caseInfo.LastAskedSlot; //ask same question again
            }
            switch (caseInfo.LastAskedSlot)
            {
                case "" :
                    return "residence";
                case "residence": //validate
                    if (caseInfo.DataElements.First(p => p.Key == "residence").Value.ToLower() != "yes")
                    {
                        //not elig
                        return "NE_residence";
                    }
                    else
                    {
                        return "child_name";
                    }
                case "child_name": //no validation required
                    caseInfo.ChildName = caseInfo.DataElements.First(p => p.Key == "child_name").Value;
                    return "child_age";
                case "child_age":
                    int age = Int32.Parse(caseInfo.DataElements.First(p => p.Key == "child_age").Value);
                    if (age >19)
                    {
                        //not elig
                        return "NE_child_age";
                    }
                    if (age > 13)
                    {
                        return "special_needs";
                    }
                    return "assets";
                case "special_needs":
                    if (caseInfo.DataElements.First(p => p.Key == "special_needs").Value.ToLower() != "yes")
                    {
                        return "NE_child_age";
                    }
                    return "assets";
                case "assets":
                    if (caseInfo.DataElements.First(p => p.Key == "assets").Value.ToLower() == "yes")
                    {
                        return "NE_assets";
                    }
                    return "father_working";
                case "father_working":
                    if (caseInfo.DataElements.First(p => p.Key == "father_working").Value.ToLower() != "yes")
                    {
                        return "father_age";
                    }
                    return "mother_working";//pending
                case "father_age":
                    int father_age = Int32.Parse(caseInfo.DataElements.First(p => p.Key == "father_age").Value);
                    if (father_age < 20)
                    {
                        return "father_in_school";
                    }
                    return "NE_work";
                case "father_in_school":
                    if (caseInfo.DataElements.First(p => p.Key == "father_in_school").Value.ToLower() != "yes")
                    {
                        return "NE_work";
                    }
                    return "mother_working";
                case "mother_working":
                    if (caseInfo.DataElements.First(p => p.Key == "mother_working").Value.ToLower() != "yes")
                    {
                        return "mother_age";
                    }
                    return "income_job"; //pending
                case "mother_age":
                    int mother_age = Int32.Parse(caseInfo.DataElements.First(p => p.Key == "mother_age").Value);
                    if (mother_age < 20)
                    {
                        return "mother_in_school";
                    }
                    return "NE_work";
                case "mother_in_school":
                    if (caseInfo.DataElements.First(p => p.Key == "mother_in_school").Value.ToLower() != "yes")
                    {
                        return "NE_work";
                    }
                    if (caseInfo.DataElements.First(p => p.Key == "mother_in_school").Value.ToLower() == "yes" && caseInfo.DataElements.Exists(p=>p.Key=="father_in_school") &&
                        caseInfo.DataElements.First(p => p.Key == "father_in_school").Value.ToLower() == "yes"
                        ) // both in school dont ask job income
                        return "income_other"; //pending
                    else
                        return "income_job";
                case "income_job": //no validation yet
                    return "income_other";
                case "income_other": //no validation yet
                    return "expense";
                case "expense":
                    int income_job = 0;
                    if (caseInfo.DataElements.Exists(p => p.Key == "income_job"))
                    {
                        income_job=Int32.Parse(caseInfo.DataElements.First(p => p.Key == "income_job").Value);
                    }
                    int income_other= Int32.Parse(caseInfo.DataElements.First(p => p.Key == "income_other").Value);
                    int expense= Int32.Parse(caseInfo.DataElements.First(p => p.Key == "expense").Value);
                    if (income_job + income_other - expense > 2771)
                    {
                        return "NE_income";
                    }
                    return "EG";
                default:
                    return "ERR";
            }
        }
    }
}