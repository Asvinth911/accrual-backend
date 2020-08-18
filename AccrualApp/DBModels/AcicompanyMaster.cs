using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class AcicompanyMaster
    {
        public AcicompanyMaster()
        {
            Acibudget = new HashSet<Acibudget>();
            Aciprojection = new HashSet<Aciprojection>();
        }

        public int AcicompanyId { get; set; }
        public string AcicompanyName { get; set; }
        public string QbcompanyId { get; set; }

        public virtual ICollection<Acibudget> Acibudget { get; set; }
        public virtual ICollection<Aciprojection> Aciprojection { get; set; }
    }
}
