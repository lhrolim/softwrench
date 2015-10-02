using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Common {
    public class ErrorDto {

        public string ErrorMessage { get; set; }
        public string ErrorStack { get; set; }
        public string FullStack { get; set; }
        [JsonIgnore]
        public Type ErrorNativeType { get; set; }
        public string ErrorType { get { return ErrorNativeType.Name; } }
        public string OutlineInformation { get; set; }


        public ErrorDto(string errorMessage, string errorStack, string fullStack) {
            ErrorMessage = PrettyMessage(errorMessage);
            ErrorStack = errorStack;
            FullStack = fullStack;
        }

        public ErrorDto([NotNull]Exception rootException) {
            ErrorMessage = GetErrorMessage(rootException);
            ErrorStack = rootException.StackTrace;
            FullStack = rootException.StackTrace;
            ErrorNativeType = rootException.GetType();
            OutlineInformation = ExceptionUtil.LastStackTraceLine(rootException);
        }

        private static string PrettyMessage(string message) {
            const string errorNested = "nested exception is: psdi.util.MXApplicationException:";
            return message.Replace(errorNested, "");
        }

        private static string GetErrorMessage(Exception rootException) {
            return PrettyMessage(rootException.Message);
        }
    }
}