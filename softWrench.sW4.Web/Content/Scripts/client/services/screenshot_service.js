var app = angular.module('sw_layout');

app.factory('screenshotService', function ($rootScope, $timeout, i18NService,$log) {

    return {

        init: function (bodyElement, datamap) {
            $log.getInstance('sw4.screenshotservice').debug('init screenshot service');
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


        handleImgHolderBlur: function (imgHolder, datamap) {
            var imgAttribute = imgHolder.id.substring('imgHolder_'.length);

            datamap[imgAttribute] = Base64.encode(imgHolder.innerHTML);

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


