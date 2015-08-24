using System.Text;
using System.Text.RegularExpressions;
using log4net.Appender;
using log4net.Core;

namespace softWrench.sW4.Util {
    /// <summary>
    /// taken from http://www.codeproject.com/Articles/249154/Logging-NHibernate-queries-with-parameters
    /// </summary>
    public class NhAppender : ForwardingAppender {
        protected override void Append(LoggingEvent loggingEvent) {
            var loggingEventData = loggingEvent.GetLoggingEventData();

            if (loggingEventData.Message.Contains("@p")) {
                var messageBuilder = new StringBuilder();

                string message = loggingEventData.Message;
                var queries = Regex.Split(message, @"command\s\d+:");

                foreach (var query in queries)
                    messageBuilder.Append
                    (ReplaceQueryParametersWithValues(query));

                loggingEventData.Message = messageBuilder.ToString();
            }

            base.Append(new LoggingEvent(loggingEventData));
        }

        private static string ReplaceQueryParametersWithValues(string query) {
            return Regex.Replace(query, @"@p\d(?=[,);\s])(?!\s*=)", match => {
                Regex parameterValueRegex = new Regex
                (string.Format(@".*{0}\s*=\s*(.*?)\s*[\[].*", match));
                return parameterValueRegex.Match(query).Groups[1].ToString();
            });
        }
    }
}
