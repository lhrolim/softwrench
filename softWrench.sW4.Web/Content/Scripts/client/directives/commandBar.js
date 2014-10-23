app.directive('commandbar', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/commandBar.html'),
        scope: {
            applicationschema: '=',
            schema: '=',
            mode: '=',
            localcommandprovider: '&',
            localschema: '=',
            cancelfn: '&',
            savefn: '&',
            datamap: '='
        },

        controller: function ($scope, $http, $element,$log, printService, i18NService, commandService, redirectService, alertService,modalService, $rootScope) {

            $scope.defaultCommands = function () {

                return {
                    save: {
                        id: "save",
                        method: $scope.save,
                        showExpressionFn: $scope.shouldDisplaySave,
                        icon: 'glyphicon-ok',
                        label: 'Submit',
                        'default': true
                    },
                    print: {
                        id: "print",
                        method: $scope.printDetail,
                        showExpressionFn: $scope.shouldDisplayPrint,
                        icon: 'glyphicon-print',
                        label: 'Print',
                        'default': true
                    },

                    cancel: {
                        id: "cancel",
                        method: $scope.cancel,
                        showExpressionFn: $scope.shouldDisplayCancel,
                        icon: 'glyphicon-backward',
                        label: GetPopUpMode().equalsAny("browser", "nomenu") ? 'Close Window' : 'Cancel',
                        'default': true
                    },
                }

            }

            function addDefault(resultCommands) {

                if ($scope.schema.commandSchema == null) {
                    return resultCommands;
                }

                var schemaDeclaredCommands = {};
                
                var ignoreUndeclared = $scope.schema.commandSchema.ignoreUndeclaredCommands;
                if (!ignoreUndeclared) {
                    $.each($scope.defaultCommands(), function (key, defaultCommand) {
                        //each of the default commands might have been overriden on the schema. If so, change their declarations for the schema´s
                        var overridenCommand = $.findFirst(schemaDeclaredCommands, function (elem) {
                            return elem.id == defaultCommand.id;
                        });

                        if (nullOrUndef(overridenCommand)) {
                            resultCommands.push(defaultCommand);
                        } else {
                            if (overridenCommand.icon == undefined) {
                                overridenCommand.icon = defaultCommand.icon;
                            }
                            resultCommands.push(overridenCommand);
                        }
                    });
                }

                $.each(schemaDeclaredCommands, function (key, command) {
                    var defaultCommand = $scope.defaultCommands()[command.id];
                    if (!ignoreUndeclared && defaultCommand != undefined) {
                        //default commands have already been handled, unless we´re completely redeclaring everything
                        return;
                    }
                    if (command.defaultPosition == "left") {
                        resultCommands.unshift(command);
                    } else {
                        resultCommands.push(command);
                    }
                });

                $scope.cachedCommands = resultCommands;
                return resultCommands;
            }

            $scope.redirectToHapagHome = function () {
                redirectService.redirectToAction(null, 'HapagHome', null, null);
            };

            $scope.shouldDisplaySave = function () {
                return $scope.shouldDisplayCommand($scope.schema.commandSchema, 'save') && $scope.mode == 'input';
            };

            $scope.shouldDisplayCancel = function () {
                return $scope.shouldDisplayCommand($scope.schema.commandSchema, 'cancel');
            };

            $scope.shouldDisplayPrint = function () {
                return $scope.shouldDisplayCommand($scope.schema.commandSchema, 'print') && $scope.isEditing($scope.schema, $scope.datamap);
            };

            $scope.save = function () {
                $scope.savefn();
            };

            $scope.cancel = function () {
                if ($rootScope.showingModal) {
                    //Hide the modal calling the modal service and not use the general cancel function 
                    modalService.hide();
                    return;
                }
                $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
            };

            $scope.printDetail = function () {
                var schema = $scope.schema;
                printService.printDetail(schema, $scope.datamap[schema.idFieldName]);
            };


            $scope.getCommands = function () {
                var log = $log.getInstance('commandbar#getCommands');
                if ($scope.cachedCommands) {
                    log.trace('returning from cache '.format(JSON.stringify($scope.cachedCommands)));
                    return $scope.cachedCommands;
                }

                var resultCommands = [];
                if ($scope.localcommandprovider == undefined) {
                    log.debug('adding default commands only');
                    //no external provider, just add the schema commands
                    return addDefault(resultCommands);
                }

                //this is for compositions or tabs, where we can have custom commands declared
                var localCommands = $scope.localcommandprovider();
                if (localCommands == undefined) {
                    log.debug('provider returned null: adding default commands only');
                    return addDefault(resultCommands);
                }

                if (localCommands.toAdd != undefined) {
                    //let´s add specific commands of the localcontext to the default bar (ex: composition add Item)
                    resultCommands = resultCommands.concat(localCommands.toAdd);
                }
                addDefault(resultCommands);
                if (isArrayNullOrEmpty(localCommands.toKeep) || localCommands.toKeep[0] == "all") {
                    $scope.cachedCommands = resultCommands;
                    return resultCommands;
                }

                //let´s see the non-default commands, i.e commands that are declared on the schema, 
                //but may be overriden by specific tab or composition_list
//                $.each($scope.schema.commandSchema.commands, function (index, value) {
//                    if ($.inArray(value.id, localCommands.toKeep) != -1) {
//                        resultCommands.push(value);
//                    } else {
//                        if ($scope.localschema != undefined) {
//                            $scope.localschema.commandSchema.toExclude.push(value.id);
//                        }
//                    }
//                });
                if (!isArrayNullOrEmpty(localCommands.toKeep)) {
                    //let´s see the default commands now cancel,print,save
                    $.each($scope.defaultCommands(), function (index, value) {
                        log.debug('checking command {0}'.format(value.id));
                        if ($.inArray(value.id, localCommands.toKeep) == -1) {
                            //this means that this default command shall not be kept
                            if ($scope.localschema != undefined) {
                                $scope.localschema.commandSchema.toExclude.push(value.id);
                                log.debug('excluding command {0}'.format(value.id));
                            }
                        }
                    });
                }
                $scope.cachedCommands = resultCommands;
                return resultCommands;
            };

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.i18NCommandLabel = function (command) {
                if (command.default != undefined && command.default == true) {
                    var i18NValue;
                    var popUpMode = GetPopUpMode();
                    if ((popUpMode == "browser" || popUpMode == "nomenu") && command.id == "cancel") {
                        i18NValue = $scope.i18N('general.close', command.label);
                    } else {
                        i18NValue = $scope.i18N('general.' + command.id, command.label);
                    }

                    return $scope.commandLabel($scope.schema, command.id, i18NValue);
                }

                return i18NService.getI18nCommandLabel(command, $scope.schema);
            };

            //this is for the defaultCommands.
            //TODO: review
            $scope.commandLabel = function (schema, id, defaultValue) {
                return commandService.commandLabel(schema, id, defaultValue);
            };

            $scope.doCommand = function (command) {
                commandService.doCommand($scope, command);
            };

            //this is used for the default commands and take in consideration remove expressions
            //TODO: review this
            $scope.shouldDisplayCommand = function (commandSchema, id) {
                if (!$scope.cachedCommands) {
                    $scope.getCommands();
                }
                if ($scope.localschema != undefined) {
                    return commandService.shouldDisplayCommand($scope.localschema.commandSchema, id);
                }
                return commandService.shouldDisplayCommand(commandSchema, id);
            };

            $scope.isCommandHidden = function (schema, command) {
                var tabId = $element.parents('.tab-pane').attr('id');
                if (command.showExpressionFn != undefined && typeof (command.showExpressionFn) === 'function') {
                    return !command.showExpressionFn();
                }
                return commandService.isCommandHidden($scope.datamap, schema, command, tabId);
            };

            $scope.isEditing = function (schema, datamap) {
                return datamap && datamap[schema.idFieldName] != null;
            };

        }
    };
});

