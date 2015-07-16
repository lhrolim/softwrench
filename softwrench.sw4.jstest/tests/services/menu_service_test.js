describe('Menu Service Test', function () {



    var menuService;
    var contextService;

    var mockedUser = {
        genericproperties: { email: 'lrolim@controltechnologysolutions.com' }
    };

    beforeEach(module('sw_layout'));
    beforeEach(inject(function (_menuService_, _userService_, _contextService_) {
        menuService = _menuService_;
        contextService = _contextService_;

    }));

    it('Test ExternalLink with parameters', function () {
        //creating mock user
        spyOn(contextService, 'getUserData').and.returnValue(mockedUser);

        var leaf = { "link": "https://secure.spaceplanfm.com/deltadental/default.asp?", "parameters": { "email": "@user.email", "target": "Request.asp" } };
        var link = menuService.parseExternalLink(leaf);
        expect(link).toBe("https://secure.spaceplanfm.com/deltadental/default.asp?email=lrolim@controltechnologysolutions.com&target=Request.asp");

        leaf = { "link": "https://secure.spaceplanfm.com/deltadental/default.asp", "parameters": { "email": "@user.email", "target": "Request.asp" }};
        link = menuService.parseExternalLink(leaf);
        expect(link).toBe("https://secure.spaceplanfm.com/deltadental/default.asp?email=lrolim@controltechnologysolutions.com&target=Request.asp");

    });

    it('Test ExternalLink without parameters', function () {
        //creating mock user

        spyOn(contextService, 'getUserData').and.returnValue(mockedUser);

        var leaf = { "link": "https://secure.spaceplanfm.com/deltadental/default.asp"};
        var link = menuService.parseExternalLink(leaf);
        expect(link).toBe("https://secure.spaceplanfm.com/deltadental/default.asp");
    });

    it('Test ExternalLink without http', function () {
        //creating mock user

        spyOn(contextService, 'getUserData').and.returnValue(mockedUser);

        var leaf = { "link": "www.secure.spaceplanfm.com/deltadental/default.asp"};
        var link = menuService.parseExternalLink(leaf);
        expect(link).toBe("http://www.secure.spaceplanfm.com/deltadental/default.asp");
    });



});