var app = angular.module('sw_layout');

app.factory('expressionService', function ($rootScope, contextService, dispatcherService) {


    var compiledDatamapRegex = /(\@\#?)(\w+(\.?\w?)*)/g;
    //var datamapRegexString = "(\@\#?)(\w+(\.?\w?)*)";

    var datamapRegexString = "(\@\#?)" +
  //                                Looks leading @ or @#
                                    "(\w+(\.?\w?)*)";
  //                                At least one word, followed by unlimited number of .word
  //    
    //                      Examples:      https://www.regex101.com/r/fB6kI9/24
  //                                @inventory_.item_.itemnum                  
  //                                @assetnum                                  
  //                                @#customfield   

    var compiledScopeRegex = /\$\.(\w+)((\.\w+)|(\[.*?\]+)|(\(.*?\)))*/g;
    //var scopeRegexString = "\$\.(\w+)((\.\w+)|(\[.*?\]+)|(\(.*?\)))*";

    var scopeRegexString = "\$\.(\w+)" +

  //                               Looks for leading $.word (will translate into scope.word)
        
  //                               The following three conditions are OR'd together 
  //                               and can be repeated 0 or more times
        
                                    "(" +
                                                "(\.\w+)"                  + "|" +
  //                                Condition 1: Looks for .word

                                                "(\[.*?\]+)"                + "|" +
  //                                Condition 2: Looks for an open bracket [ and will accept
  //                                any characters until the first closing bracket ] is found.
  //                                There can be multiple closing brackets back to back. This
  //                                is sort of a hackey way of supporting nested dictionaries

                                                "(\(.*?\))" +
  //                                Condition 3: Looks for an open parenthesis ( and will accept
  //                                any characters until the first closing parenthesis ) is found

                                    ")*";

    //                          Examples:     https://www.regex101.com/r/fB6kI9/22
                 /*                       $.previousdata.fields['wonum'].list[@assetnum]
                                          $.previousdata.fields[wonum].list[key]
                                          $.previousdata.fields('CAT')
                                          $.previousdata.fields(@#assetnum)
                                          $.previousdata.fields(var)
                                          $.previousdata.fields(var,'CAT',@assetnum)
                                          $.previousdata.fields('CAT', var, @assetnum).list[@assetnum]
                                          $.previousdata.fields(@assetnum, 'CAT', var).update(@currentVariable)
                                          $.previousdata.fields(var)
                                          $.previousdata['@wonum'].fields
                                          $.previousdata.fields.test
                                          $.previousdata.fields[@assetnum]
                                          $.lookupAssociationCode[@#lookupCode]
                                          $.currentfunction($.datamap[@assetnum])
                                          $.scope.datamap[@assetnum]                  */


    var compiledServiceRegex = /fn\:\w+\.\w+(\(.*?\)((\s?\,\s?.*?)*\))*)((\.\w+)|(\[.*?\])|\)|(\(.*?\)((\s?\,\s?.*?)*\))*))*/g;
    //var serviceRegexString = "fn\:\w+\.\w+(\(.*?\)((\s?\,\s?.*?)*\))*)((\.\w+)|(\[.*?\])|\)|(\(.*?\)((\s?\,\s?.*?)*\))*))*";

    var serviceRegexString = "fn\:\w+\.\w+" + 
  //                                Looks for leading fn:word.word (will translate into service.method call)

                                    "(\(.*?\)" + "((\s?\,\s?.*?)*\))*)" +
  //                                - Looks for an open parenthesis ( and will accept
  //                                  any characters until the first closing parenthesis ) is found
  //                                - Next, the above pattern could be followed with an unlimited number of
  //                                  commas that are followed by a word. Finally, this condition would expect
  //                                  a closed parenthesis. This part of the condition can occur 0 to unlimited number of times
  //                                  This case will allow for all variables nested within ( ) i.e. $.test($.test(), $.test())
    
  //                                The following four conditions are OR'd together 
  //                                and can be repeated 0 or more times

                                    "(" +

                                               "(\.\w+)"                        + "|" +
  //                                Condition 1: Looks for .word


                                               "(\[.*?\])"                      + "|" +
  //                                Condition 2: Looks for an open bracket [ and will accept
  //                                any characters until the first closing bracket ] is found.
  //                                This allows for you to call a function, and then refer to a 
  //                                key in its result. For example, 
  //                                     fn:contextService.getCurrentDatamap()[@assetnum]
  

                                               "\)"                             + "|" +
  //                                Condition 3: This OR with a close parenthesis ) will help when
  //                                capturing variables nested within parenthesis.

                                               "(\(.*?\)" + "(\s?\,\s?.*?\))*)" +
  //                                Condition 4:
  //                                - Looks for an open parenthesis ( and will accept
  //                                  any characters until the first closing parenthesis ) is found
  //                                - Next, the above pattern could be followed with an unlimited number of
  //                                  commas that are followed by a word. Finally, this condition would expect
  //                                  a closed parenthesis. This part of the condition can occur 0 to unlimited number of times
  //                                  This case will allow for identifying variables nested within ( ) i.e. $.test($.test(), $.test())

                                    ")*";

                                    //"((\.\w+)|" +
  //                                
                                    //"(\[.*?(?=\])\])|" +
  //                                
                                    //"(\(.*?(?=\))\)))*";
  //                                Condition 3: Looks for an open parenthesis ( and will accept
  //                                any characters until the first closing parenthesis ) is found

    /*            Examples:                 https://www.regex101.com/r/fB6kI9/21                
                    fn:customService.getTransformation(@assetnum) + fn:customService.getTransformation(@binnum)
                    fn:inventoryService.retrieveCost($.datamap[@assetnum], $.test(@assetnum), $.test()).parseDouble(fn:test.method(@assetnum))
                    fn:inventoryService.addition(@#refsiteid, fn:inventoryService.getint('test, test')) + fn:test.function()
                    fn:inventoryService.addition($.test(), fn:inventoryService.getint('test, test'))
                    fn:inventoryService.retrieveCost($.datamap[@assetnum]).parseDouble()
                    fn:inventoryService.retrieveCost($.datamap[@assetnum], $.test()).parseDouble()
                    fn:contextService.getCurrentDatamap()[@assetnum]                                    */
        

    function buildScopeVariables(variables,scopeVariables,datamap) {
        for (var i = 0; i < scopeVariables.length; i++) {
            var referenceVariable = scopeVariables[i];
            var realVariable = referenceVariable.replace(/\$\./, 'scope.');

            var declarations = extractSignaturesFromExpression(realVariable);

            //Handles any extra functions that were picked up by the variableRegex
            for (var j = 0; j < declarations.length; j++) {
                var declaration = declarations[j].trim();
                //Tests whether or not the realVariable has a subVariable within it
                var subVariable = compiledScopeRegex.test(declaration) || compiledServiceRegex.test(declaration);
                if (subVariable == true) {
                    //Updates the realValue of current with the evaluation of its child nodes
                    var subVariables = this.getVariables(declaration, datamap);

                    //For each sub variable, updated the real variable reference
                    //(the variable's true reference upon being evaluated) and add
                    // the variable sub variable to our current variables list.
                    $.each(subVariables, function (key, value) {
                        //Updates the realValue of current with the evaluation of its child nodes
                        realVariable = realVariable.replace(key, value);

                        //Deletes subVariable keys from resulting Dictionary if
                        //onlyReturnRootNode flag is true
                        if (variables[key] != undefined && variableValueFlag == true) {
                            delete variables[key];
                        }
                    });
                }
            }

            variables[referenceVariable] = realVariable;
        }
    }


    function buildServiceVariables(variables, serviceVariables, datamap, onlyReturnRootNode) {
        for (var i = 0; i < serviceVariables.length; i++) {
            var referenceVariable = serviceVariables[i];
            var realVariable = referenceVariable.replace(/fn\:/, '');

            //Extracts all function signatures from the match
            var declarations = extractSignaturesFromExpression(realVariable);

            //Handles any extra functions that were picked up by the service Regex
            for (var j = 0; j < declarations.length; j++) {
                var declaration = declarations[j].trim();
                //Tests whether or not the realVariable has a $. or fn: subVariable within it
                var subVariable = compiledScopeRegex.test(declaration) || compiledServiceRegex.test(declaration);
                if (subVariable == true) {
                    var subVariables = this.getVariables(declaration, datamap);

                    //For each sub variable, updated the real variable reference
                    //(the variable's true reference upon being evaluated) and add
                    // the variable sub variable to our current variables list.
                    $.each(subVariables, function (key, value) {
                        //Updates the realValue of current with the evaluation of its child nodes
                        realVariable = realVariable.replace(key, value);

                        //Updates the current declaration with evaluation of its child nodes.
                        //This will be used to identify/parse the parameters when injecting
                        //the custom service
                        declarations[0] = declarations[0].replace(key, eval(value));

                        //Deletes subVariable keys from resulting Dictionary if
                        //onlyReturnRootNode flag is true
                        if (variables[key] != undefined && onlyReturnRootNode == true) {
                            delete variables[key];
                        }
                    });
                }
            }

            //Captures character up to the first open parenthises (
            var functionCallStr = realVariable.substring(0, realVariable.indexOf('('));

            var functionCall = functionCallStr.split('.');
            var service = functionCall[0];
            var method = functionCall[1];

            //Regex to identify commas that are not within a nested string or parenthesis. This
            //will be used to extract the parameters from the custom service's function declaration
            var functionParameterRegex = new RegExp(/,(?=[^\)]*(?:\(|$))(?=(?:[^']*'[^']*')*[^']*$)/g);

            var parameters = declarations[0].split(functionParameterRegex);

            //Calls dispatcherService to load the custom service
            realVariable = dispatcherService.loadService(service, method, parameters);

        }
        //Updates the variables Dictionary with the evaluated value
        variables[referenceVariable] = realVariable;
    }

    function buildDatamapVariables(variables, datamapVariables, datamap) {
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

    //Extracts all of the signatures from the expression
    //into a list. For example, 
    //fn:service.method(parameter1).delete(parameter1, parameter2)
    //will return two items, 'parameter1', and 'parameter1,parameter'.
    function extractSignaturesFromExpression(expression) {
            
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
    };

    return {
        getDatamapRegex: function () {
            return new RegExp("/" + datamapRegexString + "/g");
        },

        getScopeRegex: function () {
            return new RegExp("/" + scopeRegexString) + "/g";
        },

        getServiceRegex: function () {
            return new RegExp("/" + serviceRegexString + "/g");
        },

        getExpression: function (expression, datamap, scope) {
        /*  The third parameter (boolean flag) will force the getVariables
              method to evaluate sub variables and only return a mapping for
              the root nodes. This means that nested variables will not have
              their own key/value pair in the resulting dictionary          */
            var variables = this.getVariables(expression, datamap, true, scope);

        /*  Each dictionary key is used to quickly update an expression with its
            true value. We loop through each variable, replacing any instance of the
            key (original reference in metadata) with an expression we can evaluate   */
            if (variables != null) {
                $.each(variables, function (key, value) {
                        expression = expression.replace(key, value);
                });
            }

            expression = expression.replace(/ctx:/g, 'contextService.');

            return expression;
        },

    /*  Returns a dictionary containing variables that are identified within the expression.
        The dictionary key for the variable is the expression's original value, where the 
        dictionary value is the evaluated/"mapped" value.

        The onlyReturnRootNode would be set to true when trying to evaluate an expression.
        When the flag is false, the resulting list will have all variables, including nested
        variables.                                                                     */
        getVariables: function (expression, datamap, onlyReturnRootNode) {
            var variables = {};

            var scopeVariables = expression.match(compiledScopeRegex);

            if (scopeVariables != null) {
                buildScopeVariables(variables, scopeVariables,datamap);
            }

            var serviceVariables = expression.match(compiledServiceRegex);

            if (serviceVariables != null) {
                buildServiceVariables(variables, serviceVariables, datamap, onlyReturnRootNode);
            }

            var datamapVariables = expression.match(compiledDatamapRegex);

            if (datamapVariables != null) {
                buildDatamapVariables(variables, datamapVariables, datamap);
            }

            return variables;
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


