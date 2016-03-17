/*
 * jQuery Scanner Detection
 *
 * Copyright (c) 2013 Julien Maurel
 *
 * Licensed under the MIT license:
 * http://www.opensource.org/licenses/mit-license.php
 *
 * Project home:
 * https://github.com/julien-maurel/jQuery-Scanner-Detection
 *
 * Version: 1.1.4
 *
 */
(function ($) {
    // Modified from http://stackoverflow.com/questions/5001608/how-to-know-if-the-text-in-a-textbox-is-selected
    function getSelection(textbox) {
        var selectedRange = null;
        var activeElement = document.activeElement;

        // all browsers (including IE9 and up), except IE before version 9 
        if (window.getSelection && activeElement &&
            (activeElement.tagName.toLowerCase() == "textarea" || (activeElement.tagName.toLowerCase() == "input" && activeElement.type.toLowerCase() == "text")) &&
            activeElement === textbox) {
            var startIndex = textbox.selectionStart;
            var endIndex = textbox.selectionEnd;

            if (endIndex - startIndex > 0) {
                selectedRange = { startOffset: startIndex, endOffset: endIndex };
            }
        }
        else if (document.selection && document.selection.type == "Text" && document.selection.createRange) // All Internet Explorer
        {
            selectedRange = document.selection.createRange();
        }

        return selectedRange;
    }

    $.fn.scannerDetection = function (options) {

        if (options === null) {
            //removing the scanner listener, for the sake of application restoring best performance on inputs
            $(this).data('scannerDetection', { options: {} })
                .unbind('.scannerDetection').unbind('keydown.scannerDetection');
            return this;
        }

        // If string given, call onComplete callback
        if (typeof options === "string") {
            this.each(function () {
                this.scannerDetectionTest(options);
            });
            return this;
        }

        var defaults = {
            onComplete: false, // Callback after detection of a successfull scanning (scanned string in parameter)
            onError: false, // Callback after detection of a unsuccessfull scanning (scanned string in parameter)
            onReceive: false, // Callback after receive a char (scanned char in parameter)
            timeBeforeScanTest: 100, // Wait duration (ms) after keypress event to check if scanning is finished
            avgTimeByChar: 14, // Average time (ms) between 2 chars. Used to do difference between keyboard typing and scanning
            minLength: 3, // Minimum length for a scanning
            endChar: [9, 13], // Chars to remove and means end of scanning
            stopPropagation: false, // Stop immediate propagation on keypress event
            preventDefault: false // Prevent default action on keypress event
        };
        if (typeof options === "function") {
            options = { onComplete: options }
        }
        if (typeof options !== "object") {
            options = $.extend({}, defaults);
        } else {
            options = $.extend({}, defaults, options);
        }

        this.each(function () {
            var self = this, $self = $(self), firstCharTime = 0, lastCharTime = 0, stringWriting = '', callIsScanner = false, testTimer = false;
            var charBuffer = '';
            var focusedInput;
            var initScannerDetection = function () {
                firstCharTime = 0;
                stringWriting = '';

            };
            self.scannerDetectionTest = function (s) {
                // If string is given, test it
                if (s) {
                    firstCharTime = lastCharTime = 0;
                    stringWriting = s;
                }
                // If all condition are good (length, time...), call the callback and re-initialize the plugin for next scanning
                // Else, just re-initialize
                if (stringWriting.length >= options.minLength && lastCharTime - firstCharTime < stringWriting.length * options.avgTimeByChar) {
                    if (options.onComplete) options.onComplete.call(self, stringWriting);
                    $self.trigger('scannerDetectionComplete', { string: stringWriting });
                    initScannerDetection();
                    return true;
                } else {
                    if (options.onError) {
                        options.onError.call(self, stringWriting);
                    }
                    $self.trigger('scannerDetectionError', { string: stringWriting });
                    if (focusedInput) {
                        var selectedRange = getSelection(focusedInput);
                        var newValue;
                        if (selectedRange) {
                            var originalText = $(focusedInput).val();
                            newValue = originalText.slice(0, selectedRange.startOffset) + stringWriting + originalText.slice(selectedRange.endOffset);
                        } else {
                            newValue = $(focusedInput).val() + stringWriting;
                        }
                        $(focusedInput).val(newValue);
                        $(focusedInput).trigger('input');
                        charBuffer = '';
                    }
                    initScannerDetection();
                    return false;
                }
            }
            $self.data('scannerDetection', { options: options }).unbind('.scannerDetection').bind('keydown.scannerDetection', function (e) {
                // Add event on keydown because keypress is not triggered for non character keys (tab, up, down...)
                // So need that to check endChar (that is often tab or enter) and call keypress if necessary
                if (firstCharTime && options.endChar.indexOf(e.which) !== -1) {
                    // Clone event, set type and trigger it
                    var e2 = jQuery.Event('keypress', e);
                    e2.type = 'keypress.scannerDetection';
                    if (!focusedInput) {
                        $self.triggerHandler(e2);
                    }
                    // Cancel default
                    e.preventDefault();
                    e.stopImmediatePropagation();
                }
            }).bind('keypress.scannerDetection', function (e) {


                if ((e.which >= 48 && e.which <= 90 || (e.which >= 97 && e.which <= 122)) && (e.target.type == "text" || e.target.type == "search")) {
                    e.preventDefault();
                    focusedInput = e.target;
                    charBuffer = String.fromCharCode(e.which);
                } else {
                    focusedInput = null;
                }



                if (options.stopPropagation) e.stopImmediatePropagation();
                if (options.preventDefault) e.preventDefault();

                if (firstCharTime && options.endChar.indexOf(e.which) !== -1) {
                    e.preventDefault();
                    e.stopImmediatePropagation();
                    callIsScanner = true;
                } else {
                    stringWriting += String.fromCharCode(e.which);
                    callIsScanner = false;
                }

                if (!firstCharTime) {
                    firstCharTime = Date.now();
                }
                lastCharTime = Date.now();

                if (testTimer) clearTimeout(testTimer);
                if (callIsScanner) {
                    self.scannerDetectionTest();
                    testTimer = false;
                } else {
                    testTimer = setTimeout(self.scannerDetectionTest, options.timeBeforeScanTest);
                }

                if (options.onReceive) options.onReceive.call(self, e);
                $self.trigger('scannerDetectionReceive', { evt: e });
            });
        });
        return this;
    }
})(jQuery);
