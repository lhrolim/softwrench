describe('expressionService', function () {
    var expressionService;
    beforeEach(module('sw_layout'));
    beforeEach(inject(function (_expressionService_) {
        expressionService = _expressionService_;
    }));

    it('should match datamap variables by @', function () {
        var datamap = ({
            status: 'test'
        });
        var variables = expressionService.getVariables("@status == 'test'", datamap, false);
        expect(Object.keys(variables).length).toBe(1);
        expect(eval(variables['@status'])).toBe('test');
    });



    it('should match datamap variables by @ 2', function () {
        var datamap = ({
            status: 'test',
            ticketid: '1000'
        });
        var variables = expressionService.getVariables("@status == 'test' and @ticketid!=null", datamap, false);
        expect(Object.keys(variables).length).toBe(2);
        expect(eval(variables['@status'])).toBe('test');
        expect(eval(variables['@ticketid'])).toBe('1000');
    });

    it('should match datamap variables by @ 3', function () {
        var datamap = {};
        var variables = expressionService.getVariables("@status == 'test'", datamap, false);
        expect(Object.keys(variables).length).toBe(1);
        expect(eval(variables['@status'])).toBeUndefined();
    });

    it('should match services by fn:', function () {
        var datamap = {};
        debugger;
        var variables = expressionService.getVariables("fn:service.method($.datamap,$.schema)'", null, false);
        
    });
   
    

});