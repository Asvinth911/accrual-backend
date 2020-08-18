using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class AciitemMaster
    {
        public AciitemMaster()
        {
            Acibudget = new HashSet<Acibudget>();
            Aciprojection = new HashSet<Aciprojection>();
        }

        public int AcilineItemId { get; set; }
        public string AcilineItemName { get; set; }
        public string QbaccountNum { get; set; }
        public string QbaccountName { get; set; }
        public string AciitemCategory { get; set; }
        public int AciitemTypeId { get; set; }

        public virtual ICollection<Acibudget> Acibudget { get; set; }
        public virtual ICollection<Aciprojection> Aciprojection { get; set; }
    }
}
