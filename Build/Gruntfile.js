module.exports = function (grunt) {

    var webProjectRelPath = "../softWrench.sW4.Web/";
    var path = grunt.option("path") || "";
    var fullPath = !!path ? path + "/" : webProjectRelPath;
    var customer = grunt.option("customer");
    var skipTest = grunt.option("skiptest");

    //Resolving presets on the run, so that we do not need to install node-modules on web-project
    var babelCore = require('babel-core');
    var optManager = new babelCore.OptionManager();
    var resolvedPresets = optManager.resolvePresets(['latest']);
    var node_modulesPath = __dirname + "\\node_modules";

    global.last = function (arr) {
        return arr[arr.length - 1];
    }

    grunt.initConfig({



        //#region global app config
        app: {
            content: fullPath + "Content",
            scripts: fullPath + "Content/Scripts",
            tparty: fullPath + "Content/Scripts/thirdparty",
            vendor: fullPath + "Content/vendor",
            defaultstyles: fullPath + "Content/styles/default",
            shared: fullPath + "Content/Shared/",
            customVendor: fullPath + "Content/customVendor",
            customers: fullPath + "Content/Customers",
            tests: /*fullPath +*/ "../softwrench.sw4.jstest",
            webcommons: fullPath + "Content/Shared/webcommons",
            tmp: fullPath + "Content/temp",
            dist: fullPath + "Content/dist"
        },
        //#endregion


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

        //#region ng-annotate
        ngAnnotate: {
            options: {
                separator: ";\n"
            },
            app: {
                files: [{
                    src: [
                        "<%= app.scripts %>/client/aaa_layout.js", // sw
                           // customized angular vendor modules
                      "<%= app.scripts %>/signin.js",
                      "<%= app.scripts %>/client/*!(aaa_layout).js",
                      "<%= app.scripts %>/client/services/*.js",
                      "<%= app.scripts %>/client/directives/*.js",
                      "<%= app.scripts %>/client/components/*.js",
                      "<%= app.scripts %>/client/util/*.js"
                    ],

                    dest: "<%= app.tmp %>/scripts/app.annotated.js"
                }]
            }
        },
        //#endregion


        //#region concat
        concat: {

            vendorScripts: {
                options: {
                    separator: ";\n",
                },
                src: [
                    // jquery
                    "<%= app.scripts %>/jquery/jquery-2.0.3-max.js",
                    "<%= app.scripts %>/jquery/jquery-ui-1.10.3.js",
                    "<%= app.scripts %>/jquery/jquery-file-style.js",
                    "<%= app.scripts %>/jquery/jquery-filedownload-1.2.0.js",
                    "<%= app.scripts %>/jquery/jquery-fileupload-5.40.1.js",


                    // utils
                    "<%= app.scripts %>/spin-min.js",
                    "<%= app.tparty %>/moment.js",
                    "<%= app.scripts %>/jquery.unobtrusive-ajax.js",
                    "<%= app.tparty %>/jscrollpane/*.js",

                    //angular
                    "<%= app.scripts %>/angular/angular.js",
                    "<%= app.scripts %>/angular/angular-strap.js",
                    "<%= app.scripts %>/angular/angular-sanitize.js",
                    "<%= app.scripts %>/angular/bindonce.js",

                    // bootstrap
                    "<%= app.scripts %>/bootstrap.max.js",
                    "<%= app.scripts %>/bootstrap-datepicker.js",
                    "<%= app.scripts %>/bootstrap-combobox.js",
                    "<%= app.scripts %>/bootstrap-datetimepicker.js",
                    "<%= app.scripts %>/bootstrap-collapse.js",
                    "<%= app.scripts %>/bootbox.js",
                    "<%= app.scripts %>/typeahead.js",
                    "<%= app.scripts %>/hogan.js",
                    "<%= app.scripts %>/locales/bootstrap-datepicker.de.js",
                    "<%= app.scripts %>/locales/bootstrap-datetimepicker.de.js",
                    "<%= app.scripts %>/locales/bootstrap-datepicker.es.js",
                    "<%= app.scripts %>/locales/bootstrap-datetimepicker.es.js",
                    "<%= app.scripts %>/modal.js",
                    "<%= app.scripts %>/bootstrap-multiselect.js",

                    "<%= app.scripts %>/ace/ace.js",
                    "<%= app.scripts %>/angulartreeview.js",
                    "<%= app.scripts %>/client/responsive.js"

                ],
                dest: "<%= app.tmp %>/scripts/vendor.concat.js"
            },

            appScripts: {
                options: {
                    separator: ";\n"
                },
                src: [
                       "<%= app.scripts %>/client/aaa_layout.js", // sw

                        // customized angular vendor modules
                      "<%= app.scripts %>/signin.js",
                      "<%= app.scripts %>/client/*!(aaa_layout).js",
                      "<%= app.scripts %>/client/services/*.js",
                      "<%= app.scripts %>/client/directives/*.js",
                      "<%= app.scripts %>/client/components/*.js",
                      "<%= app.scripts %>/client/util/*.js"
                ],

                dest: "<%= app.tmp %>/scripts/app.concat.js"
            }
        },
        //#endregion

        //#region uglify js
        uglify: {
            options: {
                mangle: {
                    except: [
                        "jQuery", "angular", "tableau", "LZString", "moment", "Moment", "Modernizr",
                        "app", "modules", "tinyMCE", "tinymce", "Prism"
                    ]
                }
            },


            vendors: {
                files: [{
                    src: ["<%= app.tmp %>/scripts/vendor.concat.js"],
                    dest: "<%= app.dist %>/scripts/vendor.js"
                }]
            },

            app: {
                files: [{
                    src: ["<%= app.tmp %>/scripts/app.annotated.js"],
                    dest: "<%= app.dist %>/scripts/app.js"
                }]
            }
        },
        //#endregion


    });

    //#region load npm tasks
    grunt.loadNpmTasks("grunt-sass");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-ng-annotate");
    grunt.loadNpmTasks("grunt-contrib-concat");
    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks('grunt-uncss');
    grunt.loadNpmTasks("grunt-contrib-cssmin");
    grunt.loadNpmTasks('grunt-angular-templates');
    grunt.loadNpmTasks("grunt-karma");
    require("load-grunt-tasks")(grunt);

    //#endregion

    //#region customTasks
    grunt.registerTask("cleanAll", ["clean:vendor", "clean:tmp", "clean:dist"]);

    var defaultTasks = [
        "cleanAll", // clean folders: preparing for copy
        "concat:vendorScripts", // concat vendors's scripts and distribute as 'scripts/vendor.js'
        "ngAnnotate:app", // ng-annotates app's scripts
        "concat:appScripts", // concat app's (customized from vendor's + ng-annotated + customer's)
        "uglify:app", // minify app script and distribute as 'scripts/app.js'
        "uglify:vendors" // minify app script and distribute as 'scripts/app.js'
        // "clean:vendor", "clean:tmp" // clean temporary folders 
    ];
    //    defaultTasks.push("clean:vendor"); // clean temporary folders
    //    defaultTasks.push("clean:tmp");

    grunt.registerTask("default", defaultTasks);




    //#endregion

};