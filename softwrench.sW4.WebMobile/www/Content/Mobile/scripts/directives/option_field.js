//taken from here:
//http://codepen.io/mhartington/pen/CImqy

(function (softwrench) {
    "use strict";

    softwrench.directive('optionField',
        [
            '$ionicModal','$log',
            function ($ionicModal,$log) {
                return {
                    /* Only use as <option-field> tag */
                    restrict: 'E',

                    /* Our template */
                    templateUrl: 'Content/Mobile/templates/directives/option_field.html',

                    /* Attributes to set */
                    scope: {
                        'items': '=', /* Items list is mandatory */
                        'text': '=', /* Displayed text is mandatory */
                        'value': '=', /* Selected value binding is mandatory */
                        'componentId': '@', /* componentId */
                        'callback': '=', /*TODO: refactor to use callback2*/
                        'callback2': '&',
                    },

                    link: function (scope, element, attrs) {

                        /* Default values */
                        scope.multiSelect = attrs.multiSelect === 'true' ? true : false;
                        scope.allowEmpty = attrs.allowEmpty === 'false' ? false : true;

                        /* Header used in ion-header-bar */
                        scope.headerText = attrs.headerText || '';

                        /* Text displayed on label */
                        // scope.text          = attrs.text || '';
                        scope.defaultText = scope.text || '';

                        /* Notes in the right side of the label */
                        scope.noteText = attrs.noteText || '';
                        scope.noteImg = attrs.noteImg || '';
                        scope.noteImgClass = attrs.noteImgClass || '';

                        const valueChanged = function (value) {
                            angular.forEach(scope.items, item => {
                                if (item.value === value) {
                                    item.checked = true;
                                }
                            });
                        }

                        const valuesChanged = function (values) {
                            angular.forEach(values, value => {
                                if (value) {
                                    valueChanged(value);
                                }
                            });
                        }

                        // watch changes on value to update the multiple select checked state
                        const watchForValueChanges = function () {
                            return scope.$watch("value", function (newValue, oldValue) {
                                if (newValue === oldValue || !scope.items || !scope.multiSelect) {
                                    return;
                                }

                                angular.forEach(scope.items, item => item.checked = false);

                                if (newValue) {
                                    valuesChanged(newValue.split(";"));
                                }
                            });
                        }

                        scope.deWatchValueChanges = watchForValueChanges();

                        /* Optionnal callback function */
                        // scope.callback = attrs.callback || null;

                        /* Instanciate ionic modal view and set params */

                        /* Some additionnal notes here : 
                         * 
                         * In previous version of the directive,
                         * we were using attrs.parentSelector
                         * to open the modal box within a selector. 
                         * 
                         * This is handy in particular when opening
                         * the "option field" from the right pane of
                         * a side view. 
                         * 
                         * But the problem is that I had to edit ionic.bundle.js
                         * and the modal component each time ionic team
                         * make an update of the FW.
                         * 
                         * Also, seems that animations do not work 
                         * anymore.
                         * 
                         */
                        $ionicModal.fromTemplateUrl(
                            'Content/Mobile/templates/directives/option_field_item.html',
                              { 'scope': scope }
                        ).then(function (modal) {
                            scope.modal = modal;
                        });

                        // object to store if the user moved the finger to prevent opening the modal
                        var scrolling = {
                            moved: false,
                            startX: 0,
                            startY: 0
                        };

                        // store the start coordinates of the touch start event
                        scope.onTouchStart = function (e) {
                            $log.get("optionfield#ontouchstart",["association"]).trace("ontouchstart handler");
                            scrolling.moved = false;
                            // Use originalEvent when available, fix compatibility with jQuery
                            if (typeof (e.originalEvent) !== 'undefined') {
                                e = e.originalEvent;
                            }
                            scrolling.startX = e.touches[0].clientX;
                            scrolling.startY = e.touches[0].clientY;
                        };

                        // check if the finger moves more than 10px and set the moved flag to true
                        scope.onTouchMove = function (e) {
                            $log.get("optionfield#ontouchmove", ["association"]).trace("ontouchmove handler");
                            // Use originalEvent when available, fix compatibility with jQuery
                            if (typeof (e.originalEvent) !== 'undefined') {
                                e = e.originalEvent;
                            }
                            if (Math.abs(e.touches[0].clientX - scrolling.startX) > 10 ||
                                Math.abs(e.touches[0].clientY - scrolling.startY) > 10) {
                                scrolling.moved = true;
                            }
                        };

                        /* Show list */
                        scope.showItems = function (event) {
                            if (scrolling.moved || ionic.scroll.isScrolling) {
                                return;
                            }
                            scrolling.moved = false;
                            event.preventDefault();
                            scope.modal.show();
                        }



                        /* Validate selection from header bar */
                        scope.validate = function (event) {
                            // Construct selected values and selected text
                            if (scope.multiSelect == true) {

                                // turns off the watch for value
                                // multiple select toggles are updated at this point
                                scope.deWatchValueChanges();

                                // Clear values
                                scope.value = '';
                                scope.text = '';

                                // Loop on items
                                if (scope.items) {
                                    jQuery.each(scope.items, function (index, item) {
                                        if (item.checked) {
                                            scope.value = scope.value + item.value + ';';
                                            scope.text = scope.text + item.label + ', ';
                                        }
                                    });
                                }

                                // Remove trailing comma
                                scope.value = scope.value.substr(0, scope.value.length - 1);
                                scope.text = scope.text.substr(0, scope.text.length - 2);

                                // turns the watch again on timeout to force - on timeout to force the watch run first
                                scope.deWatchValueChanges = watchForValueChanges();
                            }

                            // Select first value if not nullable
                            if (typeof scope.value == 'undefined' || scope.value == '' || scope.value == null) {
                                if (scope.allowEmpty == false) {
                                    scope.value = scope.items[0].value;
                                    scope.text = scope.items[0].label;

                                    // Check for multi select
                                    scope.items[0].checked = true;
                                } else {
                                    scope.text = scope.defaultText;
                                }
                            }

                            // Hide modal
                            scope.hideItems();

                            // Execute callback function
                            if (typeof scope.callback == 'function') {
                                scope.callback(scope.value);
                            }
                        }

                    

                        /* Hide list */
                        scope.hideItems = function () {
                            scope.modal.hide();
                        }

                        /* Destroy modal */
                        scope.$on('$destroy', function () {
                            scope.modal.remove();
                        });

                        /* Validate single with data */
                        scope.validateSingle = function (item) {

                            // Set selected text
                            scope.text = item.label;

                            // Set selected value
                            scope.value = item.value;

                            // Hide items
                            scope.hideItems();

                            if (typeof scope.callback == 'function') {
                                //TODO: remove this to use callback2
                                scope.callback(scope.value);
                            }

                            // Execute callback function
                            if (typeof scope.callback2 == 'function') {
                                scope.callback2({
                                    callback: {
                                        item: item,
                                        componentId: scope.componentId,
                                    }
                                });
                            }
                        }

                        // $formatter to show label
                        const inputElement = element[0].querySelector(".js_option_input");
                        const ngModel = angular.element(inputElement).controller("ngModel");
                        ngModel.$formatters.push(model => {
                            if (!model || !scope.items) return model;
                            const option = scope.items.find(o => o.value === model);
                            return !option ? model : option.label;
                        });

                    }
                };
            }
        ]
    );
})(softwrench);
