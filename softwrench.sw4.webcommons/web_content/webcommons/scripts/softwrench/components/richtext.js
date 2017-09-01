(function (angular) {
    "use strict";

    function richTextService() {

        const invalidTagsConfig = {
            /**
             * text processors queue
             */
            processors: [],

            regex: {
                /**
                 * matches '<(email_pattern)<(mailto:)?(email pattern)+>>'
                 */
                nestedemailtags: /\<((([^<>()[\]\.,;:\s@\"]+(\.[^<>()[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,}))+(\<(mailto\:)?((([^<>()[\]\.,;:\s@\"]+(\.[^<>()[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,}))+\>)\>/g,
                /**
                 * matches '<(mailto:)?(email pattern)+>'
                 * email pattern (accepts unicode characters and '.' before domain name) from: http://stackoverflow.com/questions/46155/validate-email-address-in-javascript#answer-16181
                 * PS: for some reason, compiling the pattern from a String (using new RegExp(pattern, "g")) did not work (didn't match the pattern)
                 * that's why the regex is in it's literal format
                 */
                emailtags: /\<(mailto\:)?((([^<>()[\]\.,;:\s@\"]+(\.[^<>()[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\.,;:\s@\"]+\.)+[^<>()[\]\.,;:\s@\"]{2,}))+\>/g,
                /**
                 * matches '<(url pattern)+>' (url pattern includes query string)
                 * from: mix of http://code.tutsplus.com/tutorials/8-regular-expressions-you-should-know--net-6149 with http://stackoverflow.com/questions/23959352/validate-url-query-string-with-regex#answer-23959662
                 */
                urltags: /\<(((https?|ftp):\/\/)([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)\/?(\?([\w-]+(=[\w-]*)?(&[\w-]+(=[\w-]*)?)*)?)?)+\>/g,
                /**
                 * matches <file://(/\\)?/(path pattern)>
                 */
                fileurltags: /\<(file:\/\/(\/\\\\)?([\da-zA-Z\.-]+)([(\/|\\)\w \.-]*)\/?(\?([\w-]+(=[\w-]*)?(&[\w-]+(=[\w-]*)?)*)?)?)+\>/g,
                /**
                 * matches '<![if ((!)?any characters)]>(any characters)<![endif]>'
                 * e.g. "<![if !supportLists]>· <![endif]>"
                 * Usually introduced by email applications around special rich text constructs (such as lists).
                 */
                iftags: /\<\!\[if\s\!?([^\<\>\[\]])+\]\>([^\<\>\[\]])+\<\!\[(endif)\]\>/g
            },
           
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
         * @param Array[RegExp] regex matchers of the invalid tags the processor handles 
         * @param Function processor receives the source (partially processed) text and a problematic tag and returns the processed text 
         */
        function registerInvalidTagProcessor(regex, processor) {
            angular.forEach(regex, function(r) {
                invalidTagsConfig.processors.push({ 'regex': r, 'processor': processor });
            });
        }

        function executeInvalidTagProcessors(text) {
            if (!text) return text;
            var processed = angular.copy(text);
            angular.forEach(invalidTagsConfig.processors, function(proc) {
                const regex = proc.regex;
                const processor = proc.processor;
                const tags = processed.match(regex);
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

        function getDisplayableValue(content) {
            // replace text line-breaks by html entity line-feed + html entity carriage return 
            content = replaceAll(content, "\n", String.fromCharCode(10, 13));
            return content;
        }

        function getDecodedValue(content) {
            const decodedHtml = getDisplayableValue(content);
            return replaceInvalidTags(decodedHtml);
        }

        function init() {
            // nested email
            registerInvalidTagProcessor([invalidTagsConfig.regex.nestedemailtags],
                function(source, tag) {
                    // extract email address from tag
                    // between '<' (position = 1) and second '<' (last index of '<')
                    const email = tag.substring(1, tag.lastIndexOf("<"));
                    // valid email tag
                    const htmlLink = buildHtmlLinkTag(email, "mailto:");
                    return source.replace(tag, htmlLink);
                });
            // email
            registerInvalidTagProcessor([invalidTagsConfig.regex.emailtags],
                function (source, tag) {
                    // extract email address from tag: 
                    // between ':'(from '<mailto:') or '<'(first character) and '>'(last character)
                    const hasmailto = tag.indexOf("<mailto:") >= 0;
                    const start = hasmailto ? tag.indexOf(":") + 1 : 1;
                    const email = tag.substring(start, tag.length - 1);
                    // creating valid html link tag and replacing invalid tag by it
                    const htmlLink = buildHtmlLinkTag(email, "mailto:");
                    return source.replace(tag, htmlLink);
                });
            // url + fileurl
            registerInvalidTagProcessor([invalidTagsConfig.regex.urltags, invalidTagsConfig.regex.fileurltags],
                function (source, tag) {
                    // extract url from tag: between '<'(first character) and '>'(last character)
                    const url = tag.substring(1, tag.length - 1);
                    // creating valid html link tag and replacing invalid tag by it
                    const htmlLink = buildHtmlLinkTag(url);
                    return source.replace(tag, htmlLink);
                });
            // if
            registerInvalidTagProcessor([invalidTagsConfig.regex.iftags],
                function (source, tag) {
                    // extract text wrapped by the 'if' tag
                    const start = tag.indexOf("]>") + 2;
                    const end = tag.lastIndexOf("<![endif]>");
                    const wrapped = tag.substring(start, end);
                    // remove the surrounding if tag
                    return source.replace(tag, wrapped);
                });
        }

        const service = {
            getDecodedValue,
            replaceInvalidTags,
            getDisplayableValue
        };
        init();
        return service;
    }

    angular.module("webcommons_services").service("richTextService", [richTextService]);

})(angular);
