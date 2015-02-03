describe('expressionService', function () {
    var expressionService;
    beforeEach(module('sw_layout'));
    beforeEach(inject(function (_expressionService_) {
        expressionService = _expressionService_;
    }));

    it('should match variables by @', function () {
        var datamap = ({
            status: 'test'
        });
        var variables = expressionService.getVariables("@status == 'test'", datamap, false);
        expect(Object.keys(variables).length).toBe(1);
        expect(variables['@status']).toBe('test');
    });

    it('should match variables by @ 2', function () {
        var datamap = ({
            status: 'test',
            ticketid: '1000'
        });
        var variables = expressionService.getVariables("@status == 'test' and @ticketid!=null", datamap, false);
        expect(Object.keys(variables).length).toBe(2);
        expect(variables['@status']).toBe('test');
        expect(variables['@ticketid']).toBe('1000');
    });

    

});