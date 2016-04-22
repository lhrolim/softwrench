describe('Format Service Test', function () {
    var formatService;

    beforeEach(module('webcommons_services'));
    beforeEach(inject(function (_formatService_) {
        formatService = _formatService_;
    }));

    it('Test format service, IsChecked returns true', function () {
        var result, i;
        var trueCases = [1, "1", "true", "TRUE", "True", true, "Y"];
        var falseCases = [0, "0", false, "false", "FALSE", "False", "N", "Trueee", "truuuu", "anythingelse"];

        for (i = 0; i < trueCases.length; i++) {
            result = formatService.isChecked(trueCases[i]);
            expect(result).toBe(true);
        }

        for (i = 0; i < falseCases.length; i++) {
            result = formatService.isChecked(falseCases[i]);
            expect(result).toBe(false);
        }
    });
});