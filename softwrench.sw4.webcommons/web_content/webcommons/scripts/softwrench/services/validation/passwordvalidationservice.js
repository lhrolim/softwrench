(function (angular, $) {
    "use strict";

    function passwordValidationService($log, $http, $injector) {
        //#region Utils
        class PasswordConfig {
            constructor() {
                this.min = 6;
                this.adjacent = 3;
                this.uppercase = true;
                this.lowercase = true;
                this.number = true;
                this.special = true;
                this.blacklist = [];
                this.login = true,
                this.placement = {
                    number: { first: false, last: false },
                    special: { first: false, last: false }
                }
            }
        }

        var config = {
            promise: null,
            'default': new PasswordConfig(),
            keys: {
                min: "/Global/Password/Min",
                adjacent: "/Global/Password/Adjacent",
                uppercase: "/Global/Password/Uppercase",
                lowercase: "/Global/Password/Lowercase",
                number: "/Global/Password/Number",
                special: "/Global/Password/Special",
                blacklist: "/Global/Password/BlackList",
                numberFirst: "/Global/Password/PlacementNumberFirst",
                numberLast: "/Global/Password/PlacementNumberLast",
                specialFirst: "/Global/Password/PlacementSpecialFirst",
                specialLast: "/Global/Password/PlacementSpecialLast",
                login: "/Global/Password/Login"
            }
        }

        var configurationService = null;

        function getConfigurationService() {
            return configurationService = (configurationService || $injector.get("configurationService"));
        }

        function formatConfig(configDictionary) {
            return {
                min: parseInt(configDictionary[config.keys.min]),
                adjacent: parseInt(configDictionary[config.keys.adjacent]),
                uppercase: "true".equalIc(configDictionary[config.keys.uppercase]),
                lowercase: "true".equalIc(configDictionary[config.keys.lowercase]),
                number: "true".equalIc(configDictionary[config.keys.number]),
                special: "true".equalIc(configDictionary[config.keys.special]),
                blacklist: window.isString(configDictionary[config.keys.blacklist]) ? configDictionary[config.keys.blacklist].split(",") : [],
                login: "true".equalIc(configDictionary[config.keys.login]),
                placement: {
                    number: {
                        first: "true".equalIc(configDictionary[config.keys.numberFirst]),
                        last: "true".equalIc(configDictionary[config.keys.numberLast])
                    },
                    special: {
                        first: "true".equalIc(configDictionary[config.keys.specialFirst]),
                        last: "true".equalIc(configDictionary[config.keys.specialLast])
                    }
                }
            }    
        }

        function fetchPasswordConfiguration() {
            const log = $log.get("passwordValidationService#fetchPasswordConfiguration", ["validation", "config", "password"]);

            const keys = Object.values(config.keys);
            const configurl = url("/api/generic/Configuration/GetConfigurations?") + $.param({ fullKeys: keys });

            config.promise = $http.get(configurl)
                .then(r => formatConfig(r.data))
                .catch(e => {
                    log.error("Error fetching password configuration\n", e, "\nusing default configuration");
                    return config.default;
                })
                .finally(() => config.promise = null);

            return config.promise;
        }

        function doValidatePassword(password, passwordConfig, extra) {
            password = password || "";
            extra = extra || {};
            const validations = [];
            if (angular.isNumber(passwordConfig.min) && passwordConfig.min > 0 && password.length < passwordConfig.min) {
                validations.push(`Lenght must greater than or equal to ${passwordConfig.min}`);
            }
            if (angular.isNumber(passwordConfig.adjacent) && passwordConfig.adjacent > 0) {
                // base regex: '(.)\1\1\1 ... +' (repeating \1 as much as you want to detect - 1 e.g. 
                // allows 3 -> want to detect 4 -> repeat 3 times i.e. always repeat passwordConfig.adjancent times)
                const adjancentregexp = new RegExp(`(.)${"\\1".repeat(passwordConfig.adjacent)}+`, "g");
                if (adjancentregexp.test(password)) {
                    validations.push(`Cannot have more than ${passwordConfig.adjacent} identical adjacent characters`);
                }
            }
            if (angular.isArray(passwordConfig.blacklist) && passwordConfig.blacklist.some(c => c === password)) {
                validations.push(`Password cannot be any of the following: ${passwordConfig.blacklist.join(", ")}`);
            }
            if (passwordConfig.uppercase && !/[A-Z]/.test(password)) {
                validations.push("Requires at least one uppercase letter");
            }
            if (passwordConfig.lowercase && !/[a-z]/.test(password)) {
                validations.push("Requires at least one lowercase letter");
            }
            const numberRegexp = /[0-9]/;
            if (passwordConfig.number && !numberRegexp.test(password)) {
                validations.push("Requires at least one number character");
            }
            const regularCharactersRegexp = /^[a-zA-Z0-9- ]*$/;
            if (passwordConfig.special && regularCharactersRegexp.test(password)) {
                validations.push("Requires at least one special character");
            }
            const username = extra.username;
            if (!passwordConfig.login && !!username && password.contains(username)) {
                validations.push("Cannot contain user's username");
            }

            if (!passwordConfig.placement) return validations;

            const firstCharacter = password.charAt(0);
            const lastCharacter = password.charAt(password.length - 1);
            if (!!passwordConfig.placement.number && !passwordConfig.placement.number.first && numberRegexp.test(firstCharacter)) {
                validations.push("First character cannot be a number");
            }
            if (!!passwordConfig.placement.number && !passwordConfig.placement.number.last && numberRegexp.test(lastCharacter)) {
                validations.push("Last character cannot be a number");
            }
            if (!!passwordConfig.placement.special && !passwordConfig.placement.special.first && !regularCharactersRegexp.test(firstCharacter)) {
                validations.push("First character cannot be a special character");
            }
            if (!!passwordConfig.placement.special && !passwordConfig.placement.special.last && !regularCharactersRegexp.test(lastCharacter)) {
                validations.push("Last character cannot be a special character");
            }

            return validations;
        }

        //#endregion

        //#region Public methods
        
       /**
        * Fetches configuration from the server.
        * Guarantees there's only one ongoing request at a time.
        * 
        * @returns {Promise<PasswordConfig>} 
        */
        function getPasswordConfigurationAsync() {
            return config.promise || fetchPasswordConfiguration();
        }

        /**
         * Get the configuration already available to the client.
         * 
         * @returns {PasswordConfig} 
         */
        function getPasswordConfiguration() {
            const configDict = {};
            Object.values(config.keys).forEach(key => configDict[key] = getConfigurationService().getConfigurationValue(key));
            const passwordConfig = formatConfig(configDict);
            return passwordConfig;
        }

        /**
         * Returns validation violation messages the password has 
         * (according to {@link #getPasswordConfiguration}).
         * 
         * @param {string} password 
         * @param {Object} extra
         * @returns {Array<string>} 
         */
        function validatePassword(password, extra) {
            const passwordConfig = this.getPasswordConfiguration();
            return doValidatePassword(password, passwordConfig, extra);
        }
        
        /**
         * Returns promise resolved with validation violation messages the password has 
         * (according to {@link #getPasswordConfigurationAsync}).
         * 
         * @param {String} password 
         * @param {Object} extra
         * @returns {Promise<Array<string>>} 
         */
        function validatePasswordAsync(password, extra) {
            return this.getPasswordConfigurationAsync().then(c => doValidatePassword(password, c, extra));
        }
        
        //#endregion

        //#region Service Instance
        const service = {
            validatePassword,
            validatePasswordAsync,
            getPasswordConfiguration,
            getPasswordConfigurationAsync
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("webcommons_services").service("passwordValidationService", ["$log", "$http", "$injector", passwordValidationService]);
    //#endregion

})(angular, jQuery);