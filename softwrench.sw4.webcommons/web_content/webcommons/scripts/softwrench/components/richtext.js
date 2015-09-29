(function (angular) {
    'use strict';

    function richTextService() {

        var invalidTagsConfig = {
            /**
             * text processors queue
             */
            processors: [],
            /**
             * matches '<mailto:(email pattern)>' or '<(email pattern)>'
             * email pattern (accepts unicode characters and '.' before domain name) from: http://stackoverflow.com/questions/46155/validate-email-address-in-javascript#answer-16181
             * PS: for some reason, compiling the pattern from a String (using new RegExp(pattern, "g")) did not work (didn't match the pattern)
             * that's why the regex is in it's literal format
             */
            invalidEmailTagsRegex: /\<mailto\:(([^<>()[\]\.,;:\s@\"]+(\.[^<>()[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})\>|\<(([^<>()[\]\.,;:\s@\"]+(\.[^<>()[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,})\>/g,
            /**
             * matches '<(url pattern)>' (url pattern includes query string)
             * from: mix of http://code.tutsplus.com/tutorials/8-regular-expressions-you-should-know--net-6149 with http://stackoverflow.com/questions/23959352/validate-url-query-string-with-regex#answer-23959662
             */
            invalidUrlTagsRegex: /\<(https?:\/\/)([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?(\?([\w-]+(=[\w-]*)?(&[\w-]+(=[\w-]*)?)*)?)?\>/g,
            /**
             * pattern to build a valid html link tag
             */
            htmlLinkPattern: "<a href='{1}{0}'>{0}</a>"
        };

        function buildHtmlLinkTag(url, prefix) {
            return invalidTagsConfig.htmlLinkPattern.format(url, prefix || "");
        }

        /**
         * Register a text processor to be executed for replacing invalid html tags on a source text.
         * It's an iterative process so the next processor in the queue always receives a 'partially processed'
         * (processed by the previously executed processor) text as parameter.
         * 
         * @param RegExp regex matcher of the invalid tag the processor handles 
         * @param Function processor receives the source (partially processed) text and a problematic tag and returns the processed text 
         */
        function registerInvalidTagProcessor(regex, processor) {
            invalidTagsConfig.processors.push({ 'regex': regex, 'processor': processor });
        }

        function executeInvalidTagProcessors(text) {
            if (!text) return text;
            var processed = angular.copy(text);
            angular.forEach(invalidTagsConfig.processors, function(proc) {
                var regex = proc.regex;
                var processor = proc.processor;
                var tags = processed.match(regex);
                if (!tags || tags.length <= 0) return;
                angular.forEach(tags, function(tag) {
                    processed = processor(processed, tag);
                });
            });
            return processed;
        }

        /**
         * Replaces invalid tags (that would otherwise case parse errors) by their valid counterpart e.g.:
         * - Replaces all '<mailto:(email pattern)>' or '<(email pattern)>' tags on the text by '<a href="mailto:email address">email address</a>'.
         * - Replaces all '<(url pattern)>' tags on the text by '<a href="url">url</a>'
         * 
         * @param String text 
         * @returns text with replaced tags 
         */
        function replaceInvalidTags(text) {
            return executeInvalidTagProcessors(text);
        }

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
            return replaceInvalidTags(decodedHtml);
        }

        function init() {
            // email
            registerInvalidTagProcessor(invalidTagsConfig.invalidEmailTagsRegex, function (source, tag) {
                // extract email address from tag: between ':' or '<'(first character) and '>'(last character)
                var start = tag.indexOf(":");
                start = start >= 0 ? start + 1 : 1;
                var email = tag.substring(start, tag.length - 1);
                // creating valid html link tag and replacing invalid tag by it
                var htmlLink = buildHtmlLinkTag(email, "mailto:");
                return source.replace(tag, htmlLink);
            });
            // url
            registerInvalidTagProcessor(invalidTagsConfig.invalidUrlTagsRegex, function (source, tag) {
                // extract url from tag: between '<'(first character) and '>'(last character)
                var url = tag.substring(1, tag.length - 1);
                // creating valid html link tag and replacing invalid tag by it
                var htmlLink = buildHtmlLinkTag(url);
                return source.replace(tag, htmlLink);
            });
        }

        var service = {
            getDecodedValue: getDecodedValue,
            replaceInvalidTags: replaceInvalidTags
        };

        init();
        return service;
    }

    angular.module('sw_layout').factory('richTextService', [richTextService]);

})(angular);
