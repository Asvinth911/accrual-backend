using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class AcicustomerMaster
    {
        public AcicustomerMaster()
        {
            Acibudget = new HashSet<Acibudget>();
        }

        public int AcicustomerId { get; set; }
        public string AcicustomerName { get; set; }
        public int AcicompanyId { get; set; }
        public string QbcustomerId { get; set; }

        public virtual ICollection<Acibudget> Acibudget { get; set; }
    }
}
