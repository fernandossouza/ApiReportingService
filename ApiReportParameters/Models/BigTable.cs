using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiReportParameters.Models
{
    public class BigTable
    {
        public int thingId { get; set; }
        public List<BigTableTag> tags { get; set; }
    }
}