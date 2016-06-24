using System;
using System.Web;
using System.Web.Mvc;

namespace cts.commons.web.Controller {
    /// <summary>
    /// taken from http://johnculviner.com/jquery-file-download-plugin-for-ajax-like-feature-rich-file-downloads/
    /// </summary>
    [System.Web.Mvc.Authorize]
    public abstract class FileDownloadController : System.Web.Mvc.Controller {

        private const string FILE_DOWNLOAD_COOKIE_NAME = "fileDownload";

        protected override void OnResultExecuting(ResultExecutingContext context) {
            CheckAndHandleFileResult(context);

            base.OnResultExecuting(context);
        }



        /// <summary>
        /// taken from http://johnculviner.com/jquery-file-download-plugin-for-ajax-like-feature-rich-file-downloads/
        /// If the current response is a FileResult (an MVC base class for files) then write a
        /// cookie to inform jquery.fileDownload that a successful file download has occured
        /// </summary>
        /// <param name="context"></param>
        private void CheckAndHandleFileResult(ResultExecutingContext context) {
            if (context.Result is FileResult) {
                //jquery.fileDownload uses this cookie to determine that a file download has completed successfully
                Response.SetCookie(new HttpCookie(FILE_DOWNLOAD_COOKIE_NAME, "true") { Path = "/" });
            } else {
                //ensure that the cookie is removed in case someone did a file download without using jquery.fileDownload
                var httpCookie = Request.Cookies[FILE_DOWNLOAD_COOKIE_NAME];
                if (httpCookie != null) {
                    httpCookie.Expires = DateTime.Now.AddYears(-1);
                }
            }
        }
    }
}
