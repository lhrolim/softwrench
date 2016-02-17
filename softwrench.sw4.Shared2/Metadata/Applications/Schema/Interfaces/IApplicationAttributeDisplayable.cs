using System;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces
{
    public interface IApplicationAttributeDisplayable : IApplicationIndentifiedDisplayable
    {

        new String Label { get; set; }

        string Qualifier { get;}

        bool IsHidden { get; set; }


    }
}