var app = angular.module('sw_layout');

app.factory('expressionService', function ($rootScope, contextService) {


    /*       This regex matches variables two different patterns:
    
        1. Strings that start with an @    -----------------------------
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

        2. Strings that start with an $.    ----------------------------
                  Matching Strings:
                        $.previousdata.fields['wonum'].list[@assetnum]
                        $.previousdata.fields[wonum].list[key]
                        $.previousdata.fields('CAT')
                        $.previousdata.fields(@#assetnum)
                        $.previousdata.fields(var)
                        $.previousdata.fields(var,'CAT',@assetnum)
                        $.previousdata.fields('CAT', var, @assetnum)
                        $.previousdata.fields(@assetnum, 'CAT', var)
                        $.previousdata.fields(var)

                        $.previousdata['@wonum'].fields
                        $.previousdata[$.test]
                        $.previousdata[$.testFunctionrefsiteid(@assetnum)]
                        $.previousdata.fields.test
                        $.previousdata.fields[@assetnum]
                        $.lookupAssociationCode[@#lookupCode]
                        $.fields(@test, datamap['test'])
                        $.testFunction($.datamap[@#refsiteid], @#test)

            In this case, the $. will be translated to scope. and the
            @ will still be translated into datamap[' {variable} ']

            NOTE: There can only be 2 inceptions of a scope function.
            For example, you can use:
                $.customFunction($.previousdata)

            But you cannot have, for example, have two scope functions with
            a third scope function/variable reference as a paramter.
            Example:
                $.customFunction($.extraMultiplier($.previousdata))
            
                     Example Regex Tester W/ Examples URL:
                      https://www.regex101.com/r/fB6kI9/12               */

    var preCompiledReplaceRegex = /(\@\#*)(\w+(\.?\w?)*)(?!\w)|\$\.(\w+)((\.\w+)|(\[\s?\'?(\$\.|\@\#*)?\w+((\.\w+)|(\[\s?\'?(\$\.|\@\#*)?\w+\'?\s?\])|(\(\s?\'?(\$\.|\@\#*)?\w+\'?\s?(\,\s?\'?(\$\.|\@\#*)?\w+\'?\s?)*\)))*\'?\s?\])|(\(\s?\'?(\$\.|\@\#*)?\w+((\.\w+)|(\[\s?\'?(\$\.|\@\#*)?\w+\'?\s?\])|(\(\s?\'?(\$\.|\@\#*)?\w+\'?\s?(\,\s?\'?(\$\.|\@\#*)?\w+\'?\s?)*\)))*\'?\s?(\,\s?\'?(\$\.|\@\#*)?\w+((\.\w+)|(\[\s?\'?(\$\.|\@\#*)?\w+\'?\s?\])|(\(\s?\'?(\$\.|\@\#*)?\w+\'?\s?(\,\s?\'?(\$\.|\@\#*)?\w+\'?\s?)*\)))*\'?\s?)*\)))*/g;

    return {
        isPrecompiledReplaceRegexMatch: function (expression) {
            return expression.match(preCompiledReplaceRegex);
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
            var matchingVariables = expression.match(preCompiledReplaceRegex);

            var datamapPath = 'datamap';
            if (datamap.fields != undefined) {
                datamapPath = 'datamap.fields';
            }

            if (matchingVariables != null) {
                for (var i = 0; i < matchingVariables.length; i++) {
                    var referenceVariable = matchingVariables[i];
                    var variableType = referenceVariable[0] == '@' ? 'DATAMAP' : 'SCOPE';
                    var realVariable = referenceVariable.substring(1);

                    if (variableType == 'DATAMAP') {
                        realVariable = datamapPath + "['" + realVariable + "']";
                        

                    } else {
                        realVariable = 'scope' + realVariable;

                        var subVariable = preCompiledReplaceRegex.test(realVariable);
                        if (subVariable == true) {
                            var subVariables = this.getVariables(realVariable, datamap);
                            $.each(subVariables, function (key, value) {
                                realVariable = realVariable.replace(key, value);
                                variables[key] = value;
                            });
                        }
                    }
                    variables[referenceVariable] = realVariable;
                }
            }

            
            


            //if (datamapVariables != null) {
            //    var datamapPath = 'datamap';
            //    if (datamap.fields != undefined) {
            //        datamapPath = 'datamap.fields';
            //    }
            //    for (var i = 0; i < datamapVariables.length; i++) {
            //        var referenceVariable = datamapVariables[i];
            //        var realVariable = referenceVariable.replace(/[\@\(\)]/g, '').trim();
            //        realVariable = referenceVariable.replace(referenceVariable, datamapPath + '[' + realVariable + ']');
            //        variables[referenceVariable] = realVariable;
            //    }
            //}

            //var datamapVariables = expression.match(datamapReferenceRegex);
            //var scopeVariables = expression.match(scopeReferenceRegex);

            //if (scopeVariables != null) {
            //    for (var i = 0; i < scopeVariables.length; i++) {
            //        var referenceVariable = scopeVariables[i];
            //        var realVariable = referenceVariable.substring(1);
            //        variables[referenceVariable] = realVariable;

            //        var subDatamapVariable = datamapReferenceRegex.test(realVariable);
            //        var subScopeVariable = scopeReferenceRegex.test(realVariable);

            //        if (subDatamapVariable != null || subScopeVariable != null) {
            //            var subVariables = this.getVariables(realVariable, datamap);
            //            if (subVariables != null) {
            //                $.each(subVariables, function (key, value) {
            //                    if (extractValFromSubVars == true) {
            //                        variables[key] = eval(value);
            //                    } else {
            //                        variables[key] = value;
            //                    }
            //                });
            //            }
            //        }
            //    }
            //}

            //if (datamapVariables != null) {
            //    var datamapPath = 'datamap';
            //    if (datamap.fields != undefined) {
            //        datamapPath = 'datamap.fields';
            //    }
            //    for (var i = 0; i < datamapVariables.length; i++) {
            //        var referenceVariable = datamapVariables[i];
            //        var realVariable = referenceVariable.replace(/[\@\(\)]/g, '').trim();
            //        realVariable = referenceVariable.replace(referenceVariable, datamapPath + '[' + realVariable + ']');
            //        variables[referenceVariable] = realVariable;
            //    }
            //}

            return variables;
        },
       
        getVariablesForWatch: function (expression, datamap) {
            var variables = this.getVariables(expression, datamap);

            var collWatch = '[';
            if (variables != null) {
                var i = 0;
                $.each(variables, function (key, value) {
                    if (!key.endsWith(')')) {
                        $.each(variables, function(key2, value2) {
                            variables[key2] = variables[key2].replace(key, value);
                        });
                        collWatch += value + ',';
                    }
                    i = i + 1;
                });
            }

            if (collWatch.charAt(collWatch.length - 1) == ",") {
                collWatch = collWatch.slice(0, -1);
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


