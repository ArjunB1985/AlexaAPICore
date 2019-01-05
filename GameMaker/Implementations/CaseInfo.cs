using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaker.Implementations
{
    public class CaseInfo
    {
        public List<KeyValuePair<string, string>> DataElements { get; set; } = new List<KeyValuePair<string, string>>();
        public string LastAskedSlot="";
        public string ChildName { get; set; }
        
        public int ChildAge { get; set; }

        public bool SpecialNeeds { get; set; }
        public bool Residence { get; set; }
        public int FatherAge { get; set; }
        public int MotherAge { get; set; }
        public bool MotherWorking { get; set; }
        public bool FatherWorking { get; set; }
        public bool MotherInSchool { get; set; }
        public bool FatherInSchool { get; set; }
        public bool FatherSelfEmployed { get; set; }
        public bool MotherSelfEmployed { get; set; }
        public int Income { get; set; }
        public int IncomeOther { get; set; }
        public int Expense { get; set; }
        public bool Assets { get; set; }

        public List<AdditionalQuestions> AdditionalQuestions { get; set; }
        public bool ModelComplete { get; set; }
        public bool ErrorFlag { get; set; }
    }

    public class AdditionalQuestions
    {
        public string SlotName { get; set; }

        public bool Status { get; set; }
    }
}
