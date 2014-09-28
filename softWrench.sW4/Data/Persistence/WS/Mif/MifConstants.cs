using System;
using System.Configuration;

namespace softWrench.sW4.Data.Persistence.WS.Mif
{
    internal class MifConstants
    {
        public static readonly string LANGCODE = "EN";
        public static readonly string SupportEmail = String.Empty;
        public static readonly string EmailHost = String.Empty;

        public static readonly string baseWsUrl = ConfigurationManager.AppSettings["baseWSURL"];
        public static readonly string baseWsPrefix = ConfigurationManager.AppSettings["baseWSPrefix"];


    }
}
