(function (angular, $) {
    "use strict";

/*
 * Used to download files via a child class of FileDownloadController instead of the wondow.local reload. 
 * This gives us access to the callbacks for success and failure.
 */
angular.module("sw_layout")
    .factory("fileService", ["$rootScope", "contextService", "$q", function ($rootScope, contextService, $q) {

    return {

        /**
         * Downloads the file located at the url.
         * Mocks an ajax call by using cookies.
         * @see jQuery.fileDownload for more details
         * 
         * @param {String} url 
         * @param {Function<any, String, String>} successCallback (html, url) => ?
         * @param {Function<any, String, String>} failCallback (html, url) => ?
         */
        download: function (url, successCallback, failCallback) {
            //needed since this is non-ajax call
            url = removeEncoding(url);

            //this is for emulating the busy cursor, since this is not an ajax call
            $rootScope.$broadcast(JavascriptEventConstants.AjaxInit);

            $.fileDownload(url, {
                successCallback: function (html, url) {
                    //this is for removing the busy cursor
                    $rootScope.$broadcast(JavascriptEventConstants.AjaxFinished);
                    if (successCallback) {
                        successCallback(html, url);
                    }
                },
                failCallback: function (html, url, formUrl) {
                    //this is for removing the busy cursor
                    $rootScope.$broadcast(JavascriptEventConstants.ErrorAjax, html);
                    if (formUrl && formUrl.indexOf("SignIn") >= 0) {
                        //sessionStorage.removeItem("swGlobalRedirectURL");
                        contextService.clearContext();
                        //this means that the server wanted a redirection to login page (302), due to session expiration.
                        //Since we´re using an inner iframe the contents of the signin might not be show. Let´s redirect manually, and there´s no way to override that.
                        window.location.reload();
                    }
                    if (failCallback) {
                        failCallback(html, url);
                    }
                }
            });
        },

        /**
         * this.download promisified.
         * 
         * @param {String} url 
         * @returns {Promise} resolved with object containing url and html 
         */
        downloadPromise: function(url) {
            const q = $q.defer();
            this.download(url,
                (html, url) =>
                    q.resolve({ html, url })
                ,
                (html, url) => 
                    q.reject({ html, url })
                );
            return q.promise;
        },

        /**
         * Converts a base64 encoded string to a Blob.
         * 
         * @param {String} b64Data 
         * @param {String} contentType 
         * @param {Number?} sliceSize chunk size for performance, defaults to 512
         * @returns {Blob} 
         */
        base64ToBlob: function (b64Data, contentType, sliceSize) {
            contentType = contentType || "";
            sliceSize = sliceSize || 512;
            const byteCharacters = atob(b64Data);
            const byteArrays = [];
            for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
                const slice = byteCharacters.slice(offset, offset + sliceSize);
                const byteNumbers = new Array(slice.length);
                for (let i = 0; i < slice.length; i++) {
                    byteNumbers[i] = slice.charCodeAt(i);
                }
                const byteArray = new Uint8Array(byteNumbers);
                byteArrays.push(byteArray);
            }
            const blob = new Blob(byteArrays, { type: contentType });
            return blob;
        }
    };
}]);
 
})(angular, jQuery);