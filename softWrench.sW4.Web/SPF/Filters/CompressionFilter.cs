using System.Net.Http;
using System.Web.Http.Filters;
using cts.commons.Util;

namespace softWrench.sW4.Web.SPF.Filters {
    public class CompressionFilter : ActionFilterAttribute {

        public override void OnActionExecuted(HttpActionExecutedContext actContext) {

            if (actContext.Response == null) {
                //playing safe here, on case of exceptions
                base.OnActionExecuted(actContext);
                return;
            }

            var content = actContext.Response.Content;
            if (content is PushStreamContent) {
                // SSE: can't preprocess response otherwise eventsource request remains pending indefinetely
                base.OnActionExecuted(actContext);
                return;
            }

            var bytes = content == null ? null : content.ReadAsByteArrayAsync().Result;
            var zlibbedContent = bytes == null ? new byte[0] : CompressionUtil.Compress(bytes);
            actContext.Response.Content = new ByteArrayContent(zlibbedContent);
            actContext.Response.Content.Headers.Remove("Content-Type");
            actContext.Response.Content.Headers.Add("Content-encoding", "gzip");
            actContext.Response.Content.Headers.Add("Content-Type", "application/json");
            base.OnActionExecuted(actContext);
        }

    }
}