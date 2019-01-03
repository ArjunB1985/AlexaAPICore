using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace QuestionGenerator
{
    public class QuestionAPIInterface
    {
        public string GET(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    return reader.ReadToEnd();
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

        public RootObject GetQuestions()
        {
            QuestionAPIInterface api = new QuestionAPIInterface();
            string value = api.GET(@"https://opentdb.com/api.php?amount=10&category=17&difficulty=easy&type=multiple");
            RootObject root = JsonConvert.DeserializeObject<RootObject>(value);
            return root;
        }

    }
}
