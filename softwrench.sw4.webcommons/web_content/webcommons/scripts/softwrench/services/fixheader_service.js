(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('fixHeaderService', ["$rootScope", "$log", "$timeout", "contextService", function ($rootScope, $log, $timeout, contextService) {

    var addClassErrorMessageListHander = function (showerrormessage) {
        var affixpaginationid = $("#affixpagination");
        var listgridtheadid = $("#listgridthread");
        var listgridid = $("#listgrid");
        var paginationerrormessageclass = "pagination-errormessage";
        var listtheaderrormessageclass = "listgrid-thead-errormessage";
        var listgriderrormessageclass = "listgrid-table-errormessage";

        if (showerrormessage) {
            affixpaginationid.addClass(paginationerrormessageclass);
            listgridtheadid.addClass(listtheaderrormessageclass);
            listgridid.addClass(listgriderrormessageclass);
        } else {
            affixpaginationid.removeClass(paginationerrormessageclass);
            listgridtheadid.removeClass(listtheaderrormessageclass);
            listgridid.removeClass(listgriderrormessageclass);
        }
    };

    var addClassSuccessMessageListHander = function (showerrormessage) {
        if ($rootScope.clientName == 'hapag') {
            var headerHeight = $('.site-header').height() + 70;
            var paginationHeight = $('.affix-pagination').height();
            var theaderHeight = $('.listgrid-thead').height();
            $('.affix-pagination').css('top', headerHeight);
            $('.listgrid-thead').css('top', headerHeight + paginationHeight);
            $('.listgrid-table').css('margin-top', paginationHeight + theaderHeight - 1);
        }
    };

    function setHeaderPosition() {
        var siteHeaderElement = $('.site-header');
        //var toolbarElement = $('.toolbar-primary:not(.affix-pagination)');
        var toolbarElement = $('.toolbar-primary:visible');
        var listTheadElement = $('.listgrid-thead:visible');

        if (siteHeaderElement.css('position') === 'fixed') {
            //if the header is fixed to the top of the page, set the location of the content, context menu, grid header and filter bar
            var headerHeight = siteHeaderElement.height();
            var toolbarHeight = toolbarElement.height();
            var theaderHeight = listTheadElement.height();
            var offsetMargin = toolbarHeight + theaderHeight - 1;

            $('.content').css('margin-top', headerHeight);

            //only adjust if toolbar is fixed 
            if (toolbarElement.css('position') === 'fixed') {
                toolbarElement.css('top', headerHeight);
                $('#crudbodyform:not([data-modal="true"])').css('margin-top', offsetMargin);

                var dashToolbar = $('.toolbar-primary:not(.affix-pagination)');
                $('.dash-header').css('margin-top', dashToolbar.height() + theaderHeight - 1);
            }

            //only adjust if table header is fixed
            if (listTheadElement.css('position') === 'fixed') {
                //move fixed listgrid header up in IE9
                var adjustment = 0;
                if (isIe9()) {
                    adjustment = 135;
                }

                var offsetTop = headerHeight + toolbarHeight - adjustment - 1;

                listTheadElement.css('top', offsetTop);
                $('.listgrid-table').css('margin-top', offsetMargin);
            }
        } else {
            //reset the lcoation of the content, context menu, grid header and filter bar
            $('.content').css('margin-top', 'auto');
            toolbarElement.css('top', 'auto');
            listTheadElement.css('top', 'auto');
            $('.listgrid-table').css('margin-top', 'auto');
        }
    };

    //register layout functions, debounced to stop repeated calls while resizing the browser window
    $(window).resize(window.debounce(setHeaderPosition, 250));

    function setHeaderColumnWidths() {
        var table = $('.listgrid-table');
        var rows = $('tbody tr', table);
        var firstRow = $('td', rows[0]);

        firstRow.each(function () {
            var tdClass = $(this)[0].classList[0];
            var tdWidth = $(this).width();
            var th = $('.listgrid-table thead th.{0}'.format(tdClass));

            th.width(tdWidth);
            $('.cell-wrapper', th).width(tdWidth);
        });
    };

    //register layout functions, debounced to stop repeated calls while resizing the browser window
    $(window).resize(window.debounce(setHeaderColumnWidths, 50));

    var topMessageAddClass = function (div) {
        div.addClass("affix-thead");
        div.addClass("topMessageAux");
    };

    var topMessageRemoveClass = function (div) {
        div.removeClass("affix-thead");
        div.removeClass("topMessageAux");
        $('html, body').animate({ scrollTop: 0 }, 'fast');
    };

    var buildTheadArray = function(log, table, emptyGrid) {
        if ($rootScope.clientName == 'hapag') {
            var thead = [];
            // loop over the first row of td's in &lt;tbody> and get the widths of individual &lt;td>'s
            var classToUse = emptyGrid ? 'thead tr:eq(0) th' : 'tbody tr:eq(0) td';

            $(classToUse, table).each(function(i, firstrow) {
                var width = $(firstrow).width();
                thead.push(width);
            });
            log.trace('thead array: ' + thead);
            var total = 0;
            for (var i = 0; i < thead.length; i++) {
                total += thead[i] << 0;
            }
            log.trace('total ' + total);
            return thead;
        }
    };

    var scrollHandlerRegistered = false;

    return {

        updateFilterZeroOrOneEntries: function () {
            /// <summary>
            /// 
            /// </summary>
            this.fixThead(null, { empty: true });
        },

        updateFilterVisibility: function (schema, theadArray) {
            /// <summary>
            ///  updates the fiter visibility for the grid, adjusting input layouts properly.
            /// </summary>
            /// <param name="schema">the schema is used to determine when we should show the advanced allowtoggle options</param>
            //update which filter row will be displayed and input text width
            var table = $(".listgrid-table");
            var showAdvancedFilter = false;

            if (schema != null) {
                var estimatedNeededSize = schema.displayables.length * 150;
                var allowtoggle = "true" == schema.properties['list.advancedfilter.allowtoggle'];
                showAdvancedFilter = allowtoggle && estimatedNeededSize > table.width();
            }

            if (showAdvancedFilter) {
                $('.hidden-too-much-columns', table).hide();
                $('.hidden-few-columns', table).show();
                return;
            }

            if (!isIe9() || !theadArray) {
                return;
            }

            //thead tr:eq(2) th ==> picks all the elements of the second line of the thead of the table, i.e the filters
            $('thead tr:eq(2) th', table).each(function (i, v) {
                var inputGroupElements = $('.input-group', v).children();
                //filtering only the inputs (ignoring divs...)
                var addonWidth = 0;
                for (var j = 1; j < inputGroupElements.length; j++) {
                    //sums both the filter input + the filter button
                    addonWidth += inputGroupElements.eq(j).outerWidth();
                }
                //first element will be the filter input itself
                var input = inputGroupElements.eq(0);
                var width = input.width();
                var inputPaddingAndBorder = input.outerWidth() - width;
                var trWidth = theadArray[i];
                var resultValue = trWidth - addonWidth - inputPaddingAndBorder;
                $log.getInstance('fixheader#updateFilterVisibility').debug("result:{0} | Previous:{1} | tr:{2} | addon:{3} | Padding{4}".format(resultValue, width, trWidth, addonWidth, inputPaddingAndBorder));
                input.width(resultValue);
            });
        },


        fixTableTop: function (tableElement, params) {
            if ($rootScope.clientName == 'hapag') {
                var thead = $('thead', tableElement);
                params = instantiateIfUndefined(params);
                $(".listgrid-table").addClass("affixed");
                $(".listgrid-table").removeClass("unfixed");
                var theadHeight = thead.height();
                $log.getInstance("fixheaderService#fixTableTop").debug("head height: " + theadHeight);
                if (isIe9() && "true" != sessionStorage.mockie9) {
                    //if mocking ie9, lets keep default behaviour, otherwise will break all the grids
                    tableElement.css('margin-top', theadHeight + 19);
                    thead.css('top', 111 - (theadHeight + 19));
                } else if (contextService.isClient("hapag")) {
                    tableElement.css('margin-top', theadHeight + 23);
                    thead.css('top', 114);
                }
            }
        },

        fixThead: function (schema, params, listTableRenderedEvent) {
            //console.log('fixThead', schema, params, listTableRenderedEvent);

            var log = $log.getInstance('sw4.fixThead');

            if ($rootScope.clientName == 'hapag') {
                log.debug('starting fix Thead');
                if (!params || !params.resizing) {
                    this.unfix();
                }
                var table = $(".listgrid-table");
                var thead = buildTheadArray(log, table, params.empty);

                $('thead tr:eq(0) th', table).each(function(i, v) {
                    $(v).width(thead[i]);
                });
                $('thead tr:eq(2) th', table).each(function(i, v) {
                    $(v).width(thead[i]);
                });

                // set the columns width back
                $('tbody tr:eq(0) td', table).each(function(i, v) {
                    $(v).width(thead[i]);
                });

                log.debug('updating filter visibility');
                this.updateFilterVisibility(schema, thead);
                contextService.insertIntoContext('currentgridarray', thead);
                log.debug('updated filter visibility');

                //update the style, to fixed
                this.fixTableTop(table, params);
            } else {
                setHeaderColumnWidths();
            }

            this.callWindowResize();
        },

        callWindowResize: window.debounce(function () { // debouncing so it doesn't trigger resize all the time 
            var log = $log.getInstance('sw4.fixheader_service');

            //trigger resize to postition fixed header elements
            $timeout(function () {
                log.debug('callWindowResize');
                setHeaderPosition();
                setHeaderColumnWidths();
            }, 0, false);
        }, 300),

        activateResizeHandler: function () {
            if ($rootScope.clientName === 'hapag') {
                var resolutionBarrier = 1200;
                var width = $(window).width();
                var highResolution = width >= resolutionBarrier;
                var fn = this;
                $(window).resize(function () {
                    var newWidth = $(this).width();
                    var isNewHighResolution = newWidth > resolutionBarrier + 15; // lets add some margin to give the browser time to render the new table...
                    var isNewLowResolution = newWidth < resolutionBarrier - 15; // lets add some margin to give the browser time to render the new table...
                    if ((isNewHighResolution && !highResolution) || (isNewLowResolution && highResolution)) {
                        $log.getInstance("crudlistdir#resize").debug('switching resolutions');
                        fn.fixThead(null, {
                            resizing: true
                        });
                        width = newWidth;
                        highResolution = width >= resolutionBarrier;
                    }
                });
            }
        },

        FixHeader: function () {
            if (scrollHandlerRegistered) return;

            var table;
            var originalOffset;
            var scrollHandler = window.throttle(function () {
                if (table == null) {
                    table = $("#listgrid");
                    originalOffset = $("thead", table).top;
                }
                var windowTop = $(window).scrollTop();
                $("thead", table).css({ "top" : (windowTop + originalOffset) + "px" });
            }, 300, { leading: false });

            $(window).scroll(scrollHandler);

            scrollHandlerRegistered = true;
        },

        unfix: function () {
            var log = $log.getInstance('sw4.fixheader_service#unfix');
            log.debug('unfix started');
            var table = $(".listgrid-table");
            table.removeClass("affixed");
            table.addClass("unfixed");
            $('.no-touch [rel=tooltip]').tooltip({ container: 'body', trigger: 'hover' });
            $('.no-touch [rel=tooltip]').tooltip('hide');
            log.debug('unfix finished');
        },

        fixSuccessMessageTop: function (isList) {
            if (isList) {
                addClassSuccessMessageListHander(true);
            }
        },

        topErrorMessageHandler: function (show, isDetail, schema) {
            if (!show) {
                addClassErrorMessageListHander(false);
                return;
            }
            if (isDetail) {
                $rootScope.hasErrorDetail = true;
            } else {
                addClassErrorMessageListHander(true);
                $rootScope.hasErrorList = true;
            }
        },

        resetTableConfig: function (schema) {
            if ($(".listgrid-table").position() != undefined) {
                addClassSuccessMessageListHander(false);
                if (!nullOrUndef(schema)) {
                    var params = {
                    };
                    this.fixThead(schema, params);
                }
                $(window).trigger('resize');
            }
        }
    };

}]);

})(angular);