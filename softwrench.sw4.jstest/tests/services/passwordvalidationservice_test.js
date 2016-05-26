describe("passwordValidationService", function() {

    var service, $q, $root;

    beforeEach(module("sw_layout"));
    beforeEach(inject((_passwordValidationService_, _$q_, _$rootScope_) => {
        service = _passwordValidationService_;
        $q = _$q_;
        $root = _$rootScope_;
    }));
    
    /**
     * @param {PasswordConfig} config 
     * {     
     *      min: Number,
     *      adjacent: Number,
     *      requiredCharacters: Array<String>,
     *      uppercase: Boolean,
     *      lowercase: Boolean,
     *      number: Boolean,
     *      special: Boolean,
     *      placement: {
     *          number: { first: Boolean, last: Boolean },
     *          special: { first: Boolean, last: Boolean }
     *      }
     * }
     */
    function mockConfig(config) {
        spyOn(service, "getPasswordConfiguration").and.returnValue(config);
        spyOn(service, "getPasswordConfigurationAsync").and.callFake(() => {
            const deferred = $q.defer();
            deferred.resolve(config);
            return deferred.promise;
        });
    }

    it("synchronous", () => {
        mockConfig({
            min: 4,
            adjacent: 2,
            uppercase: true,
            lowercase: true,
            number: true,
            special: true,
            blacklist: [
                "1234", "abcd"
            ]
        });
        // fails all constaints except 'requires number'
        expect(service.validatePassword("111").length).toBe(5);
        // fails all constraints except 'requires lowercase'
        expect(service.validatePassword("aaa").length).toBe(5);
        // fails all except 'min', 'adjacent' and 'requires special'
        expect(service.validatePassword("1234").length).toBe(4);
        // fails 'requires uppercase', 'requires number' and 'requires special'
        expect(service.validatePassword("password").length).toBe(3);
        // fails 'requires uppercase'
        expect(service.validatePassword("sw@dm1n").length).toBe(1);
        // passes all checks
        expect(service.validatePassword("bAp4%").length).toBe(0);
    });

    it("synchronous: login", () => {
        mockConfig({ login: false });
        // does not contain username: passes all checks
        expect(service.validatePassword("valid", { username: "swadmin" }).length).toBe(0);
        // contains username: fails login check
        expect(service.validatePassword("containsswadmininit", { username: "swadmin" }).length).toBe(1);
    });

    it("asynchronous", done => {
        mockConfig({
            min: 5,
            placement: {
                number: { first: false, last: false },
                special: { first: false, last: false }
            }
        });

        $q.all([
            // fails all constraints
            service.validatePasswordAsync("$").then(v => expect(v.length).toBe(3)),
            // fails 'first character cannot be number' and 'last character cannot be number'
            service.validatePasswordAsync("1aaa5").then(v => expect(v.length).toBe(2)),
            // fails 'first character cannot be special' and 'last character cannot be number'
            service.validatePasswordAsync("&aaa5").then(v => expect(v.length).toBe(2)),
            // fails 'first character cannot be number' and 'last character cannot be special'
            service.validatePasswordAsync("7aaa*").then(v => expect(v.length).toBe(2)),
            // passes all checks
            service.validatePasswordAsync("sw@dm1n").then(v => expect(v.length).toBe(0))
        ]).finally(done);

        $root.$apply();
    });
})