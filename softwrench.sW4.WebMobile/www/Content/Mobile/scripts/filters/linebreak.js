(function(app) {
    "use strict";

    app.filter("linebreak", function () {
        return function (value) {
            if (value != null) {
                value = value.toString();
                return value.replace(/\n/g, "<br/>");
            }
            return value;
        };
    });

})(softwrench);