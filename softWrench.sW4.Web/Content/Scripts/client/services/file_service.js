/*Used to download files via a child class of FileDownloadController instead of the wondow.local reload. 
 *This gives us access to the callbacks for success and failure.
 */
var app = angular.module('sw_layout');

app.factory('fileService', function ($rootScope, $timeout, i18NService) {

    return {
        download: function(url, successCallback, failCallback) {
            $.fileDownload(url, {
                successCallback: function (html, url) {
                    successCallback(html,url);
                },
                failCallback: function (html, url) {
                    failCallback(html,url);
                }
            });
        }
    }
});


