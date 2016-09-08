describe('UserProfileService Test', function () {

    var userProfileService;
    var rootScope;
    var crudContextHolderService;

    var txProp = "#transientprofiledata";

    beforeEach(module('sw_layout'));

    beforeEach(inject(function (_userProfileService_, $rootScope, _crudContextHolderService_) {
        userProfileService = _userProfileService_;
        rootScope = $rootScope;
        crudContextHolderService = _crudContextHolderService_;
    }));

    var assetObj = {
        allowCreation: true,
        allowUpdate: true,
        allowRemoval: true,
        allowView: true,
        compositionPermissions: [
            {
                schema: "detail",
                compositionKey: "subassembly",
                allowCreation: true,
                allowUpdate: true,
                allowRemoval: false
            },
        ],
        containerPermissions: [
            {
                schema: "detail",
                containerKey: "#main",
                fieldPermissions: [
                {
                    fieldKey: "location",
                    permission: "none",
                    originalpermission: "none"
                }]
            },
              {
                  schema: "detail",
                  containerKey: "tab3",
                  fieldPermissions: [
                  {
                      fieldKey: "f2",
                      permission: "none",
                      originalpermission: "none"
                  }]
              }

        ],

        actionPermissions: [
            {
                //this means, actually, that we don´t have access to that action
                schema: "detail",
                actionId: "xxx"
            }
        ]
    }


    var srObj = {
        allowCreation: true,
        allowUpdate: true,
        allowRemoval: true,
        allowView: true,
        containerPermissions: [
            {
                schema: "detail",
                containerKey: "#main",
                fieldPermissions: [
                {
                    fieldKey: "f1",
                    permission: "none",
                    originalpermission: "none"
                },
                {
                    fieldKey: "f2",
                    permission: "none",
                    originalpermission: "none"
                },
                {
                    fieldKey: "f3",
                    permission: "none",
                    originalpermission: "none"
                },


                ]
            },
        ],

        actionPermissions: [
            {
                //this means, actually, that we don´t have access to that action
                schema: "detail",
                actionId: "xxx"
            }
        ]
    }

    it("Test Merge datamap into transient after app change", (function () {

        rootScope[txProp] = {
            "asset": angular.copy(assetObj)
        };

        //simulating user was at main tab of application asset, changing to sr
        var screenDatamap = {
            "#application": "servicerequest",
            //changed from true to false
            "#appallowcreation": false,
            "#appallowupdate": true,
            "#appallowview": true,
            "#appallowremoval": true,
            "schema": "detail",
            iscompositiontab: false,
            "#selectedtab": "#main",
            "#fieldPermissions_": [
                //this has been changed on screen
                {
                    fieldKey: "location",
                    permission: "readonly",
                    originalpermission: "none"
                },
                {
                    //non existing permission, but full control --> do not store
                    fieldKey: "f2",
                    permission: "fullcontrol",
                    originalpermission: "fullcontrol"
                },
                {
                    //non existing permission --> store
                    fieldKey: "f3",
                    permission: "fullcontrol",
                    originalpermission: "readonly"
                }
            ],
            "#actionPermissions_": [
                {
                    "actionid": "xxx",
                    "_#selected": true
                },

                {
                    "actionid": "yyy",
                    "_#selected": false
                },

                {
                    "actionid": "zzz",
                    "_#selected": true
                },

            ]



        }

        crudContextHolderService.rootDataMap(null, screenDatamap);

        var dispatcher = {
            "#application": "asset",
        }

        var updatedData = userProfileService.storeFromDmIntoTransient(dispatcher);

        expect(updatedData).toBe(rootScope[txProp]);


        var mergedData = updatedData["asset"];
        expect(mergedData["_#isDirty"]).toBe(true);
        expect(mergedData.allowCreation).toBe(false);
        expect(mergedData.allowUpdate).toBe(true);

        var containerPermission = mergedData.containerPermissions.filter(function (item) {
            return item.schema === "detail" && item.containerKey === "#main";
        })[0];

        // the none needs to be added
        expect(containerPermission.fieldPermissions.length).toBe(2);

        //location changed to readonly
        expect(containerPermission.fieldPermissions[0].permission).toBe("readonly");
        expect(containerPermission.fieldPermissions[1].fieldKey).toBe("f3");
        expect(containerPermission.fieldPermissions[1].permission).toBe("fullcontrol");


        expect(mergedData.actionPermissions.length).toBe(1);
        var actionPermissions = mergedData.actionPermissions;
        //only the one marked as false
        expect(actionPermissions[0].actionId).toBe("yyy");
    }));


    it("Test Merge datamap into transient after app change: new composition", (function () {

        rootScope[txProp] = {
            "asset": angular.copy(assetObj)
        };

        //simulating user was at main tab of application asset, changing to sr
        var screenDatamap = {
            "#application": "servicerequest",
            "#appallowcreation": true,
            "#appallowupdate": true,
            "#appallowview": true,
            "#appallowremoval": true,
            "schema": "detail",
            "#selectedtab": "spareparts",
            iscompositiontab: true,
            "#compallowcreation": false,
            "#compallowupdate": false,
            "#compallowview": false,
            "#actionPermissions_": [],
            "#fieldPermissions_": [],
        }

        crudContextHolderService.rootDataMap(null, screenDatamap);


        var dispatcher = {
            "#application": "asset",
        }

        var updatedData = userProfileService.storeFromDmIntoTransient(dispatcher);

        expect(updatedData).toBe(rootScope[txProp]);

        var mergedData = updatedData["asset"];
        expect(mergedData.allowCreation).toBe(true);
        expect(mergedData.allowUpdate).toBe(true);

        var compositionPermissions = mergedData.compositionPermissions.filter(function (item) {
            return item.schema === "detail";
        });

        // the none needs to be added
        expect(compositionPermissions.length).toBe(2);
        var firstComposition = compositionPermissions[0];
        expect(firstComposition.compositionKey).toBe("subassembly");

        expect(firstComposition.allowCreation).toBe(true);
        expect(firstComposition.allowUpdate).toBe(true);
        expect(firstComposition.allowRemoval).toBe(false);
        var secondComposition = compositionPermissions[1];
        expect(secondComposition.compositionKey).toBe("spareparts");

        //location changed to readonly
        expect(secondComposition.allowCreation).toBe(false);
        expect(secondComposition.allowUpdate).toBe(false);
        expect(secondComposition.allowView).toBe(false);


    }));

    it("Test Merge datamap into transient on save: field permission scenario", (function () {

        rootScope[txProp] = {
            "servicerequest": angular.copy(srObj)
        };

        //simulating user was at main tab of application asset, changing to sr
        var screenDatamap = {
            "#application": "servicerequest",
            "#appallowcreation": true,
            "#appallowupdate": true,
            "#appallowview": true,
            "#appallowremoval": true,
            "schema": "detail",
            "#selectedtab": "#main",
            iscompositiontab: false,
            "#actionPermissions_": [],
            "#fieldPermissions_": [
            //this has been changed on screen
            {
                fieldKey: "f1",
                permission: "fullcontrol",
                originalpermission: "none"
            },
            {
                fieldKey: "f2",
                permission: "fullcontrol",
                originalpermission: "fullcontrol"
            },
            {
                fieldKey: "f3",
                permission: "fullcontrol",
                originalpermission: "fullcontrol"
            },
            {
                fieldKey: "f4",
                permission: "fullcontrol",
                originalpermission: "fullcontrol"
            },
            {
                fieldKey: "f5",
                permission: "fullcontrol",
                originalpermission: "fullcontrol"
            },
            {
                fieldKey: "f6",
                permission: "fullcontrol",
                originalpermission: "fullcontrol"
            },
            ],
        }

        crudContextHolderService.rootDataMap(null, screenDatamap);

        //save scenario
        var dispatcher = {}


        var updatedData = userProfileService.storeFromDmIntoTransient(dispatcher);

        expect(updatedData).toBe(rootScope[txProp]);

        var mergedData = updatedData["servicerequest"];
        expect(mergedData.allowCreation).toBe(true);
        expect(mergedData.allowUpdate).toBe(true);

        var containerPermission = mergedData.containerPermissions.filter(function (item) {
            return item.schema === "detail" && item.containerKey === "#main";
        })[0];

        var fieldPermissions = containerPermission.fieldPermissions;

        // the none needs to be added
        expect(fieldPermissions.length).toBe(0);
        expect(mergedData["_#isDirty"]).toBe(true);
        


    }));



    it("Test Restore transient into datamap app changed", (function () {

        rootScope[txProp] = {
            "asset": angular.copy(assetObj)
        };

        rootScope[txProp].asset.allowCreation = false;


        //simulating a change to an asset application, where basic setup should be restored
        var screenDatamap = {
            "#application": "asset",
            "#appallowcreation": true,
            "#appallowupdate": true,
            "#appallowview": true,
            "#appallowremoval": true,
            "schema": null,
            "#selectedtab": null,
            iscompositiontab: false,
            "#actionPermissions_": [],
            "#fieldPermissions_": [],
        }

        crudContextHolderService.rootDataMap(null, screenDatamap);


        var dispatcher = {
            "#application": "asset",
        }

        var updatedData = userProfileService.mergeTransientIntoDatamap(dispatcher);

        expect(updatedData).toBe(crudContextHolderService.rootDataMap());

        expect(updatedData["#appallowcreation"]).toBe(false);
        expect(updatedData["#appallowupdate"]).toBe(true);
        expect(updatedData["#appallowview"]).toBe(true);
        expect(updatedData["#appallowremoval"]).toBe(true);
        //location changed to readonly
    }));


    it("Test Restore transient into datamap schema changed --> restore actions", (function () {

        rootScope[txProp] = {
            "asset": angular.copy(assetObj)
        };

        rootScope[txProp].asset.allowCreation = false;


        //simulating a change to the detail schema
        var screenDatamap = {
            "#application": "asset",
            "#appallowcreation": true,
            "#appallowupdate": true,
            "#appallowview": true,
            "#appallowremoval": true,
            "schema": "detail",
            "#selectedtab": "subassembly",
            iscompositiontab: true,
            "#actionPermissions_": [
              {
                  "actionid": "xxx",
                  "_#selected": true
              },

              {
                  "actionid": "yyy",
                  "_#selected": true
              },

              {
                  "actionid": "zzz",
                  "_#selected": true
              },

            ],
            "#fieldPermissions_": [],
        }

        crudContextHolderService.rootDataMap(null, screenDatamap);


        var dispatcher = {
            schema: "detail",
        }

        var updatedData = userProfileService.mergeTransientIntoDatamap(dispatcher);

        expect(updatedData).toBe(crudContextHolderService.rootDataMap());

        //this action needs to be restored from transient data
        var actionPermissions = updatedData["#actionPermissions_"];

        //#region actionPermissions
        expect(actionPermissions.length).toBe(3);
        expect(actionPermissions[0].actionid).toBe("xxx");
        //this will be restored
        expect(actionPermissions[0]["_#selected"]).toBe(false);

        expect(actionPermissions[1].actionid).toBe("yyy");
        expect(actionPermissions[1]["_#selected"]).toBe(true);

        expect(actionPermissions[2].actionid).toBe("zzz");
        expect(actionPermissions[2]["_#selected"]).toBe(true);
        //#endregion


        //        expect(updatedData["#compallowcreation"]).toBe(true);
        //        expect(updatedData["#compallowupdate"]).toBe(true);
        //        expect(updatedData["#compallowremoval"]).toBe(false);


    }));


    it("Test Restore transient into datamap tab changed --> restore composition flags", (function () {

        rootScope[txProp] = {
            "asset": angular.copy(assetObj)
        };

        rootScope[txProp].asset.allowCreation = false;


        //simulating a change to the detail schema
        var screenDatamap = {
            "#application": "asset",
            "#appallowcreation": true,
            "#appallowupdate": true,
            "#appallowview": true,
            "#appallowremoval": true,
            "schema": "detail",
            "#selectedtab": "subassembly",
            iscompositiontab: true,
            "#actionPermissions_": [
              {
                  "actionid": "xxx",
                  "_#selected": true
              },

              {
                  "actionid": "yyy",
                  "_#selected": true
              },

              {
                  "actionid": "zzz",
                  "_#selected": true
              },

            ],
            "#fieldPermissions_": [],
        }

        crudContextHolderService.rootDataMap(null, screenDatamap);


        var dispatcher = {
            tab: "subassembly",
        }

        var updatedData = userProfileService.mergeTransientIntoDatamap(dispatcher);

        expect(updatedData).toBe(crudContextHolderService.rootDataMap());

        //this action needs to be restored from transient data
        expect(updatedData["#compallowcreation"]).toBe(true);
        expect(updatedData["#compallowupdate"]).toBe(true);
        expect(updatedData["#compallowremoval"]).toBe(false);


    }));


    it("Test Restore transient into datamap tab changed to main --> restore fields", (function () {

        rootScope[txProp] = {
            "asset": angular.copy(assetObj)
        };

        rootScope[txProp].asset.allowCreation = false;


        //simulating a change to the detail schema
        var screenDatamap = {
            "#application": "asset",
            "#appallowcreation": true,
            "#appallowupdate": true,
            "#appallowview": true,
            "#appallowremoval": true,
            "schema": "detail",
            "#selectedtab": "#main",
            iscompositiontab: true,
            "#fieldPermissions_": [
             //this has been changed on screen
             {
                 fieldKey: "location",
                 permission: "fullcontrol"
             },
             {
                 //non existing permission, but full control --> do not store
                 fieldKey: "f2",
                 permission: "fullcontrol"
             },
            ]
        }

        crudContextHolderService.rootDataMap(null, screenDatamap);
        var dispatcher = {
            tab: "#main",
        }
        var updatedData = userProfileService.mergeTransientIntoDatamap(dispatcher);

        expect(updatedData).toBe(crudContextHolderService.rootDataMap());
        //restore only the fields present on screen
        var fieldPermissions = updatedData["#fieldPermissions_"];
        expect(fieldPermissions.length).toBe(2);

        expect(fieldPermissions[0].fieldKey).toBe("location");
        expect(fieldPermissions[0].permission).toBe("none");

        expect(fieldPermissions[1].fieldKey).toBe("f2");
        expect(fieldPermissions[1].permission).toBe("fullcontrol");


    }));


});
