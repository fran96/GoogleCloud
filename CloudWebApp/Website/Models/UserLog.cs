using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class UserLog
    {
        public string Key { get; set; }
        public string Email { get; set; }
        public DateTime LoggedOn { get; set; }
        public DateTime? LoggedOut { get; set; }
        public long Id { get; set; }
    }
}