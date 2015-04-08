modules.webcommons.factory('dispatcherService', function ($injector, $log,$q) {
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

        loadBaseDeferred:function() {
            var deferred = $q.defer();
            var promise = deferred.promise;

            promise.success = function (fn) {
                promise.then(fn);
                return promise;
            }
            promise.error = function (fn) {
                promise.then(null, fn);
                return promise;
            }
            return deferred;
        }
    };

});


