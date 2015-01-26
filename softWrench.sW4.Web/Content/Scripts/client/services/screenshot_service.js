﻿var app = angular.module('sw_layout');

app.factory('screenshotService', function ($rootScope, $timeout, i18NService, $log) {

    return {

        init: function (bodyElement, datamap) {
            $log.getInstance('sw4.screenshotservice').debug('init screenshot service');
            var fn = this;
            var imgHolder = $('.image-holder', bodyElement);

            $.each(imgHolder, function (key, v) {
                //theorically we could have many screenshots inside one single form, although usually we only have one
                var value = $(v);
                var attributeName = value.attr('name');
                var richBoxPlaceHolder = $("div[contenteditable='true']", value);
                var isRichTextBox = richBoxPlaceHolder != null;
                if (isRichTextBox) {
                    //for richtextbox we should put the imgs inside the contenteditable div instead
                    // we could have been using only the ordinary screenshot renderer instead...
                    imgHolder = richBoxPlaceHolder;
                }

                imgHolder.bind('paste', function (event) {
                    fn.handleImgHolderPaste(this, event.originalEvent, isRichTextBox);
                });

                imgHolder.bind('blur', function (event) {
                    fn.handleImgHolderBlur(this, datamap, attributeName, isRichTextBox);
                });
                $('.richtextbox', bodyElement).parents('form:first').bind('submit', function (event) {
                    fn.handleRichTextBoxSubmit($(this), event.originalEvent);
                });
            });
        },

        hasScreenshotData: function () {
            $('.richtextbox', form).each(function () {
                if (this.contentWindow.asciiData != null && this.contentWindow.asciiData() != undefined && this.contentWindow.asciiData() != "") {
                    return true;
                }
                return false;
            });
        },

        handleRichTextBoxSubmit: function (form, event) {

            $('.richtextbox', form).each(function () {

                if (this.contentWindow.asciiData != null && this.contentWindow.asciiData() != undefined && this.contentWindow.asciiData() != "") {



                    var rtbAttribute = this.id.substring('richTextBox_'.length);

                    $log.getInstance('sw4.screenshotservice').debug('handling screenshot paste name = ' + rtbAttribute);

                    form.append("<input type='hidden' name='" + rtbAttribute + "' value='" + Base64.encode(this.contentWindow.binaryData()) + "' />");

                    var now = new Date();
                    var timestamp = '' + now.getFullYear() + (now.getMonth() + 1) + now.getDate();

                    form.append("<input type='hidden' name='" + rtbAttribute + "_path' value='" + "Screen" + timestamp + ".rtf" + "' />");
                }
            });
        },


        handleImgHolderBlur: function (imgHolder, datamap, attributeName, isRichTextBox) {
            if (isRichTextBox) {
                //richtextbox screenshots will be handled as ordinary longdescriptions
                return;
            }

            $log.getInstance('sw4.screenshotservice#handleImgHolderBlur').debug('handling screenshot blur');
            var dataContent = imgHolder.innerHTML;
            datamap[attributeName + "_attachment"] = Base64.encode(dataContent);
            var now = new Date();
            var timestamp = '' + now.getFullYear() + (now.getMonth() + 1) + now.getDate();
            datamap[attributeName + "_path"] = "Screen" + timestamp + ".html";
        },

        handleImgHolderPaste: function (imgHolder, e,isRichTextBox) {
            

            if (isFirefox()) {
                // Firefox: the pasted object will be automaticaly included on imgHolder, so do nothing
                $timeout(function() {
                    $('img', imgHolder).attr('max-width', $(imgHolder).css('width'));
                }, 150, true);
                return;
            }

            // Chrome: check if clipboardData is available
            if (e.clipboardData == undefined || e.clipboardData.items == undefined) {
                return;
            }



            var items = e.clipboardData.items;
            for (var i = 0; i < items.length; i++) {
                if (items[i].type.indexOf("image") === -1) {
                    //just handling images
                    continue;
                }
                var blob = items[i].getAsFile();
                var url = window.URL || window.webkitURL;
                var source = url.createObjectURL(blob);
                this.createImage(imgHolder, source);
                e.preventDefault();
                break;
            }

        },

        createImage: function (imgHolder, source) {
            var pastedImage = new Image();

            pastedImage.onload = function () {
                var img = new Image();
                $(img).attr('contenteditable', 'true');
                //                $(img).css('max-width', '100%');
                //temporary cross-browser solution
                var jimgHolder = $(imgHolder);
                $(img).css('max-width', jimgHolder.css('width'));
                img.src = imgToBase64(this);
                jimgHolder.empty();
                jimgHolder.attr('hasimage', 'true');
                imgHolder.appendChild(img);
            };
            pastedImage.src = source;
            $(pastedImage).attr('class', 'pastedimage');
        }


    };
});


