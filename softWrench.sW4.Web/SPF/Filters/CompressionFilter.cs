﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.SPF.Filters {
    public class CompressionFilter : ActionFilterAttribute {

        public override void OnActionExecuted(HttpActionExecutedContext actContext) {
            if (actContext.Response == null) {
                //playing safe here, on case of exceptions
                base.OnActionExecuted(actContext);
                return;
            }
             var content = actContext.Response.Content;
            var bytes = content == null ? null : content.ReadAsByteArrayAsync().Result;
            var zlibbedContent = bytes == null ? new byte[0] :
            CompressionUtil.Compress(bytes);
            actContext.Response.Content = new ByteArrayContent(zlibbedContent);
            actContext.Response.Content.Headers.Remove("Content-Type");
            actContext.Response.Content.Headers.Add("Content-encoding", "gzip");
            actContext.Response.Content.Headers.Add("Content-Type", "application/json");
            base.OnActionExecuted(actContext);
        }

    }
}
