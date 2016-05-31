describe("SecurityService test", function() {

    const mocked = {
        storage: {},
        userData: {
            "UserName": "swadmin",
            "OrgId": "orgid",
            "SiteId": "siteid"
        }
    };

    var localStorageService, securityService;

    beforeEach(module("softwrench"));
    beforeEach(inject((_securityService_, _localStorageService_) => {
        securityService = _securityService_;
        localStorageService = _localStorageService_;
        spyOn(localStorageService, "put").and.callFake((key, value, options) => mocked.storage[key] = value);
        spyOn(localStorageService, "get").and.callFake(key => mocked.storage[key]);
    }));

    it("Testing local login functions", () => {
        // no logged user
        expect(securityService.hasAuthenticatedUser()).toBeFalsy();
        // login locally
        securityService.loginLocal(mocked.userData);
        expect(securityService.hasAuthenticatedUser()).toBeTruthy();
        expect(securityService.currentUser()).toEqual(mocked.userData["UserName"]);
        expect(securityService.currentFullUser()).toEqual(mocked.userData);
    });

});