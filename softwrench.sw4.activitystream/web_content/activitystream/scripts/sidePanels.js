(function (angular) {
    "use strict";

    // directive responsible to add the sidepanels - it does only after it knows which panels should be added and their expanded or not state
    // this avoids layout recalculations due to side panels loading in different times
    angular.module("sw_layout").directive("sidepanels", ["$rootScope", "$compile", "$q", "contextService", "configurationService", "crudContextHolderService", "sidePanelService", "crudSearchService",
        function ($rootScope, $compile, $q, contextService, configurationService, crudContextHolderService, sidePanelService, crudSearchService) {
        "ngInject";

        const bbEnabledKey = "/Global/BulletinBoard/Enabled";

        return {
            restrict: "E",
            template: "<div></div>",
            replace: true,
            link: function (scope, element) {

                const expandedSidePanel = sidePanelService.getExpandedPanelFromPreference();
                const csEnabled = contextService.fetchFromContext("crudSearchFlag", false, true);

                function addPanel(panelid, handlewidth, template) {
                    // register the current side panel on service
                    // the next registers need this to know the panel handle top value
                    sidePanelService.registerPanel(panelid, handlewidth);

                    if (expandedSidePanel === panelid && !sidePanelService.isOpened(panelid) && $rootScope.deviceType === "desktop") {
                        sidePanelService.toggle(panelid);
                    }

                    element.append(template);
                }

                function getConfigs() {
                    return configurationService.fetchConfigurations([bbEnabledKey]);
                }

                function getApplicationAndSchema() {
                    if (!csEnabled) {
                        return $q.when(null);
                    }

                    const appDefered = $q.defer();

                    scope.$on("ngLoadFinished", function (event) {
                        appDefered.resolve({
                            applicationName: crudContextHolderService.currentApplicationName(),
                            schema: crudContextHolderService.currentSchema()
                        });
                    });

                    return appDefered.promise;
                }

                // only tries to init the side panels when all info is gathered:
                // - the configurations
                // - the current application name (only needed if a crud search exists and is on a crud page)
                // - the seach schema (only needed if a crud search exists and is on a crud page)
                $q.all([getConfigs(), getApplicationAndSchema()]).then((values) => {
                    const configs = values[0];
                    if (!csEnabled) {
                        init(configs);
                    }

                    const applicationName = values[1].applicationName;
                    const schema = values[1].schema;
                    const searchSchemaId = crudSearchService.getSearchSchemaId(applicationName, schema);
                    crudSearchService.getSearchSchema(applicationName, searchSchemaId).then((searchSchema) => {
                        init(configs, applicationName, searchSchema);
                    });
                });

                function init(configs, applicationName, searchSchema) {
                    const asEnabled = contextService.fetchFromContext("activityStreamFlag", false, true);
                    if (asEnabled) {
                        const template = '<sidepanel panelid="activitystream" handleicon="fa-rss" handletitle="Activity"><activitystream class="notificationstream"></activitystream></sidepanel>';
                        addPanel("activitystream", 128, template);
                    }

                    if (csEnabled) {
                        addCrudSearch(applicationName, searchSchema);
                    }

                    const bbConfigValue = configs[bbEnabledKey] || configs[bbEnabledKey.toLocaleLowerCase()];
                    const bbEnabled = [true, "true", "True", 1, "1"].includes(bbConfigValue);
                    if (bbEnabled) {
                        const template = '<sidepanel panelid="bulletinboard" handleicon="fa-bullhorn" handletitle="Bulletin Board"><bulletin-board panelid="bulletinboard" class="notificationstream"></bulletin-board></sidepanel>';
                        addPanel("bulletinboard", 156, template);
                    }

                    $compile(element.contents())(scope);
                }

                function addCrudSearch(applicationName, searchSchema) {
                    const panelid = "crudsearch";
                    const template = '<sidepanel panelid="crudsearch"><crudsearch></crudsearch></sidepanel>';
                    addPanel(panelid, null, template);

                    crudSearchService.updateCrudSearchSidePanel(panelid, searchSchema);

                    const ctx = sidePanelService.getContext(panelid);
                    ctx.application = applicationName;
                    ctx.schema = searchSchema;
                }
            }
        }
    }]);0
})(angular);