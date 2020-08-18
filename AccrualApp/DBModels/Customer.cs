using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class Customer
    {
        public Customer()
        {
            Budget = new HashSet<Budget>();
            Transaction = new HashSet<Transaction>();
        }

        public string CustomerId { get; set; }
        public string RegionId { get; set; }
        public string CustomerName { get; set; }

        public virtual Region Region { get; set; }
        public virtual ICollection<Budget> Budget { get; set; }
        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
