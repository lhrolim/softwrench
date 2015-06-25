var constants = constants || {};


mobileServices.factory('crudFilterContextService', function ($q, $log,contextService) {

    'use strict';

    var filterContext = {
        showPending: false,
        showDirty: true,
        showProblems: false,
    }

    return {

      

        showPending: function (value) {
            if (value != undefined) {
                filterContext.showPending = value;
                contextService.insertIntoContext("filterContext", filterContext);
            }
            return filterContext.showPending;
        },

        showProblems: function (value) {
            if (value != undefined) {
                filterContext.showProblem = value;
                contextService.insertIntoContext("filterContext", filterContext);
            }
            return filterContext.showProblem;
        },


        showDirty: function (value) {
            if (value != undefined) {
                filterContext.showDirty = value;
                contextService.insertIntoContext("filterContext", filterContext);
            }
            return filterContext.showDirty;
        },

     


    }

});