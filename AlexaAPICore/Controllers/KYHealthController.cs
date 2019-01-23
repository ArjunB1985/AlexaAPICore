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
    public class KYHealthController : ControllerBase
    {
        LoggingConfiguration config = new LoggingConfiguration();
        Logger logger = null;
        public KYHealthController()
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

        [HttpPost, Route("api/alexa_kyhealth")]
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
            logger.Debug("Request:" + JsonConvert.SerializeObject(alexaRequestInput.Request));

            switch (alexaRequestInput.Request.Type)
            {

                case "LaunchRequest":

                 //   logger.Debug("Launch request in");

                    response.Response = new ResponseBody();
                    response.Response.Card = new SimpleCard()
                    {
                        Content = "Hello!! Welcome to the Kentucky Health eligibility finder! Lets answer a few questions together to see the benefits you could have through the Kentucky HEALTH program. Say \"I am ready\" when ready to start.",

                        Title = "Welcome!!"
                    };
                    response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Hello!! Welcome to Kentucky Health eligibility finder! Lets answer a few questions together and see what types of requirements and benefits you may have through the Kentucky HEALTH program. When ready, say, \"I am ready!\"" };
                    //response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Hello!! say, \"I am ready!\"" };
                    response.Response.Reprompt = new Reprompt("Please say, I am ready.");
                    response.Response.ShouldEndSession = false;

                  //  logger.Debug("Launch request out");
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
                        CaseInfo caseInfo = Helpers.GetCaseInfo(alexaRequestInput.Session.SessionId);
                        IntentRequest intentRequest = (IntentRequest)alexaRequestInput.Request;
                        //if (intentRequest.Intent.Name == "repeat")
                        //{
                        //    response.Response = GetResponseForSlot(caseInfo.LastAskedSlot, caseInfo.ChildName, caseInfo.EligibilityGroup);
                        //    response.Version = "1.0";
                        //    response.Response.ShouldEndSession = false;
                        //}
                        
                            if (intentRequest.Intent.Name == "PreScreen" )
                        {
                            UpdateModel(caseInfo, intentRequest.Intent);
                            string slot = GetNextSlot(caseInfo);
                            
                            ResponseBody body = GetResponseForSlot(slot, caseInfo.ChildName,caseInfo.EligibilityGroup);
                            if (slot.Contains("EG_"))
                            {
                                caseInfo.EligibilityGroup = slot;
                                slot = "conclude";
                            }
                            caseInfo.LastAskedSlot = slot;
                            response.Response = body;
                            if (body.ShouldEndSession == true)
                            {
                                Helpers.RemoveCaseInfo(alexaRequestInput.Session.SessionId);
                            }

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

        private string GetGroupInfo(string eligibilityGroup)
        {
            var text = "Please complete the pre-screening first to know more about eligibility groups"; 
            switch (eligibilityGroup)
            { 
              //  
                case "EG_Child":
                    text= "If you are considered part of the Children eligibility group within Kentucky HEALTH, " +
                        "there will be no changes to your requirements. For more specific information concerning your " +
                        "requirements and benefits through Kentucky HEALTH, please visit us online at KentuckyHEALTH.ky.gov. " +
                        "Thanks for using the Kentucky HEALTH Eligibility Finder, <Name>. Have a great day!";
            break;
                case "EG_Pregnant":
                    text = "If you are considered part of the Pregnant Women eligibility group within Kentucky HEALTH, " +
                        "there will be some changes to your requirements including access to your My Rewards Account, " +
                        "but your Medicaid benefits will not change. Also, you are not required to complete PATH Community " +
                        "Engagement activities while pregnant and until 60 days after your pregnancy. For more specific information " +
                        "concerning your requirements and benefits through Kentucky HEALTH, please visit us online at KentuckyHEALTH.ky.gov. " +
                        "Thanks for using the Kentucky HEALTH Eligibility Finder, <Name>. Have a great day!";
                    break;
                case "EG_FosterYouth":
                    text = "If you are considered part of the Former Foster Youth eligibility group within Kentucky HEALTH, " +
                        "there will be some changes to your requirements, but your Medicaid benefits will not change. " +
                        "You are not required to complete PATH Community Engagement activities, and you may have access to your " +
                        "My Rewards Account. For more specific information concerning your requirements and benefits through Kentucky HEALTH," +
                        " please visit us online at KentuckyHEALTH.ky.gov. Thanks for using the Kentucky HEALTH Eligibility Finder, <Name>. " +
                        "Have a great day!";
                    break;
                case "EG_MedicallyFrail":
                    text = "If you are considered part of the Medically Frail eligibility group within Kentucky HEALTH, " +
                        "there will be some changes to your requirements, but your Medicaid benefits will not change. " +
                        "You are not required to complete PATH Community Engagement activities, and you may have access to your My Rewards Account." +
                        " For more specific information concerning your requirements and benefits through Kentucky HEALTH, please visit us online at KentuckyHEALTH.ky.gov. " +
                        "Thanks for using the Kentucky HEALTH Eligibility Finder, <Name>. Have a great day!";
                    break;
               

                case "EG_IncomeEligParents":
                    text = "If you are considered part of the Income Eligible Parents eligibility group within Kentucky HEALTH, there will be some changes to your requirements, " +
                        "but your Medicaid benefits will not change. You are required to complete 80 hours of PATH Community Engagement activities, and are required to pay monthly premiums to your MCO . " +
                        "For more specific information concerning your requirements and benefits through Kentucky HEALTH, please visit us online at KentuckyHEALTH.ky.gov. " +
                        "Thanks for using the Kentucky HEALTH Eligibility Finder, <Name>. Have a great day!";
                    break;
                case "EG_IncEligAdult":
                    text = "If you are considered part of the Income Eligible Adult eligibility group within Kentucky HEALTH, " +
                        "there will be some changes to your requirements. Non-medical dental and vision services will now be covered through your My Rewards Account. " +
                        "You are required to complete 80 hours of PATH Community Engagement activities, and are required to pay monthly premiums to your MCO . " +
                        "For more specific information concerning your requirements and benefits through Kentucky HEALTH, please visit us online at KentuckyHEALTH.ky.gov. " +
                        "Thanks for using the Kentucky HEALTH Eligibility Finder, <Name>. Have a great day! ";
                    break;
               
            
            }
            
            return text;
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
                    caseInfo.DataElements.Add(new KeyValuePair<string, string>(caseInfo.LastAskedSlot, intent.Slots[caseInfo.LastAskedSlot].Resolution.Authorities[0].Values[0].Value.Id));
                }
                else
                {
                    caseInfo.ErrorFlag = true;
                }
            }
            else
            {
                if (intent.Slots[caseInfo.LastAskedSlot].Value == "?" || intent.Slots[caseInfo.LastAskedSlot].Value == null)
                {
                    caseInfo.ErrorFlag = true;
                }
                else
                {
                    caseInfo.DataElements.Add(new KeyValuePair<string, string>(caseInfo.LastAskedSlot, intent.Slots[caseInfo.LastAskedSlot].Value));
                }
            }

        }

        private ResponseBody GetResponseForSlot(string slot, string name,string eligibilityGroup)
        {
            ResponseBody body;
            string title = "";
            string cardContent = "";
            string audiotext = "";
            bool sessionend = false; ;

            switch (slot)
            {
                case "consent":
                  //  audiotext = cardContent = "Great!! Lets get started. I will now read you a short consent statement, please confirm if you agree, or ask me to repeat.  \"You understand this is not an official determination of your eligibility for Kentucky HEALTH. You also understand that to know your Kentucky HEALTH eligibility group, you can refer to the notice about your Medicaid coverage that you received in the mail, visit benefind.ky.gov, or go to your local D.C.B.S. office.\" Do you agree?";
                    audiotext = cardContent = "Great!!Lets get started. Before we begin, please agree to the following statement:  I understand this is not an official determination of my eligibility for Kentucky HEALTH. I also understand that to know my Kentucky HEALTH eligibility group, I can refer to the notice about my Medicaid coverage that I received in the mail, visit benefind.ky.gov, or go to my local DCBS office. Do you agree ?";


                  //audiotext = cardContent = "Consent, Do you agree?";

                  title = "Consent";

                    break;
                case "name":
                    audiotext = cardContent = "Please tell me your first name.";
                    title = "Name";
                    break;
                case "age":
                    audiotext = cardContent = "Thanks <Name>. Now tell me how old are you?";
                    title = "Age";
                    break;
                case "gender":
                    audiotext = cardContent = "Please tell me your gender? Male or Female";
                    title = "Gender";
                    break;
                case "hh_members":
                    audiotext = cardContent = "Great; including you, " +
                        "how many people are in your household? These are the individuals who you file your taxes with. ";
                    title = "Household size";
                    break;
                case "income":
                    audiotext = cardContent = "What is your monthly household income?";
                    title = "Household Income";
                    break;
                case "blind_dis":
                    audiotext = cardContent = "<Name>, Does any of the following questions apply to you? You may reply, \"Yes\", \"No\", or ask me to Repeat. Are you blind or disabled ?";
                    title = "Disablility & Blindness";
                    break;
                case "medicare":
                    audiotext = cardContent = "Are you a Medicare recipient? ";
                    title = "Medicare";
                    break;
                case "nursing":
                    audiotext = cardContent = "Do you reside in nursing facility or Intermediate Care Facilities?"; 
                    title = "Nursing";
                    break;
                case "foster":
                    audiotext = cardContent = "Are you currently in foster care or subsidized adoption?";
                    title = "Foster care";
                    break;
                case "cancer":
                    audiotext = cardContent = "Are you a participant in the Breast and Cervical Cancer Treatment Program?";
                    title = "Cancer Treatment Program";
                    break;
                case "ssi":
                    audiotext = cardContent = "Do you receive Supplemental Security Income?"; ;
                    title = "SSI Recipient";
                    break;
                case "buyin":
                    audiotext = cardContent = "Are you a working disabled adult in the Medicaid buy-in program?";
                    title = "Medicaid buy-in program";
                    break;
                case "hcbs":
                    audiotext = cardContent = "Are you a recipient of Home and Community Based Service Waiver?";
                    title = "Home and Community Based Service Waiver";
                    break;
                case "preg":
                    audiotext = cardContent = "<Name>, are you pregnant OR have you been pregnant within the last 60 days? ";
                    title = "Pregnancy";
                    break;
                case "foster_eighteen":
                    audiotext = cardContent = "Since you are younger than 26, were you in foster care on your 18th birthday?";
                    title = "Foster Youth";
                    break;
                case "ltc":
                    audiotext = cardContent = "Has a health care professional diagnosed you with a severe, long-term condition? ";
                    title = "Long Term Care";
                    break;
                case "special_needs":
                    audiotext = cardContent = "Do you currently need help every day with activities of daily living ?";
                    title = "Special Needs";
                    break;
                case "homeless":
                    audiotext = cardContent = "Are you Homeless?";
                    title = "Child Support Expense";
                    break;
                    
                case "guardian":
                    audiotext = cardContent = "Thanks for your patience. Here is the last question: Are you a parent or guardian of a child age 18 or younger?";
                    title = "Child Support Expense";
                    break;
               

               
                case "NE_Consent":
                    audiotext = cardContent = "Sorry, I cannot proceed without your consent.";
                    title = "Pre-screening Results";
                    sessionend = true;

                    break;
                case "EG_Child":
                    
                    audiotext = cardContent = "Great, thanks! Based on the information you provided, you may be part of the Children eligibility group within Kentucky HEALTH. Would you like to learn more about your eligibility group? ";
                    title = "Pre-screening Results";
                    sessionend = false;
                    
                    break;
                case "NE_Situation":
                    audiotext = cardContent = "Thanks, <Name>! That’s all I need to know. Based on the information you provided, you may not be eligible for Kentucky HEALTH. For more information, please visit us online at KentuckyHEALTH.ky.gov. Have a great day!";
                    title = "Pre-screening Results";
                    sessionend = true;
                    break;
                case "NE_Income":
                    audiotext = cardContent = "Thanks, <Name>! That’s all I need to know. Based on the income information you provided, you may not be eligible for Kentucky HEALTH. For more information, please visit us online at KentuckyHEALTH.ky.gov. Have a great day! ";
                    title = "Pre-screening Results";
                    sessionend = true;
                    break;
                case "EG_Pregnant":
                    
                    audiotext = cardContent = "Great, thanks! Based on the information you provided, you may be part of the Pregnant Women eligibility group within Kentucky HEALTH. Would you like to learn more about your eligibility group?";
                    title = "Pre-screening Results";
                    sessionend = false;
                   
                    break;
                case "EG_FosterYouth":
                    audiotext = cardContent = "Great, thanks! Based on the information you provided, you may be part of the Former Foster Youth eligibility group within Kentucky HEALTH. Would you like to learn more about your eligibility group?";
                    title = "Pre-screening Results";
                    sessionend = false;
                    
                    break;
                case "EG_MedicallyFrail":
                    audiotext = cardContent = "Great, thanks! Based on the information you provided, you may be part of the Medically Frail eligibility group within Kentucky HEALTH. Would you like to learn more about your eligibility group?";
                    title = "Pre-screening Results";
                    sessionend = false;
                   
                    break;
             
                case "EG_IncomeEligParents":
                    audiotext = cardContent = "Great, thanks! Based on the information you provided, you may be part of the Income-Eligible Parent/Guardian eligibility group within Kentucky HEALTH. Would you like to learn more about your eligibility group?";
                    title = "Pre-screening Results";
                    sessionend = false;
                   
                    break;
                case "EG_IncEligAdult":
                    
                    audiotext = cardContent = "Great, thanks! Based on the information you provided, you may be part of the Income-Eligible Adults eligibility group within Kentucky HEALTH. Would you like to learn more about your eligibility group?";
                    title = "Pre-screening Results";
                    sessionend = false;
                   
                    break;
                case "End_Detail":
                    audiotext = cardContent = GetGroupInfo(eligibilityGroup);
                    title = "More Details";
                   
                    sessionend = true;
                    break;
                case "End_NoDetail":
                    audiotext = cardContent = "Thanks for using Kentucky Health eligibility finder! <Name>, you have a good day.";
                    title = "Thank You.";
                    sessionend = true;
                    break;
                case "ERR":
                    audiotext = cardContent = "Oops, the application encountered an error. Please try again later. Sorry for the inconvenience";
                    title = "Error";
                    sessionend = true;
                    break;
                case "conclude":
                    audiotext = cardContent = "Do you want more information about your eligibility group?";
                    title = "More information.";
                    sessionend = false ;
                    break;

                default:
                    audiotext = cardContent = "Oops, the application encountered an error. Please try again later. Sorry for the inconvenience";
                    title = "Error";
                    sessionend = true;
                    break;

            }
            body = Helpers.GetPlainTextResponseBody(audiotext.Replace("<Name>", name), true, title);
            body.Reprompt = new Reprompt("Sorry, I didn't hear you. Can you repeat?");
            body.ShouldEndSession = sessionend;
            if (!sessionend)
            {
                if (slot.Contains("EG_"))
                {
                    slot = "conclude";
                }
                body.Directives.Add(new DialogElicitSlot(slot));
            }

            return body;
        }

        private string GetNextSlot(CaseInfo caseInfo)
        {
            if (caseInfo.ErrorFlag == true)
            {
                caseInfo.ErrorFlag = false;
                return caseInfo.LastAskedSlot; //ask same question again
            }
            switch (caseInfo.LastAskedSlot)
            {
                case "":
                    return "consent";
                case "consent": //validate
                    if (caseInfo.DataElements.First(p => p.Key == "consent").Value.ToLower() != "y")
                    {
                        //not elig
                        return "NE_Consent";
                    }
                    else
                    {
                        return "name";
                    }
                case "name": //no validation required
                    caseInfo.ChildName = caseInfo.DataElements.First(p => p.Key == "name").Value;
                    
                    
                    return "age";
                case "age":
                    int age = Int32.Parse(caseInfo.DataElements.First(p => p.Key == "age").Value);
                    if (age < 19)
                    {
                        //not elig
                        return "EG_Child";
                    }
                    if (age > 65)
                    {
                        return "NE_Situation";
                    }
                    caseInfo.Gender = Helpers.GetGender(caseInfo.ChildName, AlexaConstants.BuildMode, AlexaConstants.Key);
                    if (caseInfo.Gender.ToLower() == "e")
                    {
                        return "gender";
                    }
                    else
                    {
                        return "hh_members";
                    }
                case "gender":
                    caseInfo.Gender = caseInfo.DataElements.First(p => p.Key == "gender").Value;
                    return "hh_members";
                case "hh_members":
                    int members;
                    if(!Int32.TryParse(caseInfo.DataElements.First(p => p.Key == "hh_members").Value, out members))
                    {
                        return "hh_members";
                    }
                    caseInfo.MemberCount = members;
                        return "income";
                    
                  case "income":
                    int income;
                    if (!Int32.TryParse(caseInfo.DataElements.First(p => p.Key == "income").Value, out income))
                    {
                        return "income";
                    }
                    caseInfo.Income = income;
                    bool eligible = RunIncomeRule(income,caseInfo.MemberCount);
                    if (eligible)
                    {
                        return "blind_dis";
                    }
                    else
                    {
                        return "NE_Income";
                    }
                case "blind_dis":
                    if (caseInfo.DataElements.First(p => p.Key == "blind_dis").Value.ToLower() == "y")
                    {
                        return "NE_Situation";
                    }
                    return "medicare";//pending
                case "medicare":
                    if (caseInfo.DataElements.First(p => p.Key == "medicare").Value.ToLower() == "y")
                    {
                        return "NE_Situation";
                    }
                    return "nursing";//pending
                case "nursing":
                    if (caseInfo.DataElements.First(p => p.Key == "nursing").Value.ToLower() == "y")
                    {
                        return "NE_Situation";
                    }
                    return "foster";
                case "foster":
                    if (caseInfo.DataElements.First(p => p.Key == "foster").Value.ToLower() == "y")
                    {
                        return "NE_Situation";
                    }
                    if (caseInfo.Gender.ToLower() == "female")
                    {
                        return "cancer"; //pending
                    }
                    else
                    {
                        return "ssi";
                    }
                case "cancer":
                
                    if (caseInfo.DataElements.First(p => p.Key == "cancer").Value.ToLower() == "y")
                    {
                        return "NE_Situation";
                    }
                    return "ssi";

                case "ssi":

                    if (caseInfo.DataElements.First(p => p.Key == "ssi").Value.ToLower() == "y")
                    {
                        return "NE_Situation";
                    }
                    return "buyin";
                case "buyin": //no validation yet
                    if (caseInfo.DataElements.First(p => p.Key == "buyin").Value.ToLower() == "y")
                    {
                        return "NE_Situation";
                    }
                    return "hcbs";
                case "hcbs": //no validation yet
                    if (caseInfo.DataElements.First(p => p.Key == "hcbs").Value.ToLower() == "y")
                    {
                        return "NE_Situation";
                    }//add gender check
                    if (caseInfo.Gender.ToLower() == "female")
                    {
                        return "preg";
                    }
                    else
                    {
                        int age2 = Int32.Parse(caseInfo.DataElements.First(p => p.Key == "age").Value);
                        if (age2 < 26)
                        {
                            return "foster_eighteen";
                        }
                        else
                        {
                            return "ltc";
                        }
                    }
                   
                case "preg":
                    if (caseInfo.DataElements.First(p => p.Key == "preg").Value.ToLower() == "y")
                    {
                       
                            return "EG_Pregnant";
                       
                    }
                    int age1 = Int32.Parse(caseInfo.DataElements.First(p => p.Key == "age").Value);
                    if (age1 < 26)
                    {
                        return "foster_eighteen";
                    }
                    return "ltc";
                case "foster_eighteen":
                    if (caseInfo.DataElements.First(p => p.Key == "foster_eighteen").Value.ToLower() == "y")
                    {
                        return "EG_FosterYouth";
                    }
                    return "ltc";
                case "ltc":
                    if (caseInfo.DataElements.First(p => p.Key == "ltc").Value.ToLower() == "y")
                    {
                        return "EG_MedicallyFrail";
                    }
                    return "special_needs";
                case "special_needs":
                    if (caseInfo.DataElements.First(p => p.Key == "special_needs").Value.ToLower() == "y")
                    {
                        return "EG_MedicallyFrail";
                    }
                    return "homeless";
                case "homeless":
                    if (caseInfo.DataElements.First(p => p.Key == "homeless").Value.ToLower() == "y")
                    {
                        return "EG_MedicallyFrail";
                    }
                    return "guardian";
                case "guardian":
                    if (caseInfo.DataElements.First(p => p.Key == "guardian").Value.ToLower() == "y" )
                    {
                        if (CheckIncome(caseInfo.Income, caseInfo.MemberCount))
                        {
                            return "EG_IncomeEligParents";
                        }
                        else
                        {
                            return "EG_IncEligAdult";
                        }

                    }
                    else
                    {
                        return "EG_IncEligAdult";
                    }

                    
                case "conclude":
                    if (caseInfo.DataElements.First(p => p.Key == "conclude").Value.ToLower() == "y")
                    {
                        return "End_Detail";
                    }
                    else {
                        return "End_NoDetail";
                    }
                   
                default:
                    return "ERR";
            }
        }

        private bool CheckIncome(int income, int memberCount)
        {
            switch (memberCount)
            {
                case 1: return (income < 235);
                case 2: return (income < 291);
                case 3: return (income < 338);
                case 4: return (income < 419);
                case 5: return (income < 492);
                case 6: return (income < 556);
                case 7: return (income < 621);
                
                default:
                    return true;

            }
        }

        private bool RunIncomeRule(int income, int memberCount)
        {
            switch (memberCount)
            {
                case 1: return (income < 1413);
                case 2: return (income < 1916);
                case 3: return (income < 2418);
                case 4: return (income < 2921);
                case 5: return (income < 3423);
                case 6: return (income < 3926);
                case 7: return (income < 4429);
                case 8: return (income < 4932);
                default:
                   return true;

            }
        }
    }
}