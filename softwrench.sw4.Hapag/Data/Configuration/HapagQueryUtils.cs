﻿using System;

namespace softwrench.sw4.Hapag.Data.Configuration {
    internal class HapagQueryUtils {
        
        internal static string GetDefaultQuery(string qualifier = "") {
            var replacement = String.Empty;
            
            if (!String.IsNullOrWhiteSpace(qualifier))
            {
                replacement = qualifier;
            }

            return String.Format(HapagQueryConstants.DefaultQualifiedQuery, replacement);
        }
    }
}
