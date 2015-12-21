(function (app, angular, $) {
    "use strict";
    
    // inlined templates
    var templates = {
        // directive template
        directive: "<div class='secondary-content-section-trigger'>" +
                        "<div class='secondary-content-section-button' ng-click='expandSection()'>" +
                            "<span>Show {{ config.expanded ? 'Less ' : 'More ' }}</span>" +
                            "<i class='fa' ng-class=\"{'fa-chevron-up': config.expanded, 'fa-chevron-down' : !config.expanded}\"></i>" +
                        "</div>" +
                        "<hr>" +
                        "</div>",

        section: {
            // 'actual' detail section template
            detail: "<div class='secondary-content-section' ng-show='config.expanded'>" +
                        "<section-element-input extraparameters='extraparameters'" +
                        "schema='schema'" +
                        "datamap='datamap'" +
                        "is-dirty='isDirty'" +
                        "displayables='displayables'" +
                        "association-options='associationOptions'" +
                        "association-schemas='associationSchemas'" +
                        "blockedassociations='blockedassociations'" +
                        "renderer-parameters='rendererParameters'" +
                        "elementid='{{elementid}}'" +
                        "orientation='{{orientation}}'" +
                        "islabelless='{{islabelless}}'" +
                        "lookup-associations-code='lookupAssociationsCode'" +
                        "lookup-associations-description='lookupAssociationsDescription' />" +
                        "</div>",

            // 'actual' list section template: not supported yet 
            list: null   
        }
    }
    
    // error messages
    var error = {
        unsupportedStereotype: "Cannot compile expandable section: Unsupported schema stereotype \"{0}\""
    };

    // expandable section: click to expand
    // in the first expansion it will dynamically compile the template then show it's content
    // after that it's content becomes expandable
    app.directive("crudExpandableSection", [function () { 
        var directive = {
            replace: false,
            transclude: false,
            // just data to pass to the 'actual' section
            scope: {
                schema: "=",
                datamap: "=",
                isDirty: "=",
                displayables: "=",
                associationOptions: "=",
                associationSchemas: "=",
                blockedassociations: "=",
                extraparameters: "=",
                elementid: "@",
                orientation: "@",
                islabelless: "@",
                lookupAssociationsCode: "=",
                lookupAssociationsDescription: "=",
                rendererParameters: "="
            },
            // inlining simple template for performance
            template: templates.directive,

            controller: ["$scope", "$element", "$compile", "$timeout", "spinService", "associationService","crudContextHolderService",
                function ($scope, $element, $compile, $timeout, spinService, associationService, crudContextHolderService) {
                $scope.config = {
                    compiled: false,
                    expanded: false
                };

                var toggleExpansion = function () {
                    $scope.config.expanded = !$scope.config.expanded;
                    // scroll to expanded section
                    if ($scope.config.expanded) {
                        $timeout(function() {
                            $(document.body).animate({ scrollTop: $element.offset().top }, 500);
                        }, 0, false);
                    }

                    $timeout(function () {
                        $(window).trigger("resize");
                    }, 0, false);
                };

                var showTemplateLoading = function() {
                    var spinneroptions = {
                        small: true,
                        top: String(Math.round(window.innerHeight / 2)) + "px",
                        left: String(Math.round(window.innerWidth / 2)) + "px"
                    }
                    var spinner = spinService.startSpinner(document.body, spinneroptions);
                    return spinner;
                };

                var compileTemplate = function () {
                    var stereotype = $scope.schema.stereotype;
                    // validate supported stereotype
                    if (!stereotype.contains("detail") && !stereotype.contains("Detail")) {
                        throw new Error(error.unsupportedStereotype.format(stereotype));
                    }

                    /*
                        the following code looks ugly but it's the only way to make the spinner stop 
                        only after the template is compiled and appended to the DOM  
                     */

                    // start spinner
                    var spinner = showTemplateLoading();
                    // schedule compilation and DOM manipulation
                    $timeout(function () {
                        var templateElement = $compile(templates.section.detail)($scope);
                        $scope.config.compiled = true;
                        $element.append(templateElement);
                        // schedule spinner stop after the compilation and DOM manipulation
                        $timeout(function () { spinner.stop() }, 0, false);

                    }, 0, false);
                };

                $scope.expandSection = function () {
                    // make sure compilation happens only once per $scope/directive instance
                    if (!$scope.config.compiled) {
                        compileTemplate();
                        associationService.loadSchemaAssociations(crudContextHolderService.rootDataMap(), crudContextHolderService.currentSchema(), { avoidspin: true, showmore:true });
                    }
                    toggleExpansion();
                };
            }]

        };

        return directive;
    }]);

})(app, angular, jQuery);