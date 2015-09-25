(function (angular) {
    'use strict';

    function richTextService() {
        /**
         * matches '<mailto:(email pattern)>'
         * email pattern (accepts unicode characters and '.' before domain name) from: http://stackoverflow.com/questions/46155/validate-email-address-in-javascript#answer-16181
         * PS: for some reason, compiling the pattern from a String (using new RegExp(pattern, "g")) did not work (didn't match the pattern)
         * that's why the regex is in it's literal format
         */
        var mailtoTagRegex = /\<mailto\:(([^<>()[\]\.,;:\s@\"]+(\.[^<>()[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})\>/g;
        var htmlLinkPattern = "<a href='mailto:{0}'>{0}</a>";

        /**
         * Replaces all '<mailto: ... >' tags on the text
         * by '<a href="mailto: ... "> ... </a>'
         * 
         * @param String text 
         * @returns text with replaced tags 
         */
        function replaceMailToTags(text) {
            if (!text) return text;
            var tags = text.match(mailtoTagRegex);
            if (!tags || tags.lenght <= 0) {
                 return text;
            }
            var replaced = angular.copy(text);
            angular.forEach(tags, function (tag) {
                // email address between ':' and '>'
                var email = tag.substring(tag.indexOf(":") + 1, tag.length - 1);
                var htmlLink = htmlLinkPattern.format(email);
                replaced = replaced.replace(tag, htmlLink);
            });
            return replaced;
        }

        function getDecodedValue(content) {
            var decodedHtml = content;

            // Matches any encoded html tag - &lt; &gt;
            var regexEncode = new RegExp("&(lt|gt);");
            // Also make sure non of these tags are present to truly confirm this is encoded HTML
            var regexHTML = new RegExp("(<|>)");

            if (regexEncode.test(content) && !regexHTML.test(content)) {
                decodedHtml = $('<div/>').html(content).text();
            }
            return replaceMailToTags(decodedHtml);
        }


        var service = {
            getDecodedValue: getDecodedValue,
            replaceMailToTags: replaceMailToTags
        };

        return service;
    }

    angular.module('sw_layout').factory('richTextService', [richTextService]);

})(angular);
