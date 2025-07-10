using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsoBoiler.Tests.Helpers
{
    public class ExampleResponse
    {
        public string RequestName { get; set; } = string.Empty;
        public string RequestDescription { get; set; } = string.Empty;
        public int RequestID { get; set; }
        public int ResponseID { get; set; }
        public bool Successful { get; set; }
    }
}
