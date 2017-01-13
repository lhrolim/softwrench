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
        var variables = expressionService.getVariablesBeforeJordanFuckedUp("@status == 'test'", datamap, false);
        expect(variables.length).toBe(1);
        expect(variables[0]).toBe('status');
    });



    it('should match datamap variables by @ 2', function () {
        var datamap = ({
            status: 'test',
            ticketid: '1000'
        });
        var variables = expressionService.getVariablesBeforeJordanFuckedUp("@status == 'test' and @b_.ticketid!=null", datamap, false);
        expect(variables.length).toBe(2);
        expect(variables[0]).toBe('status');
        expect(variables[1]).toBe('b_.ticketid');
    });

    it('should match datamap variables by @ 3', function () {
        var datamap = {};
        var variables = expressionService.getVariablesBeforeJordanFuckedUp("@status == 'test'", datamap, false);
        expect(variables.length).toBe(1);
        expect(variables[0]).toBe('status');
        
        //        expect(eval(variables['@status'])).toBeUndefined();
    });

    //    it('should match services by fn:', function () {
    //        var datamap = {};
    //        var expression = expressionService.getExpression("fn:service.method($.datamap,$.schema)'", null, false);
    //        expect(expression).toEqual("dispatcherService.invokeService('service','method',[scope.datamap,scope.schema])");
    //    });



});