describe("SecurityService test", function() {

    const mocked = {
        storage: {},
        userData: {
            "UserName": "swadmin",
            "OrgId": "orgid",
            "SiteId": "siteid"
        }
    };

    var securityService, $rootScope;

    beforeEach(module("softwrench"));
    beforeEach(inject((_securityService_, _localStorageService_, _cookieService_, _$rootScope_, _$q_) => {
        securityService = _securityService_;
        $rootScope = _$rootScope_;

        spyOn(_localStorageService_, "put").and.callFake((key, value, options) => mocked.storage[key] = value);
        spyOn(_localStorageService_, "get").and.callFake(key => mocked.storage[key]);
        spyOn(_cookieService_, "persistCookie").and.callFake(() => _$q_.when());
    }));

    it("Testing local login functions", done => {
        // no logged user
        expect(securityService.hasAuthenticatedUser()).toBeFalsy();
        // login locally
        securityService.loginLocal(mocked.userData).then(() => {
            expect(securityService.hasAuthenticatedUser()).toBeTruthy();
            expect(securityService.currentUser()).toEqual(mocked.userData["UserName"]);
            expect(securityService.currentFullUser()).toEqual(mocked.userData);
        })
        .catch(fail)
        .finally(done);

        $rootScope.$digest();
    });

});