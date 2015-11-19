(function (app, angular) {
    "use strict";
    
    // inlined templates
    var templates = {
        // directive template
        directive: "<div class='expandable-section-trigger'>" +
                        "<div class='expandable-section-button'>" +
                            "<div ng-click='expandSection()'>" +
                                "<span>Show {{ config.expanded ? 'Less ' : 'More ' }}</span>" +
                                "<i class='fa' ng-class=\"{'fa-chevron-up': config.expanded, 'fa-chevron-down' : !config.expanded}\"></i>" +
                            "</div>" +
                        "<hr>" +
                        "</div>" +
                        "</div>",

        section: {
            // 'actual' detail section template
            detail: "<div class='expandable-section' ng-class=\"{ 'hidden' : !config.expanded  }\">" +
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

            controller: ["$scope", "$element", "$compile", function ($scope, $element, $compile) {
                $scope.config = {
                    compiled: false,
                    expanded: false
                };

                var toggleExpansion = function () {
                    $scope.config.expanded = !$scope.config.expanded;
                };

                var compileTemplate = function () {
                    if ($scope.schema.stereotype !== "Detail" && $scope.schema.stereotype !== "detail") {
                        throw new Error(error.unsupportedStereotype);
                    }
                    var templateElement = $compile(templates.section.detail)($scope);
                    $scope.config.compiled = true;
                    $element.append(templateElement);
                };

                $scope.expandSection = function () {
                    // make sure compilation happens only once per $scope
                    if (!$scope.config.compiled) compileTemplate();
                    toggleExpansion();
                };
            }]

        };

        return directive;
    }]);

})(app, angular);