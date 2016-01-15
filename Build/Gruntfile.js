module.exports = function (grunt) {

    var webProjectRelPath = "../softWrench.sW4.Web/";
    var path = grunt.option("path") || "";
    var fullPath = !!path ? path + "/" : webProjectRelPath;
    var customer = grunt.option("customer");

    grunt.initConfig({
        //#region global app config
        app: {
            content: fullPath + "Content",
            vendor: fullPath + "Content/vendor",
            customVendor: fullPath + "Content/customVendor",
            customers: fullPath + "Content/Customers",
            tests: /*fullPath +*/ "../softwrench.sw4.jstest",
            webcommons: fullPath + "Content/Shared/webcommons",
            tmp: fullPath + "Content/temp",
            dist: fullPath + "Content/dist"
        },
        //#endregion

        //#region sass
        sass: {
            prod: {
                options: {
                    sourceMap: false,
                    outputStyle: "compressed"
                },
                files: [
                    {
                        expand: true,
                        cwd: fullPath + "Content/Customers/",
                        dest: fullPath + "Content/Customers/",
                        src: ["**/*.scss"],
                        ext: ".css"
                    },
                    {
                        expand: true,
                        cwd: fullPath + "Content/Shared/",
                        dest: fullPath + "Content/Shared/",
                        src: ["**/*.scss"],
                        ext: ".css"
                    },
                    {
                        expand: true,
                        cwd: fullPath + "Content/styles/",
                        dest: fullPath + "Content/styles/",
                        src: ["**/*.scss"],
                        ext: ".css"
                    }
                ]
            }
        },
        //#endregion

        //#region clean
        clean: {
            options: {
                force: true // required to clean outside current folder
            },
            vendor: [
                "<%= app.vendor %>/css/*",
                "<%= app.vendor %>/scripts/*",
                "<%= app.vendor %>/fonts/*"
            ],
            tmp: [
                "<%= app.tmp %>"
            ],
            dist: [
                "<%= app.dist %>"
            ]
        },
        //#endregion

        //#region bowercopy
        bowercopy: {
            css: {
                options: {
                    destPrefix: "<%= app.vendor %>/css"
                },
                files: {
                    // bootstrap
                    "bootstrap.css": "bootstrap/dist/css/bootstrap.min.css",
                    "bootstrap-theme.css": "bootstrap/dist/css/bootstrap-theme.min.css",
                    "bootstrap-datetimepicker.css": "eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.min.css",
                    "selectize.css": "selectize/dist/css/selectize.bootstrap3.css",
                    // font-awesome
                    "font-awesome.css": "font-awesome/css/font-awesome.min.css",
                    // angular
                    "textAngular.css": "textAngular/dist/textAngular.css",
                    "angular-ui-select.css": "ui-select/dist/select.min.css",
                }
            },
            fonts: {
                options: {
                    destPrefix: "<%= app.dist %>"
                },
                files: {
                    "fonts": [
                        "font-awesome/fonts/*",
                        "bootstrap/dist/fonts/*"
                    ]
                }
            },
            scripts: {
                options: {
                    destPrefix: "<%= app.vendor %>/scripts"
                },
                files: {
                    // jquery
                    "jquery.js": "jquery/dist/jquery.min.js",
                    "jquery-ui.js": "jquery-ui/ui/minified/jquery-ui.min.js",
                    // angular
                    "angular.js": "angular/angular.min.js",
                    "angular-sanitize.js": "angular-sanitize/angular-sanitize.min.js",
                    "angular-strap.js": "angular-strap/dist/angular-strap.min.js",
                    "angular-bindonce.js": "angular-bindonce/bindonce.min.js",
                    "angular-animate.js": "angular-animate/angular-animate.min.js",
                    "angular-xeditable.js": "angular-xeditable/dist/js/xeditable.min.js",
                    "angular-file-upload.js": "angular-file-upload/angular-file-upload.min.js",
                    // bootstrap
                    "bootstrap.js": "bootstrap/dist/js/bootstrap.min.js",
                    "bootstrap-datetimepicker.js": "eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js",
                    // utils
                    "moment.js": "moment/min/moment.min.js",
                    "spin.js": "spin.js/spin.min.js",
                    "lz-string.js": "lz-string/libs/lz-string.min.js",
                    // unminified vendors
                    "raw/jquery-file-style.js": "jquery.filestyle/jquery.filestyle.js",
                    "raw/jquery-file-download.js": "jquery-file-download/src/Scripts/jquery.fileDownload.js",
                    "raw/jquery-file-upload.js": "blueimp-file-upload/js/jquery.fileupload.js",
                    "raw/bootstrap-multiselect.js": "bootstrap-multiselect/dist/js/bootstrap-multiselect.js",
                    "raw/moment-locale-de.js": "moment/locale/de.js",
                    "raw/moment-locale-es.js": "moment/locale/es.js"
                }
            }
        },
        //#endregion

        //#region ng-annotate
        ngAnnotate: {
            options: {
                separator: ";\n"
            },
            app: {
                files: [{
                    src: [
                        // customized angular vendor modules
                        "<%= app.customVendor %>/scripts/angular/**/*.js",
                        // modules
                        "<%= app.webcommons %>/scripts/softwrench/sharedservices_module.js", // webcommons
                        "<%= app.content %>/Scripts/client/crud/aaa_layout.js", // sw
                        // webcommons
                        "<%= app.webcommons %>/scripts/**/*!(sharedservices_module).js",
                        // app
                        "<%= app.content %>/Scripts/client/crud/**/*!(aaa_layout).js",
                        "<%= app.content %>/Scripts/client/services/*.js",
                        "<%= app.content %>/Scripts/client/*.js",
                        "<%= app.content %>/Scripts/client/adminresources/*.js",
                        "<%= app.content %>/Scripts/client/directives/*.js",
                        "<%= app.content %>/Scripts/client/directives/menu/*.js",
                        "<%= app.content %>/Templates/commands/**/*.js",
                        "<%= app.content %>/modules/**/*.js",
                        // Shared
                        "<%= app.content %>/Shared/{**/*.js, !(webcommons)/**/*.js}",
                        // base otb
                        "<%= app.content %>/Scripts/customers/otb/*.js",
                        // customers shared
                        "<%= app.content %>/Scripts/customers/shared/*.js"
                    ].concat(!customer ? [] : ["<%= app.customers %>/" + customer + "/scripts/**/*.js"]), // scpecific customer

                    dest: "<%= app.tmp %>/scripts/app.annotated.js"
                }]
            }
        },
        //#endregion

        //#region concat
        concat: {
            vendorStyles: {
                src: [
                    "<%= bowercopy.css.options.destPrefix %>/*.css",
                    // TODO: have customVendor be a part of the app's css instead of the vendor's css
                    "<%= app.tmp %>/css/customVendor.min.css"
                ],
                dest: "<%= app.dist %>/css/vendor.css"
            },
            vendorScripts: {
                options: {
                    separator: ";\n",
                },
                src: [
                     // utils
                    "<%= bowercopy.scripts.options.destPrefix %>/moment.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/spin.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/lz-string.js",
                    // jquery
                    "<%= bowercopy.scripts.options.destPrefix %>/jquery.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/jquery-ui.js",
                    // bootstrap
                    "<%= bowercopy.scripts.options.destPrefix %>/bootstrap.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/bootstrap-datetimepicker.js",
                    // angular
                    "<%= bowercopy.scripts.options.destPrefix %>/angular.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/angular-sanitize.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/angular-strap.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/angular-bindonce.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/angular-animate.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/angular-xeditable.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/angular-file-upload.js",
                    // minified raw vendors
                    "<%= app.tmp %>/scripts/rawVendor.min.js"
                ],
                dest: "<%= app.dist %>/scripts/vendor.js"
            },
            appScripts: {
                options: {
                    separator: ";\n"
                },
                src: [
                    // customVendors (except angular)
                    "<%= app.customVendor %>/scripts/{**/*.js, !(angular)/**/*.js}",
                    // actual app: ng-annotated source
                    "<%= app.tmp %>/scripts/app.annotated.js"
                ],

                dest: "<%= app.tmp %>/scripts/app.concat.js"
            }
        },
        //#endregion

        //#region minify css
        cssmin: {
            customVendor: {
                files: [{
                    src: ["<%= app.customVendor %>/css/*.css"],
                    dest: "<%= app.tmp %>/css/customVendor.min.css",
                    ext: ".min.css"
                }]
            }
        },
        //#endregion

        //#region uglify js
        uglify: {
            options: {
                mangle: {
                    except: [
                        "jQuery", "angular", "tableau", "LZString", "moment", "Moment", "Modernizr",
                        "app", "modules"
                    ]
                }
            },
            rawVendors: {
                files: [{
                    src: [
                        "<%= bowercopy.scripts.options.destPrefix %>/raw/moment-locale-de.js",
                        "<%= bowercopy.scripts.options.destPrefix %>/raw/moment-locale-es.js",
                        "<%= bowercopy.scripts.options.destPrefix %>/raw/jquery-file-style.js",
                        "<%= bowercopy.scripts.options.destPrefix %>/raw/jquery-file-download.js",
                        "<%= bowercopy.scripts.options.destPrefix %>/raw/jquery-file-upload.js",
                        "<%= bowercopy.scripts.options.destPrefix %>/raw/bootstrap-multiselect.js",
                    ],
                    dest: "<%= app.tmp %>/scripts/rawVendor.min.js"
                }]
            },
            app: {
                files: [{
                    src: ["<%= concat.appScripts.dest %>"],
                    dest: "<%= app.dist %>/scripts/app.js"
                }]
            }
        },
        //#endregion

        //#region karma
        karma: {
            options: {
                configFile: "<%= app.tests %>/karma.conf.js",
                logLevel: "WARN",
                browsers: ["PhantomJS"],
                singleRun: true,
                basePath: "",
                // preprocess matching files before serving them to the browser
                // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
                preprocessors: {
                    '../softWrench.sW4.Web/Content/Templates/**/*.html': ["ng-html2js"]
                },
                ngHtml2JsPreprocessor: {
                    // If your build process changes the path to your TEMPLATES,
                    // use stripPrefix and prependPrefix to adjust it.
                    stripPrefix: "(.*)softWrench.sW4.Web",
                    // the name of the Angular module to create
                    moduleName: "sw.templates"
                },
                files: [
                    "<%= app.dist %>/scripts/vendor.js",
                    "<%= app.dist %>/scripts/app.js",
                    "../softWrench.sW4.Web/Content/Templates/**/*.html",
                    "<%= app.tests %>/angular_mock.js",
                    "<%= app.tests %>/tests/**/*.js"
                ]
            },
            target: {}
        }
        //#endregion

    });

    //#region load npm tasks
    grunt.loadNpmTasks("grunt-sass");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-bowercopy");
    grunt.loadNpmTasks("grunt-ng-annotate");
    grunt.loadNpmTasks("grunt-contrib-concat");
    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks("grunt-contrib-cssmin");
    grunt.loadNpmTasks("grunt-karma");
    //#endregion

    //#region customTasks
    grunt.registerTask("cleanAll", ["clean:vendor", "clean:tmp", "clean:dist"]);
    grunt.registerTask("copyAll", ["bowercopy:css", "bowercopy:fonts", "bowercopy:scripts"]);
    grunt.registerTask("default", [
        "sass:prod", // compile scss sources
        "cleanAll", // clean folders: preparing for copy
        "copyAll", // copying bower files
        "cssmin:customVendor", // minify and concat 'customized from vendor' css
        "concat:vendorStyles", // concat vendors's css + minified 'customized from vendor' and distribute as 'css/vendor.css'
        "uglify:rawVendors", // minifies unminified vendors
        "concat:vendorScripts", // concat vendors's scripts and distribute as 'scripts/vendor.js'
        "ngAnnotate:app", // ng-annotates app's scripts
        "concat:appScripts", // concat app's customized from vendor's + ng-annotated + customer's)
        "uglify:app", // minify app script and distribute as 'scripts/app.js'
        "karma:target", // run tests on minified scripts
        "clean:vendor", "clean:tmp" // clean temporary folders
    ]);
    //#endregion

};