(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("richtextField", ["contextService", "$interval", "$log", function (contextService, $interval, $log) {
        const directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/directives/richtextfield.html"),
            replace: false,
            scope: {
                content: "=",
                readonly: "="
            },

            controller: ["$scope", "richTextService", "crudContextHolderService", function ($scope, richTextService, crudContextHolderService) {
                const log = $log.get("richtextfield#controller", ["richtext"]);

                $scope.content = richTextService.getDisplayableValue($scope.content);

                $scope.richtext = {
                    config: {
                        plugins: [
                            // from tinymce's basic + paste + codesample
                            "advlist autolink lists link image charmap print preview hr anchor pagebreak",
                            "searchreplace wordcount visualblocks visualchars code fullscreen",
                            "insertdatetime nonbreaking save table contextmenu directionality",
                            "emoticons template paste textcolor colorpicker textpattern imagetools",
                            "codesample paste"
                        ],

                        skin_url: url("Content/customVendor/css/tinymce/skins/lightgray"),
                        height: 250,

                        paste_data_images: true, // paste SS
                        paste_retain_style_properties: "color font-size",
                        paste_word_valid_elements: "b,strong,i,em,h1,h2",

                        codesample_dialog_height: window.innerHeight - 100,
                        codesample_dialog_width: window.innerWidth - 100,
                        codesample_style: url("Content/customVendor/css/prism.css"),

                        readonly: $scope.readonly,
                        debounce: true,

                        menubar: $scope.readonly ? false : "edit insert table",
                        toolbar: $scope.readonly ? false : "styleselect blockquote | bold italic underline bullist numlist undo redo | alignleft aligncenter alignright alignjustify | link image codesample",
                        statusbar: false,

                        // hide/show toolbar+menubar on blur/focus
                        setup: function(editor) {
                            const executeOnActionBars = (action, el) => {
                                const parentElement = $(el.contentAreaContainer.parentElement);
                                parentElement.find("div.mce-toolbar-grp")[action]();
                                parentElement.find("div.mce-menubar")[action]();
                            }
                            editor.on("focus", function() {
                                executeOnActionBars("show", this);
                            });
                            editor.on("blur", function() {
                                executeOnActionBars("hide", this);
                            });
                            editor.on("init", function () {
                                log.debug("tinymce editor's init", editor);
                                executeOnActionBars("hide", this);
                                // click/focus on codesamples trigger focus on the editor
                                const localEditor = editor;
                                $(this.getBody().querySelectorAll("pre[class^='language-']")).on("click", () => localEditor.focus());
                            });
                        }
                    }
                };
            }],

            link: function (scope, element, attrs) {
                const log = $log.get("richtextfield#link", ["richtext"]);
                // very very dirty hack to ensure tinymce editor is in the screen
                // for some dynamically added fields this hack is necessary (e.g. inside modals, composition_masterdetails)
                var canceled = false;
                const interval = $interval(() => {
                    log.debug("loop to check tinymce's editor is in the screen");
                    const tinyMceFrame = element[0].querySelector("iframe");
                    if (!tinyMceFrame) {
                        log.debug("tinymce's iframe not yet present. skipping");
                        return;
                    }
                    const contentId = $(tinyMceFrame.contentWindow.document.body).attr("data-id");
                    if (!contentId) {
                        log.debug("refreshing angular-ui-tinymce and cancelling check loop");
                        scope.$broadcast("$tinymce:refresh");
                        $interval.cancel(interval);
                        canceled = true;
                    }
                }, 500, null, false);

                scope.$on("$destroy", () => {
                    if (!canceled) {
                        $interval.cancel(interval);
                        canceled = true;
                    }
                });
            }
        };

        return directive;
    }]);

    // so user can focus on tinymce dialogs (e.g. codesample, link, image) when the richtext input is inside a sw modal
    // (bugfix for bootstrap+tinymce dialogs)
    $(document).on("focusin", function (e) {
        if ($(e.target).closest(".mce-window").length) {
            e.stopImmediatePropagation();
        }
    });

})(angular);