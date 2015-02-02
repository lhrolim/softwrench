describe('expressionService', function () {
    var expressionService;
    beforeEach(module('sw_layout'));
    beforeEach(inject(function (_expressionService_) {
        expressionService = _expressionService_;
    }));
    it('should match variables by @', function () {
        var variables = expressionService.getVariables("@status == 'test' and @ticketid!=null");
        expect(variables.length).toBe(2);
    });
});