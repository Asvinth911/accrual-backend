using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class Acibudget
    {
        public int AcibudgetId { get; set; }
        public int AcicompanyId { get; set; }
        public int AcilineItemId { get; set; }
        public int AcicustomerId { get; set; }
        public DateTime AcitransactionDate { get; set; }
        public double Aciamount { get; set; }
        public DateTime Timestatmp { get; set; }

        public virtual AcicompanyMaster Acicompany { get; set; }
        public virtual AcicustomerMaster Acicustomer { get; set; }
        public virtual AciitemMaster AcilineItem { get; set; }
    }
}
