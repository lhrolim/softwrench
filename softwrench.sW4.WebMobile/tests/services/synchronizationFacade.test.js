describe("synchronizationFacade test", function () {

    var synchronizationFacade, swdbDAO, $q, $rootScope, entities;
    // constants to make it more visible
    var CREATE = "crud_create";
    var UPDATE = "crud_update";

    //#region helper constructors
    var BatchItem = (function () {
        function BatchItem(id, application, operation, problem) {
            this.problem = !!problem ? {} : null;
            this.crudoperation = operation;
            this.dataentry = {
                id: String(id),
                application: application
            };
        }
        return BatchItem;
    })();

    var PreparedStatement = (function () {
        PreparedStatement.prototype.query = null; // to be defined after
        function PreparedStatement(args) {
            var ids = _.map(args[0], function (id) {
                return (String(id));
            });
            this.args = [ids, args[1]];
        }
        return PreparedStatement;
    })();
   //#endregion

    //#region test config
    var config = {
        statements: [],
        results: {
            noitems: {
                input: [
                    {
                        loadeditems: [
                            new BatchItem(1, "workorder", UPDATE),
                            new BatchItem(2, "workorder", CREATE, true),
                            new BatchItem(3, "workorder", UPDATE, true)
                        ]
                    },
                    {
                        loadeditems: [
                            new BatchItem(1, "servicerequest", UPDATE),
                            new BatchItem(2, "servicerequest", CREATE, true),
                            new BatchItem(3, "servicerequest", UPDATE, true)
                        ]
                    }
                ],
                expected: []
            },
            singleapplication: {
                input: [{
                    loadeditems: [
                            new BatchItem(1, "workorder", CREATE),
                            new BatchItem(2, "workorder", UPDATE, true),
                            new BatchItem(3, "workorder", CREATE),
                            new BatchItem(4, "workorder", UPDATE),
                            new BatchItem(5, "workorder", CREATE),
                            new BatchItem(6, "workorder", CREATE, true),
                            new BatchItem(7, "workorder", CREATE)
                    ]
                }],
                expected: [
                    new PreparedStatement([ [1,3,5,7], "workorder" ])
                ]
            },
            multipleapplications: {
                input: [
                    {
                        loadeditems: [
                            new BatchItem(1, "workorder", CREATE),
                            new BatchItem(2, "workorder", UPDATE, true),
                            new BatchItem(3, "workorder", CREATE),
                            new BatchItem(4, "workorder", UPDATE),
                            new BatchItem(5, "workorder", CREATE),
                            new BatchItem(6, "workorder", CREATE, true),
                            new BatchItem(7, "workorder", CREATE)
                        ]
                    },
                    {
                        loadeditems: [
                            new BatchItem(8, "servicerequest", CREATE),
                            new BatchItem(9, "servicerequest", UPDATE, true),
                            new BatchItem(10, "servicerequest", CREATE),
                            new BatchItem(11, "servicerequest", UPDATE),
                            new BatchItem(12, "servicerequest", CREATE),
                            new BatchItem(13, "servicerequest", CREATE, true),
                            new BatchItem(14, "servicerequest", CREATE)
                        ]
                    },
                    {
                        loadeditems: [
                            new BatchItem(15, "asset", CREATE),
                            new BatchItem(16, "asset", UPDATE, true),
                            new BatchItem(17, "asset", CREATE),
                            new BatchItem(18, "asset", UPDATE),
                            new BatchItem(19, "asset", CREATE),
                            new BatchItem(20, "asset", CREATE, true),
                            new BatchItem(21, "asset", CREATE)
                        ]
                    }
                ],
                expected: [
                    new PreparedStatement([ [1, 3, 5, 7], "workorder"]),
                    new PreparedStatement([ [8, 10, 12, 14], "servicerequest"]),
                    new PreparedStatement([ [15, 17, 19, 21], "asset"])
                ]
            }
        }
    }

    beforeEach(module("softwrench"));
    beforeEach(inject(function (_synchronizationFacade_, _swdbDAO_, _$q_, _$rootScope_, _offlineEntities_) {
        synchronizationFacade = _synchronizationFacade_;
        swdbDAO = _swdbDAO_;
        $q = _$q_;
        $rootScope = _$rootScope_;
        entities = _offlineEntities_;
        PreparedStatement.prototype.query = entities.DataEntry.deleteInIdsStatement;
        // clear statements and store them after the promise is resolved for comparing
        spyOn(swdbDAO, "executeQueries").and.callFake(function (statements) {
            config.statements = [];
            return $q.when().then(function () {
                config.statements = statements;
                return statements;
            });
        });
    }));
    
    //#endregion test config

    function runHandleDeletableDataEntriesTest(testConfig, done) {
        synchronizationFacade.handleDeletableDataEntries(testConfig.input).then(function (batches) {
            var expectedStatements = testConfig.expected;
            var resultStatements = config.statements;
            // resolved with the input value
            expect(batches).toEqual(testConfig.input);
            // testing result length
            expect(resultStatements.length).toBe(expectedStatements.length);
            // testing each statement
            _.each(expectedStatements, function (statement, index) {
                // testing query
                expect(resultStatements[index].query).toEqual(statement.query);
                // testing args length
                expect(resultStatements[index].args.length).toBe(statement.args.length);
                // testing application in args
                expect(resultStatements[index].args[0]).toEqual(statement.args[0]);
                // testing ids in args: sorting just in case the order affects the comparison
                // (order is not important for the business rules)
                expect(_.sortBy(resultStatements[index].args[0])).toEqual(_.sortBy(statement.args[0]));
            });

        }).finally(done);
        // resolve promises
        $rootScope.$digest();
    }

    it("Testing 'handleDeletableDataEntries': no statements", function (done) {
        runHandleDeletableDataEntriesTest(config.results.noitems, done);
    });

    it("Testing 'handleDeletableDataEntries': single application", function (done) {
        runHandleDeletableDataEntriesTest(config.results.singleapplication, done);
    });

    it("Testing 'handleDeletableDataEntries': multiple applications", function (done) {
        runHandleDeletableDataEntriesTest(config.results.multipleapplications, done);
    });

});
