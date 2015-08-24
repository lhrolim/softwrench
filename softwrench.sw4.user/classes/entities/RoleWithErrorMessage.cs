using System;

namespace softwrench.sw4.user.classes.entities {
    /// <summary>
    /// This is a role with an error message associated,so that, even if the user has a role, it will only be authorized if the flag is set to true.
    /// </summary>
    //TODO: make it persistent, and modify the RoleManager properly, so that user that are not associated gain a Instance with Authorized=false 
    public class RoleWithErrorMessage : Role {

        /// <summary>
        /// If the Authorized flag is false, this message will be thrown as an exception on screen 
        /// </summary>
        public string UnauthorizedMessage { get; set; }


        public Boolean Authorized { get; set; }

        public override bool Active { get { return true; } }
    }
}
