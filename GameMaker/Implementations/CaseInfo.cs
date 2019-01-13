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
        public DateTime CreateDateTime { get; set; }

        public int Income { get; set; }
        
        public int MemberCount { get; set; }
        public bool ErrorFlag { get; set; }
        public string Gender { get; set; }
        public string EligibilityGroup { get; set; }
    }

    public class AdditionalQuestions
    {
        public string SlotName { get; set; }

        public bool Status { get; set; }
    }
}
