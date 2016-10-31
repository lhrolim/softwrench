(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("bulletinBoard", ["contextService", function (contextService) {
        const directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Shared/activitystream/templates/bulletinboard.html"),
            scope: {
                panelid: "@"
            },

            controller: ["$scope", "sidePanelService", "richTextService", "formatService", "configurationService", "restService", "sseService", "$log", function ($scope, sidePanelService, richTextService, formatService, configurationService, restService, sseService, $log) {

                $scope.bulletinboard = {
                    messages: [],
                    filterText: "",
                    enableFilter: false
                };

                const configuration = {
                    enabled: "/Global/BulletinBoard/Enabled"
                };

                var bulletinBoardEventSource = null;

                $scope.setPaneHeight = function () {
                    return sidePanelService.calculateScrollPanelHeight($scope.panelid);
                };

                $scope.getItemsCountStyle = function() {
                    return { top: sidePanelService.getContext($scope.panelid).top - 85 + "px" };
                };

                $scope.toggleFilter = function() {
                    $scope.bulletinboard.enableFilter = !$scope.bulletinboard.enableFilter;
                    if (!$scope.bulletinboard.enableFilter) {
                        $scope.bulletinboard.filterText = "";
                    }
                };

                function hydrateMessage(message) {
                    message.formattedMessage = richTextService.getDecodedValue(message.message);
                    message.messagePreview = message.formattedMessage.length <= 200
                        ? message.formattedMessage
                        : `${message.formattedMessage.substring(0, 200)}...`;
                    message.formattedPostDate = formatService.formatDate(message.postDate);
                    message.formattedExpireDate = formatService.formatDate(message.expireDate);
                    message.meta = { more: false };
                    return message;
                }

                function onBulletinBoardUpdate(data) {
                    $scope.bulletinboard.messages = data.messages.map(hydrateMessage);
                }

                function fetchActiveMessages() {
                    return restService.get("BulletinBoard", "ActiveMessages", null, { avoidspin: true })
                        .then(r => onBulletinBoardUpdate(r.data));
                }

                function init() {
                    // start hidden: we don't know yet if customer has enabled the bulletin board
                    sidePanelService.hide($scope.panelid);

                    // fetch bulletin board feature configs then start
                    configurationService.fetchConfigurations(Object.values(configuration))
                        .then(configs => {
                            const enabled = configs[configuration.enabled];
                            if (![true, "true", "True", 1, "1"].includes(enabled)) return;
                            sidePanelService.show($scope.panelid);

                            // connect to bulletinboard SSE source
                            bulletinBoardEventSource = sseService.connect("BulletinBoard", "Subscribe");
                            bulletinBoardEventSource
                                .onMessage(event => onBulletinBoardUpdate(event.data), { runApply: true, dataType: "json" })
                                .on("subscriber:count", event => $log.get("bulletinboard#onSubscriberChanged", ["bulletinboard"]).debug(`SSE subscriber count = ${event.data}`))
                                .onOpen(event => $log.get("bulletinboard#onConnect", ["bulletinboard"]).debug("SSE connected", event))
                                .onError(error => $log.get("bulletinboard#onError", ["bulletinboard"]).error("SSE error", error));

                            // initial bulletinboard data
                            return fetchActiveMessages();
                        });
                }

                $scope.$on("$destroy", () => {
                    if (bulletinBoardEventSource) {
                        bulletinBoardEventSource.close();
                    }
                });

                init();
            }]
            
        };

        return directive;
    }]);

})(angular);