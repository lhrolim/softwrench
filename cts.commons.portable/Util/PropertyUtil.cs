﻿using System;
using System.Collections.Generic;

namespace cts.commons.portable.Util {
    public class PropertyUtil {


        public static IDictionary<string, object> ConvertToDictionary(string propertyString, Boolean throwException = true) {
            try {
                var result = new Dictionary<string, object>();
                if (propertyString == null) {
                    return result;
                }
                if (propertyString.EndsWith(";")) {
                    propertyString = propertyString.Substring(0, propertyString.Length - 1);
                }
                var strings = propertyString.Split(';');
                foreach (String s in strings) {
                    var equals = s.IndexOf("=");
                    //TODO: Make key lower cases... 
                    result.Add(s.Substring(0, equals).Trim(), s.Substring(equals + 1, s.Length - equals - 1).Trim());
                }
                return result;
            } catch {
                if (throwException) {
                    throw new InvalidOperationException(String.Format("Error parsing property string {0}", propertyString));
                }
                return new Dictionary<string, object>();
            }
        }
    }
}