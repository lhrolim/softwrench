﻿namespace softwrench.sw4.user.classes.entities {


    /// <summary>
    /// This data will come from Maximo 
    /// </summary>
    public class Person
    {
        private string _language;

        public string Password { get; set; }

        public virtual string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Email { get; set; }

        public string SiteId { get; set; }
        
        public string OrgId { get; set; }

        public string Department { get; set; }

        public string Phone { get; set; }

        public string Storeloc { get; set; }
        
        public virtual string Language {
            get { return _language == null ? null : _language.Trim().ToUpper(); }
            set { _language = value; }
        }

        // When the user is saved, we need to access the maximo database and update the person record
        public void Save()
        {
            
        }
    }
}