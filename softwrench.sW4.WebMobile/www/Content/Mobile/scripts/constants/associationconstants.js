(function (angular) {
    "use strict";

    angular.module("sw_mobile_services").constant("associationConstants", {
        Option: class {
            constructor(value, label, text, extra, checked) {
                this.value = value;
                this.label = label || this.value;
                this.text = text || this.label;
                this.extrafields = extra;
                this.checked = Boolean(checked);
            }
            setChecked(value) {
                this.checked = Boolean(value);
                return this;
            }
        }
    });

})(angular);