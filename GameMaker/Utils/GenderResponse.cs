using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameMaker.Utils
{
    public class GenderResponse
    {
        public string name { get; set; }
        public string name_sanitized { get; set; }
        public string country { get; set; }
        public string gender { get; set; }
        public int samples { get; set; }
        public int accuracy { get; set; }
        public string duration { get; set; }
        public int credits_used { get; set; }
    }
}
