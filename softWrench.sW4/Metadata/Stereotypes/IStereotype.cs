using System;
using System.Collections.Generic;

namespace softWrench.sW4.Metadata.Stereotypes
{
    public interface IStereotype
    {
//        string LookupValue(string key);

        IDictionary<string, string> StereotypeProperties();

        IStereotype Merge(IStereotype stereotype);

     

    }
}