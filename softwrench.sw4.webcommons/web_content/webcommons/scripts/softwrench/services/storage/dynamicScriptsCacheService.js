
(function (angular) {
    "use strict";

    let customServiceName, customServiceCodeKeyName;

    /**
     * Manages the logic for custom angular services.
     * 
     * The codes are lazy loaded into angular context, here´s how it works in a glance
     * 
     * 
     * Upon each new login hits the server to fetch any script updates (on the offline, upon the sync)
     * 
     * These scripts are then stored into 2 main localstorage regions:
     * 
     *  1) to hold the script names, uncompressed
     *  2) another to hold each of the scripts code (one region each)
     * 
     * The codes and names are then stored, and upon the next refresh, when this class is reconstructed, the _loadedServices memory variable will be updated with the latest script names.
     * 
     * Note that, the real code will only get registered into angular container when it´s asked for execution, i.e, upon injector.get called
     * 
     */
    class dynamicScriptsCacheService {

        constructor($rootScope,$q,$log,localStorageService,$injector) {
            this.localStorageService = localStorageService;
            this.$injector = $injector;
            //DO not inject this one, to reduce number of dependencies. Check clientawareserviceprovider.js
//            this.restService = restService;
            this.initEntriesFromLocalStorage();
            this.$q = $q;
            this.$rootScope = $rootScope;
            this.$log = $log;

            //#region private fns

            /**
             * Name of the custom service for the injector
             */
            customServiceName = (service) => {
                return "$sw:custom_" + service;
            }

            /**
             * Name of the code entry for the localstorage
             */
            customServiceCodeKeyName = (service) => {
                return "sw:customservice_" + service;
            }


            //#endregion
            
        }

        useCustomServiceIfPresent(serviceName) {
            const log = this.$log.get("dynamicScriptsCacheService#useCustomServiceIfPresent", ["dynscripts", "angular", "services"]);
            log.trace(`checking custom service + ${serviceName}`);

            const loadedService = this._loadedServices[serviceName];
            if (!!loadedService) {
                const serviceNameToCheck = loadedService.custom ? customServiceName(serviceName) : serviceName;
                if (this.$injector.has(serviceNameToCheck)) {
                    log.debug(`returning custom service + ${serviceName}`);
                    return this.$injector.get(serviceNameToCheck);
                }
                const code = this.localStorageService.get(customServiceCodeKeyName(serviceName));
                var script = this.registerScript(serviceNameToCheck, code);
                if (!script) {
                    //fallback to default implementation
                    return null;
                }
                log.debug(`returning custom service + ${serviceName}`);
                return this.$injector.get(serviceNameToCheck);
            }
            return null;
        }

        /**
         * Used also by the offline codebase
         * @returns {} 
         */
        getClientState() {
            const clientState = {};
            Object.keys(this._loadedServices).forEach(k => {
                clientState[k] = this._loadedServices[k].rowstamp;
            });
            return clientState;
        }

        /**
         * Method to be called once, after login authentication, to update the client side customscripts localstorage and inmemory structures
         * 
         * Note that there´s no easy way to unregister a service, so we shall rely on a browser refresh in order to have the services reloaded correctly
         * 
         * @param {} serverSideData 
         * @returns {} 
         */
        syncWithServerSideScripts(serverSideData = null) {
            const log = this.$log.get("dynamicScriptsCacheService#syncWithServerSideScripts", ["dynscripts","angular", "services"]);
            //            const clientState = { "items": _loadedServices };
            const clientState = this.getClientState();
            if (!this.restService) {
                //Do not inject rest service as it would broad the dependency graph due to the presence of the ajax_interceptor class, making less important services candidates for replacement.
                this.restService = this.$injector.get("restService");
            }

            let promise;
            if (serverSideData) {
                //we already have the data resolved, no need to fetch it
                promise = this.$q.when({ data: serverSideData });
            } else {
                promise = this.restService.postPromise("Scripts", "BuildSyncMap", null,clientState );
            }
            //maps to ScriptSyncResultDTO    
        
            return promise.then(response => {
                    
                const items = response.data;
                var hasUpdate = items.length > 0;
                items.forEach(script => {
                    if (script.toDelete) {
                        delete this._loadedServices[script.target];
                        this.localStorageService.remove("sw:customservice_" + script.target);
                    } else {
                        let loadedService = this._loadedServices[script.target];
                        if (!loadedService) {
                            this._loadedServices[script.target] = {};
                            loadedService = this._loadedServices[script.target];
                        }
                        loadedService.rowstamp = script.rowstamp;
                        loadedService.custom = this.$injector.has(script.target);
                        log.info("updating script cache for (refresh requested)" + script.target);
                        this.localStorageService.put(customServiceCodeKeyName(script.target),script.code,{compress:true});
                    }
                });
                if (hasUpdate) {
                    this.localStorageService.put("sw:customservicesentries", this._loadedServices);
                    //true updates will only be applied upon next browser refresh, so forcing it, especially for offline scenarios
                    //TODO: figure out a way
                    window.location.reload();
                }
            });
        }

        


            registerScript($provide, scriptName,serviceClassBody) {
                const log = this.$log.get("dynamicScriptsCacheService#register", ["dynscripts","angular", "services"]);
                const evaluatedClass = eval(serviceClassBody);
                if (!evaluatedClass) {
                    log.warn(`error evaluating class for script ${scriptName}`);
                    return null;
                }

                const scriptInjections  = angular.injector.$$annotate(evaluatedClass);
                if (!!scriptInjections) {
                    evaluatedClass["$inject"] = scriptInjections;
                }

                log.info(`registering script ${scriptName}`);

                return $provide.service(scriptName, evaluatedClass);
            }

            /**
             * Builds the cache of custom services that are present on the instance from the localstorage
             * 
             * Note that the actual script is evaluated lazily, at his first utilization, due to a performance hit it would imply on a Browser Refresh to uncompress potentially large javascript files.
             * 
             * Also, this is structure is yet to be merged with the server upon login to sync eventually out of date scripts
             * 
             * The main structure consists of the following object (name,rowstamp dict)
             * 
             * {
              * xxx: {
                     rowstamp:1231413141341
                     custom: true
                }
                yyy:{
                    rowstamp:1231413141344
                    custom:false
                }
               }
             */
            initEntriesFromLocalStorage() {
                const entries = this.localStorageService.get("sw:customservicesentries") || {};
                this._loadedServices = entries;
            }

            clearEntries() {
                this.localStorageService.remove("sw:customservicesentries");
                const hadKeys = this._loadedServices.length > 0;
                Object.keys(this._loadedServices).forEach(k => {
                    var name = customServiceCodeKeyName(k);
                    this.localStorageService.remove(name);
                });
                this._loadedServices = {};
                if (hadKeys) {
                    window.location.reload();
                }
            }

     

        }

        dynamicScriptsCacheService["$inject"] = ["$rootScope","$q","$log","localStorageService", "$injector"];

        angular.module("sw_rootcommons").service("dynamicScriptsCacheService", dynamicScriptsCacheService);

        })(angular);