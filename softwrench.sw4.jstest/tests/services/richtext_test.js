﻿describe("richText", function () {

    var config = {
        mailtotag: { // <mailto:email_pattern>
            input: [
                "blah blah <mailto:support24@kongsberg.com> blah blah", // single email
                "blah blah <mailto:rbotti@controltechnologysolutions.com> blah blah " + // multiple email
                    "bleh bleh <mailto:lrolim@controltechnologysolutions.com> bleh bleh " +
                    "blih blih <mailto:jbaffa@controltechnologysolutions.com> blih blih",
                "<div> aaaa <mailto:rodrigo.botti@gmail.com> " + // multiple 'weird' emails
                    "bbbb <mailto:rodrigo_rocks@hotmail.com> " +
                    "cccc <mailto:rodrigo_brasil@uol.com.br> " +
                    "dddd <mailto:!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp> </div>",
            ],
            expected: [ 
                "blah blah <a href='mailto:support24@kongsberg.com'>support24@kongsberg.com</a> blah blah",
                "blah blah <a href='mailto:rbotti@controltechnologysolutions.com'>rbotti@controltechnologysolutions.com</a> blah blah " +
                    "bleh bleh <a href='mailto:lrolim@controltechnologysolutions.com'>lrolim@controltechnologysolutions.com</a> bleh bleh " +
                    "blih blih <a href='mailto:jbaffa@controltechnologysolutions.com'>jbaffa@controltechnologysolutions.com</a> blih blih",
                "<div> aaaa <a href='mailto:rodrigo.botti@gmail.com'>rodrigo.botti@gmail.com</a> " +
                    "bbbb <a href='mailto:rodrigo_rocks@hotmail.com'>rodrigo_rocks@hotmail.com</a> " +
                    "cccc <a href='mailto:rodrigo_brasil@uol.com.br'>rodrigo_brasil@uol.com.br</a> " +
                    "dddd <a href='mailto:!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp'>!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp</a> </div>",
            ]
        },
        emailtag: { // <email_pattern>
            input: [
                "blah blah <support24@kongsberg.com> blah blah", // single email
                "blah blah <rbotti@controltechnologysolutions.com> blah blah " + // multiple email
                    "bleh bleh <lrolim@controltechnologysolutions.com> bleh bleh " +
                    "blih blih <jbaffa@controltechnologysolutions.com> blih blih",
                "<div> aaaa <rodrigo.botti@gmail.com> " + // multiple 'weird' emails
                    "bbbb <rodrigo_rocks@hotmail.com> " +
                    "cccc <rodrigo_brasil@uol.com.br> " +
                    "dddd <!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp> </div>",
            ],
            expected: [
                "blah blah <a href='mailto:support24@kongsberg.com'>support24@kongsberg.com</a> blah blah",
                "blah blah <a href='mailto:rbotti@controltechnologysolutions.com'>rbotti@controltechnologysolutions.com</a> blah blah " +
                    "bleh bleh <a href='mailto:lrolim@controltechnologysolutions.com'>lrolim@controltechnologysolutions.com</a> bleh bleh " +
                    "blih blih <a href='mailto:jbaffa@controltechnologysolutions.com'>jbaffa@controltechnologysolutions.com</a> blih blih",
                "<div> aaaa <a href='mailto:rodrigo.botti@gmail.com'>rodrigo.botti@gmail.com</a> " +
                    "bbbb <a href='mailto:rodrigo_rocks@hotmail.com'>rodrigo_rocks@hotmail.com</a> " +
                    "cccc <a href='mailto:rodrigo_brasil@uol.com.br'>rodrigo_brasil@uol.com.br</a> " +
                    "dddd <a href='mailto:!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp'>!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp</a> </div>",
            ]
        },
        mixedmailtags: { // <mailto:email_pattern> + <email_pattern>
            input: [
                "<div> aaaa <mailto:rodrigo.botti@gmail.com> " +
                    "bbbb <rodrigo_rocks@hotmail.com> " +
                    "cccc <mailto:rodrigo_brasil@uol.com.br> " +
                    "dddd <!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp> " +
                    "eeee <mailto:!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp> </div>"
            ],
            expected: [
                "<div> aaaa <a href='mailto:rodrigo.botti@gmail.com'>rodrigo.botti@gmail.com</a> " +
                    "bbbb <a href='mailto:rodrigo_rocks@hotmail.com'>rodrigo_rocks@hotmail.com</a> " +
                    "cccc <a href='mailto:rodrigo_brasil@uol.com.br'>rodrigo_brasil@uol.com.br</a> " +
                    "dddd <a href='mailto:!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp'>!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp</a> " +
                    "eeee <a href='mailto:!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp'>!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp</a> </div>"
            ]
        },
        urltags: { // <url_pattern>
            input: [
                "aaaa <http://google.com> aaaa",
                "aaaa <https://google.com> aaaa",
                "aaaa <http://api.icndb.com/jokes/random?firstName=Boris&lastName=Botti> aaaa"
            ],
            expected: [
                "aaaa <a href='http://google.com'>http://google.com</a> aaaa",
                "aaaa <a href='https://google.com'>https://google.com</a> aaaa",
                "aaaa <a href='http://api.icndb.com/jokes/random?firstName=Boris&lastName=Botti'>http://api.icndb.com/jokes/random?firstName=Boris&lastName=Botti</a> aaaa"
            ]
        },
        alltags: {
            input: [
                "<div> aaaa <mailto:rodrigo.botti@gmail.com> " +
                    "bbbb <rodrigo_rocks@hotmail.com> " +
                    "cccc <mailto:rodrigo_brasil@uol.com.br> " +
                    "dddd <!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp> " +
                    "eeee <mailto:!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp> " +
                    "ffff <http://api.icndb.com/jokes/random?firstName=Boris&lastName=Botti> </div>",
                "<div><b>DON'T CHANGE ME</b><br><b>I DON'T ANY INVALID MAILTO TAGS</b></div>"
            ],
            expected: [
                "<div> aaaa <a href='mailto:rodrigo.botti@gmail.com'>rodrigo.botti@gmail.com</a> " +
                    "bbbb <a href='mailto:rodrigo_rocks@hotmail.com'>rodrigo_rocks@hotmail.com</a> " +
                    "cccc <a href='mailto:rodrigo_brasil@uol.com.br'>rodrigo_brasil@uol.com.br</a> " +
                    "dddd <a href='mailto:!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp'>!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp</a> " +
                    "eeee <a href='mailto:!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp'>!#_$%ü&*-´`^~@!#_$%ü&*-´`^~.com.jp</a> " +
                    "ffff <a href='http://api.icndb.com/jokes/random?firstName=Boris&lastName=Botti'>http://api.icndb.com/jokes/random?firstName=Boris&lastName=Botti</a> </div>",
                "<div><b>DON'T CHANGE ME</b><br><b>I DON'T ANY INVALID MAILTO TAGS</b></div>"
            ]
        }
    };
    
    var richTextService;

    /**
     * Runs richTextService.replaceInvalidTags method test.
     * @param {} testConfig containing input array and matching expected result
     */
    function runInvalidTagReplacementTest(testConfig) {
        for (var i = 0; i < testConfig.input.length; i++) {
            expect(richTextService.replaceInvalidTags(testConfig.input[i])).toEqual(testConfig.expected[i]);
            expect(testConfig.input[i]).toEqual(testConfig.input[i]); // didn't change original text
        }
    }

    beforeEach(module("sw_layout"));
    beforeEach(inject(function (_richTextService_) {
        richTextService = _richTextService_;
    }));

    it("mailto tags replacement: 'blank' inputs", function () {
        expect(richTextService.replaceInvalidTags(undefined)).toBeUndefined();
        expect(richTextService.replaceInvalidTags(null)).toBeNull(null);
        expect(richTextService.replaceInvalidTags("")).toBe("");
    });

    it("mailto tags replacement", function () {
        runInvalidTagReplacementTest(config.mailtotag);
    });

    it("email tags replacement", function () {
        runInvalidTagReplacementTest(config.emailtag);
    });

    it("mailto and email tags replacement", function () {
        runInvalidTagReplacementTest(config.mixedmailtags);
    });

    it("url tags replacement", function () {
        runInvalidTagReplacementTest(config.urltags);
    });

    it("url and email tags replacement", function () {
        runInvalidTagReplacementTest(config.alltags);
    });

})