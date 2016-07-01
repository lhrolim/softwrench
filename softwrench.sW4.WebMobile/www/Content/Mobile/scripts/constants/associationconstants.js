(function (angular) {
    "use strict";

    angular.module("sw_mobile_services").constant("associationConstants", {

        Option: class {
          constructor (value, label, extra) {
              this.value = value;
              this.label = label;
              this.extrafields = extra;
          }  
        },

    });

})(angular);