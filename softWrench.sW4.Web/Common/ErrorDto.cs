using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace softWrench.sW4.Web.Common {
    public class ErrorDto {
        public string ErrorMessage { get; set; }
        public string ErrorStack { get; set; }
        public string FullStack { get; set; }


        public ErrorDto(string errorMessage, string errorStack, string fullStack) {
            ErrorMessage = errorMessage;
            ErrorStack = errorStack;
            FullStack = fullStack;
        }

        public ErrorDto(Exception rootException) {
            ErrorMessage = GetErrorMessage(rootException);
            ErrorStack = rootException.StackTrace;
            FullStack = rootException.StackTrace;
        }

        private static string GetErrorMessage(Exception rootException) {
            const string errorNested = "nested exception is: psdi.util.MXApplicationException:";
            return rootException == null ? null : rootException.Message.Replace(errorNested, "");
        }
    }
}