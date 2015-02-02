describe('expressionService', function () {
    var expressionService;
    beforeEach(module('sw_layout'));
    beforeEach(inject(function (_expressionService_) {
        expressionService = _expressionService_;
    }));

    it('should match variables by @', function () {
        var variables = expressionService.getVariables("@status == 'test'");
        expect(variables.length).toBe(1);
        expect(variables[0]).toBe('status');
    });

    it('should match variables by @ 2', function () {
        var variables = expressionService.getVariables("@status == 'test' and @ticketid!=null");
        expect(variables.length).toBe(2);
        expect(variables[0]).toBe('status');
        expect(variables[1]).toBe('ticketid');
    });

    

});