var app = angular.module('sw_layout');

app.factory('changeservice', function ($http, redirectService, formatService, fieldService, alertService) {

    /**
     * Parses a string into a date. Expects the format 'dd/MM/yyyy HH:mm'.
     * @param {String} value 
     * @returns {Date} 
     */
    function parseTargetDate(value) {
        var datetime = value.split(" ");
        var date = datetime[0] || "01/01/1970";
        var time = datetime[1] || "00:00";

        var dateparts = date.split("/");
        var day = parseInt(dateparts[0]     || 1);
        var month = parseInt(dateparts[1]   || 1);
        var year = parseInt(dateparts[2]    || 1970);

        var timeparts = time.split(":");
        var hours = parseInt(timeparts[0]   || 0);
        var minutes = parseInt(timeparts[1] || 0);

        return new Date(year, month, day, hours, minutes, 0, 0);
    }

    return {

        /// <summary>
        ///  Method used for opening a detail of a change request, from the grid.it s declared on the metadata, using list.click.service property. 
        /// 
        /// Since change grids contains both SR and Changes itself, there´s a custom logic determining which detail should be opened based both:
        /// 
        /// 1)which column was clicked (SR ID ==> always SR),
        /// 2)The data itself==> if it has no wonum (sr created by hapag), or if it has the magic number -666 on it
        /// 
        /// </summary>
        /// <param name="datamap"></param>
        /// <param name="column"></param>
        opendetail: function (datamap, column) {

            var wonum = datamap['wonum'];
            //this "wild" number (-666) is used because we have a custom join of 2 tables inside of a grid, and we need a way to sort it properly
            var isSr = wonum == null || wonum == "-666" || column.attribute == 'hlagchangeticketid';
            var application = isSr ? 'servicerequest' : 'change';
            var id = isSr ? datamap['hlagchangeticketid'] : wonum;
            if (id == null) {
                if (isSr) {
                    alertService.alert("No Service Request is related to this entry. Cannot open Service Request details");
                    return;
                }
                
            }
            var parameters = { id: id, popupmode: 'browser' };
            redirectService.goToApplicationView(application, 'detail', 'output', null, parameters);
        },

        duplicate: function (schema, datamap) {

            var initialData = {};

            var changeid = datamap['wonum'];

            /* workaround to make samelinepicker work - REFACTOR */
            var targstartdate = datamap['targstartdate'];
            if (targstartdate != null) {
                var targstartdateColumn = fieldService.getDisplayableByKey(schema, 'targstartdate');
                targstartdate = formatService.format(targstartdate, targstartdateColumn);
                targstartdate = Date.parse(targstartdate).toString('MM/dd/yyyy HH:mm');
            }

            var targcompdate = datamap['targcompdate'];
            if (targcompdate != null) {
                var targcompdateColumn = fieldService.getDisplayableByKey(schema, 'targcompdate');
                targcompdate = formatService.format(targcompdate, targcompdateColumn);
                targcompdate = Date.parse(targcompdate).toString('MM/dd/yyyy HH:mm');
            }
            /* workaround end */

            initialData['description'] = datamap['description'];
            initialData['cinum'] = datamap['cinum'];
            initialData['category'] = datamap['worktype'];
            initialData['impact'] = datamap['pmcomimpact'];
            initialData['urgency'] = datamap['pmcomurgency'];
            initialData['reasonforchange'] = datamap['reasonforchange'];
            initialData['reasondetails'] = datamap['reasonforchange_.ldtext'];
            initialData['targstartdate'] = targstartdate;
            initialData['targcompdate'] = targcompdate;
            initialData['priority'] = datamap['wopriority'];
            initialData['remarks'] = 'Copy of ' + changeid + '\n' + datamap['longdescription_.ldtext'];

            var parameters = {
                popupmode: 'nomenu'
            };

            redirectService.goToApplicationView('newchange', 'newchange', 'input', 'New Change Request', parameters, initialData);


        },

        validateNewForm: function (schema, datamap) {
            var arr = [];

            var targstartdate = datamap["targstartdate"];
            var targstartdateParsed = parseTargetDate(targstartdate);

            var targcompdate = datamap["targcompdate"];
            var targcompdateParsed = parseTargetDate(targcompdate);

            if (targstartdateParsed.getTime() > targcompdateParsed.getTime()) {
                arr.push("Target Start Date cannot be later than Target Finish Date");
            }
            return arr;
        }
    };


});