﻿
(function (angular) {
    "use strict";

    const formsInfo = {
        app: "_FormMetadata",
        schemaid: "listselection",
    }

    const cloneModalInfo = {
        app: "_FormMetadata",
        schemaid: "detail",
    }


    const defaultValues = {
        yesLabel: "Yes",
        noLabel: "N/A"
    }


    const baseTemplate = (headerLabel, attribute, yesLabel, noLabel, optionsLabel) => {


        return {
            "type": "TableDefinition",
            "showExpression": "true",
            "label": headerLabel,
            "isReadOnly": false,
            "attribute": attribute,
            "role": attribute,
            "headers": [
                yesLabel,
                noLabel,
                optionsLabel
            ]
        }
    }

    const rowTemplate = (yesLabel, noLabel, optionsLabel, item) => {
        return [
            {
                "$type": `softwrench.sW4.Shared2.Metadata.Applications.Schema.OptionField, softwrench.sw4.Shared2`,
                "options": [
                    {
                        "$type": `softwrench.sw4.Shared2.Data.Association.AssociationOption, softwrench.sw4.Shared2`,
                        "value": "yes",
                        "label": yesLabel,
                        "type": "AssociationOption"
                    },
                    {
                        "$type": `softwrench.sw4.Shared2.Data.Association.AssociationOption, softwrench.sw4.Shared2`,
                        "value": "na",
                        "label": noLabel,
                        "type": "AssociationOption"
                    }
                ],
                "isHidden": false,
                "renderer": {
                    "rendererType": "radio",
                    "parameters": {
                        "width": "8%"
                    }
                },
                "rendererParameters": {
                    "width": "8%"
                },
                "rendererType": "radio",
                "filterParameters": {},
                "associationKey": "yesna",
                "target": "yesna",
                "dependantFields": [],
                "sort": false,
                "applicationPath": "yesna",
                "extraProjectionFields": [],
                "applicationName": "_FormDatamap",
                "label": "",
                "attribute": "yesna",
                "requiredExpression": "false",
                "showExpression": "true",
                "enableExpression": "true",
                "isReadOnly": false,
                "type": "OptionField",
                "role": "yesna",
                "events": {},
                "$$hashKey": "object:901"
            },
            {
                "$type": `softwrench.sW4.Shared2.Metadata.Applications.Schema.TableColumnPlaceHolder, softwrench.sw4.Shared2`,
                "type": "TableColumnPlaceHolder",
                "role": "yesna",
                "showExpression": "true",
                "label": "",
                "isReadOnly": false,
                "attribute": "yesna",
                "rendererParameters": {
                    "width": "8%"
                },
                "parentIndex": 0,
                "indexOnParent": 1,
            },
            {
                "$type": `softwrench.sW4.Shared2.Metadata.Applications.Schema.ApplicationFieldDefinition, softwrench.sw4.Shared2`,
                "enableDefault": "true",
                "autoGenerated": false,
                "isHidden": false,
                "renderer": {
                    "rendererType": "label",
                    "parameters": {
                        "hidelabel": "true"
                    }
                },
                "isAssociated": false,
                "rendererType": "label",
                "rendererParameters": {},
                "filterParameters": {},
                "applicationName": "_FormDatamap",
                "label": optionsLabel,
                "attribute": "optionslabel",
                "requiredExpression": "false",
                "defaultValue": item,
                "showExpression": "true",
                "enableExpression": "true",
                "isReadOnly": false,
                "type": "ApplicationFieldDefinition",
                "role": "optionslabel",
                "events": {},
            }
        ];
    }


    class checkListTableBuilderService {

        constructor() {

        }


        //#region Utils
        createTable({ fattribute, flabel, items, yesLabel, noLabel, optionsLabel } = defaultValues) {

            const baseTable = baseTemplate(flabel, fattribute, yesLabel, noLabel, optionsLabel);
            baseTable.rows = [];
            items = items || [];
            items.forEach(i => {
                baseTable.rows.push(rowTemplate(yesLabel, noLabel, optionsLabel, i));
            });

            return baseTable;
        }

        convertRowsIntoArray(tableMetadata) {
            const result = [];
            tableMetadata.rows.forEach(r => {
                result.push(r[2].defaultValue);
            });
            return result;
        }

        convertArraysIntoRows({ yesLabel, noLabel, optionsLabel},tableMetadata, rows) {
            tableMetadata.rows = [];
            rows.forEach(i => {
                tableMetadata.rows.push(rowTemplate(yesLabel, noLabel, optionsLabel, i));
            });
            return tableMetadata;
        }

    }


    checkListTableBuilderService["$inject"] = [];

    angular.module("sw_layout").service("checkListTableBuilderService", checkListTableBuilderService);

})(angular);