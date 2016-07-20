using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using softwrench.sw4.api.classes.exception;
using softwrench.sw4.api.classes.integration;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Util;

namespace softWrench.sW4.Exceptions {
    public class ErrorDto : IErrorDto {

        public string ErrorMessage {
            get; set;
        }
        public string WarnMessage {
            get; set;
        }
        public string ErrorStack {
            get; set;
        }
        public string FullStack {
            get; set;
        }

        public bool NotifyException {
            get; set;
        }

        [JsonIgnore]
        public Type ErrorNativeType {
            get; set;
        }
        public string ErrorType {
            get {
                return ErrorNativeType == null ? null : ErrorNativeType.Name;
            }
        }
        public string OutlineInformation {
            get; set;
        }
        public TargetResult ResultObject {
            get; set;
        }


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
            OutlineInformation = ExceptionUtil.FirstProjectStackTraceLine(rootException);
            var baseSwException = rootException as BaseSwException;
            if (baseSwException != null) {
                NotifyException = baseSwException.NotifyException;
            }

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