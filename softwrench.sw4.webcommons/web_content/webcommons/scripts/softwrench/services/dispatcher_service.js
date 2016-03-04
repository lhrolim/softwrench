(function (modules) {
    "use strict";

    modules.webcommons.factory('dispatcherService', ["$injector", "$log", "$q", "$rootScope", function ($injector, $log, $q, $rootScope) {
    var loadService = function(service, method) {
        var log = $log.getInstance('dispatcherService#loadService');

        if (service === undefined || method === undefined) {
            return null;
        }
        //see clientawareserviceprovider.js
        var dispatcher = $injector.getInstance(service);
        if (!dispatcher) {
            log.warn('Service {0} missing '.format(service));
            return null;
        }
        var fn = dispatcher[method];
        if (!fn) {
            log.warn('Method {0} not found on service {1}'.format(method, service));
            return null;
        }
        return fn;
    };
    return {

        loadServiceByString:function(serviceString) {
            var serviceArray = serviceString.split(".");
            if (serviceArray.length != 2) {
                throw new Error("wrong metadata configuration. service string should be 'servicexxx.methodxxx'".format(serviceString));
            }
            return this.loadService(serviceArray[0], serviceArray[1]);
        },

        loadService: function(service, method) {
            return loadService(service, method);
        },

        invokeService: function(service, method, parameters) {
            var fn = this.loadService(service, method, parameters);
            if (fn == null) {
                return null;
            }
            if (parameters != null) {
                var args = [];
                for (var i = 0; i < parameters.length; i++) {
                    args.push(parameters[i]);
                }
                return fn.apply(this, args);
            }
        },

        invokeServiceByString: function (serviceString, parameters) {
            var serviceArray = serviceString.split(".");
            if (serviceArray.length != 2) {
                return null;
            }
            return this.invokeService(serviceArray[0], serviceArray[1], parameters);
        },



        /// <summary>
        /// use to create a default instance of the deferred/promise instance, useful for transforming sync methods into async ones, and for chaining them.
        /// 
        /// Add success/error methods on top of the promise, to make it more uniform along with other jquery/angular services (such as $http)
        /// 
        /// </summary>
        /// <returns type="">a deferred object</returns>
        loadBaseDeferred: function () {
            
            var deferred = $q.defer();
            var promise = deferred.promise;

            promise.success = function (fn) {
                promise.then(fn);
                return promise;
            }
            promise.error = function (fn) {
                promise.catch(fn);
                return promise;
            }
            return deferred;
        },

        dispatchevent: function (eventName) {
            $rootScope.$broadcast(eventName, Array.prototype.slice.call(arguments, 1));
        }
    };

}]);

})(modules);