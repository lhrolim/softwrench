(function (angular) {
    'use strict';

    class laborTimeSheetIconProvider {

        constructor() {

        }

        //#region Public methods
        getIconClass(item) {
            if (item.datamap["#runninglabor"]){
                return "hasaction";
            }
            return item.datamap.labtransid != null ? null : "isdirty";
        }

        getIconIcon(item) {
            if (item.datamap["#runninglabor"]){
                return "clock-o";
            }

            return item.datamap.labtransid != null ? "check" : "refresh";
        }

        getIconColor(item){
            return item.datamap.labtransid != null ? "#39b54a" : "refresh";
        }

        getIconText(item){
            return "";
        }

        getTextColor(item){
            return item.datamap.labtransid != null ? "white" : null;
        }


    }


    laborTimeSheetIconProvider['inject$'] = [];

    angular.module('sw_mobile_services').service('laborTimeSheetIconProvider', laborTimeSheetIconProvider);

})(angular);