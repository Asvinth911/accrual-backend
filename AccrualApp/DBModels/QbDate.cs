using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class QbDate
    {
        public DateTime Date { get; set; }
        public string Week { get; set; }
        public string Include { get; set; }
    }
}
