using System;

namespace softwrench.sW4.Shared.Metadata.Applications.Schema.Interfaces
{
    public interface IApplicationAttributeDisplayable :IApplicationDisplayable
    {
        String Attribute { get; }
    }
}