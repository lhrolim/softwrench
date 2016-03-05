/// <binding />
module.exports = function (grunt) {

    // Project configuration.

    //#region Scripts

    /** downloaded and customized or not distributed by bower */
    var customVendorScripts = [
        "www/Content/Vendor/downloadedvendor/persistence.js",
        "www/Content/Vendor/downloadedvendor/persistence.store.sql.js",
        "www/Content/Vendor/downloadedvendor/persistence.store.cordovasql.js",
        "www/Content/Vendor/downloadedvendor/jquery.scannerdetection.js",
        "www/Content/Vendor/downloadedvendor/rolling-log.js",
        "www/Content/Vendor/downloadedvendor/ionautocomplete.js"
    ];

    /** offline reusable lib scripts (ours) */
    var commonScripts = [
        // persistence.offline
        "www/Content/Mobile/scripts/persistence/module.js",
        "www/Content/Mobile/scripts/persistence/services/**/*.js",
        "www/Content/Mobile/scripts/persistence/config.entities.js",
        // audit.offline
        "www/Content/Shared/audit_offline/scripts/offline/audit.js"
    ];

    /** reusable online & offline lib scripts (ours)  */
    var sharedScripts = [
        "www/Content/Shared/webcommons/scripts/softwrench/sharedservices_module.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/statuscolor_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/aa_stringutils.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/aa_utils.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/object_util.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/context_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/tabs_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/i18n_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/schema_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/field_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/validation_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/alert_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/format_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/event_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/expression_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/dispatcher_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/scannerCommons.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/user_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/composition_commons.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/storage/localstorageservice.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/data/datamapSanitize_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/notificationService.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/physicalinventory_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/log_enhacer.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/clientawareserviceprovider.js"
    ];

    /** app scripts: required for bootstraping the app */
    var appBootstrapScripts = [
        "www/scripts/platformOverrides.js",
        "www/Content/Mobile/scripts/mobile_bootstrap.js",
        "www/Content/Mobile/scripts/utils/mobileconstants.js"
    ];

    /** app scripts: angular constructs */
    var appScripts = [
        "www/Content/Mobile/scripts/utils/**/*.js",
        "www/Content/Mobile/scripts/constants/**/*.js",
        "www/Content/Mobile/scripts/controllers/**/*.js",
        "www/Content/Mobile/scripts/services/**/*.js",
        "www/Content/Mobile/scripts/directives/**/*.js",
        "www/Content/Mobile/scripts/maximoservices/**/*.js",
        "www/Content/Mobile/scripts/filters/**/*.js",
        "www/Content/Mobile/scripts/decorators/**/*.js"
    ];

    //TODO: make a client-based build??
    //make it download the customer scripts from the server at runtime?
    var customerScripts = [];
    var customer = grunt.option("customer") || "pae"; // -> harcoding the only customer that has custom scripts
    //if (customer) {
    customerScripts = [
        "www/Content/Customers/" + customer + "_offline/scripts/**/*.mobile.js"
    ];
    //}

    var solutionScripts = []
        .concat(customVendorScripts)
        .concat(commonScripts)
        .concat(sharedScripts)
        .concat(appBootstrapScripts)
        .concat(appScripts)
        .concat(customerScripts);

    var vendorScripts = [
        // complete paths to guarantee load order (instead of **/*.js)
        "www/Content/Vendor/scripts/angular.js",
        "www/Content/Vendor/scripts/angular-ui-router.js",
        "www/Content/Vendor/scripts/angular-sanitize.js",
        "www/Content/Vendor/scripts/angular-animate.js",
        "www/Content/Vendor/scripts/ionic.min.js",
        "www/Content/Vendor/scripts/ionic-angular.min.js",
        "www/Content/Vendor/scripts/jquery.js",
        "www/Content/Vendor/scripts/ng-cordova.js",
        "www/Content/Vendor/scripts/moment.js",
        "www/Content/Vendor/scripts/underscore.js",
        "www/Content/Vendor/scripts/persistence.store.websql.js"
    ];
    
    
    var testScripts = [
        "bower_components/angular-mocks/angular-mocks.js",
        "tests/**/*.js"
    ];

    var allScripts = []
        .concat(vendorScripts)
        .concat(solutionScripts)
        .concat(testScripts);

    //#endregion

    grunt.initConfig({
        pkg: grunt.file.readJSON("package.json"),

        app: {
            index: "www/layout.html",
            vendors: vendorScripts
        },

        //#region clean directories
        clean: {
            vendor: [
                "www/Content/Vendor/scripts/",
                "www/Content/Vendor/css/"
            ],
            temp: ["tmp/"],
            pub: ["www/Content/public/"]
        },
        //#endregion

        //#region copy bower dependencies
        bowercopy: {
            dev: {
                options: {
                    destPrefix: "www/Content/Vendor/scripts"
                },
                files: {
                    "angular-sanitize.js": "angular-sanitize/angular-sanitize.js",
                    "angular-ui-router.js": "angular-ui-router/release/angular-ui-router.js",
                    "angular-animate.js": "angular-animate/angular-animate.js",
                    "angular.js": "angular/angular.js",
                    "jquery.js": "jquery/dist/jquery.js",
                    "ng-cordova.js": "ngCordova/dist/ng-cordova.js",
                    "persistence.store.websql.js": "persistence/lib/persistence.store.websql.js",
                    "moment.js": "moment/moment.js",
                    "ionic.min.js": "ionic/release/js/ionic.js",
                    "ionic-angular.min.js": "ionic/release/js/ionic-angular.js",
                    "underscore.js" : "underscore/underscore.js"
                }
            },
            css: {
                options: {
                    destPrefix: "www/Content/Vendor/css"
                },
                files: {
                    "ionic.min.css": "ionic/release/css/ionic.min.css",
                    "ionautocomplete.min.css": "ion-autocomplete/dist/ion-autocomplete.min.css"
                }
            },
            fontsdev: {
                options: {
                    destPrefix: "www/Content/Vendor/fonts"
                },
                files: {
                    "ionicons.eot": "ionic/release/fonts/ionicons.eot",
                    "ionicons.svg": "ionic/release/fonts/ionicons.svg",
                    "ionicons.ttf": "ionic/release/fonts/ionicons.ttf",
                    "ionicons.woff": "ionic/release/fonts/ionicons.woff"
                }
            },
            fontsrelease: {
                options: {
                    destPrefix: "www/Content/public/fonts"
                },
                files: {
                    "ionicons.eot": "ionic/release/fonts/ionicons.eot",
                    "ionicons.svg": "ionic/release/fonts/ionicons.svg",
                    "ionicons.ttf": "ionic/release/fonts/ionicons.ttf",
                    "ionicons.woff": "ionic/release/fonts/ionicons.woff"
                }
            },
            prod: {
                options: {
                    destPrefix: "www/Content/Vendor/scripts"
                },
                files: {
                    "angular.js": "angular/angular.min.js",
                    "angular-sanitize.js": "angular-sanitize/angular-sanitize.min.js",
                    "angular-ui-router.js": "angular-ui-router/release/angular-ui-router.min.js",
                    "angular-animate.js": "angular-animate/angular-animate.min.js",
                    "jquery.js": "jquery/dist/jquery.min.js",
                    "ng-cordova.js": "ngCordova/dist/ng-cordova.min.js",
                    "persistence.store.websql.js": "persistence/lib/persistence.store.websql.js",
                    "moment.js": "moment/min/moment.min.js",
                    "ionic.min.js": "ionic/release/js/ionic.min.js",
                    "ionic-angular.min.js": "ionic/release/js/ionic-angular.min.js",
                    "underscore.js": "underscore/underscore-min.js"
                }
            }
        },
        //#endregion

        //#region generating imports in /index.html
        tags: {
            /* 
                DEV: tags doesn't work with an outer object around the actual tasks
                such as `tags: { dev { buildScripts: { ... } } }` 
                and then calling `tags:dev` -> nested tasks aren't called
            */
            // app's js
            buildScripts: {
                options: {
                    openTag: "<!-- start auto template script tags, grunt will generate it for dev environment, do not remove this -->",
                    closeTag: "<!-- end auto template script tags -->"
                },
                src: solutionScripts,
                dest: "<%= app.index %>"
            },
            // vendors's js
            buildVendorScripts: {
                options: {
                    scriptTemplate: "<script type=\"text/javascript\" src=\"{{ path }}\"></script>",
                    openTag: "<!-- start auto template VENDOR script tags, grunt will generate it for dev environment, do not remove this -->",
                    closeTag: "<!-- end auto template VENDOR script tags -->"
                },
                src: [
                    "<%= app.vendors %>"
                ],
                dest: "<%= app.index %>"
            },
            // app's css
            buildLinks: {
                options: {
                    openTag: "<!-- start auto template style tags, grunt will generate it for dev environment, do not remove this -->",
                    closeTag: "<!-- end auto template style tags -->"
                },
                src: [
                    "www/css/**/*.css",
                    "www/Content/Mobile/**/*.css"
                ],
                dest: "<%= app.index %>"
            },
            // vendors's css
            buildVendorLinks: {
                options: {
                    linkTemplate: "<link rel=\"stylesheet\" type=\"text/css\" href=\"{{ path }}\" />",
                    openTag: "<!-- start auto template VENDOR style tags, grunt will generate it for dev environment, do not remove this -->",
                    closeTag: "<!-- end auto template VENDOR style tags -->"
                },
                src: [
                    "www/Content/Vendor/css/**/*.css"
                ],
                dest: "<%= app.index %>"
            },
            /* END DEV */

            /* RELEASE */
            // app's js
            buildReleaseScripts: {
                options: {
                    scriptTemplate: "<script type=\"text/javascript\" src=\"{{ path }}\"></script>",
                    openTag: "<!-- start auto template script tags, grunt will generate it for dev environment, do not remove this -->",
                    closeTag: "<!-- end auto template script tags -->"
                },
                src: [
                    "www/Content/public/app.min.js"
                ],
                dest: "www/layout.html"
            },
            // vendors's js
            buildReleaseVendorScripts: {
                options: {
                    scriptTemplate: "<script type=\"text/javascript\" src=\"{{ path }}\"></script>",
                    openTag: "<!-- start auto template VENDOR script tags, grunt will generate it for dev environment, do not remove this -->",
                    closeTag: "<!-- end auto template VENDOR script tags -->"
                },
                src: [
                    "www/Content/public/vendor/vendor.min.js"
                ],
                dest: "www/layout.html"
            },
            // app's css
            buildReleaseLinks: {
                options: {
                    linkTemplate: "<link rel=\"stylesheet\" type=\"text/css\" href=\"{{ path }}\" />",
                    openTag: "<!-- start auto template style tags, grunt will generate it for dev environment, do not remove this -->",
                    closeTag: "<!-- end auto template style tags -->"
                },
                src: [
                    "www/Content/public/app.min.css"
                ],
                dest: "www/layout.html"
            },
            // vendors's css
            buildReleaseVendorLinks: {
                options: {
                    linkTemplate: "<link rel=\"stylesheet\" type=\"text/css\" href=\"{{ path }}\" />",
                    openTag: "<!-- start auto template VENDOR style tags, grunt will generate it for dev environment, do not remove this -->",
                    closeTag: "<!-- end auto template VENDOR style tags -->"
                },
                src: [
                    "www/Content/public/vendor/vendor.min.css"
                ],
                dest: "www/layout.html"
            }
            /* END RELEASE */
        },
        //#endregion

        //#region concat
        concat: {
            appScripts: {
                options: {
                    separator: ";\n"
                },
                src: solutionScripts,
                dest: "tmp/concat/app.js"
            },
            vendorScripts: {
                options: {
                    separator: ";\n"
                },
                src: "<%= app.vendors %>",
                dest: "www/Content/public/vendor/vendor.min.js" // already minified by vendors
            },
            appStyles: {
                src: [
                    "www/css/**/*.css",
                    "www/Content/Mobile/**/*.css"
                ],
                dest: "tmp/concat/app.css"
            },
            vendorStyles: {
                src: [
                    "www/Content/Vendor/css/**/*.css"
                ],
                dest: "www/Content/public/vendor/vendor.min.css" // already minified by vendors
            }
        },
        //#endregion

        //#region minify javascript
        uglify: {
            options: {
                mangle: {
                    except: ["jQuery", "angular", "persistence", "constants", "ionic", "_"]
                }
            },
            release: {
                // uglify the result of concat
                files: {
                    "www/Content/public/app.min.js": "<%= concat.appScripts.dest %>"
                }
            }
        },
        //#endregion

        //#region minify css
        cssmin: {
            release: {
                // minify the result of concat
                files: {
                    "www/Content/public/app.min.css": "<%= concat.appStyles.dest %>"
                }
            }
        },
        //#endregion

        //#region copy
        copy: {
            build: {
                // applies /overrides files
                files: [
                    { expand: true, src: ["**/*", "!cordova.js"], dest: "platforms/", cwd: "overrides/" }
                ]
            }
        },
        //#endregion

        //#region karma
        karma: {
            options: {
                configFile: "karma.conf.js",
                logLevel: "WARN",
                files: ["overrides/cordova.js"].concat(allScripts),
                browsers: ["PhantomJS"],
                singleRun: true
            },
            tdd: { // dev environment
                autoWatch: true,
                singleRun: false,
                logLevel: "DEBUG",
                browsers: ["Chrome"]
            },
            debug: { // CI dev
            },
            release: { // CI release
                files: [{
                    src: [
                        "overrides/cordova.js",
                        "www/Content/public/vendor/vendor.min.js",
                        "www/Content/public/app.min.js"
                    ].concat(testScripts)
                }]
            }
        }
        //#endregion
    });

    //#region grunt plugins
    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks("grunt-contrib-concat");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-bowercopy");
    grunt.loadNpmTasks("grunt-script-link-tags");
    grunt.loadNpmTasks("grunt-contrib-copy");
    grunt.loadNpmTasks("grunt-contrib-cssmin");
    grunt.loadNpmTasks("grunt-karma");
    //#endregion

    //#region dev tasks
    grunt.registerTask("cleanall", ["clean:vendor", "clean:temp", "clean:pub"]);
    grunt.registerTask("tagsdev", ["tags:buildScripts", "tags:buildVendorScripts", "tags:buildLinks", "tags:buildVendorLinks"]);
    grunt.registerTask("fulldev", ["cleanall", "bowercopy:dev", "bowercopy:css", "bowercopy:fontsdev", "tagsdev"]);
    grunt.registerTask("default", ["fulldev"]);
    //#endregion

    //#region release:prepare tasks
    grunt.registerTask("tagsrelease", ["tags:buildReleaseScripts", "tags:buildReleaseVendorScripts", "tags:buildReleaseLinks", "tags:buildReleaseVendorLinks"]);
    grunt.registerTask("concatall", ["concat:appScripts", "concat:vendorScripts", "concat:appStyles", "concat:vendorStyles"]);
    grunt.registerTask("minify", ["uglify:release", "cssmin:release"]);

    grunt.registerTask("preparerelease", "prepares the project for release build", [
        "cleanall", // cleans destination folders
        "bowercopy:prod", "bowercopy:css", "bowercopy:fontsrelease", // copy bower dependencies to appropriate project folders
        "concatall", // concats the scripts and stylesheets
        "minify", // uglyfies scripts and minifies stylesheets
        "tagsrelease" // generates import tags for the prepared files in main template file (layout.html)
    ]);
    //#endregion

    //#region ** BUILD DEVICE ARTIFACTS **
    var fs = require("fs-extra");
    var path = require("path");
    var Q = require("q");
    var taco = require("taco-team-build");

    // ripped from taco-team-build.js
    // Utility method that coverts args into a consistant input understood by cordova-lib
    function getCallArgs(platforms, args) {
        // Processes single platform string (or array of length 1) and an array of args or an object of args per platform
        args = args || [];
        if (typeof (platforms) == "string") {
            platforms = [platforms];
        }
        // If only one platform is specified, check if the args is an object and use the args for this platform if so
        if (platforms.length === 1) {
            if (args instanceof Array) {
                return { platforms: platforms, options: args };
            } else {
                return { platforms: platforms, options: args[platforms[0]] };
            }
        }
    }

    // ripped from taco-team-build.js
    // Prep for build by adding platforms and setting environment variables
    function addPlatformsToProject(cordova, cordovaPlatforms) {
        var promise = Q();
        var projectPath = process.cwd();
        cordovaPlatforms.forEach(function (platform) {
            promise = promise.then(function () {
                return cordova.raw.platform('rm', platform);
            }).then(function () {
                console.log("Adding platform " + platform + "...");
                // Fix for when the plugins/<platform>.json file is accidently checked into source control 
                // without the corresponding contents of the platforms folder. This can cause the behavior
                // described here: http://stackoverflow.com/questions/30698118/tools-for-apache-cordova-installed-plugins-are-skipped-in-build 
                var platformPluginJsonFile = path.join(projectPath, "plugins", platform.trim() + ".json")
                if (fs.existsSync(platformPluginJsonFile)) {
                    console.log(platform + ".json file found at \"" + platformPluginJsonFile + "\". Removing to ensure plugins install properly in newly added platform.")
                    fs.unlinkSync(platformPluginJsonFile);
                }
            }).then(function () {
                // Now add the platform
                return cordova.raw.platform('add', platform);
            }).then(function () {
                // apply overrides
                // TODO: copy all files in overrides
                var ori = path.join(projectPath, "overrides", platform.trim(), "cordova", "lib", "build.js");
                var dest = path.join(projectPath, "platforms", platform.trim(), "cordova", "lib", "build.js");
                return fs.copySync(ori, dest);
            });

        });
        return promise;
    }

    // ripped and modified from taco-team-build.js
    function buildProject(cordovaPlatforms, args) {
        if (typeof (cordovaPlatforms) == "string") {
            cordovaPlatforms = [cordovaPlatforms];
        }
        return taco.setupCordova().then(function(cordova) {
            // Add platforms if not done already
            var promise = addPlatformsToProject(cordova, cordovaPlatforms);
            //Build each platform with args in args object
            cordovaPlatforms.forEach(function (platform) {
                promise = promise.then(function () {
                    // Build app with platform specific args if specified
                    var callArgs = getCallArgs(platform, args);
                    console.log("Queueing build for platform " + platform + " w/options: " + callArgs.options);
                    return cordova.raw.build(callArgs);
                });
            });
            return promise;
        });
    }

    grunt.registerTask("build", "builds app for devices", function (env) {
        var done = this.async();

        //var platformsToBuild = process.platform == "darwin" ? ["ios"] : ["android", "windows", "wp8"*/], // Darwin == OSX
        var cliEnv = "--" + env;

        var platformsToBuild = process.platform === "darwin" ? ["ios"] : ["android"];
        var buildArgs = {
            android: [cliEnv],    // Warning: Omit the extra "--" when referencing platform
            ios: [cliEnv, "--device"],     // specific preferences like "-- --ant" for Android
            windows: [cliEnv],             // or "-- --win" for Windows. You may also encounter a
            wp8: [cliEnv]                  // "TypeError" after adding a flag Android doesn"t recognize
        };                                      // when using Cordova < 4.3.0. This is fixed in 4.3.0.

        return buildProject(platformsToBuild, buildArgs)
            .then(function () {
                return taco.packageProject(platformsToBuild);
            })
            .then(function() {
            		return done();
            })
            .catch(function(e) {
            		console.error("Error building project:\n", e);
            		return done(false);
            });
    });

    grunt.registerTask("vs2015", "intended for CI: prepares workspace, executes karma tests and builds the app for devices", function (env) {
        env = env || "debug";
        switch (env) {
            case "release":
                return grunt.task.run(["preparerelease", "karma:" + env, "build:" + env]);
            case "debug":
                return grunt.task.run(["fulldev", "karma:" + env, "build:" + env]);
            default:
                throw new Error("Unsupported build environment: " + env);
        }
    });

    //#endregion
};
