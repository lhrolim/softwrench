describe("richText", function () {

    var richTextService;
    var mailto = {
        before: [
            "blah blah <mailto:support24@kongsberg.com> blah blah", // single email
            "blah blah <mailto:rbotti@controltechnologysolutions.com> blah blah " + // multiple email
                "bleh bleh <mailto:lrolim@controltechnologysolutions.com> bleh bleh " +
                "blih blih <mailto:jbaffa@controltechnologysolutions.com> blih blih",
            "<div> aaaa <mailto:rodrigo.botti@gmail.com> " + // multiple 'weird' emails
                "bbbb <mailto:rodrigo_rocks@hotmail.com> " +
                "cccc <mailto:rodrigo_brasil@uol.com.br> " +
                "dddd <mailto:!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp> </div>",
            "<div><b>DON'T CHANGE ME</b><br><b>I DON'T HAVE MAILTO TAGS</b></div>"
        ],
        after: [
            "blah blah <a href='mailto:support24@kongsberg.com'>support24@kongsberg.com</a> blah blah",
            "blah blah <a href='mailto:rbotti@controltechnologysolutions.com'>rbotti@controltechnologysolutions.com</a> blah blah " +
                "bleh bleh <a href='mailto:lrolim@controltechnologysolutions.com'>lrolim@controltechnologysolutions.com</a> bleh bleh " +
                "blih blih <a href='mailto:jbaffa@controltechnologysolutions.com'>jbaffa@controltechnologysolutions.com</a> blih blih",
            "<div> aaaa <a href='mailto:rodrigo.botti@gmail.com'>rodrigo.botti@gmail.com</a> " +
                "bbbb <a href='mailto:rodrigo_rocks@hotmail.com'>rodrigo_rocks@hotmail.com</a> " +
                "cccc <a href='mailto:rodrigo_brasil@uol.com.br'>rodrigo_brasil@uol.com.br</a> " +
                "dddd <a href='mailto:!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp'>!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp</a> </div>",
            "<div><b>DON'T CHANGE ME</b><br><b>I DON'T HAVE MAILTO TAGS</b></div>"
        ]
    }

    beforeEach(module("sw_layout"));
    beforeEach(inject(function (_richTextService_) {
        richTextService = _richTextService_;
    }));

    it("mailto tags replacement: 'blank' inputs", function () {
        expect(richTextService.replaceMailToTags(undefined)).toBeUndefined();
        expect(richTextService.replaceMailToTags(null)).toBeNull(null);
        expect(richTextService.replaceMailToTags("")).toBe("");
    });

    it("mailto tags replacement", function () {
        for (var i = 0; i < mailto.before.length; i++) {
            expect(richTextService.replaceMailToTags(mailto.before[i])).toEqual(mailto.after[i]);
            expect(mailto.before[i]).toEqual(mailto.before[i]); // didn't change original text
        }
    });

})