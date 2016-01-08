(function (angular) {
    angular.module("angularTreeview", []).directive("treeModel", function($compile) {
        return {
            restrict: "A",
            link: function(scope, element, attr) {
                var treeModel = attr.treeModel;
                var label = attr.nodeLabel || "label";
                var children = attr.nodeChildren || "children";
                if (isIe9() && $(element).closest('#rootprintdiv').length!=0) {
                    //avoids this on ie9 printing, due to HAP-356
                    return;
                }

                var template =
                    '<ul><li data-ng-click="selectNodeHead(node, $event)" ' +
                        'data-ng-repeat="node in ' + treeModel + '">' +
                        '<i class="collapsed" data-ng-show="node.' + children + '.length && node.collapsed" data-ng-click="selectNodeHead(node, $event)">' +
                        '</i><i class="expanded" data-ng-show="node.' + children + '.length && !node.collapsed" data-ng-click="selectNodeHead(node, $event)">' +
                        '</i><i class="normal" data-ng-hide="node.' +
                        children + '.length"></i> ' +
                        '<span data-ng-class="node.selected" data-ng-click="selectNodeLabel(node, $event);showDefinitions(node)">{{node.' + label + '}}' +
                        '</span>' +
                        '<div data-ng-hide="node.collapsed" data-tree-model="node.' + children + '" data-node-id=' + (attr.nodeId || "id") + " data-node-label=" + label + " data-node-children=" + children + ">" +
                        "</div></li></ul>";

                treeModel && treeModel.length && (attr.angularTreeview ?
                    (
                        scope.$watch(treeModel, function(newValue, b) {
                            element.empty().html($compile(template)(scope));
                        }, false),
                        scope.selectNodeHead = scope.selectNodeHead || function(scope, b) {
                            b.stopPropagation && b.stopPropagation();
                            b.preventDefault && b.preventDefault();
                            b.cancelBubble = true;
                            b.returnValue = false;
                            scope.collapsed = !scope.collapsed;
                        },
                        scope.selectNodeLabel = scope.selectNodeLabel || function(attr, b) {
                            b.stopPropagation && b.stopPropagation();
                            b.preventDefault && b.preventDefault();
                            b.cancelBubble = true;
                            b.returnValue = false;
                            scope.currentNode && scope.currentNode.selected
                                && (scope.currentNode.selected = void 0);
                            attr.selected = "selected";
                            scope.currentNode = attr;
                        }
                    )
                    : element.html($compile(template)(scope)));
            }
        }
    });
})(angular);