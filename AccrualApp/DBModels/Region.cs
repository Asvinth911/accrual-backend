using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class Region
    {
        public Region()
        {
            Budget = new HashSet<Budget>();
            Customer = new HashSet<Customer>();
            Transaction = new HashSet<Transaction>();
        }

        public string RegionId { get; set; }
        public string RegionName { get; set; }

        public virtual ICollection<Budget> Budget { get; set; }
        public virtual ICollection<Customer> Customer { get; set; }
        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
