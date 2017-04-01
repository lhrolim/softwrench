(function (angular, modules) {
    "use strict";

    modules.webcommons.config(['$provide', function ($provide) {

        $provide.decorator('$rootScope', ["$delegate", "$log", function ($delegate, $log) {
        const _emit = $delegate.$emit;
        const _broadcast = $delegate.$broadcast;
        const _on = $delegate.$on;

        $delegate.constructor.prototype.$onRootScope = function (name, listener) {
            var unsubscribe = $delegate.$on(name, listener);
            this.$on('$destroy', unsubscribe);
            return $delegate;
        }

        $delegate.$emit = function () {
            const log = $log.get("rootscope#emit");
            if (!arguments[0]) {
                throw new Error("event broadcasted with null value. Check JAvascriptEventConstants");
            }
            log.info(arguments[0]);
            log.debug(...arguments);
//            console.log.apply(console, arguments);
            return _emit.apply(this, arguments);
        };


        $delegate.$broadcast = function () {
            const log = $log.get("rootscope#broadcast");
            if (!arguments[0]) {
                throw new Error("event broadcasted with null value. Check JAvascriptEventConstants");
            }
            log.info(arguments[0]);
            log.debug(...arguments);
            //            console.log.apply(console, arguments);
            return _broadcast.apply(this, arguments);
        };


        $delegate.$on = function () {
            const log = $log.get("rootscope#on");
            log.info(arguments[0]);
            log.debug(...arguments);
            //            console.log.apply(console, arguments);
            return _on.apply(this, arguments);
        };

        return $delegate;
    }]);
}]);

})(angular, modules);