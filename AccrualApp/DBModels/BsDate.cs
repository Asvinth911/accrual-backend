using System;
using System.Collections.Generic;

namespace AccrualApp.DBModels
{
    public partial class BsDate
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
    }
}
