using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiReportParameters.Models
{
    public class BigTableTag
    {
        public string name { get; set; }
        public string group { get; set; }
        public List<long> timestamp { get; set; }
        public List<string> value { get; set; }
    }
}