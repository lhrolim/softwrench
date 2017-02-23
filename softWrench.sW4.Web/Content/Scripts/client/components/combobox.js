var app = angular.module('sw_layout');

app.factory('cmpCombobox', function ($rootScope, $timeout, fieldService) {

    "ngInject";

    /// <summary>
    ///  for ie9 we have to handle crazy angular bug where we need to repaint the component whenever it´s values changes by a non "user-action"
    /// </summary>
    /// <param name="$rootScope"></param>
    /// <param name="$timeout"></param>
    /// <param name="fieldService"></param>
    /// <returns type=""></returns>
    function fixIe9Bug(displayable) {
        var element = $("select[data-comboassociationkey='" + displayable.associationKey + "']");
        element.hide();
        element.show();
        //this workaround, fixes a bug where only the fist charachter would show...
        //taken from http://stackoverflow.com/questions/5908494/select-only-shows-first-char-of-selected-option
        element.css('width', 0);
        element.css('width', '');
    }

    return {

        unblock: function (displayable) {
            if (isIe9()) {
                fixIe9Bug(displayable);
            }
        },

        block: function (displayable) {
            if (isIe9()) {
                fixIe9Bug(displayable);
            }
        },

        refreshFromAttribute: function (displayable) {
            if (isIe9()) {
                fixIe9Bug(displayable);
            }
        },

        init: function (bodyElement, datamap, schema, scope) {

        }



    }

});


