var app = angular.module('sw_layout');

app.factory('expressionService', function ($rootScope, contextService, dispatcherService) {
        
//                   Example Regex Tester W/ Examples URL:
//                    https://www.regex101.com/r/fB6kI9/12               */

    var compiledDatamapReplaceRegex = /(\@\#?)(\w+(\.?\w?)*)/g;

  //var datamapReplaceRegexString = "(\@\#?)(\w+(\.?\w?)*)";
    var datamapReplaceRegexString = "(\@\#?)" +
  //                                Looks leading @ or @#
                                    "(\w+(\.?\w?)*)";
  //                                At least one word, followed by unlimited number of .word
  //    
  //                        Examples:
  //                                @inventory_.item_.itemnum                  
  //                                @assetnum                                  
  //                                @#customfield   

    var compiledScopeReplaceRegex = /\$\.(\w+)((\.\w+)|(\[.*?\]+)|(\(.*?\)))*/g;

  //var scopeReplaceRegexString = "\$\.(\w+)((\.\w+)|(\[.*?\]+)|(\(.*?\)))*";
    var scopeReplaceRegexString = "\$\.(\w+)" +

  //                               Looks for leading $.word (will translate into scope.word)
        
  //                               The following three conditions are OR'd together 
  //                               and can be repeated 0 or more times
        
                                   "((\.\w+)|" +
  //                                Condition 1: Looks for .word
                                    "(\[.*?\]+)|" +
  //                                Condition 2: Looks for an open bracket [ and will accept
  //                                any characters until the first closing bracket ] is found.
  //                                There can be multiple closing brackets back to back. This
  //                                is sort of a hackey way of supporting nested dictionaries
                                    "(\(.*?\)))*";
  //                                Condition 3: Looks for an open parenthesis ( and will accept
  //                                any characters until the first closing parenthesis ) is found

  //                          Examples:
  //                                    $.createFlag
  //                                    $.selectedValue()
  //                                    $.previousdata[@assetnum]
  //                                    $.saveItem($.datamap, $.schema, @assetnum)
  //                                    $.previousdata.fields['wonum'].list[@assetnum]
  //                                    $.previousdata.lookupAssociationCode[@assetnum].[$.previousdata.fields()]
  //                                    $.previousdata.fields('CAT', var, @assetnum).list[@assetnum]
  //                                    $.previousdata.fields(@assetnum, 'CAT', var).update(@currentVariable)

    var compiledServiceReplaceRegex = /fn\:\w+\.\w+(\(.*?\)(\s?\,\s?.*?\))*)((\.\w+)|(\[.*?\])|\)|(\(.*?\)(\s?\,\s?.*?\))*))*/g;

  //var serviceReplaceRegexString = "fn\:\w+\.\w+(\(.*?\)(\s?\,\s?.*?\))*)((\.\w+)|(\[.*?\])|\)|(\(.*?\)(\s?\,\s?.*?\))*))*";
    var serviceReplaceRegexString = "fn\:\w+\.\w+(\(.*?\)(\s?\,\s?.*?\))*)((\.\w+)|(\[.*?\])|\)|(\(.*?\)(\s?\,\s?.*?\))*))*";
  //                                Looks for leading fn:word.word (will translate into service.method call)

  //                                Looks for an open parenthesis ( and will accept
  //                                any characters until the first closing parenthesis ) is found


  //                                The following three conditions are OR'd together 
  //                                and can be repeated 0 or more times
                                    //"((\.\w+)|" +
  //                                Condition 1: Looks for .word
                                    //"(\[.*?(?=\])\])|" +
  //                                Condition 2: Looks for an open bracket [ and will accept
  //                                any characters until the first closing bracket ] is found
                                    //"(\(.*?(?=\))\)))*";
  //                                Condition 3: Looks for an open parenthesis ( and will accept
  //                                any characters until the first closing parenthesis ) is found

  //                            Examples:
  //                                    fn:inventoryService.addition(@#refsiteid, fn:inventoryService.getint('test, test'))
  //                                    fn:inventoryService.addition(@#refsiteid, fn:inventoryService.getint('test, test')) + fn:test.function()
  //                                    fn:inventoryService.retrieveCost($.datamap[@assetnum]).parseDouble()
  //                                    $.saveItem($.datamap, $.schema, @assetnum)
  //                                    $.previousdata.fields['wonum'].list[@assetnum]
  //                                    $.previousdata.lookupAssociationCode[@assetnum].[$.previousdata.fields()]
  //                                    $.previousdata.fields('CAT', var, @assetnum).list[@assetnum]
  //                                    $.previousdata.fields(@assetnum, 'CAT', var).update(@currentVariable)
        


    return {
        getDatamapReplaceRegex: function () {
            return new RegExp("/" + datamapReplaceRegexString + "/g");
        },

        getScopeReplaceRegex: function () {
            return new RegExp("/" + scopeReplaceRegexString) + "/g";
        },

        getServiceReplaceRegex: function () {
            return new RegExp("/" + serviceReplaceRegexString + "/g");
        },

        getExpression: function (expression, datamap, scope) {
            /*The third parameter (boolean flag) will force the getVariables
              method to evaluate sub variables and only return a mapping for
              the highest nodes. This means that nested variables will not have
              their own key/value pair in the resulting dictionary*/
            var variables = this.getVariables(expression, datamap, true, scope);

            /*Each dictionary key is used to quickly update an expression with its
            true value. We loop through each variable, replacing any instance of the
            key (original reference in metadata) with an expression we can evaluate*/
            if (variables != null) {
                $.each(variables, function (key, value) {
                        expression = expression.replace(key, value);
                });
            }

            expression = expression.replace(/ctx:/g, 'contextService.');

            return expression;
        },

        getVariables: function (expression, datamap, variableValueFlag, scope) {
            var variables = {};

            var scopeVariables = expression.match(compiledScopeReplaceRegex);

            if (scopeVariables != null) {
                for (var i = 0; i < scopeVariables.length; i++) {
                    var referenceVariable = scopeVariables[i];
                    var realVariable = referenceVariable.replace(/\$\./, 'scope.');


                    var declarations = this.extractSignaturesFromExpression(realVariable);

                    //Handles any extra functions that were picked up by the variableRegex
                    for (var j = 0; j < declarations.length; j++) {
                        var declaration = declarations[j].trim();
                        //Tests whether or not the realVariable has a subVariable within it
                        //For example, the reference variable $.lookupAssociatonCode[@assetnum]
                        //will translate into a real variable of scope.lookupAssociationCode[@assetnum].
                        //Because the match has @assetnum as a subvariable, the below function will return true.
                        var subVariable = compiledScopeReplaceRegex.test(declaration) || compiledServiceReplaceRegex.test(declaration);
                        if (subVariable == true) {
                            var subVariables = this.getVariables(declaration, datamap);

                            //For each sub variable, updated the real variable reference
                            //(the variable's true reference upon being evaluated) and add
                            // the variable sub variable to our current variables list.
                            $.each(subVariables, function (key, value) {
                                realVariable = realVariable.replace(key, value);
                                declarations[0] = declarations[0].replace(key, value);

                                if (variables[key] != undefined && variableValueFlag == true) {
                                    delete variables[key];
                                }
                            });
                        }
                    }

                    variables[referenceVariable] = realVariable;
                }
            }

            var serviceVariables = expression.match(compiledServiceReplaceRegex);

            if (serviceVariables != null) {
                for (var i = 0; i < serviceVariables.length; i++) {
                    var referenceVariable = serviceVariables[i];
                    var realVariable = referenceVariable.replace(/fn\:/, '');

                    //Extracts all function signatures from the match
                    var declarations = this.extractSignaturesFromExpression(realVariable);

                    //Handles any extra functions that were picked up by the service Regex
                    for (var j = 0; j < declarations.length; j++) {
                        var declaration = declarations[j].trim();
                        //Tests whether or not the realVariable has a $. or fn: subVariable within it
                        var subVariable = compiledScopeReplaceRegex.test(declaration) || compiledServiceReplaceRegex.test(declaration);
                        if (subVariable == true) {
                            var subVariables = this.getVariables(declaration, datamap);

                            //For each sub variable, updated the real variable reference
                            //(the variable's true reference upon being evaluated) and add
                            // the variable sub variable to our current variables list.
                            $.each(subVariables, function (key, value) {
                                realVariable = realVariable.replace(key, value);
                                declarations[0] = declarations[0].replace(key, eval(value));

                                if (variables[key] != undefined && variableValueFlag == true) {
                                    delete variables[key];
                                }
                            });
                        }
                    }
                    var functionCallStr = realVariable.substring(0, realVariable.indexOf('('));

                    var functionCall = functionCallStr.split('.');
                    var service = functionCall[0];
                    var method = functionCall[1];

                    //Regex to identify commas that are not within a nested string or parenthesis. This
                    //will be used to extract the parameters from the custom service's function declaration
                    var functionParameterRegex = new RegExp(/,(?=[^\)]*(?:\(|$))(?=(?:[^']*'[^']*')*[^']*$)/g);

                    var parameters = declarations[0].split(functionParameterRegex);

                    realVariable = dispatcherService.loadService(service, method, parameters);

                }


                variables[referenceVariable] = realVariable;

            }

            var datamapVariables = expression.match(compiledDatamapReplaceRegex);

            if (datamapVariables != null) {
                var datamapPath = 'datamap';
                if (datamap.fields != undefined) {
                    datamapPath = 'datamap.fields';
                }
                for (var i = 0; i < datamapVariables.length; i++) {
                    var referenceVariable = datamapVariables[i];
                    var realVariable = referenceVariable.replace(/\@/, '');
                    realVariable = datamapPath + "['" + realVariable + "']";
                    variables[referenceVariable] = realVariable;
                }
            }

            return variables;
        },

        extractSignaturesFromExpression: function (expression) {
            
            var signatures = [];

            var parenCount = 1;

            while (expression.indexOf('(') != -1) {
                //remove text prior to first variable in signature
                expression = expression.substring(expression.indexOf('(') + 1);
                var i = 0;
                for (i; i < expression.length && parenCount > 0; i++) {
                    if (expression[i] == '(') {
                        parenCount ++;
                    } else if (expression[i] == ')') {
                        parenCount--;
                    }
                }
                signatures.push(expression.substring(0, i-1));
                expression = expression.substring(i + 1);
            }

            return signatures;
        },
       
        getVariablesForWatch: function (expression, datamap, scope) {
            var variables = this.getVariables(expression, datamap, false, scope);

            var collWatch = '[';
            if (variables != null) {
                var i = 0;
                $.each(variables, function (key, value) {
                    //Checks whether or not the variable is a function. If it is, it will
                    //not be included as a variable to watch. Instead, the getVariables function
                    //will return any parameters that are truely variables so that we can watch
                    //them and re-evaluate the expression when a variable changes.
                    if (!key.endsWith(')') && !key.toUpperCase().startsWith('FN:')) {
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


