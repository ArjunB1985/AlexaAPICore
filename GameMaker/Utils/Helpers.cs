using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using GameMaker.Implementations;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace GameMaker.Utils
{
    public static class Helpers
    {
        public static string GetGender(string name,string mode,string key)
        {
            if (mode != "PROD")
            {
                return "E";
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://gender-api.com/get?key="+key+"&name="+name);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    var responseJSON= reader.ReadToEnd();
                    GenderResponse root = JsonConvert.DeserializeObject<GenderResponse>(responseJSON);
                    if (root.accuracy > 80)
                    {
                        return root.gender;
                    }
                    else
                    {
                        return "E";
                    }
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw;
            }
        }

        public static async System.Threading.Tasks.Task SendEmailAsync(string emailid,string name,  string date, string time)
        {
            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress("mymail.arjun@gmail.com", "Benefind Assistant"));

            var recipients = new List<EmailAddress>
            {
             new EmailAddress(emailid, name),
            };
            msg.AddTos(recipients);

            msg.SetSubject("Your upcoming appointment");

            msg.AddContent(MimeType.Html, "<h2>Your upcoming appointment</h2><p> "+name+", You have an appointment at Benefind office on:  "+date+" at " +time+"</p>");

            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var response =  await client.SendEmailAsync(msg);



            //var apiKey = "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";
            //var client = new SendGridClient(apiKey);
            //var from = new EmailAddress("mymail.arjun@gmail.com", "Alexa Benefind Scheduler");
            //var subject = "Your upcoming appointment";
            //var to = new EmailAddress("mymail.arjun@gmail.com", "Arjun");
            //var plainTextContent = "and easy to do anywhere, even with C#";
            //var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            //var response = await client.SendEmailAsync(msg);


        }
        public static UserProfile GetUserProfile(SkillRequest alexaRequestInput)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.amazon.com/user/profile?access_token=" + alexaRequestInput.Context.System.User.AccessToken);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    var responseJSON = reader.ReadToEnd();
                    UserProfile root = JsonConvert.DeserializeObject<UserProfile>(responseJSON);
                    return root;
                }

            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw;
            }
        }

        private static List<KeyValuePair<string, CaseInfo>> CaseInfoCollection { get; set; } = new List<KeyValuePair<string, CaseInfo>>();
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
        private static CaseInfo caseInfo = new CaseInfo();
        public static CaseInfo GetCaseInfo(string sessionid)
        {
            //housekeeping clear any old sessions
            
            CaseInfoCollection.RemoveAll(p => p.Value.CreateDateTime.AddHours(1.0) < DateTime.Now);

            if (CaseInfoCollection.Select(p => p.Key).Contains(sessionid))
            {
                return CaseInfoCollection.First(p => p.Key == sessionid).Value;
            }
            else
            {
                var info = new KeyValuePair<string, CaseInfo>(sessionid, new CaseInfo() { CreateDateTime = DateTime.Now });
                CaseInfoCollection.Add(info);
                return info.Value;
            }
        }
        public static void RemoveCaseInfo(string sessionid)
        {
            CaseInfoCollection.RemoveAll(p => p.Key == sessionid);
        }

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
        
    }

    
    public class EligibilityResult
    {
        public bool Pass { get; set; }
        public string FailReason { get; set; }
    }
}
