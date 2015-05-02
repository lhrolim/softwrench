var app = angular.module('sw_layout');

app.factory('focusService', function ($rootScope,fieldService) {
    return{

        resetFocusToCurrent:function(schema,attribute) {
            var idx =fieldService.getDisplayableIdxByKey(schema, attribute);

            $rootScope.$broadcast("sw_reset_focus", idx);
        },

        moveFocus:function() {
            
        }


    };

});


