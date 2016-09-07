using System;
using softwrench.sw4.user.classes.entities;

namespace softWrench.sW4.Email {
    public class BaseEmailDto {
        public string Customer { get; set; }
        public User CurrentUser { get; set; }
        public string ChangedByFullName { get; set; }
        public DateTime ChangedOnUTC { get; set; }
        public string IPAddress { get; set; }
        public string Comment { get; set; }
        public string SendTo { get; set; }
        public string SentBy { get; set; }
        public string Subject { get; set; }
    }
}