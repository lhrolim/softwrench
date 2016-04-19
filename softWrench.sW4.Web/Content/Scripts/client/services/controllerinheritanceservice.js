
(function (angular) {
    "use strict";

    function controllerInheritanceService($injector) {
        //#region Utils: fluent api helper 'classes'
        /**
         * OverrideBuilder 'class'
         */
        var OverrideBuilder = (function () {
            /**
             * Constructor function (call with 'new' operator).
             * Initiates an override chain on the childCtrl:
             * ```
             * new OverrideBuider()
             *      .scope($scope, 'method_1' ...)...scope($scope, 'method_n')
             *      .controller(parentCtrl_1, 'method' ...)...controller(parentCtrl_n, ...)
             * ```
             * @param NgController childCtrl 
             * @returns OverrideBuilder instance 
             */
            function _OverrideBuilder(childCtrl) {
                this.childCtrl = childCtrl;
            };
            /**
             * Proxies a method on the obj object with the proxyMethod.
             * @param {} obj 
             * @param String methodName an obj's method's name
             * @param Function proxyMethod with the signature: 
             *          `(original: obj's original method unbinded, params: obj's original method's arguments as an array, context: context object passed) => {}`
             * @param {} [context] proxyMethod's `this`, defaults to instance if none is passed
             * @returns OverrideBuilder this instance 
             */
            _OverrideBuilder.prototype.proxy = function(obj, methodName, proxyMethod, context) {
                var original = obj[methodName];
                var ctx = context || obj;
                obj[methodName] = function() {
                    return proxyMethod.apply(ctx, [original, [].slice.call(arguments), ctx]);
                }
                return this;
            };
            /**
             * Helper around `#proxy` for proxying a NgController's NgScope method:
             * - defaults context to `this.childCtrl` if none is provided
             * @param NgScope scope 
             * @param String methodName
             * @param Function proxyMethod 
             * @param {} [context] defaults to `this.childCtrl` if not passed
             * @returns OverrideBuilder this instance
             */
            _OverrideBuilder.prototype.scope = function (scope, methodName, proxyMethod, context) {
                return this.proxy(scope, methodName, proxyMethod, context || this.childCtrl);
            };
            /**
             * Helper around `#proxy` for proxying a NgController's instance methods:
             * - passes context as `this.childCtrl`
             * @param AnnotatedNgController parentCtrl 
             * @param String methodName 
             * @param Function proxyMethod 
             * @returns OverrideBuilder this instance 
             */
            _OverrideBuilder.prototype.controller = function (parentCtrl, methodName, proxyMethod) {
                return this.proxy(parentCtrl, methodName, proxyMethod, this.childCtrl);
            };

            return _OverrideBuilder;
        })();
        /**
         * InheritanceBuilder 'class'
         */
        var InheritanceBuilder = (function () {
            /**
             * Constructor function (call with 'new' operator).
             * Initiates an inheritance chain on the childCtrl:
             * `new InheritanceBuilder(childCtrl).inherit(parentCtrl_1, {...})...inherit(parentCtrl_n)`.
             * 
             * @param NgController childCtrl 
             * @returns InheritanceBuilder instance 
             */
            function _InheritanceBuilder(childCtrl) {
                this.childCtrl = childCtrl;
            };
            /**
             * Makes `this.childCtrl` inherit from the parentCtrl (using `$injector.invoke` strategy)
             * @param AnnotatedNgController parentCtrl 
             * @param {} [locals] If preset then any argument names are read from this object first
             * @returns this InheritanceBuilder instance 
             */
            _InheritanceBuilder.prototype.inherit = function (parentCtrl, locals) {
                $injector.invoke(parentCtrl, this.childCtrl, locals);
                return this;
            };
            /**
             * Initiates an override chain on this.childCtrl:
             * ```
             * ...inherit(parent_n, ...).overrides()
             *      .scope($scope, 'method_1' ...)...scope($scope, 'method_n')
             *      .controller(parentCtrl_1, 'method' ...)...controller(parentCtrl_n, ...)
             * ```
             * @see OverrideBuilder for more informtion
             * @returns OverrideBuilder new instance (with same childCtrl)
             */
            _InheritanceBuilder.prototype.overrides = function() {
                return new OverrideBuilder(this.childCtrl);
            };
            return _InheritanceBuilder;
        })();
        //#endregion

        //#region Public methods
        /**
         * Initiates an inheritance chain on the childCtrl.
         * Complete example - inheriting from BaseController and CompositionListController 
         * and overriding $scope methods 'selectPage' (execute data transformation after page is selected), 
         * 'edit' (change a $scope flag variable before start editting) and 'hasLookupModel' (change the logic completely):
         * ```
         * function MyController($scope, $element, $attrs, controllerInheritanceService, ...other services...) {
         *   
         *  function transformDataList() { 
         *    // ...do data transformation... 
         *  }
         *   
         *  // init controller
         *  (function(self) {
         *      controllerInheritanceService.begin(self) // initiate 'inheritance' chain
         *          .inherit(BaseController, { 
         *              $scope: $scope 
         *          })
         *          .inherit(CompositionListController, { 
         *              $scope: $scope, 
         *              $element: $element, 
         *              $attrs: $attrs 
         *          })
         *          .overrides() // initiate 'override' chain
         *              .scope($scope, "selectPage", function(original, params, context) { 
         *                  var selectPagePromise = original.apply(context, params);
         *                  return selectPagePromise.then(transformCompositionList);
         *              })
         *              .scope($scope, "edit", function(original, params, context) {
         *                  $scope.isEditing = true;
         *                  original.apply(context, params);
         *              })
         *              .scope($scope, "hasLookupModel", function(original, params, context) {  
         *                  var schema = params[0];
         *                  return schema.applicationName === "SR";
         *              });
         * 
         *   })(this); // self as `this` (the controller function)
         * 
         * }
         * 
         * ```
         * 
         * @see InheritanceBuilder for more information
         * @param NgController childCtrl
         * @returns InheritanceBuilder new instance
         */
        function begin(childCtrl) {
            return new InheritanceBuilder(childCtrl);
        }
        //#endregion

        //#region Service Instance
        var service = {
            begin: begin
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_layout").factory("controllerInheritanceService", ["$injector", controllerInheritanceService]);
    //#endregion

})(angular);