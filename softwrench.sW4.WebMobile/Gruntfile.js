/// <binding ProjectOpened='watch:sass, watch:scripts, default' />
module.exports = function (grunt) {

    // Project configuration.

    //#region Scripts

    /** downloaded and customized or not distributed by bower */
    var customVendorScripts = [
        "www/Content/Vendor/downloadedvendor/persistence.js",
        "www/Content/Vendor/downloadedvendor/persistence.store.sql.js",
        "www/Content/Vendor/downloadedvendor/persistence.store.cordovasql.js",
        "www/Content/Vendor/downloadedvendor/persistence.migrations.js",
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
        "www/Content/Mobile/scripts/persistence/config.migrations.js",
        // audit.offline
        "www/Content/Shared/audit_offline/scripts/offline/audit.js"
    ];

    /** localbuild script to indicate that the app is running on local debug mode */
    var localDebugScript = [
        "www/Content/Mobile/scripts/localdev_script.js"
    ];

    /** reusable online & offline lib scripts (ours)  */
    var sharedScripts = [
        "www/Content/Shared/webcommons/scripts/softwrench/sharedservices_module.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/applications/statuscolor_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/constants/JavascriptEventConstants.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/aa_stringutils.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/aa_utils.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/aa_arrayutils.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/object_util.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/cycle.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/data/context_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/crud/tabs_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/crud/tabs_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/i18n_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/crud/schema_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/crud/field_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/validation/passwordvalidationservice.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/validation/validation_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/layout/alert_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/data/format_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/event_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/rest_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/expression_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/crud/dispatcher_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/scannerCommons.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/applications/person/user_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/crud/composition_commons.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/storage/compressionservice.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/storage/localstorageservice.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/storage/dynamicScriptsCacheService.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/storage/schemaCacheService.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/data/datamapSanitize_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/crud/notificationService.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/application/physicalinventory_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/crud/commandcommons.js",
        "www/Content/Shared/webcommons/scripts/softwrench/components/richtext.js",
        "www/Content/Shared/webcommons/scripts/softwrench/directive/floatconverter.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/angular/log_enhacer.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/angular/scope_enhacer.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/angular/lazy_service_provider.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/angular/clientawareserviceprovider.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/applications/inventory/inventory_service_shared.js"
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
        "www/Content/Mobile/scripts/decorators/**/*.js",
        "www/Content/Mobile/scripts/migrations/**/*.js"
    ];

    //TODO: make a client-based build??
    //make it download the customer scripts from the server at runtime?
    var customerScripts = [];
    var customer = grunt.option("customer") || "pae"; // -> harcoding the only customer that has custom scripts
    //if (customer) {
    customerScripts = [
        "www/Content/Customers/pae_offline/scripts/**/*.mobile.js",
        "www/Content/Customers/firstsolar_offline/scripts/**/*.mobile.js"
    ];
    //}

    var solutionScripts = []
        .concat(customVendorScripts)
        .concat(commonScripts)
        .concat(sharedScripts)
        .concat(appBootstrapScripts)
        .concat(appScripts)
        .concat(customerScripts);

    var solutionScriptsDev = solutionScripts
        .concat(localDebugScript);


    var vendorScripts = [
        // complete paths to guarantee load order (instead of **/*.js)
        "www/Content/Vendor/scripts/polyfill.js",
        "www/Content/Vendor/scripts/angular.js",
        "www/Content/Vendor/scripts/angular-ui-router.js",
        "www/Content/Vendor/scripts/angular-sanitize.js",
        "www/Content/Vendor/scripts/angular-animate.js",

        "www/Content/Vendor/scripts/ionic.min.js",
        "www/Content/Vendor/scripts/ionic-angular.min.js",

        "www/Content/Vendor/scripts/ng-material-floating-button.js",
        "www/Content/Vendor/scripts/ng-material-floating-button-directive.js",

        "www/Content/Vendor/scripts/tinymce.js",
        "www/Content/Vendor/scripts/themes/modern/theme.js",
        "www/Content/Vendor/scripts/angular-ui-tinymce.js",

        "www/Content/Vendor/scripts/jquery.js",
        "www/Content/Vendor/scripts/ng-cordova.js",
        "www/Content/Vendor/scripts/moment.js",
        "www/Content/Vendor/scripts/underscore.js",
        "www/Content/Vendor/scripts/persistence.store.websql.js",
        "www/Content/Vendor/scripts/lz-string.js",
        "www/Content/Vendor/scripts/ngTouch.js"
    ];


    var testScripts = [
        "tests/**/*.js"
    ];

    var ngMockScript = ["bower_components/angular-mocks/angular-mocks.js"];

    var allScripts = []
        .concat(vendorScripts).concat(ngMockScript)
        .concat(solutionScripts)
        .concat(testScripts);

    function getKarmaPreprocessorsConfig(scripts) {
        var preprocessors = {};
        scripts.forEach(function (s) {
            preprocessors[s] = ["babel"];
        });
        return preprocessors;
    }

    var currentPlatform = grunt.option("platform") || "android";

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
                    // es6 polyfills
                    "polyfill.js": "babel-polyfill/browser-polyfill.js",
                    // angular
                    "angular-sanitize.js": "angular-sanitize/angular-sanitize.js",
                    "angular-ui-router.js": "../node_modules/angular-ui-router/release/angular-ui-router.js",
                    "angular-animate.js": "angular-animate/angular-animate.js",
                    "angular.js": "angular/angular.js",
                    // ionic
                    "ionic.min.js": "ionic/release/js/ionic.js",
                    "ionic-angular.min.js": "ionic/release/js/ionic-angular.js",
                    // fab
                    "ng-material-floating-button.js": "ng-material-floating-button/mfb/dist/mfb.js",
                    "ng-material-floating-button-directive.js": "ng-material-floating-button/src/mfb-directive.js",
                    // tinymce
                    "tinymce.js": "tinymce-dist/tinymce.min.js",
                    "angular-ui-tinymce.js": "angular-ui-tinymce/dist/tinymce.min.js",
                    "themes/modern/theme.js": "tinymce-dist/themes/modern/theme.min.js",

                    // utils
                    "jquery.js": "jquery/dist/jquery.js",
                    "ng-cordova.js": "ngCordova/dist/ng-cordova.js",
                    "persistence.store.websql.js": "persistence/lib/persistence.store.websql.js",
                    "moment.js": "moment/moment.js",
                    "underscore.js": "underscore/underscore.js",
                    "lz-string.js": "lz-string/libs/lz-string.js",
                    "ngTouch.js": "ngtouch/src/ngTouch.js"
                }
            },
            css: {
                options: {
                    destPrefix: "www/Content/Vendor/css"
                },
                files: {
                    "ionic.min.css": "ionic/release/css/ionic.min.css",
                    "ionautocomplete.min.css": "ion-autocomplete/dist/ion-autocomplete.min.css",

                    "font-awesome.min.css": "font-awesome/css/font-awesome.min.css",

                    "ng-material-floating-button.css": "ng-material-floating-button/mfb/dist/mfb.min.css",

                    "skins/lightgray/skin.min.css": "tinymce-dist/skins/lightgray/skin.min.css",
                    "skins/lightgray/content.min.css": "tinymce-dist/skins/lightgray/content.min.css"
                }
            },
            fontsdev: {
                options: {
                    destPrefix: "www/Content/Vendor/fonts"
                },
                files: {
                    "fontawesome-webfont.eot": "font-awesome/fonts/fontawesome-webfont.eot",
                    "fontawesome-webfont.svg": "font-awesome/fonts/fontawesome-webfont.svg",
                    "fontawesome-webfont.ttf": "font-awesome/fonts/fontawesome-webfont.ttf",
                    "fontawesome-webfont.woff": "font-awesome/fonts/fontawesome-webfont.woff",

                    "ionicons.eot": "ionic/release/fonts/ionicons.eot",
                    "ionicons.svg": "ionic/release/fonts/ionicons.svg",
                    "ionicons.ttf": "ionic/release/fonts/ionicons.ttf",
                    "ionicons.woff": "ionic/release/fonts/ionicons.woff",

                    "tinymce.eot": "tinymce-dist/skins/lightgray/fonts/tinymce.eot",
                    "tinymce.svg": "tinymce-dist/skins/lightgray/fonts/tinymce.svg",
                    "tinymce.ttf": "tinymce-dist/skins/lightgray/fonts/tinymce.ttf",
                    "tinymce.woff": "tinymce-dist/skins/lightgray/fonts/tinymce.woff",
                    "tinymce-small.eot": "tinymce-dist/skins/lightgray/fonts/tinymce-small.eot",
                    "tinymce-small.svg": "tinymce-dist/skins/lightgray/fonts/tinymce-small.svg",
                    "tinymce-small.ttf": "tinymce-dist/skins/lightgray/fonts/tinymce-small.ttf",
                    "tinymce-small.woff": "tinymce-dist/skins/lightgray/fonts/tinymce-small.woff",
                }
            },
            fontsrelease: {
                options: {
                    destPrefix: "www/Content/public/fonts"
                },
                files: {
                    "fontawesome-webfont.eot": "font-awesome/fonts/fontawesome-webfont.eot",
                    "fontawesome-webfont.svg": "font-awesome/fonts/fontawesome-webfont.svg",
                    "fontawesome-webfont.ttf": "font-awesome/fonts/fontawesome-webfont.ttf",
                    "fontawesome-webfont.woff": "font-awesome/fonts/fontawesome-webfont.woff",

                    "ionicons.eot": "ionic/release/fonts/ionicons.eot",
                    "ionicons.svg": "ionic/release/fonts/ionicons.svg",
                    "ionicons.ttf": "ionic/release/fonts/ionicons.ttf",
                    "ionicons.woff": "ionic/release/fonts/ionicons.woff",

                    "tinymce.eot": "tinymce-dist/skins/lightgray/fonts/tinymce.eot",
                    "tinymce.svg": "tinymce-dist/skins/lightgray/fonts/tinymce.svg",
                    "tinymce.ttf": "tinymce-dist/skins/lightgray/fonts/tinymce.ttf",
                    "tinymce.woff": "tinymce-dist/skins/lightgray/fonts/tinymce.woff",
                    "tinymce-small.eot": "tinymce-dist/skins/lightgray/fonts/tinymce-small.eot",
                    "tinymce-small.svg": "tinymce-dist/skins/lightgray/fonts/tinymce-small.svg",
                    "tinymce-small.ttf": "tinymce-dist/skins/lightgray/fonts/tinymce-small.ttf",
                    "tinymce-small.woff": "tinymce-dist/skins/lightgray/fonts/tinymce-small.woff",
                }
            },
            prod: {
                options: {
                    destPrefix: "www/Content/Vendor/scripts"
                },
                files: {
                    // es6 polyfills
                    "polyfill.js": "babel-polyfill/browser-polyfill.js",
                    // angular
                    "angular.js": "angular/angular.min.js",
                    "angular-sanitize.js": "angular-sanitize/angular-sanitize.min.js",
                    "angular-ui-router.js": "../node_modules/angular-ui-router/release/angular-ui-router.min.js",
                    "angular-animate.js": "angular-animate/angular-animate.min.js",
                    // ionic
                    "ionic.min.js": "ionic/release/js/ionic.min.js",
                    "ionic-angular.min.js": "ionic/release/js/ionic-angular.min.js",
                    // fab
                    "ng-material-floating-button.js": "ng-material-floating-button/mfb/dist/mfb.min.js",
                    "ng-material-floating-button-directive.js": "ng-material-floating-button/src/mfb-directive.js",
                    // tinymce
                    "tinymce.js": "tinymce-dist/tinymce.min.js",
                    "angular-ui-tinymce.js": "angular-ui-tinymce/dist/tinymce.min.js",
                    "themes/modern/theme.js": "tinymce-dist/themes/modern/theme.min.js",
                    // utils
                    "jquery.js": "jquery/dist/jquery.min.js",
                    "ng-cordova.js": "ngCordova/dist/ng-cordova.min.js",
                    "persistence.store.websql.js": "persistence/lib/persistence.store.websql.js",
                    "moment.js": "moment/min/moment.min.js",
                    "underscore.js": "underscore/underscore-min.js",
                    "lz-string.js": "lz-string/libs/lz-string.min.js",
                    "ngTouch.js": "ngtouch/build/ngTouch.min.js"
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
                src: solutionScriptsDev,
                dest: "<%= app.index %>"
            },
            buildTranspiledScripts: {
                options: {
                    openTag: "<!-- start auto template script tags, grunt will generate it for dev environment, do not remove this -->",
                    closeTag: "<!-- end auto template script tags -->"
                },
                src: solutionScriptsDev.map(function (s) { return "www/Content/public/" + s; }),
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
                    "www/css/**/*.css"
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

        //#region babel
        babel: {
            options: {
                sourceMap: false,
                presets: ["latest"],
            },
            release: { // transpiles result of concat
                files: {
                    "tmp/es6/app.es6.js": "<%= concat.appScripts.dest %>"
                }
            },
            debug: {
                options: {
                    sourceMap: "inline"
                },
                files: [{
                    expand: true,
                    src: solutionScripts,
                    dest: "www/Content/public/"
                }]
            }
        },
        //#endregion

        //#region watch
        watch: {
            sass: {
                files: [
                    "www/css/**/*.scss",
                    "www/Content/Mobile/styles/**/*.scss"
                ],
                tasks: [
                    "sass:dev"
                ]
            },
            scripts: {
                files: [
                    "www/Content/Mobile/scripts/**/*.js",
                    "www/Content/Shared/**/*.js"
                ],
                options: {
                    event: ["added", "deleted"]
                },
                tasks: [
                    "tags:buildScripts"
                ]
            }
        },
        sass: {
            dev: {
                options: {
                    sourceMap: true,
                    outputStyle: "nested"
                },
                files: [
                    { expand: true, cwd: "www/css/", dest: "www/css/", src: ["**/*.scss"], ext: ".css" }
                ]
            },
            prod: {
                options: {
                    sourceMap: false,
                    outputStyle: "compressed"
                },
                files: [
                    { expand: true, cwd: "www/css/", dest: "www/css/", src: ["**/*.scss"], ext: ".css" }
                ]
            }
        },
        //#endregion


        //#region minify javascript
        uglify: {
            options: {
                mangle: {
                    except: ["jQuery", "angular", "persistence", "constants", "ionic", "_", "LZString"]
                }
            },
            release: {
                // uglify the result of es6 transpile
                files: {
                    "www/Content/public/app.min.js": "tmp/es6/app.es6.js"
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
                    { expand: true, src: ["**/*", "!cordova.js"], dest: "platforms/" + currentPlatform, cwd: "overrides/" + currentPlatform }
                ]
            },
            customerTemplates: { // copies customer templates so they are available to the app (symlinks wont work in prod/device)
                files: [
                    { expand: true, src: ["*/templates/**/*.html"], dest: "www/Content/Customers/templates", cwd: "www/Content/Customers" }
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
            tdd: { // TDD: local dev environment
                autoWatch: true,
                singleRun: false,
                logLevel: "DEBUG",
                browsers: ["Chrome"]
            },
            dev: { // single run: local dev environment
                options: {
                    babelPreprocessor: {
                        options: {
                            presets: ["latest"],
                            sourceMap: false
                        }
                    },
                    preprocessors: getKarmaPreprocessorsConfig(solutionScripts.concat(testScripts))
                }
            },
            debug: { // CI dev
                options: {
                    babelPreprocessor: {
                        options: {
                            presets: ["latest"],
                            sourceMap: false
                        }
                    },
                    preprocessors: getKarmaPreprocessorsConfig(testScripts),
                    files: ["overrides/cordova.js"]
                        .concat(vendorScripts).concat(ngMockScript)
                        .concat(solutionScripts.map(function (s) { return "www/Content/public/" + s; }))
                        .concat(testScripts)
                }
            },
            release: { // CI release
                options: {
                    babelPreprocessor: {
                        options: {
                            presets: ["latest"],
                            sourceMap: false
                        }
                    },
                    preprocessors: getKarmaPreprocessorsConfig(testScripts),
                    files: [
                        "overrides/cordova.js",
                        "www/Content/public/vendor/vendor.min.js"
                    ].concat(ngMockScript).concat(["www/Content/public/app.min.js"]).concat(testScripts)
                }
            }
        },
        //#endregion

        //#region xmlpoke
        xmlpoke: {
            bundleid: {
                options: {
                    namespaces: {
                        "w": "http://www.w3.org/ns/widgets"
                    },
                    replacements: [{
                        xpath: "/w:widget/@id",
                        value: function (node) {
                            return currentPlatform === "ios" ? "ControlTechnologySolutions.softWrench" : "io.cordova.softwrench.sW4.WebMobile";
                        }
                    }]
                },
                files: {
                    "config.xml": "config.xml"
                }
            }
        }
        //#endregion

    });

    //#region grunt plugins
    grunt.loadNpmTasks("grunt-babel");
    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks("grunt-contrib-concat");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-contrib-watch");
    grunt.loadNpmTasks("grunt-bowercopy");
    grunt.loadNpmTasks("grunt-sass");
    grunt.loadNpmTasks("grunt-script-link-tags");
    grunt.loadNpmTasks("grunt-contrib-copy");
    grunt.loadNpmTasks("grunt-contrib-cssmin");
    grunt.loadNpmTasks("grunt-contrib-rename");
    grunt.loadNpmTasks("grunt-karma");
    grunt.loadNpmTasks("grunt-xmlpoke");
    //#endregion

    //#region dev tasks
    grunt.registerTask("cleanall", ["clean:vendor", "clean:temp", "clean:pub"]);
    grunt.registerTask("tagsdev", ["tags:buildScripts", "tags:buildVendorScripts", "tags:buildLinks", "tags:buildVendorLinks"]);
    grunt.registerTask("tagsdevbuild", ["tags:buildTranspiledScripts", "tags:buildVendorScripts", "tags:buildLinks", "tags:buildVendorLinks"]);
    grunt.registerTask("devlocal", ["cleanall", "xmlpoke:bundleid", "copy:customerTemplates", "bowercopy:dev", "bowercopy:css", "bowercopy:fontsdev", "sass:dev", "tagsdev"]);
    grunt.registerTask("devbuild", "prepares the project for a 'debug mode' build", ["cleanall", "xmlpoke:bundleid", "bowercopy:dev", "bowercopy:css", "bowercopy:fontsdev", "sass:dev", "babel:debug", "tagsdevbuild", "copy:customerTemplates", "copy:build"]);
    grunt.registerTask("default", ["devlocal"]);
    //#endregion

    //#region release:prepare tasks
    grunt.registerTask("tagsrelease", ["tags:buildReleaseScripts", "tags:buildReleaseVendorScripts", "tags:buildReleaseLinks", "tags:buildReleaseVendorLinks"]);
    grunt.registerTask("concatall", ["concat:appScripts", "concat:vendorScripts", "concat:appStyles", "concat:vendorStyles"]);
    grunt.registerTask("minify", ["uglify:release", "cssmin:release"]);

    grunt.registerTask("preparerelease", "prepares the project for release build", [
        "cleanall", // cleans destination folders
        "copy:customerTemplates", // copies customer templates inside the app from the symlinks
        "xmlpoke:bundleid", // update bundleid according to the platform
        "bowercopy:prod", "bowercopy:css", "bowercopy:fontsrelease", // copy bower dependencies to appropriate project folders
        "concatall", // concats the scripts and stylesheets
        "sass:prod", // compiles sass files
        "babel:release", // transpiles es6 app scripts
        "minify", // uglyfies scripts and minifies stylesheets
        "tagsrelease", // generates import tags for the prepared files in main template file (layout.html)
        "clean:temp"
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
                var platformPluginJsonFile = path.join(projectPath, "plugins", platform.trim() + ".json");
                if (fs.existsSync(platformPluginJsonFile)) {
                    console.log(platform + ".json file found at \"" + platformPluginJsonFile + "\". Removing to ensure plugins install properly in newly added platform.")
                    fs.unlinkSync(platformPluginJsonFile);
                }
            }).then(function () {
                // Now add the platform
                return cordova.raw.platform('add', platform);
            });

        });
        return promise;
    }

    // ripped and modified from taco-team-build.js
    function buildProject(cordovaPlatforms, args) {
        if (typeof (cordovaPlatforms) == "string") {
            cordovaPlatforms = [cordovaPlatforms];
        }
        return taco.setupCordova().then(function (cordova) {
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
            .then(function () {
                return done();
            })
            .catch(function (e) {
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
                return grunt.task.run(["devbuild", "karma:" + env, "build:" + env]);
            default:
                throw new Error("Unsupported build environment: " + env);
        }
    });

    //#endregion
};
