using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using GameMaker.Implementations;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaker.Utils
{
    public static class Helpers
    {
        public static string PlayerNameToSlotName(string playerName)
        {
            string number = playerName.Trim().Substring(7);
            switch (number)
            {
                case "1":
                    return "playerone";
                case "2":
                    return "playertwo";
                case "3":
                    return "playerthree";
                case "4":
                    return "playerfour";
                case "5":
                    return "playerfive";
                case "6":
                    return "playersix";
                case "7":
                    return "playerseven";
                case "8":
                    return "playereight";
                case "9":
                    return "playernine";
                case "10":
                    return "playerten";
                default:
                    throw new Exception("Invalid player name");
            }
        }
        public static CaseInfo caseInfo = new CaseInfo();
        public static ResponseBody GetPlainTextResponseBody(string text, bool needCard, string title = null, string cardText = null)
        {
            ResponseBody body = new ResponseBody();
            if (needCard)
            {
                if (cardText == null)
                {
                    cardText = text;
                }
                body.Card = new SimpleCard()
                {
                    Content = cardText,
                    Title = title
                };
            }

            body.OutputSpeech = new PlainTextOutputSpeech() { Text = text };
            return body;

        }
        public static EligibilityResult EvaluateEligibility()
        {
            var result = new EligibilityResult();
            if (caseInfo.ChildAge > 13 && caseInfo.SpecialNeeds == false)
            {
                result.Pass = false;
                result.FailReason = "Child age is more than the limit for the program.";
                return result;
            }
            if (caseInfo.Residence == false)
            {
                result.Pass = false;
                result.FailReason = "You are not resident of Kentucky or citizen of US.";
                return result;
            }
            if (caseInfo.Assets == true)
            {
                result.Pass = false;
                result.FailReason = "You have too much assets to qualify.";
                return result;
            }
            //income
            int netincome = caseInfo.Income + caseInfo.IncomeOther - caseInfo.Expense;
            if (netincome > 2771)
            {
                result.Pass = false;
                result.FailReason = "You have more income than required to qualify.";
                return result;
            }
            if (caseInfo.FatherAge > 20 && caseInfo.MotherAge > 20 && !caseInfo.FatherWorking && !caseInfo.MotherWorking)
            {
                result.Pass = false;
                result.FailReason = "You are not meeting work requirements.";
                return result;
            }
            if ((caseInfo.FatherAge < 20 && !caseInfo.FatherInSchool))
            {
                result.Pass = false;
                result.FailReason = "You are not meeting work requirements.";
                return result;
            }
            if (caseInfo.MotherAge < 20 && !caseInfo.MotherInSchool)
            {
                result.Pass = false;
                result.FailReason = "You are not meeting work requirements.";
                return result;
            }
            result.Pass = true;
            return result;
        }

        public static void LoadModel(IntentRequest intentRequest)
        {
            CaseInfo caseInfo = new CaseInfo();
            caseInfo.ChildName = intentRequest.Intent.Slots["child_name"].Value;
            caseInfo.ChildAge = Int32.Parse(intentRequest.Intent.Slots["child_age"].Value);
            caseInfo.SpecialNeeds = (intentRequest.Intent.Slots["special_needs"].Value.ToLower() == "yes");
            caseInfo.Residence = intentRequest.Intent.Slots["residence"].Value.ToLower() == "yes";
            caseInfo.Assets = intentRequest.Intent.Slots["assets"].Value.ToLower() == "yes";
            caseInfo.FatherAge = Int32.Parse(intentRequest.Intent.Slots["father_age"].Value);
            caseInfo.MotherAge = Int32.Parse(intentRequest.Intent.Slots["mother_age"].Value);
            caseInfo.MotherWorking = intentRequest.Intent.Slots["mother_working"].Value.ToLower() == "yes";
            caseInfo.FatherWorking = intentRequest.Intent.Slots["father_working"].Value.ToLower() == "yes";
            caseInfo.Income = Int32.Parse(intentRequest.Intent.Slots["income_job"].Value);
            caseInfo.IncomeOther = Int32.Parse(intentRequest.Intent.Slots["income_other"].Value);
            caseInfo.Expense = Int32.Parse(intentRequest.Intent.Slots["expense"].Value);
            caseInfo.FatherInSchool = intentRequest.Intent.Slots["father_in_school"].Value!=null && intentRequest.Intent.Slots["father_in_school"].Value.ToLower()=="yes";
            caseInfo.MotherInSchool = intentRequest.Intent.Slots["mother_in_school"].Value != null && intentRequest.Intent.Slots["mother_in_school"].Value.ToLower() == "yes";

            if (Helpers.caseInfo != null)
            {
                caseInfo.AdditionalQuestions = Helpers.caseInfo.AdditionalQuestions;
            }
            Helpers.caseInfo = caseInfo;
        }
    }

    public class EligibilityResult
    {
        public bool Pass { get; set; }
        public string FailReason { get; set; }
    }
}
