using SSTC_BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTC.Modules.Calculator.ModuleInternals
{
    class PresentedResultantSpan
    {
        // PROPS
        public string OrdinalDescription { get; }
        public double Solution { get; }
        public ResultantSpan SpanPresentedResults { get; }
        // CTOR
        public PresentedResultantSpan(string ordinalDescription, double solution, ResultantSpan resultantSpan)
        {
            OrdinalDescription = ordinalDescription;
            Solution = solution;
            SpanPresentedResults = resultantSpan;
        }
    }
}
