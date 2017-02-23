var app = angular.module('sw_layout');

app.factory('screenshotService', function ($rootScope, $timeout, i18NService, $log) {

    "ngInject";

    return {

        init: function (bodyElement, datamap) {
            $log.getInstance('screenshotservice#init').debug('init screenshot service');
            var fn = this;
            // Configure image holders
            $('.image-holder', bodyElement).bind('paste', function (event) {
                fn.handleImgHolderPaste(this, event.originalEvent);
            });
            $('.image-holder', bodyElement).bind('blur', function (event) {
                fn.handleImgHolderBlur(this, datamap);
            });
            $('.richtextbox', bodyElement).parents('form:first').bind('submit', function (event) {
                fn.handleRichTextBoxSubmit($(this), event.originalEvent);
            });
        },

        hasScreenshotData: function () {
            $('.richtextbox', form).each(function () {
                return this.contentWindow.asciiData() != null && this.contentWindow.asciiData() != "";
            });
        },

        handleRichTextBoxSubmit: function (form, event) {

            $('.richtextbox', form).each(function () {
                //this is for ie9
                if (this.contentWindow.asciiData() == null || this.contentWindow.asciiData() == "") {
                    //if nothing pasted, do nothing
                    return;
                }

                var rtbAttribute = this.id.substring('richTextBox_'.length);
                var log = $log.getInstance('screenshotservice#ie9conversion');

                var t0 = performance.now();
                var binaryData = this.contentWindow.binaryData();
                var t1 = performance.now();
                log.debug("get binary data took {0} ms".format(t1 - t0));

                form.append("<input type='hidden' name='" + rtbAttribute + "' value='" + binaryData + "' />");
                var now = new Date();
                var timestamp = '' + now.getFullYear() + (now.getMonth() + 1) + now.getDate();
                form.append("<input type='hidden' name='" + rtbAttribute + "_path' value='" + "Screen" + timestamp + ".rtf" + "' />");
            });
        },


        handleImgHolderBlur: function (imgHolder, datamap) {
            var t0 = performance.now();
            var imgAttribute = imgHolder.id.substring('imgHolder_'.length);
            datamap[imgAttribute] = Base64.encode(imgHolder.innerHTML);
            var t1 = performance.now();
            $log.getInstance('screenshotservice#handleImgHolderBlur').debug('base64 converstion took {0}'.format(t1 - t0));
            var now = new Date();
            var timestamp = '' + now.getFullYear() + (now.getMonth() + 1) + now.getDate();
            datamap[imgAttribute + "_path"] = "Screen" + timestamp + ".html";
        },

        handleImgHolderPaste: function (imgHolder, e) {

            var imgAttribute = imgHolder.id.substring('imgHolder_'.length);

            // Chrome: check if clipboardData is available
            if (e.clipboardData != undefined && e.clipboardData.items != undefined) {

                var items = e.clipboardData.items;
                for (var i = 0; i < items.length; i++) {

                    // Check if an image was pasted
                    if (items[i].type.indexOf("image") !== -1) {
                        var blob = items[i].getAsFile();
                        var url = window.URL || window.webkitURL;
                        var source = url.createObjectURL(blob);
                        this.createImage(imgAttribute, imgHolder, source);
                        return;
                    }
                }
                imgHolder.innerHTML = '';
                e.preventDefault();
            } else {
                // Firefox: the pasted object will be automaticaly included on imgHolder, so do nothing
            }
        },

        createImage: function (imgAttribute, imgHolder, source) {
            var pastedImage = new Image();

            pastedImage.onload = function () {
                var img = new Image();
                img.src = imgToBase64(this);
                imgHolder.appendChild(img);
            };
            pastedImage.src = source;
        }


    };
});


