(function ($, angular, _) {
    "use strict";

    /**
     * Allows screen tilt in mobile devices (required for iOS).
     * 
     * @param {Number} degrees 
     * @returns {Boolean} 
     */
    window.shouldRotateToOrientation = degrees => true;

    window.isRippleEmulator = () => document.URL.indexOf("http://") >= 0 || document.URL.indexOf("https://") >= 0;

    /**
     * Function that returns the angular $scope attached to an element.
     * It helps debug the app when deployed in Ripple (batarang's $scope inspection doesn't work in iframe);
     * 
     * @param {} element DOM element 
     * @returns {} $scope 
     */
    window.$s = element => {
        const elementWrapper = angular.element(element);
        return !angular.isFunction(elementWrapper["scope"]) ? null : elementWrapper.scope();
        //if (!scope || !scope['$parent']) {
        //    return scope;
        //}
        //return scope.$parent;
    };

    window.Validate = Object.freeze(class {
        static notNull(value, name) {
            if (value === undefined || value === null) throw new Error(`${name || "value"} cannot be null`);
        }
        static notEmpty(value, name) {
            Validate.notNull(value, name);
            if (_.isString(value) && !value) throw new Error(`String ${name || "value"} cannot be empty`);
            if (_.isArray(value) && value.length <= 0) throw new Error(`Array ${name || "value"} cannot be empty`);
            if (_.isObject(value) && _.isEmpty(value)) throw new Error(`Object ${name || "value"} cannot be empty`);
        }
    });

    window.getService = name => angular.element(document.body).injector().get(name);

    window.offlineFullClear = function () {
        var dao = angular.element(document.body).injector().get("swdbDAO");
        dao.dropDataBase()
            .then(() => {
                console.log("database dropped");
                localStorage.clear();
                console.log("localStorage cleared");
                sessionStorage.clear();
                console.log("sessionStorage cleared", "reloading application");
                location.reload();
            })
            .catch(e => console.error("Failed to drop database", e));
    }

    window.getResourcePath = function (path) {
        if (!window.lastreleasebuildtime) {
            return path;
        }
        return path + "?" + window.lastreleasebuildtime;
    }
})(jQuery, angular, _);


