module.exports = function (grunt) {

    // Project configuration.

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
        "www/Content/Shared/webcommons/scripts/softwrench/services/format_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/event_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/expression_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/dispatcher_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/rest_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/scannerCommons.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/user_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/composition_commons.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/log_enhacer.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/clientawareserviceprovider.js"
    ];

    /** app scripts: required for bootstraping the app */
    var appBootstrapScripts = [
        "www/scripts/platformOverrides.js",
        "www/scripts/index.js",
        "www/Content/Mobile/scripts/mobile_bootstrap.js",
        "www/Content/Mobile/scripts/utils/mobileconstants.js"
    ];

    /** app scripts: angular constructs */
    var appScripts = [
        "www/Content/Mobile/scripts/controllers/**/*.js",
        "www/Content/Mobile/scripts/services/**/*.js",
        "www/Content/Mobile/scripts/directives/**/*.js",
        "www/Content/Mobile/scripts/maximoservices/**/*.js",
        "www/Content/Mobile/scripts/utils/**/*.js",
        "www/Content/Mobile/scripts/filters/**/*.js",
        "www/Content/Mobile/scripts/decorators/**/*.js",
        "www/Content/Mobile/scripts/constants/**/*.js"
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

    grunt.initConfig({
        pkg: grunt.file.readJSON("package.json"),

        app: {
            index: "www/layout.html",
            vendors: [ // complete paths to guarantee load order (instead of **/*.js)
                "www/Content/Vendor/scripts/angular.js",
                "www/Content/Vendor/scripts/angular-ui-router.js",
                "www/Content/Vendor/scripts/angular-sanitize.js",
                "www/Content/Vendor/scripts/angular-animate.js",
                "www/Content/Vendor/scripts/ionic.min.js",
                "www/Content/Vendor/scripts/ionic-angular.min.js",
                "www/Content/Vendor/scripts/jquery.js",
                "www/Content/Vendor/scripts/ng-cordova.js",
                "www/Content/Vendor/scripts/moment.js",
                "www/Content/Vendor/scripts/persistence.store.websql.js"
            ]
        },

        clean: {
            vendor: [
                "www/Content/Vendor/scripts/",
                "www/Content/Vendor/css/"
            ],
            temp: ["tmp/"],
            pub: ["www/Content/public/"]
        },

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
                    "ionic.min.js": "ionic/release/js/ionic.min.js",
                    "ionic-angular.min.js": "ionic/release/js/ionic-angular.min.js",
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
                    "ionic-angular.min.js": "ionic/release/js/ionic-angular.min.js"
                }
            }
        },

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

        uglify: {
            options: {
                mangle: {
                    except: ["jQuery", "angular", "persistence", "constants", "ionic"]
                }
            },
            release: {
                // uglify the result of concat
                files: {
                    "www/Content/public/app.min.js": "<%= concat.appScripts.dest %>"
                }
            }
        },

        cssmin: {
            release: {
                // minify the result of concat
                files: {
                    "www/Content/public/app.min.css": "<%= concat.appStyles.dest %>"
                }
            }
        },

        copy: {
            build: { // applies /overrides files
                files: [
                    { expand: true, src: ["**/*"], dest: "platforms/", cwd: "overrides/" }
                ]
            }
        }

    });


    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks("grunt-contrib-concat");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-bowercopy");
    grunt.loadNpmTasks("grunt-script-link-tags");
    grunt.loadNpmTasks("grunt-contrib-copy");
    grunt.loadNpmTasks("grunt-contrib-cssmin");

    // Default task(s).

    grunt.registerTask("cleanall", ["clean:vendor", "clean:temp", "clean:pub"]);

    // dev

    grunt.registerTask("tagsdev", ["tags:buildScripts", "tags:buildVendorScripts", "tags:buildLinks", "tags:buildVendorLinks"]);
    grunt.registerTask("fulldev", ["cleanall", "bowercopy:dev", "bowercopy:css", "bowercopy:fontsdev", "tagsdev"]);
    grunt.registerTask("default", ["fulldev"]);

    // release: prepare

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

    // release: build


    grunt.registerTask("vs2015", "builds the app for devices", function (env) {
        env = env || "debug";
        switch (env) {
            case "release":
                return grunt.task.run(["preparerelease", "build:" + env]);
            case "debug":
                return grunt.task.run(["fulldev", "build:" + env]);
            default:
                throw new Error("Unsupported build environment: " + env);
        }
    });

    grunt.registerTask("build", function (env) {
        var cordovaBuild = require("taco-team-build"),
        done = this.async();

        //var platformsToBuild = process.platform == "darwin" ? ["ios"] : ["android", "windows", "wp8"*/], // Darwin == OSX
        var cliEnv = "--" + env;

        var platformsToBuild = process.platform === "darwin" ? ["ios"] : ["android"];
        var buildArgs = {
            android: [cliEnv],    // Warning: Omit the extra "--" when referencing platform
            ios: [cliEnv, "--device"],     // specific preferences like "-- --ant" for Android
            windows: [cliEnv],             // or "-- --win" for Windows. You may also encounter a
            wp8: [cliEnv]                  // "TypeError" after adding a flag Android doesn"t recognize
        };                                      // when using Cordova < 4.3.0. This is fixed in 4.3.0.

        cordovaBuild.buildProject(platformsToBuild, buildArgs)
            .then(function () {
                grunt.task.run(["copy:build"]);
                return cordovaBuild.packageProject(platformsToBuild);
            })
            .done(done);
    });


};