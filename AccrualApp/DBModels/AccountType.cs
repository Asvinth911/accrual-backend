using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class AccountType
    {
        public AccountType()
        {
            Account = new HashSet<Account>();
        }

        public int AccountTypeId { get; set; }
        public string AccountTypeName { get; set; }
        public string BsLevel1 { get; set; }
        public string BsLevel2 { get; set; }
        public string BsLevel3 { get; set; }
        public string BsLevel4 { get; set; }
        public string PAndL { get; set; }
        public string CashFlow { get; set; }
        public string ConsolidatedAdjustedEbitda { get; set; }

        public virtual ICollection<Account> Account { get; set; }
    }
}
