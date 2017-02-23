function FaqController($scope, i18NService, redirectService, $timeout) {
    "ngInject";
    var isOpen = 0;
    $scope.showDefinitions = function (data) {
        if (data != null && data.solutionId != null) {
            redirectService.goToApplicationView("solution", "detail", "output", "FAQ", { id: data.solutionId, faqid: data.faqId, lang: data.lang, popupmode: 'browser' });
        }
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.doInit = function () {
        var data = $scope.resultData;
        var lang = data.lang;

        $scope.faqLanguageFilter = lang == null ? 'en' : lang;
        $scope.faqTextFilter = data.search;

        var dataObject = JSON.parse(data.faqModelResponseJson);
        if (dataObject != null) {
            $scope.isFaqOk = true;
            $scope.roleList = dataObject;
            $scope.isSearchOk = !jQuery.isEmptyObject($scope.roleList);
        }
    };

    $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
        if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("Faq.html") != -1) {
            $scope.doInit();
        }
    });

    $scope.filterFaq = function () {
        if ($scope.faqTextFilter == null) {
            $scope.faqTextFilter = "";
        }
        var parameters = {};
        parameters.lang = $scope.faqLanguageFilter;
        parameters.search = $scope.faqTextFilter;
        $scope.$parent.doAction(null, "FaqApi", "Index", parameters);
    };

    $scope.doInit();
};