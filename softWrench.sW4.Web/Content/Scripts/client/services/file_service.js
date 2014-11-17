/*Used to download files via a child class of FileDownloadController instead of the wondow.local reload. 
*This gives us access to the callbacks for success and failure.
*/
var app = angular.module('sw_layout');

app.factory('fileService', function ($rootScope) {

    return {
        download: function (url, successCallback, failCallback) {
            //needed since this is non-ajax call
            url = removeEncoding(url);

            //this is for emulating the busy cursor, since this is not an ajax call
            $rootScope.$broadcast('sw_ajaxinit');


            $.fileDownload(url, {
                successCallback: function (html, url) {
                    //this is for removing the busy cursor
                    $rootScope.$broadcast('sw_ajaxend');
                    if (successCallback) {
                        successCallback(html, url);
                    }
                },
                failCallback: function (html, url) {
                    //this is for removing the busy cursor
                    $rootScope.$broadcast('sw_ajaxend');
                    if (failCallback) {
                        failCallback(html, url);
                    }
                }
            });
        }
    }
});