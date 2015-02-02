﻿var app = angular.module('sw_layout');

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
                $.customFunction($.previousdata[@assetnum])
                $.previousdata[$.customFunction(@assetnum)]
                $scope

            But you cannot have, for example, have two scope functions with
            a third scope function/variable reference as a paramter.
            Example:
                $.customFunction($.extraMultiplier($.previousdata))
                $.previousdata[$.customFunction($.lookupAssociationCode[@assetnum])]

            
                     Example Regex Tester W/ Examples URL:
                      https://www.regex101.com/r/fB6kI9/12               */

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

    var compiledScopeReplaceRegex = /\$\.(\w+)((\.\w+)|(\[.*?(?=\])\])|(\(.*?(?=\))\)))*/g;

  //var scopeReplaceRegexString = "\$\.(\w+)((\.\w+)|(\[.*?(?=\])\])|(\(.*?(?=\))\)))*";
    var scopeReplaceRegexString = "\$\.(\w+)" +
  //                              Looks for leading $.word (will translate into scope.word)

  //                              The following three conditions are OR'd together 
  //                              and can be repeated 0 or more times

                                  "((\.\w+)|" +
  //                                Condition 1: Looks for .word
                                   "(\[.*?(?=\])\])|" +
  //                                Condition 2: Looks for an open bracket [ and will accept
  //                                any characters until the first closing bracket ] is found
                                   "(\(.*?(?=\))\)))*";
  //                                Condition 3: Looks for an open parenthesis ( and will accept
  //                                any characters until the first closing parenthesis ) is found

  //                          Examples:
  //                                  $.createFlag
  //                                  $.selectedValue()
  //                                  $.previousdata.fields['wonum'].list[@assetnum]
  //                                  $.previousdata.lookupAssociationCode[@assetnum].[$.previousdata.fields()]
  //                                  $.previousdata.fields('CAT', var, @assetnum).list[@assetnum]
  //                                  $.previousdata.fields(@assetnum, 'CAT', var).update(@currentVariable)

    var compiledServiceReplaceRegex = /fn\:\w+\.\w+(\(.*?(?=\))\))((\.\w+)|(\[.*?(?=\])\])|(\(.*?(?=\))\)))*/g;

  //var serviceReplaceRegexString = "fn\:\w+\.\w+(\(.*?(?=\))\))((\.\w+)|(\[.*?(?=\])\])|(\(.*?(?=\))\)))*";
    var serviceReplaceRegexString = "fn\:\w+\.\w+" +
  //                                Looks for leading fn:word.word (will translate into service.method call)
                                    "(\(.*?(?=\))\))" +
  //                                Looks for an open parenthesis ( and will accept
  //                                any characters until the first closing parenthesis ) is found


  //                                The following three conditions are OR'd together 
  //                                and can be repeated 0 or more times
                                    "((\.\w+)|" +
  //                                Condition 1: Looks for .word
                                    "(\[.*?(?=\])\])|" +
  //                                Condition 2: Looks for an open bracket [ and will accept
  //                                any characters until the first closing bracket ] is found
                                    "(\(.*?(?=\))\)))*";
  //                                Condition 3: Looks for an open parenthesis ( and will accept
  //                                any characters until the first closing parenthesis ) is found

  //                            Examples:
  //                                    $.createFlag
  //                                    $.selectedValue()
  //                                    $.previousdata[@assetnum]
  //                                    $.saveItem($.datamap, $.schema, @assetnum)
  //                                    $.previousdata.fields['wonum'].list[@assetnum]
  //                                    $.previousdata.lookupAssociationCode[@assetnum].[$.previousdata.fields()]
  //                                    $.previousdata.fields('CAT', var, @assetnum).list[@assetnum]
  //                                    $.previousdata.fields(@assetnum, 'CAT', var).update(@currentVariable)
        


    return {
        getDatamapReplaceRegex: function () {
            return new RegExp("/" + datamapReplaceRegexString + "/g");
        },

        getScopeReplaceRegex: function (expression) {
            return new RegExp("/" + scopeReplaceRegexString) + "/g";
        },

        getServiceReplaceRegex: function (expression) {
            return new RegExp("/" + serviceReplaceRegexString + "/g");
        },

        getExpression: function (expression, datamap) {
            var variables = this.getVariables(expression, datamap);

            //Each dictionary key is used to quickly update an expression with its true value.
            //We loop through each variable, replacing any instance of the key (original reference in metadata) 
            //with the new value (the true reference upon being evaluated)
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

            var scopeVariables = expression.match(compiledScopeReplaceRegex);

            if (scopeVariables != null) {
                for (var i = 0; i < scopeVariables.length; i++) {
                    var referenceVariable = scopeVariables[i];
                    var realVariable = referenceVariable.replace(/\$\./, 'scope.');

                    //Tests whether or not the realVariable has a subVariable within it
                    //For example, the reference variable $.lookupAssociatonCode[@assetnum]
                    //will translate into a real variable of scope.lookupAssociationCode[@assetnum].
                    //Because the match has @assetnum as a subvariable, the below function will return true.
                    var subVariable = compiledScopeReplaceRegex.test(realVariable) || compiledServiceReplaceRegex.test(realVariable);
                    if (subVariable == true) {
                        var subVariables = this.getVariables(realVariable, datamap);

                        //For each sub variable, updated the real variable reference
                        //(the variable's true reference upon being evaluated) and add
                        // the variable sub variable to our current variables list.
                        $.each(subVariables, function (key, value) {
                            realVariable = realVariable.replace(key, value);
                            if (variables[key] === undefined) {
                                variables[key] = value;
                            }
                        });
                    }
                    //Updates variable dictionary key (the variable's original reference in the metadata)
                    //with the real variable reference (the variable's true reference upon being evaluated).
                    //The key can be used to quickly update an expression with its true value. A good example
                    //of this occurs in getExpression, we loop through each variable, replacing any instance of
                    //the key (original reference in metadata) with the new value (the true reference upon being evaluated)
                    variables[referenceVariable] = realVariable;
                }
            }

            var serviceVariables = expression.match(compiledServiceReplaceRegex);

            if (serviceVariables != null) {
                for (var i = 0; i < serviceVariables.length; i++) {
                    var referenceVariable = serviceVariables[i];
                    var realVariable = referenceVariable.replace(/fn\:/, '');

                    //Tests whether or not the realVariable has a subVariable within it
                    //For example, the reference variable $.lookupAssociatonCode[@assetnum]
                    //will translate into a real variable of scope.lookupAssociationCode[@assetnum].
                    //Because the match has @assetnum as a subvariable, the below function will return true.
                    var subVariable = compiledScopeReplaceRegex.test(realVariable) || compiledServiceReplaceRegex.test(realVariable);
                    if (subVariable == true) {
                        var subVariables = this.getVariables(realVariable, datamap);

                        //For each sub variable, updated the real variable reference
                        //(the variable's true reference upon being evaluated) and add
                        // the variable sub variable to our current variables list.
                        $.each(subVariables, function (key, value) {
                            realVariable = realVariable.replace(key, value);
                            if (variables[key] === undefined) {
                                variables[key] = value;
                            }
                        });
                    }
                    //Updates variable dictionary key (the variable's original reference in the metadata)
                    //with the real variable reference (the variable's true reference upon being evaluated).
                    //The key can be used to quickly update an expression with its true value. A good example
                    //of this occurs in getExpression, we loop through each variable, replacing any instance of
                    //the key (original reference in metadata) with the new value (the true reference upon being evaluated)
                    variables[referenceVariable] = realVariable;
                }
            }

            /*var matchingVariables = expression.match(preCompiledReplaceRegex);

            var datamapPath = 'datamap';
            if (datamap.fields != undefined) {
                datamapPath = 'datamap.fields';
            }

            if (matchingVariables != null) {
                for (var i = 0; i < matchingVariables.length; i++) {
                    var referenceVariable = matchingVariables[i];
                    var variableType = referenceVariable[0] == '@' ? 'DATAMAP' : 'SCOPE';

                    //Removes initial character from matches (@ or $)
                    var realVariable = referenceVariable.substring(1);

                    if (variableType == 'DATAMAP') {
                        //Translates datamap reference to a format that can be evaluated
                        realVariable = datamapPath + "['" + realVariable + "']";
                    } else {
                        realVariable = 'scope' + realVariable;

                        //Tests whether or not the realVariable has a subVariable within it
                        //For example, the reference variable $.lookupAssociatonCode[@assetnum]
                        //will translate into a real variable of scope.lookupAssociationCode[@assetnum].
                        //Because the match has @assetnum as a subvariable, the below function will return true.
                        var subVariable = preCompiledReplaceRegex.test(realVariable);
                        if (subVariable == true) {
                            var subVariables = this.getVariables(realVariable, datamap);

                            //For each sub variable, updated the real variable reference
                            //(the variable's true reference upon being evaluated) and add
                            // the variable sub variable to our current variables list.
                            $.each(subVariables, function (key, value) {
                                realVariable = realVariable.replace(key, value);
                                variables[key] = value;
                            });
                        }
                    }
                    //Updates variable dictionary key (the variable's original reference in the metadata)
                    //with the real variable reference (the variable's true reference upon being evaluated).
                    //The key can be used to quickly update an expression with its true value. A good example
                    //of this occurs in getExpression, we loop through each variable, replacing any instance of
                    //the key (original reference in metadata) with the new value (the true reference upon being evaluated)
                    variables[referenceVariable] = realVariable;
                }
            }*/

            return variables;
        },
       
        getVariablesForWatch: function (expression, datamap) {
            var variables = this.getVariables(expression, datamap);

            var collWatch = '[';
            if (variables != null) {
                var i = 0;
                $.each(variables, function (key, value) {
                    //Checks whether or not the variable is a function. If it is, it will
                    //not be included as a variable to watch. Instead, the getVariables function
                    //will return any parameters that are truely variables so that we can watch
                    //them and re-evaluate the expression when a variable changes.
                    if (!key.endsWith(')')) {
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


