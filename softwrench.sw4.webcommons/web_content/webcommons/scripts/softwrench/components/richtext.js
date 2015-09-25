(function (angular) {
    'use strict';

    function richTextService() {

        function getDecodedValue(content) {
            content = replaceAll(content,"\n", "<br/>");
            var decodedHtml = content;
            // Matches any encoded html tag - &lt; &gt;
            var regexEncode = new RegExp("&(lt|gt);");
            // Also make sure non of these tags are present to truly confirm this is encoded HTML
            var regexHTML = new RegExp("(<|>)");

            if (regexEncode.test(content) && !regexHTML.test(content)) {
                decodedHtml = $('<div/>').html(content).text();
            }
            return decodedHtml;
        }


        var service = {
            getDecodedValue: getDecodedValue
        };

        return service;
    }

    angular.module('sw_layout').factory('richTextService', [richTextService]);

})(angular);
