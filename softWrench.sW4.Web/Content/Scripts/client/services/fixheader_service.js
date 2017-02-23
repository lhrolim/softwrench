var app = angular.module('sw_layout');

app.factory('fixHeaderService', function ($rootScope, $log, $timeout, contextService, fieldService) {
    "ngInject";

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
        var affixpaginationid = $("#affixpagination");
        var listgridtheadid = $("#listgridthread");
        var listgridid = $("#listgrid");

        var paginationsuccessrmessageclass = "pagination-successmessage";
        var listtheadsuccessmessageclass = "listgrid-thead-successmessage";
        var listgridsuccessmessageclass = "listgrid-table-successmessage";
        var listgridtheadreset = "listgrid-thead-reset";
        var listgridtablereset = "listgrid-table-reset";

        var listtheadsuccessmessageieclass = "listgrid-thead-successmessage-ie";
        var listgridsuccessmessageieclass = "listgrid-table-successmessage-ie";

        if (showerrormessage) {
            affixpaginationid.addClass(paginationsuccessrmessageclass);
            if (isIe9() && $rootScope.clientName == 'hapag') {
                listgridtheadid.addClass(listtheadsuccessmessageieclass);
                listgridid.addClass(listgridsuccessmessageieclass);
            } else {
                listgridtheadid.removeClass(listgridtheadreset);
                listgridtheadid.addClass(listtheadsuccessmessageclass);
                listgridid.removeClass(listgridtablereset);
                listgridid.addClass(listgridsuccessmessageclass);
            }
        } else {
            affixpaginationid.removeClass(paginationsuccessrmessageclass);
            if (isIe9() && $rootScope.clientName == 'hapag') {
                listgridtheadid.removeClass(listtheadsuccessmessageieclass);
                listgridid.removeClass(listgridsuccessmessageieclass);
            } else {
                listgridtheadid.removeClass(listtheadsuccessmessageclass);
                listgridtheadid.addClass(listgridtheadreset);
                listgridid.removeClass(listgridsuccessmessageclass);
                listgridid.addClass(listgridtablereset);
            }
        }
    };

    var topMessageAddClass = function (div) {
        div.addClass("affix-thead");
        div.addClass("topMessageAux");
    };

    var topMessageRemoveClass = function (div) {
        div.removeClass("affix-thead");
        div.removeClass("topMessageAux");
        $('html, body').animate({ scrollTop: 0 }, 'fast');
    };

    var buildTheadArray = function (log, table, emptyGrid) {
        var thead = [];
        //if the grid is empty, let´s just keep the th array
        var classToUse = emptyGrid ? 'thead tr:eq(0) th' : 'tbody tr:eq(0) td';

        $(classToUse, table).each(function (i, firstrowIterator) {
            var firstTd = $(firstrowIterator);
            if (!(firstTd.css("display") == "none")) {
                //let´s push only the visible entries
                var width = firstTd.width();
                thead.push(width);
            } else {
                thead.push(0);
            }
        });
        log.trace('thead array: ' + thead);
        var total = 0;
        for (var i = 0; i < thead.length; i++) {
            total += thead[i] << 0;
        }
        log.trace('total ' + total);
        return thead;
    }

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


            //                        thead tr:eq(1) th ==> picks all the elements of the first line of the thead of the table, i.e the filters
            $('thead tr:eq(1) th', table).each(function (i, v) {
                var trWidth = theadArray[i];
                if (trWidth == 0) {
                    //hidden fields
                    return;
                }

                var inputGroupElements = $('.input-group', v).children();
                //filtering only the inputs (ignoring divs...)
                var addonWidth = 0;
                for (var j = 1; j < inputGroupElements.length; j++) {
                    //sums both the filter input + the filter button
                    addonWidth += inputGroupElements.eq(j).outerWidth();
                }

                if (addonWidth == 0) {
                    //there´s no filter to update
                    return;
                }


                //first element will be the filter input itself
                var input = inputGroupElements.eq(0);
                var width = input.width();
                var inputPaddingAndBorder = input.outerWidth() - width;
                
                var resultValue = trWidth - addonWidth - inputPaddingAndBorder;
                $log.getInstance('fixheaderService#updateFilterVisibility').debug("result:{0} | Previous:{1} | tr:{2} | addon:{3} | Padding{4}".format(resultValue, width, trWidth, addonWidth, inputPaddingAndBorder));
                input.width(resultValue);
            });
        },


        fixTableTop: function (tableElement, params) {
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
            } else {
                tableElement.css('margin-top', theadHeight + 23);
                thead.css('top', 114);
            }
        },

        fixThead: function (schema, params) {
            var log = $log.getInstance('fixheaderService#fixThead');
            log.debug('starting fix Thead');

            if (!params || !params.resizing) {
                this.unfix();
            }
            var table = $(".listgrid-table");
            var thead = buildTheadArray(log, table, params.empty);

            $('thead tr:eq(0) th', table).each(function (i, v) {
                $(v).width(thead[i]);
            });
            $('thead tr:eq(1) th', table).each(function (i, v) {
                $(v).width(thead[i]);
            });

            // set the columns width back
            $('tbody tr:eq(0) td', table).each(function (i, v) {
                $(v).width(thead[i]);
            });

            log.debug('updating filter visibility');
            this.updateFilterVisibility(schema, thead);
            contextService.insertIntoContext('currentgridarray', thead);
            log.debug('updated filter visibility');

            //update the style, to fixed
            this.fixTableTop(table, params);

            //hack to fix HAP-610 T-ITOM-015
            $('#pagesize').width($('#pagesize').width());

            log.debug('finishing fix Thead');
        },

        activateResizeHandler: function () {
            var resolutionBarrier = 1200;
            var width = $(window).width();
            var highResolution = width >= resolutionBarrier;
            var fn = this;
            $(window).resize(function () {
                var newWidth = $(this).width();

                //SM - HAP-393, resize regardless of screen size
                //var isNewHighResolution = newWidth > resolutionBarrier + 15; // lets add some margin to give the browser time to render the new table...
                //var isNewLowResolution = newWidth < resolutionBarrier - 15; // lets add some margin to give the browser time to render the new table...
                //if ((isNewHighResolution && !highResolution) || (isNewLowResolution && highResolution)) {
                $log.getInstance("fixheaderService#resize").debug('switching resolutions');
                fn.fixThead(null, {
                    resizing: true
                });
                //width = newWidth;
                //highResolution = width >= resolutionBarrier;
                //}
            });
        },

        FixHeader: function () {
            var table;
            var originalOffset;
            $(window).scroll(function () {
                if (table == null) {
                    table = $("#listgrid");
                    originalOffset = $("thead", table).top;
                }
                var windowTop = $(window).scrollTop();
                $("thead", table).css("top", windowTop + originalOffset);
            });
        },

        unfix: function () {
            var log = $log.getInstance('fixheaderService#unfix');
            log.debug('unfix started');
            var table = $(".listgrid-table");
            table.removeClass("affixed");
            table.addClass("unfixed");
            $('[rel=tooltip]').tooltip('hide');
            log.debug('unfix finished');
        },

        fixSuccessMessageTop: function (isList) {
            if (isList) {
                addClassSuccessMessageListHander(true);
            }
        },

        topErrorMessageHandler: function (show, isDetail, schema) {
            if (show) {
                if (!isDetail) {
                    addClassErrorMessageListHander(true);
                    $rootScope.hasErrorList = true;
                } else {
                    $rootScope.hasErrorDetail = true;
                }
            } else {
                addClassErrorMessageListHander(false);
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

});


// In chrome you have to fire the window.location.reload event to fire a print event when a socket event is in progress.. 
// http://stackoverflow.com/questions/18622626/chrome-window-print-print-dialogue-opens-only-after-page-reload-javascript
//if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
//                    window.location.reload();
//}