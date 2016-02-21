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
        allowViewOnly: true,
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
                    permission: "none"
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

    it("Test Merge datamap into transient after app change", (function () {

        rootScope[txProp] = {
            "asset": assetObj
        };

        //simulating user was at main tab of application asset, changing to sr
        var screenDatamap = {
            application: "servicerequest",
            //changed from true to false
            "#appallowcreation": false,
            "#appallowupdate": true,
            "#appallowviewOnly": true,
            "#appallowRemoval": true,
            "schema": "detail",
            iscompositiontab: false,
            "#selectedtab": "#main",
            "#fieldPermissions_": [
                //this has been changed on screen
                {
                    fieldKey: "location",
                    permission: "readonly"
                },
                {
                    //non existing permission, but full control --> do not store
                    fieldKey: "f2",
                    permission: "fullcontrol"
                },
                {
                    //non existing permission --> store
                    fieldKey: "f3",
                    permission: "none"
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
            application: "asset",
        }

        var updatedData = userProfileService.storeFromDmIntoTransient(dispatcher);

        expect(updatedData).toBe(rootScope[txProp]);

        var mergedData = updatedData["asset"];
        expect(mergedData.allowCreation).toBe(false);
        expect(mergedData.allowUpdate).toBe(true);

        var containerPermission = mergedData.containerPermissions.filter(function (item) {
            return item.schema === "detail" && item.containerKey === "#main";
        })[0];

        // the none needs to be added
        expect(containerPermission.fieldPermissions.length).toBe(2);

        //location changed to readonly
        debugger;
        expect(containerPermission.fieldPermissions[0].permission).toBe("readonly");
        expect(containerPermission.fieldPermissions[1].fieldKey).toBe("f3");
        expect(containerPermission.fieldPermissions[1].permission).toBe("none");


        expect(mergedData.actionPermissions.length).toBe(1);
        var actionPermissions = mergedData.actionPermissions;
        //only the one marked as false
        expect(actionPermissions[0].actionId).toBe("yyy");
    }));


    it("Test Merge datamap into transient after app change: new composition", (function () {

        rootScope[txProp] = {
            "asset": assetObj
        };

        //simulating user was at main tab of application asset, changing to sr
        var screenDatamap = {
            application: "servicerequest",
            "#appallowcreation": true,
            "#appallowupdate": true,
            "#appallowviewOnly": true,
            "#appallowRemoval": true,
            "schema": "detail",
            "#selectedtab": "spareparts",
            iscompositiontab: true,
            "#compallowcreation": false,
            "#compallowupdate": false,
            "#compallowviewonly": false,
            "#actionPermissions_" : [],
            "#fieldPermissions_" : [],
        }

        crudContextHolderService.rootDataMap(null, screenDatamap);


        var dispatcher = {
            application: "asset",
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
        expect(secondComposition.allowViewOnly).toBe(false);


    }));




});
