module.exports = function (grunt) {

    var webProjectRelPath = "../softWrench.sW4.Web/";
    var path = grunt.option("path") || "";
    var fullPath = !!path ? path + "/" : webProjectRelPath;
    var customer = grunt.option("customer");

    var scssFilesToCompile = [
        {
            expand: true,
            cwd: "Content/Customers/",
            dest: "Content/Customers/",
            src: ["**/*.scss"],
            ext: ".css"
        },
        {
            expand: true,
            cwd: "Content/Shared/",
            dest: "Content/Shared/",
            src: ["**/*.scss"],
            ext: ".css"
        },
        {
            expand: true,
            cwd: "Content/styles/",
            dest: "Content/styles/",
            src: ["**/*.scss"],
            ext: ".css"
        }
    ].map(function (file) {
        file.cwd = (fullPath) + file.cwd;
        file.dest = (fullPath) + file.dest;
        return file;
    });

    grunt.initConfig({
        //#region global app config
        app: {
            content: fullPath + "Content",
            vendor: fullPath + "Content/vendor",
            customVendor: fullPath + "Content/customVendor",
            customers: fullPath + "Content/Customers",

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
                files: scssFilesToCompile
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
                    "bootstrap/bootstrap.css": "bootstrap/dist/css/bootstrap.min.css",
                    "bootstrap/bootstrap-theme.css": "bootstrap/dist/css/bootstrap-theme.min.css",
                    "bootstrap/bootstrap-datetimepicker.css": "eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.min.css",
                    "bootstrap/selectize.css": "selectize/dist/css/selectize.bootstrap3.css",
                    // font-awesome
                    "font-awesome/font-awesome.css": "font-awesome/css/font-awesome.min.css",
                    // angular
                    "angular/textAngular.css": "textAngular/dist/textAngular.css",
                    "angular/angular-ui-select.css": "ui-select/dist/select.min.css",
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
                    // clean: true,
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
                    "raw/bootstrap-combobox.js": "bootstrap-combobox/js/bootstrap-combobox.js",
                    "raw/bootstrap-multiselect.js": "bootstrap-multiselect/dist/js/bootstrap-multiselect.js",
                    "raw/bootbox.js": "bootbox.js/bootbox.js",
                    "raw/moment-locale-de.js": "moment/locale/de.js",
                    "raw/moment-locale-es.js": "moment/locale/es.js",
                }
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
                    stripBanners: {
                        block: true,
                        line: true
                    }
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
                    "<%= bowercopy.scripts.options.destPrefix %>/angular-file-upload.js"

                ],
                dest: "<%= app.dist %>/scripts/vendor.js"
            },
            appScripts: {
                options: {
                    separator: ";\n",
                    stripBanners: {
                        block: true,
                        line: true
                    }
                },
                src: [
                    // unminified 
                    // TODO: have unminified vendors be a part of vendor's instead of app's script
                    "<%= bowercopy.scripts.options.destPrefix %>/rawmoment-locale-de.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/rawmoment-locale-es.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/rawjquery-file-style.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/rawjquery-file-download.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/rawjquery-file-upload.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/rawbootstrap-combobox.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/rawbootstrap-multiselect.js",
                    "<%= bowercopy.scripts.options.destPrefix %>/rawbootbox.js",
                    // customVendors
                    "<%= app.customVendor %>/scripts/**/*.js",
                    // actual app
                    "<%= app.content %>/Scripts/client/crud/**/*.js",
                    "<%= app.content %>/Scripts/client/services/*.js",
                    "<%= app.content %>/Scripts/client/client/*.js",
                    "<%= app.content %>/Scripts/client/client/adminresources/*.js",
                    "<%= app.content %>/Scripts/client/client/directives/*.js",
                    "<%= app.content %>/Scripts/client/client/directives/menu/*.js",
                    "<%= app.content %>/Templates/commands/**/*.js",
                    "<%= app.content %>/modules/**/*.js"
                    // customer
                ].concat(!customer ? [] : ["<%= app.customers %>/" + customer + "/scripts/**/*.js"]),

                dest: "<%= app.tmp %>/scripts/app.concat.js"
            }
        },
        //#endregion

        //#region minify css
        cssmin: {
            customVendor: {
                files: [{
                    //expand: false,
                    //cwd: "<%= app.customVendor %>/css",
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
                    except: ["jQuery", "angular", "tableau"]
                },
                screwIE8: true
            },
            app: {
                files: [{
                    //expand: false,
                    //cwd: "<%= app.tmp %>/scripts",
                    src: ["<%= concat.appScripts.dest %>"],
                    dest: "<%= app.dist %>/scripts/app.js"
                }]
            }
        },
        //#endregion

    });

    //#region load npm tasks
    grunt.loadNpmTasks("grunt-sass");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-bowercopy");
    grunt.loadNpmTasks("grunt-contrib-concat");
    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks("grunt-contrib-cssmin");
    grunt.loadNpmTasks("grunt-karma");
    //#endregion

    //#region customTasks
    grunt.registerTask("cleanAll", ["clean:vendor", "clean:tmp", "clean:dist"]);
    grunt.registerTask("copyAll", ["bowercopy:css", "bowercopy:fonts", "bowercopy:scripts"]);
    grunt.registerTask("default", [
        "cleanAll", // clean folders: preparing for copy
        "copyAll", // copying bower files
        "cssmin:customVendor", // minify and concat 'customized from vendor' css
        "concat:vendorStyles", // concat vendors's css + minified 'customized from vendor' and distribute as 'css/vendor.css'
        "concat:vendorScripts", // concat minified vendors's scripts and distribute as 'scripts/vendor.js' 
        "concat:appScripts", // concat app's scripts (unminified vendors's + customized from vendor's + actual app's + customer's)
        "uglify:app", // minify app script and distribute and distribute as 'scripts/app.js'
        "clean:vendor" , "clean:tmp" // clean temporary folders
    ]);
    //#endregion
   
};