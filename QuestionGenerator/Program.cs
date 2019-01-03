using GameMaker.Implementations;
using GameMaker.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuestionGenerator
{
    class Program
    {
        public static void Main()
        {
            //QuestionAPIInterface api = new QuestionAPIInterface();
            //string value=api.GET(@"https://opentdb.com/api.php?amount=10&category=17&difficulty=easy&type=multiple");
            //RootObject root=JsonConvert.DeserializeObject<RootObject>(value);

            IGameController gameMaker = new ScoreKeepingGameController();
            var x = gameMaker.GetGameScore("amzn1.ask.account.AEC6AY6VTQEMLNQP6MBFYDKT3L5JBK3IERAHKEDSXVSNYA2BDXW5KXLQF6QK3XIAH7MRIQ2AFAAA44UWB6UAQ6O2TZJTSIFUNNNAPKBVGASJWA2BJQOEC5WM5FPII3ULBGW4E4WOLNBGRQ4LPGLOC4GOCB6FYJCEQGKKVQC3E7C5BMMTTLLYXF6RLZSTS3O7WV7AR3UGJVLTY5Y", null);

        }
    }
}
