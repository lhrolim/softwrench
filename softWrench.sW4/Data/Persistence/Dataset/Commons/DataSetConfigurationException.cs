﻿using System;
using cts.commons.portable.Util;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    public class DataSetConfigurationException : Exception {

        public DataSetConfigurationException(string message)
            : base(message) {

        }

        public static DataSetConfigurationException SWDBApplicationRequired(Type type) {
            return new DataSetConfigurationException("Error in DataSet {0}. Swdb DataSets must refer to an application starting with _. Rename it to _{0}".Fmt(type.Name));
        }

    }
}