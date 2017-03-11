(function (angular, modules) {
    "use strict";

//    angular.module('sw_layout').factory('$xhrFactory', function () {
//        return function createXhr(method, url) {
//            const isNoDigest = url.startsWith("nodigest:");
//            if (!isNoDigest) {
//                return new window.XMLHttpRequest();
//            }
//            const xhr = new window.XMLHttpRequest();
//
//            const originalOpenFn = xhr.open.bind(xhr);
//            const originalgetHeadersFn = xhr.getAllResponseHeaders.bind(xhr);
//            const originalLoad = xhr.onload.bind(xhr);
//
//            xhr.open = (method, url, async) => {
//                return originalOpenFn(method, url.substring(9), async);
//            }
//
//            xhr.getAllResponseHeaders = () => {
//                var headerSt = originalgetHeadersFn();
//                return headerSt + "ignoredigest:true";
//            }
//
//            xhr.onload = () => {
//                originalLoad();
//            }
//
//            return xhr;
//
//        };
//    });

    angular.module('sw_layout').config([
        '$provide', '$httpProvider', function ($provide, $httpProvider) {
            $provide.decorator('$httpBackend', ["$delegate", "$rootScope", function ($delegate, $rootScope) {

                if (angular.mock) {
                    //do not register for unit tests
                    return $delegate;
                }

                const decoratedHttpBackend = function (method, url, data, callback, headers, timeout, withCredentials, responseType) {
                    if (!url.startsWith("nodigest:")) {
                        return $delegate.call(this, method, url, data, callback, headers, timeout, withCredentials, responseType);
                    }
                    const parsedUrl = url.substring(9);
            
                    return $delegate.call(this, method, parsedUrl, data, function(status, response, headersString, statusText) {
                        $rootScope.$$phase = true;
                        $httpProvider.useApplyAsync(false);
                        callback(status, response, headersString, statusText);
                        $httpProvider.useApplyAsync(true);
                        $rootScope.$$phase = undefined;
                    }, headers, timeout, withCredentials, responseType);

                };

                return decoratedHttpBackend;
            }]);

        }
    ]);


})(angular, modules);