(function (angular) {
    "use strict";
    // https://www.npmjs.com/package/angular-clipboard
    // adapted to use clipboard.js
    angular.module("angular-clipboard", [])
    .directive("clipboard", ["$q", function ($q) {
        // https://github.com/lgarron/clipboard.js
        var clipboard = {};

        clipboard.copy = (function () {
            var _intercept = false;
            var _data; // Map from data type (e.g. "text/html") to value.

            document.addEventListener("copy", function (e) {
                if (_intercept) {
                    _intercept = false;
                    angular.forEach(_data, function(value, key) {
                        e.clipboardData.setData(key, value);
                    });
                    e.preventDefault();
                }
            });

            return function (data) {
                var deferred = $q.defer();
                _intercept = true; // Race condition?
                _data = (typeof data === "string" ? { "text/plain": data } : data);
                try {
                    if (document.execCommand("copy")) {
                        // document.execCommand is synchronous: http://www.w3.org/TR/2015/WD-clipboard-apis-20150421/#integration-with-rich-text-editing-apis
                        // So we can call resolve() back here.
                        deferred.resolve();
                    }
                    else {
                        _intercept = false;
                        deferred.reject(new Error("Unable to copy. Perhaps it's not available in your browser?"));
                    }
                } catch (e) {
                    _intercept = false;
                    deferred.reject(e);
                }
                return deferred.promise;
            };
        }());

        clipboard.paste = (function () {
            var _intercept = false;
            var _resolve;
            var _dataType;

            document.addEventListener("paste", function (e) {
                if (_intercept) {
                    _intercept = false;
                    e.preventDefault();
                    _resolve(e.clipboardData.getData(_dataType));
                }
            });

            return function (dataType) {
                var deferred = $q.defer();
                _intercept = true; // Race condition?
                _resolve = deferred.resolve.bind(deferred);
                _dataType = dataType || "text/plain";
                try {
                    if (!document.execCommand("paste")) {
                        _intercept = false;
                        deferred.reject(new Error("Unable to paste. Perhaps it's not available in your browser?"));
                    }
                } catch (e) {
                    _intercept = false;
                    deferred.reject(new Error(e));
                }
                return deferred.promise;
            };
        }());

        // Handle IE behaviour.
        if (typeof ClipboardEvent === "undefined" &&
            typeof window.clipboardData !== "undefined" &&
            typeof window.clipboardData.setData !== "undefined") {

            clipboard.copy = function (data) {
                var deferred = $q.defer();
                // IE supports string and URL types: https://msdn.microsoft.com/en-us/library/ms536744(v=vs.85).aspx
                // We only support the string type for now.
                if (typeof data !== "string" && !("text/plain" in data)) {
                    deferred.reject(new Error("You must provide a text/plain type."));
                }

                var strData = (typeof data === "string" ? data : data["text/plain"]);
                var copySucceeded = window.clipboardData.setData("Text", strData);
                copySucceeded ? deferred.resolve() : deferred.reject(new Error("Copying was rejected."));
                return deferred.promise;
            }

            clipboard.paste = function (data) {
                var deferred = $q.defer();
                var strData = window.clipboardData.getData("Text");
                if (strData) {
                    deferred.resolve(strData);
                } else {
                    // The user rejected the paste request.
                    deferred.reject(new Error("Pasting was rejected."));
                }
                return deferred.promise;
            }
        }
        window.clipboard = clipboard;

        return {
            restrict: "A",
            scope: {
                onCopied: "&",
                onError: "&",
                text: "="
            },
            link: function (scope, element) {
                // copies the text to clipboard
                var copy = function (event) {
                    clipboard.copy(scope.text).then(function () {
                        if (scope.onCopied) scope.onCopied();
                    }).catch(function (err) {
                        if (scope.onError) scope.onError({ err: err });
                    });
                };
                // bind click event to copy callback
                element.on("click", copy);
                // unbind when $scope gets destroyed
                scope.$on("$destroy", function() {
                    element.off("click", copy);
                });
            }
        };
    }]);


})(angular);