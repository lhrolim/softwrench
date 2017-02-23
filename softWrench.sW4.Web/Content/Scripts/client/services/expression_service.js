var app = angular.module('sw_layout');

app.factory('expressionService', function ($rootScope,contextService) {

    "ngInject";

    var preCompiledReplaceRegex = /(?:^|\W)@(\#*)(\w+)(?!\w)/g;

    return {
        getExpression: function (expression, datamap) {
            if (datamap.fields != undefined) {
                expression = expression.replace(/\@/g, 'datamap.fields.');
            } else {
                expression = expression.replace(/\@/g, 'datamap.');
            }
            expression = expression.replace(/ctx:/g, 'contextService.');
            return expression;
        },


        getVariables: function (expression) {
            var variables = expression.match(preCompiledReplaceRegex);
            if (variables != null) {
                for (var i = 0; i < variables.length; i++) {
                    variables[i] = variables[i].replace(/[\@\(\)]/g, '').trim();
                }
            }
            return variables;
        },

        getVariablesForWatch: function (expression) {
            var variables = this.getVariables(expression);
            var collWatch = '[';
            for (var i = 0; i < variables.length; i++) {
                collWatch += 'datamap.' + variables[i];
                if (i != variables.length-1) {
                    collWatch += ",";
                }
            }

            collWatch += ']';
            return collWatch;
        },


        evaluate: function (expression, datamap) {
            if (expression == "true") {
                return true;
            }
            if (expression == "false") {
                return false;
            }
            var expressionToEval = this.getExpression(expression, datamap);
            try {
                return eval(expressionToEval);
            } catch (e) {
                if ($rootScope.isLocal) {
                    console.log(e);
                }
                return true;
            }
        },


    };

});


