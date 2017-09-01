(function (angular) {
    "use strict";

    angular.module("sw_prelogin").directive("richtextField", ["contextService", "$interval", "$log", "$q", "$timeout", "printAwaitableService",
        function (contextService, $interval, $log, $q, $timeout, printAwaitableService) {

        const directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/directives/richtextfield.html"),
            replace: false,
            scope: {
                content: "=",
                readonly: "=",
                forprint: "=",
                height: "@"
            },

            controller: ["$scope", "$element", "richTextService", function ($scope, $element, richTextService) {
                const log = $log.get("richtextfield#controller", ["richtext"]);

                $scope.content = richTextService.getDecodedValue($scope.content);

                if ($scope.forprint) {
                    $scope.printDefered = $q.defer();
                    printAwaitableService.registerAwaitable($scope.printDefered.promise);
                }

                const height = !!$scope.height ? $scope.height:250;

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
                        

                        height: height,
                        resize: true,

                        paste_data_images: true, // paste SS
                        paste_retain_style_properties: "color font-size",
                        paste_word_valid_elements: "b,strong,i,em,h1,h2",

                        codesample_dialog_height: window.innerHeight - 100,
                        codesample_dialog_width: window.innerWidth - 100,
                        codesample_style: url("Content/customVendor/css/prism.css"),

                        readonly: $scope.readonly,
                        debounce: true,
                        inline: !!$scope.forprint,

                        images_dataimg_filter: function(img) {
                            return false;
                        },

                        images_dataimg_filter: function (img) {
                            return false;
                        },

                        menubar: $scope.readonly ? false : "edit insert table",
                        toolbar: $scope.readonly ? false : "styleselect blockquote | bold italic underline bullist numlist undo redo | alignleft aligncenter alignright alignjustify | link image codesample",
                        statusbar: false,

                        // add images as blobs - workaround to big images with too big base64 urls
                        file_picker_callback: function (callback, value, meta) {
                            if (meta.filetype !== "image") {
                                return;
                            }
                            const input = $($element).find(".richtext-file-selector")[0];
                            input.click();
                            input.onchange = function () {
                                const file = input.files[0];
                                input.value = null;
                                const reader = new FileReader();
                                reader.onload = function (e) {
                                    const blob = new Blob([e.target.result], { type: file.type });
                                    const url = window.URL.createObjectURL(blob);
                                    callback(url, {
                                        alt: file.name
                                    });
                                };
                                reader.readAsArrayBuffer(file);
                            };
                        },

                        // hide/show toolbar+menubar on blur/focus
                        setup: function(editor) {
                            const executeOnActionBars = (action, el) => {
                                if (!el.contentAreaContainer) {
                                    return;
                                }
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
                                if (!$scope.printDefered) {
                                    return;
                                }
                                $timeout(function () {
                                    $scope.printDefered.resolve();
                                }, 0);
                            });

                            // after an image is added if its a blob converts back to base64
                            // workaround to big images with too big base64 urls
                            editor.on("change", () => {
                                const imgs = editor.dom.$("img");
                                if (imgs.length === 0) {
                                    return;
                                }
                                angular.forEach(imgs, (img) => {
                                    if (!img.src || !img.src.startsWith("blob:")) {
                                        return;
                                    }

                                    const xhr = new XMLHttpRequest();
                                    xhr.open("GET", img.src, true);
                                    xhr.responseType = "blob";
                                    xhr.onload = function () {
                                        if (this.status !== 200) {
                                            return;
                                        }
                                       
                                        const blob = this.response;
                                        const reader = new FileReader();
                                        //handler executed once reading(blob content referenced to a variable) from blob is finished. 
                                        reader.addEventListener("loadend", function (e) {
                                            img.src = e.srcElement.result;
                                        });
                                        //start the reading process.
                                        reader.readAsDataURL(blob);
                                    };
                                    xhr.send();
                                });
                            });
                        }
                    }
                };
            }],

            link: function (scope, element, attrs) {
                const log = $log.get("richtextfield#link", ["richtext"]);
                // very very dirty hack to ensure tinymce editor is in the screen
                // for some dynamically added fields this hack is necessary (e.g. inside modals, composition_masterdetails)
                var interval;
                const stopInterval = () => {
                    if (angular.isDefined(interval)) {
                        $interval.cancel(interval);
                        interval = undefined;
                    }
                };

                interval = $interval(() => {
                    log.trace("loop to check tinymce's editor is in the screen");
                    const tinyMceFrame = element[0].querySelector("iframe");
                    if (!tinyMceFrame) {
                        log.debug("tinymce's iframe not yet present. skipping");
                        return;
                    }
                    const contentId = $(tinyMceFrame.contentWindow.document.body).attr("data-id");
                    if (!contentId) {
                        log.debug("refreshing angular-ui-tinymce and cancelling check loop");
                        scope.$broadcast("$tinymce:refresh");
                        stopInterval();
                    } 
                }, 500, null, false);

                scope.$on("$destroy", stopInterval);
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