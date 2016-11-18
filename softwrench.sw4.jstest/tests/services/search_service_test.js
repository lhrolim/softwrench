describe('SearchService Test', function () {


    var searchService;
    beforeEach(module('sw_layout'));
    beforeEach(inject(function (_searchService_) {
        searchService = _searchService_;
    }));


    it('Test Search values build, for between operation', function () {
        // Get the record counts for the compositions
        var result = searchService.buildSearchValuesString({
            "reportdate": '14/03/2015',
            "reportdate_end": '20/03/2015',
            assetnum: "1000"
        }, {
            "reportdate": {
                id: 'BTW',
                begin:''
            },
            "assetnum": {
                id: 'EQ',
                begin: '=',
                end:''
            }
        });
        // Make sure they match the values from the mockCompositionResult
        expect(result).toBe("14/03/2015__20/03/2015,,,=1000");

    });

    it('Test Search parameters response, for between operation', function () {
        // Get the record counts for the compositions
        var result = searchService.buildSearchDataAndOperations("reportdate","03/14/2016__03/20/2016");

        var expectedData = {
            reportdate: "03/14/2016",
            reportdate_end: "03/20/2016"
        }

        // Make sure they match the values from the mockCompositionResult
        expect(result.searchData).toEqual(expectedData);
        expect(Object.keys(result.searchOperator).length).toBe(1);
        expect(result.searchOperator["reportdate"].id).toBe("BTW");

    });

    it('parse multisort test', function () {
        // Get the record counts for the compositions
        var result = searchService.parseMultiSort("xxx asc, yyy desc");
        
        // Make sure they match the values from the mockCompositionResult
        expect(result.length).toEqual(2);
        expect(result[0].columnName).toEqual("xxx");
        expect(result[0].isAscending).toEqual(true);

        expect(result[1].columnName).toEqual("yyy");
        expect(result[1].isAscending).toEqual(false);


        var result = searchService.parseMultiSort("xxx asc");

        // Make sure they match the values from the mockCompositionResult
        expect(result.length).toEqual(1);
        expect(result[0].columnName).toEqual("xxx");
        expect(result[0].isAscending).toEqual(true);

        
    });

 

      it('Test filter parameters response, for not contains operation', function () {
       //testing SWWEB-2183
        var expectedData = {
            ticketid: "1234"
        }

        var result = searchService.buildSearchDataAndOperations("ticketid", "!%1234%");
        expect(result.searchData).toEqual(expectedData);
        expect(Object.keys(result.searchOperator).length).toBe(1);
        expect(result.searchOperator["ticketid"].id).toBe("NCONTAINS");
    });

    it('Test filter parameters response: % in the middle of the search string should be preserved', function() {
        var expectedData = {
            description: "Contains % in between"
        }

        var result = searchService.buildSearchDataAndOperations("description", "!%Contains % in between%");
        expect(result.searchData).toEqual(expectedData);
        expect(result.searchOperator["description"].id).toBe("NCONTAINS");

        result = searchService.buildSearchDataAndOperations("description", ">Contains % in between%");
        expect(result.searchData).toEqual(expectedData);
        expect(result.searchOperator["description"].id).toBe("GT");

        result = searchService.buildSearchDataAndOperations("description", "%Contains % in between%");
        expect(result.searchData).toEqual(expectedData);
        expect(result.searchOperator["description"].id).toBe("CONTAINS");
    });

});
