(function (angular) {
    "use strict";

    angular.module("sw_layout").directive("richtextField", ["contextService", "$timeout", function (contextService, $timeout) {
        const directive = {
            restrict: "E",
            templateUrl: contextService.getResourceUrl("/Content/Templates/directives/richtextfield.html"),
            replace: false,
            scope: {
                content: "=",
                readonly: "="
            },

            controller: ["$scope", "richTextService", function ($scope, richTextService) {
                $scope.content = richTextService.getDecodedValue($scope.content);

                $scope.richtext = {
                    configured: false,
                    config: {
                        plugins: [
                            // from tinymce's basic + paste + codesample
                            "advlist autolink lists link image charmap print preview hr anchor pagebreak",
                            "searchreplace wordcount visualblocks visualchars code fullscreen",
                            "insertdatetime nonbreaking save table contextmenu directionality",
                            "emoticons template paste textcolor colorpicker textpattern imagetools",
                            "codesample paste"
                        ],

                        menubar: $scope.readonly ? false : "edit insert table",
                        toolbar: $scope.readonly ? false : "styleselect blockquote | bold italic underline bullist numlist undo redo | alignleft aligncenter alignright alignjustify | link image codesample",
                        statusbar: false,

                        skin_url: "Content/customVendor/css/tinymce/skins/lightgray",
                        height: 250,

                        paste_data_images: true, // paste SS
                        paste_retain_style_properties: "color font-size",
                        paste_word_valid_elements: "b,strong,i,em,h1,h2",

                        readonly: $scope.readonly,
                        debounce: true,

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
                console.log("linked!");
                $timeout(() => scope.richtext.configured = true, 500, false)
                    .then(() => $timeout(() => scope.$broadcast("$tinymce:refresh"), 0, false));
            }
        };

        return directive;
    }]);

})(angular);