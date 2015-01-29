var app = angular.module('sw_layout');

app.factory('expressionService', function ($rootScope, contextService) {


    /*       This regex matches variables that start with an @     
          Matching Strings:                                      
                      @inventory_.item_.itemnum                  
                      @assetnum                                  
                      @#customfield                              
                                                                 
           The leading @ is then removed from the matches and the
             variable is placed within the datamap[''] dictionary    
          Resulting Strings:                                     
                      datamap['inventory_.item_.itemnum']                  
                      datamap['assetnum']                                   
                      datamap['#customfield']                               
                                                                 
                https://www.regex101.com/r/fB6kI9/2               */

    var preCompiledReplaceRegex = /(@\#*)(\w+(\.?\w?)*)(?!\w)|\$\.(\w+)((\.?\w?)*((\[\'?\w+\'\])|\[?\w+\])(\.\w+)*)*(\.\w+)*/g;

    var datamapReferenceRegex = /(@\#*)(\w+(\.?\w?)*)(?!\w)/g;
    var scopeReferenceRegex = /\$\.(\w+)((\.?\w?)*((\[\'?\w+\'\])|\[?\w+\])(\.\w+)*)*(\.\w+)*(\(\s?\'?(\@\#*)?\w+\'?\s?(\,\s?\'?(\@\#*)?\w+\'?\s?)*\))?/g;

    return {
        isPrecompiledReplaceRegexMatch: function (expression) {
            return expression.match(preCompiledReplaceRegex);
        },

        isDatamapReferenceRegex: function (expression) {
            return expression.match(datamapReferenceRegex);
        },

        isScopeReferenceRegex: function (expression) {
            return expression.match(scopeReferenceRegex);
        },

        //getVariables: function(expression) {
        //    var variables = expression.match(preCompiledReplaceRegex);
        //    if (variables != null) {
        //        for (var i = 0; i < variables.length; i++) {
        //            variables[i] = variables[i].replace(/[\@\(\)]/g, '').trim();
        //        }
        //    }
        //    return variables;
        //},

        getExpression: function (expression, datamap) {
            var variables = this.getVariables(expression, datamap);

            if (variables != null) {
                $.each(variables, function (key, value) {
                    expression = expression.replace(key, value);
                });
            }

            expression = expression.replace(/ctx:/g, 'contextService.');

            return expression;
        },

        getVariables: function (expression, datamap) {
            var variables = {};
            var datamapVariables = expression.match(datamapReferenceRegex);
            var scopeVariables = expression.match(scopeReferenceRegex);

            if (datamapVariables != null) {
                var datamapPath = 'datamap';
                if (datamap.fields != undefined) {
                    datamapPath = 'datamap.fields';
                }
                for (var i = 0; i < datamapVariables.length; i++) {
                    var referenceVariable = datamapVariables[i];
                    var realVariable = referenceVariable.replace(/[\@\(\)]/g, '').trim();
                    realVariable = referenceVariable.replace(referenceVariable, datamapPath + '[' + realVariable + ']');
                    variables[referenceVariable] = realVariable;
                }
            }

            if (scopeVariables != null) {
                for (var i = 0; i < scopeVariables.length; i++) {
                    var referenceVariable = scopeVariables[i];
                    var realVariable = referenceVariable.replace('$', 'scope');
                    variables[referenceVariable] = realVariable;

                    var subDatamapVariable = datamapReferenceRegex.test(realVariable);
                    var subScopeVariable = scopeReferenceRegex.test(realVariable);

                    if (subDatamapVariable != null || subScopeVariable != null) {
                        var subVariables = this.getVariables(realVariable, datamap);
                        if (subVariables != null) {
                            $.each(subVariables, function (key, value) {
                                variables[key] = value;
                            });
                        }
                    }
                }
            }

            return variables;
        },
       
        getVariablesForWatch: function (expression, datamap) {
            var variables = this.getVariables(expression, datamap);
            var collWatch = '[';
            for (var i = 0; i < Object.keys(variables).length; i++) {
                collWatch += variables[i];
                if (i != variables.length - 1) {
                    collWatch += ",";
                }
            }

            collWatch += ']';
            return collWatch;
        },


        evaluate: function (expression, datamap, scope) {
            if (expression === undefined || expression === "true" || expression === true) {
                return true;
            }
            if (expression === "false" || expression === false) {
                return false;
            }
            var expressionToEval = this.getExpression(expression, datamap, scope);
            try {
                return eval(expressionToEval);
            } catch (e) {
                if (contextService.isLocal()) {
                    console.log(e);
                }
                return expression;
            }
        },


    };

});


