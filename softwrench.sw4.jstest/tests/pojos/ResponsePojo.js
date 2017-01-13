
class ResponsePojo {

    static CrudUpdateBaseResponse(id, userId) {
        userId = userId || id;

        return {
            "successMessage": `Service Request ${userId} successfully updated`,
            "fullRefresh": false,
            "timeStamp": "2016-12-22T08:02:47",
            "type": "BlankApplicationResponse",
            "id": id
        };
    }


    static CrudDetailResponse() {
        return ResponsePojo.CrudCreateCachedBaseResponse();
    }


    static ListSchemaLoadResult() {
        return {
            eagerOptions: {},
            preFetchLazyOptions:{}
        }
    }

    static LookpOptionsResult(returnArray = null) {

        const ob1 = { value: "test1", label: "test1 label" };
        const ob2 = { value: "test2", label: "test2 label" };
        const ob11 = { value: "test11", label: "test11 label" };
        let baseArray = [ob1, ob2, ob11];
        if (!!returnArray) {
            baseArray = returnArray;
        }

        return {
            resultObject: {
                associationData:baseArray
            }
        }
    }


    static DetailSchemaLoadResult() {
        return {
            eagerOptions: {
                classification: [{ value: "100", label: "label 100" }, { value: "101", label: "label 101" }]
            },
            preFetchLazyOptions: {}
        }
    }

    //TODO: finish filling for a true dashboard response
    static DashboardResponse() {
        return {
            "crudSubTemplate": "/Content/Shared/dashboard/templates/Dashboard.html",
            "type": "GenericApplicationResponse",
            "resultObject": {
                dashboards: [],
                applications: [],
                permissions: {

                },
                profiles: [],
                schemas: {}
            },
            "redirectURL": "/Content/Controller/Application.html",
            "timeStamp": "2016-12-22T08:55:04",
            "extraParameters": {}
        };
    }

    static ListResponse() {
        return {
            "pageResultDto": {
                "totalCount": 1445,
                "pageNumber": 1,
                "numberOfPages": 0,
                "pageSize": 10,
                "paginationOptions": [
                    10,
                    30,
                    100
                ],
                "shouldPaginate": true,
                "pageCount": 145,
                "hasNext": true,
                "hasPrevious": false,
                "compositionsToFetch": [],
                "needsCountUpdate": true,
                "searchSort": "sr.ticketuid desc",
                "expressionSort": false,
                "searchAscending": false,
                "ignoreWhereClause": false,
                "isDefaultInstance": false,
                "addPreSelectedFilters": false,
                "projectionFields": [],
                "getNestedFieldsToConsiderInRelationships": [],
                "forceEmptyResult": false
            },
            "cachedSchemaId": "list",
            "affectedProfiles": [],
            "associationOptions": {
                "eagerOptions": {},
                "preFetchLazyOptions": {}
            },
            "mode": "none",
            "applicationName": "servicerequest",
            "totalCount": 1445,
            "pageNumber": 1,
            "pageSize": 10,
            "paginationOptions": [
                10,
                30,
                100
            ],
            "pageCount": 145,
            "pagesToShow": [
                {
                    "active": true,
                    "pageNumber": 1
                },
                {
                    "active": false,
                    "pageNumber": 2
                },
                {
                    "active": false,
                    "pageNumber": 3
                },
                {
                    "active": false,
                    "pageNumber": 4
                },
                {
                    "active": false,
                    "pageNumber": 5
                },
                {
                    "active": false,
                    "pageNumber": 6
                },
                {
                    "active": false,
                    "pageNumber": 7
                },
                {
                    "active": false,
                    "pageNumber": 8
                },
                {
                    "active": false,
                    "pageNumber": 9
                },
                {
                    "active": false,
                    "pageNumber": 10
                }
            ],
            "resultObject": [
                {
                    "ticketid": "2852",
                    "description": "aaa",
                    "reportedby": "SWADMIN",
                    "owner": null,
                    "siteid": "BEDFORD",
                    "status": "NEW",
                    "internalpriority": null,
                    "communicationcounter": 0,
                    "attachmentcounter": 0,
                    "relatedcounter": 0,
                    "ticketuid": 6053,
                    "application": "SR"
                },
                {
                    "ticketid": "2851",
                    "description": "aaaabbb",
                    "reportedby": "SWADMIN",
                    "owner": null,
                    "siteid": "BEDFORD",
                    "status": "NEW",
                    "internalpriority": null,
                    "communicationcounter": 0,
                    "attachmentcounter": 0,
                    "relatedcounter": 0,
                    "ticketuid": 6052,
                    "application": "SR"
                },
                {
                    "ticketid": "2850",
                    "description": "test",
                    "reportedby": "SWADMIN",
                    "owner": null,
                    "siteid": "BEDFORD",
                    "status": "NEW",
                    "internalpriority": null,
                    "communicationcounter": 0,
                    "attachmentcounter": 0,
                    "relatedcounter": 0,
                    "ticketuid": 6051,
                    "application": "SR"
                },
                {
                    "ticketid": "2849",
                    "description": "aaa",
                    "reportedby": "SWADMIN",
                    "owner": null,
                    "siteid": "BEDFORD",
                    "status": "NEW",
                    "internalpriority": null,
                    "communicationcounter": 0,
                    "attachmentcounter": 0,
                    "relatedcounter": 0,
                    "ticketuid": 6050,
                    "application": "SR"
                },
                {
                    "ticketid": "2848",
                    "description": null,
                    "reportedby": null,
                    "owner": null,
                    "siteid": "BEDFORD",
                    "status": "NEW",
                    "internalpriority": null,
                    "communicationcounter": 0,
                    "attachmentcounter": 0,
                    "relatedcounter": 0,
                    "ticketuid": 5962,
                    "application": "SR"
                },
                {
                    "ticketid": "2847",
                    "description": null,
                    "reportedby": null,
                    "owner": null,
                    "siteid": "BEDFORD",
                    "status": "NEW",
                    "internalpriority": null,
                    "communicationcounter": 0,
                    "attachmentcounter": 0,
                    "relatedcounter": 0,
                    "ticketuid": 5961,
                    "application": "SR"
                },
                {
                    "ticketid": "2846",
                    "description": "FW: KDI - Email Listener formatter - error",
                    "reportedby": "ASHAMS",
                    "owner": null,
                    "siteid": "BEDFORD",
                    "status": "NEW",
                    "internalpriority": null,
                    "communicationcounter": 3,
                    "attachmentcounter": 1,
                    "relatedcounter": 0,
                    "ticketuid": 5960,
                    "application": "SR"
                },
                {
                    "ticketid": "2845",
                    "description": "FW: testing formatng",
                    "reportedby": "TCOTTIER",
                    "owner": null,
                    "siteid": "BEDFORD",
                    "status": "NEW",
                    "internalpriority": null,
                    "communicationcounter": 2,
                    "attachmentcounter": 3,
                    "relatedcounter": 1,
                    "ticketuid": 5959,
                    "application": "SR"
                },
                {
                    "ticketid": "2844",
                    "description": "testing formatng",
                    "reportedby": "TCOTTIER",
                    "owner": null,
                    "siteid": null,
                    "status": "NEW",
                    "internalpriority": null,
                    "communicationcounter": 2,
                    "attachmentcounter": 3,
                    "relatedcounter": 0,
                    "ticketuid": 5958,
                    "application": "SR"
                },
                {
                    "ticketid": "2843",
                    "description": "Testing to see if ticket gets created with CC",
                    "reportedby": "TCOTTIER",
                    "owner": null,
                    "siteid": null,
                    "status": "NEW",
                    "internalpriority": null,
                    "communicationcounter": 2,
                    "attachmentcounter": 0,
                    "relatedcounter": 0,
                    "ticketuid": 5957,
                    "application": "SR"
                }
            ],
            "redirectURL": "/Content/Controller/Application.html",
            "title": "Service Request Grid",
            "timeStamp": "2016-12-23T12:48:10",
            "type": "ApplicationListResult",
            "extraParameters": {}
        };
    }

