var app = angular.module('sw_layout');

app.factory('expressionService', function ($rootScope, contextService) {


    /*       This regex matches variables that start with an @     */
    /*      Matching Strings:                                      */
    /*                  @inventory_.item_.itemnum                  */
    /*                  @assetnum                                  */
    /*                  @#customfield                              */
    /*                                                             */
    /*       The leading @ is then removed from the matches        */
    /*      Resulting Strings:                                     */
    /*                  inventory_.item_.itemnum                   */
    /*                  assetnum                                   */
    /*                  #customfield                               */

    var preCompiledReplaceRegex = /(?:^|\W)@(\#*)(\w+(\.?\w?)*)(?!\w)/g;

    return {
        isPrecompiledReplaceRegexMatch: function (expression) {
            return expression.match(preCompiledReplaceRegex);
        },

        getExpression: function (expression, datamap) {
            var variables = this.getVariables(expression);

            if (variables == null)
                return expression;

            for (var i = 0, len = variables.length; i < len; i++) {
                var datamapPath = 'datamap';
                if (datamap.fields != undefined) {
                    datamapPath = 'datamap.fields';
                }

                var currentVariable = variables[i];
                var variableRegex = new RegExp('@' + currentVariable);
                expression = expression.replace(variableRegex, datamapPath + "['" + currentVariable + "']");
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
                if (i != variables.length - 1) {
                    collWatch += ",";
                }
            }

            collWatch += ']';
            return collWatch;
        },


        evaluate: function (expression, datamap) {
            if (expression == undefined || expression == "true" || expression == true) {
                return true;
            }
            if (expression == "false" || expression == false) {
                return false;
            }
            var expressionToEval = this.getExpression(expression, datamap);
            try {
                return eval(expressionToEval);
            } catch (e) {
                if (contextService.isLocal()) {
                    console.log(e);
                }
                return true;
            }
        },


    };

});


