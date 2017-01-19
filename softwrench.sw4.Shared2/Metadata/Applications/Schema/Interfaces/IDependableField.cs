using System.Collections.Generic;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces
{
    public interface IDependableField
    {
        ISet<string> DependantFields { get;}
        string AssociationKey { get; }

        
    }
}