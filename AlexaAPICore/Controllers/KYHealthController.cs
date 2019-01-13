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

                    logger.Debug("Launch request in");

                    response.Response = new ResponseBody();
                    response.Response.Card = new SimpleCard()
                    {
                        Content = "Hello!! Welcome to Kentucky Health eligibility finder! Let's discuss a few questions today, and see what types of requirements and benefits you may have through the Kentucky HEALTH program. When ready, say, \"I am ready!\".",

                        Title = "Welcome!!"
                    };
                    response.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Hello!! Welcome to Kentucky Health eligibility finder! Lets answer a few questions together and see what types of requirements and benefits you may have through the Kentucky HEALTH program. When ready, say, \"I am ready!\"" };
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
                        CaseInfo caseInfo = Helpers.GetCaseInfo(alexaRequestInput.Session.SessionId);
                        IntentRequest intentRequest = (IntentRequest)alexaRequestInput.Request;
                        //if (intentRequest.Intent.Name == "repeat")
                        //{
                        //    response.Response = GetResponseForSlot(caseInfo.LastAskedSlot, caseInfo.ChildName, caseInfo.EligibilityGroup);
                        //    response.Version = "1.0";
                        //    response.Response.ShouldEndSession = false;
                        //}
                            if (intentRequest.Intent.Name == "PreScreen")
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
                    text= "If you are considered part of the Children Kentucky HEALTH eligibility group, there will be no changes to your requirements. " +
                "Your Medicaid benefits WILL NOT CHANGE. Dental and vision services will still be covered by your Managed Care Organization. Non-emergency medical transportation costs will be paid by the Commonwealth. " +
                "You DO NOT have any new out-of-pocket costs for health services. " +
                "You are NOT REQUIRED to complete PATH Community Engagement activities. " +
                "You DO NOT have access to a My Rewards Account. " +
                "Thanks for using Kentucky Health Eligibility Finder. <Name>, you have a good day.";
            break;
                case "EG_Pregnant":
                    text = "If you are considered part of the Pregnant Women Kentucky HEALTH eligibility group, there will be some changes to your requirements. " +
                        "Your Medicaid benefits WILL NOT CHANGE. Dental and vision services will still be covered by your Managed Care Organization until 60 days after the end of your pregnancy. Non-emergency medical transportation costs will be paid by the Commonwealth. " +
                        "You DO NOT have any out-of-pocket costs for health services. " +
                        "You are NOT REQUIRED to complete PATH Community Engagement activities while you are pregnant and until 60 days after the end of your pregnancy. " +
                        "If you are at least 19 years old, you WILL HAVE access to your My Rewards Account without needing to pay premiums. " +
                        "Thanks for using Kentucky Health Eligibility Finder. <Name>, you have a good day.";
                    break;
                case "EG_FosterYouth":
                    text = "If you are considered part of the Former Foster Youth (up to age 26) Kentucky HEALTH eligibility group, there will be some changes to your requirements. " +
                        "Your Medicaid benefits WILL NOT CHANGE. Dental and vision services will still be covered by your Managed Care Organization. Non-emergency medical transportation costs will be paid by the Commonwealth. " +
                        "Out-of-pocket costs are OPTIONAL. You may choose to pay premiums each month to your Managed Care Organization if you want to use your My Rewards Account. If you choose not to pay premiums to your MCO, you will not have access to your My Rewards Account. " +
                        "You are NOT REQUIRED to complete PATH Community Engagement activities. " +
                        "You MAY HAVE access to a My Rewards Account if you pay a premium each month to your Managed Care Organization. " +
                        "Thanks for using Kentucky Health Eligibility Finder. <Name>, you have a good day.";
                    break;
                case "EG_MedicallyFrail":
                    text = "If you are considered part of the Medically Frail group, there will be some changes to your requirements. " +
                        "Your Medicaid benefits WILL NOT CHANGE. Dental and vision services will still be covered by your Managed Care Organization. Non-emergency medical transportation costs will be paid by the Commonwealth. " +
                        " Out-of-pocket costs are OPTIONAL. You may choose to pay premiums each month to your Managed Care Organization if you want to use your My Rewards Account. If you choose not to pay premiums to your MCO, you will not have access to your My Rewards Account. " +
                        "You are NOT REQUIRED to complete PATH Community Engagement activities. " +
                        "You MAY HAVE access to a My Rewards Account if you pay a premium each month to your Managed Care Organization. " +
                        "Thanks for using Kentucky Health Eligibility Finder. <Name>, you have a good day.";
                    break;
               

                case "EG_IncomeEligParents":
                    text = "If you are considered part of the Income-Eligible Parent/Guardian Kentucky HEALTH eligibility group, there will be some changes to your requirements. " +
                        "Your Medicaid benefits WILL NOT CHANGE. Dental and vision services will still be covered by your Managed Care Organization. Non-emergency medical transportation costs will be paid by the Commonwealth. " +
                        "You are REQUIRED to pay premiums each month to your Managed Care Organization. Based on the income you reported, your premium will probably be $1 per month, but you will get the official amount from your MCO (it will be between $1 and $15). If you do not pay the premium, you will be REQUIRED to pay copays for each of your services when you go to the doctor or get a prescription. " +
                        "You are REQUIRED to complete 80 hours of PATH Community Engagement activities each month. However, PATH Community Engagement MAY NOT be required if you report that you are a full-time student or a primary caregiver of a dependent child. " +
                        "You MAY HAVE access to a My Rewards Account if you pay a premium each month to your Managed Care Organization and meet your PATH Community Engagement requirement (unless exempt). " +
                        "Thanks for using Kentucky Health Eligibility Finder. <Name>, you have a good day.";
                    break;
                case "EG_IncEligAdult":
                    text = "If you are considered part of the Income-Eligible Adults Kentucky HEALTH eligibility group, there will be some changes to your requirements. " +
                        "Many medical benefits are still covered by your Managed Care Organization. However, non-medical dental and vision services are covered through your My Rewards Account, NOT your Managed Care Organization. In addition, non-emergency medical transportation costs will NOT be paid for by the Commonwealth. " +
                        "You are REQUIRED to pay premiums each month to your Managed Care Organization. The monthly premium amount will be between $1 and $15 for the first two years. If you do not pay the premium, you will be REQUIRED to pay copays for each of your services when you go to the doctor or get a prescription or you MAY BE SUSPENDED from Kentucky HEALTH, depending on your income level. " +
                        "You are REQUIRED to complete 80 hours of PATH Community Engagement activities each month. However, PATH Community Engagement MAY NOT be required if you report that you are a full-time student or a primary caregiver of a dependent child. " +
                        "You MAY HAVE access to a My Rewards Account if you pay a premium each month to your Managed Care Organization and meet your PATH Community Engagement requirement (unless exempt). The money in your My Rewards Account can be used to pay for non-medical vision and dental services. " +
                        "Thanks for using Kentucky Health Eligibility Finder. <Name>, you have a good day.";
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
                    caseInfo.DataElements.Add(new KeyValuePair<string, string>(caseInfo.LastAskedSlot, intent.Slots[caseInfo.LastAskedSlot].Value));
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
                    audiotext = cardContent = "Great!! Lets get started. I will now read you a short consent statement, please confirm if you agree, or ask me to repeat.  \"You understand this is not an official determination of your eligibility for Kentucky HEALTH. You also understand that to know your Kentucky HEALTH eligibility group, you can refer to the notice about your Medicaid coverage that you received in the mail, visit benefind.ky.gov, or go to your local D.C.B.S. office.\" Do you agree?";
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
                    audiotext = cardContent = "Including you, how many members are in your household?";
                    title = "Household size";
                    break;
                case "income":
                    audiotext = cardContent = "What is your monthly household income?";
                    title = "Household Income";
                    break;
                case "blind_dis":
                    audiotext = cardContent = "<Name>, Does any of these apply to you, you can reply in, Yes or No, or say Repeat. Are you blind or disabled ?";
                    title = "Disablility & Blindness";
                    break;
                case "medicare":
                    audiotext = cardContent = "Are you a Medicare recipient? ";
                    title = "Medicare";
                    break;
                case "nursing":
                    audiotext = cardContent = "Residing in nursing facility or Intermediate Care Facilities?"; 
                    title = "Nursing";
                    break;
                case "foster":
                    audiotext = cardContent = "Receiving foster care or subsidized adoption?";
                    title = "Foster care";
                    break;
                case "cancer":
                    audiotext = cardContent = "Participant in the Breast and Cervical Cancer Treatment Program?";
                    title = "Cancer Treatment Program";
                    break;
                case "ssi":
                    audiotext = cardContent = "Are you SSI recipient?"; ;
                    title = "SSI Recipient";
                    break;
                case "buyin":
                    audiotext = cardContent = "Are you a working disabled adult in the Medicaid buy-in program?";
                    title = "Medicaid buy-in program";
                    break;
                case "hcbs":
                    audiotext = cardContent = "Are you recipient of Home and Community Based Service Waiver?";
                    title = "Home and Community Based Service Waiver";
                    break;
                case "preg":
                    audiotext = cardContent = "<Name>, are you pregnant OR has your pregnancy ended within the last 60 days? ";
                    title = "Pregnancy";
                    break;
                case "foster_eighteen":
                    audiotext = cardContent = "Since you are younger than 26, tell me if you were in foster care on your 18th birthday?";
                    title = "Foster Youth";
                    break;
                case "ltc":
                    audiotext = cardContent = "Has a health care professional diagnosed you with a severe, long-term condition? ";
                    title = "Long Term Care";
                    break;
                case "special_needs":
                    audiotext = cardContent = "Do you currently need help every day with activities of daily living (including toileting, bathing, dressing, preparing meals, walking in the home or outside, or walking more than 20 feet)?";
                    title = "Special Needs";
                    break;
                case "homeless":
                    audiotext = cardContent = "Are you Homeless?";
                    title = "Child Support Expense";
                    break;
                    
                case "guardian":
                    audiotext = cardContent = "Thanks for your patience, and here comes the last question. Are you a parent or guardian of a child age 18 or younger?";
                    title = "Child Support Expense";
                    break;
               

               
                case "NE_Consent":
                    audiotext = cardContent = "Sorry, I cannot proceed without your consent.";
                    title = "Pre-screening Results";
                    sessionend = true;

                    break;
                case "EG_Child":
                    
                    audiotext = cardContent = "That's all I need for now. Based on what we discussed, you might be eligible to be placed in Kentucky HEALTH eligibility group for Children. Do you want to know more about your eligibility group? ";
                    title = "Pre-screening Results";
                    sessionend = false;
                    
                    break;
                case "NE_Situation":
                    audiotext = cardContent = "That's all I need for now. Based on your situation, you may not be eligible for Kentucky HEALTH.";
                    title = "Pre-screening Results";
                    sessionend = true;
                    break;
                case "NE_Income":
                    audiotext = cardContent = "That's all I need for now. Based on your income, you may not be eligible for Kentucky HEALTH.";
                    title = "Pre-screening Results";
                    sessionend = true;
                    break;
                case "EG_Pregnant":
                    
                    audiotext = cardContent = "That's all I need for now. Based on what we discussed, you might be eligible to be placed in Kentucky HEALTH eligibility group for Pregnant Women. Do you want to know more about your eligibility group?";
                    title = "Pre-screening Results";
                    sessionend = false;
                   
                    break;
                case "EG_FosterYouth":
                    audiotext = cardContent = "That's all I need for now. Based on what we discussed, you might be eligible to be placed in Kentucky HEALTH eligibility group for Former Foster Youth.  Do you want to know more about your eligibility group?";
                    title = "Pre-screening Results";
                    sessionend = false;
                    
                    break;
                case "EG_MedicallyFrail":
                    audiotext = cardContent = "That's all I need for now. Based on what we discussed, you might be eligible to be placed in Kentucky HEALTH eligibility group for Medically Frail.  Do you want to know more about your eligibility group?";
                    title = "Pre-screening Results";
                    sessionend = false;
                   
                    break;
                //case "EG_IncomeElig":
                //    audiotext = cardContent = "That's all I need for now. Based on what we discussed, you might be eligible to be placed in Kentucky HEALTH eligibility group for Medically Frail. To know more about this eligibility group, say: \"Tell, me more.\"";
                //    title = "Pre-screening Results";
                //    sessionend = false ;
                //    slot = "conclude";
                //    break;
                case "EG_IncomeEligParents":
                    audiotext = cardContent = "That's all I need for now. Based on your input, your Kentucky HEALTH eligibility group may be Income-Eligible Parents/Guardians.  Do you want to know more about your eligibility group?";
                    title = "Pre-screening Results";
                    sessionend = false;
                   
                    break;
                case "EG_IncEligAdult":
                    audiotext = cardContent = "That's all I need for now. Based on your input, your Kentucky HEALTH eligibility group may be Income-Eligible Adults.  Do you want to know more about your eligibility group?";
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
            body = Helpers.GetPlainTextResponseBody(audiotext.Replace("<Name>", name), true, title);
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
                    if (caseInfo.DataElements.First(p => p.Key == "consent").Value.ToLower() != "yes")
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
                    if (caseInfo.DataElements.First(p => p.Key == "blind_dis").Value.ToLower() == "yes")
                    {
                        return "NE_Situation";
                    }
                    return "medicare";//pending
                case "medicare":
                    if (caseInfo.DataElements.First(p => p.Key == "medicare").Value.ToLower() == "yes")
                    {
                        return "NE_Situation";
                    }
                    return "nursing";//pending
                case "nursing":
                    if (caseInfo.DataElements.First(p => p.Key == "nursing").Value.ToLower() == "yes")
                    {
                        return "NE_Situation";
                    }
                    return "foster";
                case "foster":
                    if (caseInfo.DataElements.First(p => p.Key == "foster").Value.ToLower() == "yes")
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
                
                    if (caseInfo.DataElements.First(p => p.Key == "cancer").Value.ToLower() == "yes")
                    {
                        return "NE_Situation";
                    }
                    return "ssi";

                case "ssi":

                    if (caseInfo.DataElements.First(p => p.Key == "ssi").Value.ToLower() == "yes")
                    {
                        return "NE_Situation";
                    }
                    return "buyin";
                case "buyin": //no validation yet
                    if (caseInfo.DataElements.First(p => p.Key == "buyin").Value.ToLower() == "yes")
                    {
                        return "NE_Situation";
                    }
                    return "hcbs";
                case "hcbs": //no validation yet
                    if (caseInfo.DataElements.First(p => p.Key == "hcbs").Value.ToLower() == "yes")
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
                    if (caseInfo.DataElements.First(p => p.Key == "preg").Value.ToLower() == "yes")
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
                    if (caseInfo.DataElements.First(p => p.Key == "foster_eighteen").Value.ToLower() == "yes")
                    {
                        return "EG_FosterYouth";
                    }
                    return "ltc";
                case "ltc":
                    if (caseInfo.DataElements.First(p => p.Key == "ltc").Value.ToLower() == "yes")
                    {
                        return "EG_MedicallyFrail";
                    }
                    return "special_needs";
                case "special_needs":
                    if (caseInfo.DataElements.First(p => p.Key == "special_needs").Value.ToLower() == "yes")
                    {
                        return "EG_MedicallyFrail";
                    }
                    return "homeless";
                case "homeless":
                    if (caseInfo.DataElements.First(p => p.Key == "homeless").Value.ToLower() == "yes")
                    {
                        return "EG_MedicallyFrail";
                    }
                    return "guardian";
                case "guardian":
                    if (caseInfo.DataElements.First(p => p.Key == "guardian").Value.ToLower() == "yes" )
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
                    if (caseInfo.DataElements.First(p => p.Key == "conclude").Value.ToLower() == "yes")
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