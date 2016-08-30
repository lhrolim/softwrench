using System;

namespace cts.commons.simpleinjector {
    
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute :Attribute{
        
        /// <summary>
        /// specifies a different type for this registration
        /// </summary>
        public virtual Type RegistrationType { get; set; }

        public virtual string Name { get; set; }


        public virtual string ClientFilters {get; set;}

    }
}
