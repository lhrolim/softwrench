using System;

namespace softWrench.sW4.SimpleInjector {
    
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute :Attribute{
        
        /// <summary>
        /// specifies a different type for this registration
        /// </summary>
        public virtual Type RegistrationType { get; set; }

        public virtual String Name { get; set; }

    }
}
