﻿var app = angular.module('sw_layout');

app.factory('changeservice', function ($http, redirectService, formatService, fieldService) {

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
            var parameters = { id: id, popupmode: 'browser' };
            redirectService.goToApplicationView(application, 'detail', 'output', null, parameters);
        },

        duplicate: function (schema, datamap) {
            
            var initialData = {};
            
            var changeid = datamap['wonum'];

            /* workaround to make samelinepicker work - REFACTOR */
            var targstartdateColumn = fieldService.getDisplayableByKey(schema, 'targstartdate');
            var targstartdate = formatService.format(datamap['targstartdate'], targstartdateColumn);
            targstartdate = Date.parse(targstartdate).toString('MM/dd/yyyy HH:mm');

            var targcompdateColumn = fieldService.getDisplayableByKey(schema, 'targcompdate');
            var targcompdate = formatService.format(datamap['targcompdate'], targcompdateColumn);
            targcompdate = Date.parse(targcompdate).toString('MM/dd/yyyy HH:mm');
            /* workaround end */

            initialData['description'] = datamap['description'];
            initialData['infrastructureasset'] = datamap['cinum'];
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

            
        }
    };


});