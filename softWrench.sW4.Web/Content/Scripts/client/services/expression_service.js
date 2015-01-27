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
    /*                                                             */
    /*         https://www.regex101.com/r/fB6kI9/2                 */

    var preCompiledReplaceRegex = /(@\#*)(\w+(\.?\w?)*)(?!\w)|\$\.(\w+)((\.?\w?)*((\[\'?\w+\'\])|\[?\w+\])(\.\w+)*)*(\.\w+)*/g;

    return {
        isPrecompiledReplaceRegexMatch: function (expression) {
            return expression.match(preCompiledReplaceRegex);
        },

        getExpression: function (expression, datamap) {
            var variables = expression.match(preCompiledReplaceRegex);

            if (variables == null)
                return expression;

            var datamapPath = 'datamap';
            if (datamap.fields != undefined) {
                datamapPath = 'datamap.fields';
            }

            for (var i = 0, len = variables.length; i < len; i++) {
                var variable = variables[i];
                if (variable.startsWith('@')) {
                    variable = variable.replace(/[\@\(\)]/g, '').trim();
                    expression = variable.replace(variable, datamapPath + "['" + variable + "']");
                } else if (variable.startsWith('$')) {
                    expression = variable.replace(/[\$\(\)]/g, 'scope').trim();
                }
            }

            expression = expression.replace(/ctx:/g, 'contextService.');
            return expression;
        },

        getVariablesForWatch: function (expression) {
            var variables = expression.match(preCompiledReplaceRegex);
            var collWatch = '[';
            for (var i = 0; i < variables.length; i++) {
                variables[i] = variables[i].replace(/[\@\(\)]/g, 'datamap.').trim();
                variables[i] = variables[i].replace(/[\$\(\)]/g, 'scope').trim();
                
                collWatch += variables[i];
                if (i != variables.length - 1) {
                    collWatch += ",";
                }
            }

            collWatch += ']';
            return collWatch;
        },


        evaluate: function (expression, datamap, scope) {
            if (expression == undefined || expression == "true" || expression == true) {
                return true;
            }
            if (expression == "false" || expression == false) {
                return false;
            }
            var expressionToEval = this.getExpression(expression, datamap, scope);
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


