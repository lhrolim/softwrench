﻿using System;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces
{
    public interface IApplicationAttributeDisplayable :IApplicationDisplayable
    {
        String Attribute { get; set; }

        String Label { get; set; }

        string Qualifier { get;}    


    }
}