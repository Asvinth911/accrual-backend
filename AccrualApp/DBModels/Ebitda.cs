using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class Ebitda
    {
        public int Id { get; set; }
        public string AccountNum { get; set; }
        public string ConsolidatedAdjustedEbitda { get; set; }
        public string Type { get; set; }
    }
}
