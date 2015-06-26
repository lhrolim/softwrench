(function(regitry) {
    "use strict";

    regitry.Problem = persistence.define("Problem", {
        message: "TEXT"
    });

    regitry.BatchItem.hasOne("problem", regitry.Problem);

})(entities);