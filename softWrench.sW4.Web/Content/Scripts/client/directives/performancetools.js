//ie9 fix, since performance object doesn´t exist
if (!performance) {
    var performance = {};
}
if (!performance.now) {
    performance.now = function () {
        return new Date().getTime();
    };
}


app.directive('ngrepeatinspector', function ($timeout, $log) {

    "ngInject";

    return {
        restrict: 'A',
        scope: {
            name: "@"
        },
        link: function (scope, element, attr) {
            var name = scope.name == null ? "" : scope.name;
            var log = $log.getInstance('inspector#ngrepeat');
            if (scope.$first) {
                log.debug('init ngrepeat for {0}'.format(name));
            } else {
                log.trace('ngrepeat processed for {0}'.format(name));
            }
            if (scope.$last === true || (scope.datamap && scope.datamap.length == 0)) {
                log.debug('finish ngrepeat for {0}').format(name);
            }
        }
    };
});

app.directive('localClick', ['$parse', '$rootScope', '$exceptionHandler', function ($parse, $rootScope, $exceptionHandler) {

    "ngInject";

    var directiveName = 'localClick';
    var eventName = 'click';
    return {
        restrict: 'A',
        compile: function ($element, attr) {

            $rootScope.$localApply = function $localApply(expr) {
                try {
                    this.$$phase = '$apply';
                    return this.$eval(expr);
                } catch (e) {
                    $exceptionHandler(e);
                } finally {
                    this.$$phase = null;
                    try {
                        // instead of starting dirty checking at the root
                        // $rootScope.$digest();
                        // start at the scope where called
                        this.$digest();
                    } catch (e) {
                        $exceptionHandler(e);
                        throw e;
                    }
                }
            };

            var fn = $parse(attr[directiveName]);
            return function limitedClickHandler(scope, element) {
                element.on(eventName, function (event) {
                    var callback = function () {
                        fn(scope, { $event: event });
                    };
                    // use $localApply instead of $apply
                    $rootScope.$localApply(callback);
                });
            };
        }
    };
}])