    static CrudCreateCachedBaseResponse() {
        return {
            "fullRefresh": false,
            "associationOptions": {
                "eagerOptions": {},
                "preFetchLazyOptions": {}
            },
            "cachedSchemaId": "editdetail",
            "compositions": {},
            "type": "ApplicationDetailResult",
            "applicationName": "workorder",
            "allAssociationsFetched": false,
            "resultObject": {
                "workorderid": 6583,
                "orgid": "EAGLENA",
                "woclass": "WORKORDER",
                "class": "WORKORDER",
                "pmnum": null,
                "cinum": null,
                "description": "a",
                "wonum": "2329",
                "siteid": "BEDFORD",
                "reportdate": "2016-12-22T08:54:00",
                "targstartdate": null,
                "targcompdate": null,
                "schedstart": null,
                "schedfinish": null,
                "actstart": null,
                "actfinish": null,
                "jpnum": null,
                "location": null,
                "assetnum": null,
                "classstructureid": null,
                "worktype": null,
                "wopriority": null,
                "owner": null,
                "ownergroup": null,
                "reportedby": "SWADMIN",
                "failurecode": null,
                "problemcode": null,
                "glaccount": null,
                "onbehalfof": null,
                "status": "WAPPR",
                "rowstamp": 49799641,
                "synstatus_.description": "Waiting on Approval",
                "synstatus_.maxvalue": "WAPPR",
                "failurelist_.description": null,
                "failurelist_.failurelist": null,
                "ld_.ldtext": null,
                "application": "workorder",
                "approwstamp": 49799641,
                "multiassetlocci_": []
            },
            "redirectURL": "/Content/Controller/Application.html",
            "successMessage": "Work Order 2329 successfully created",
            "timeStamp": "2016-12-22T08:55:04",
            "extraParameters": {}
        };
    }

    static Hidden(attribute) {

        return {
            attribute,
            isHidden: true
        };
    }

    static Required(attribute) {

        return {
            attribute,
            requiredExpression: true
        };
    }




}