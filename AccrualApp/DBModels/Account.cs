using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class Account
    {
        public Account()
        {
            Budget = new HashSet<Budget>();
            Transaction = new HashSet<Transaction>();
        }

        public string AccountId { get; set; }
        public int AccountTypeId { get; set; }
        public string AccountNum { get; set; }
        public string AccountName { get; set; }
        public string Description { get; set; }

        public virtual AccountType AccountType { get; set; }
        public virtual ICollection<Budget> Budget { get; set; }
        public virtual ICollection<Transaction> Transaction { get; set; }
    }
}
