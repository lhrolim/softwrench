var app = angular.module('sw_layout');

app.factory('submitService', function ($rootScope, fieldService,contextService,restService,spinService) {

    "ngInject";

    return {
        ///used for ie9 form submission
        submitForm: function (formToSubmit, parameters, jsonString, applicationName) {

            //this quick wrapper ajax call will validate if the user is still logged in or not
            restService.getPromise("ExtendedData", "PingServer").then(function () {
                

                // remove from session the redirect url... the redirect url will be returned when the form submit response comes from server
                sessionStorage.removeItem("swGlobalRedirectURL");

                for (var i in parameters) {
                    formToSubmit.append("<input type='hidden' name='" + i + "' value='" + parameters[i] + "' />");
                }
                if (sessionStorage.mockmaximo == "true") {
                    formToSubmit.append("<input type='hidden' name='%%mockmaximo' value='true'/>");
                }


                formToSubmit.append("<input type='hidden' name='currentmodule' value='" + contextService.retrieveFromContext('currentmodule') + "' />");

                formToSubmit.append("<input type='hidden' name='application' value='" + applicationName + "' />");
                formToSubmit.append("<input type='hidden' name='json' value='" + replaceAll(jsonString, "'", "&apos;") + "' />");

                // start spin befor submitting form
                var savingMain = true === $rootScope.savingMain;
                spinService.start(savingMain);

                // submit form
                formToSubmit.attr("action", url("/Application/Input"));
                formToSubmit.submit();
            });
        },

        ///return if a field which is not on screen (but is not a hidden instance), and whose value is null from the datamap, avoiding sending useless (and wrong) data
        removeNullInvisibleFields: function (displayables, datamap) {
            var fn = this;
            $.each(displayables, function (key, value) {
                if (fieldService.isNullInvisible(value, datamap)) {
                    delete datamap[value.attribute];
                }
                if (value.displayables != undefined) {
                    fn.removeNullInvisibleFields(value.displayables, datamap);
                }

            });
        },

        getFormToSubmitIfHasAttachement: function (displayables, datamap) {

            var form = $("#crudbodyform");
            var formId = null;
            $('.richtextbox', form).each(function () {

                if (this.contentWindow.asciiData != null && this.contentWindow.asciiData() != undefined && this.contentWindow.asciiData() != "") {
                    formId = $(this).closest('form');
                }
            });

            var isValidfn = this.isValidAttachment;

            $('input[type="file"]', form).each(function () {
                if (this.value != null && this.value != "" && isValidfn(this.value)) {
                    formId = $(this).closest('form');
                }
            });
            return formId;
        },

        isValidAttachment: function(value){
            var fileName = value.match(/[^\/\\]+$/);
            var validFileTypes = contextService.fetchFromContext('allowedfiles', true);
            var extensionIdx = value.lastIndexOf(".");
            var extension = value.substring(extensionIdx + 1).toLowerCase();
            if ($.inArray(extension, validFileTypes) == -1) {
                return false;
            }
            return true;
        },




        removeExtraFields: function (datamap, clone, schema) {
            if (!datamap.extrafields) {
                if (clone) {
                    return jQuery.extend(true, {}, datamap);
                }
                return datamap;
            }

            var data;
            if (clone) {
                data = jQuery.extend(true, {}, datamap);
            } else {
                data = datamap;
            }
            $.each(data, function (key, value) {
                //TODO: replace this gambi
                var isAssociatedData = key.indexOf(".") === -1;
                var isSafeKeyNeeded = key === "asset_.primaryuser_.personid";
                if (data.extrafields[key] != undefined) {

                    if (fieldService.getDisplayableByKey(schema, key) == undefined && !isSafeKeyNeeded) {
                        delete data[key];
                    }

                }
            });
            delete data.extrafields;
            return data;
        },

        translateFields: function (displayables, datamap) {
            var fieldsToTranslate = $.grep(displayables, function (e) {
                return e.attributeToServer != null;
            });
            for (var i = 0; i < fieldsToTranslate.length; i++) {
                var field = fieldsToTranslate[i];
                datamap[field.attributeToServer] = datamap[field.attribute];
                delete datamap[field.attribute];
            }
        }

    };

});