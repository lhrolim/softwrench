/// <binding ProjectOpened='watch' />
module.exports = function (grunt) {
    grunt.initConfig({
        //#region global app config 
        app: {
            content: "Content",
            customVendor: "Content/customVendor",
            webcommons: "Content/Shared/webcommons",
            vendor: "Content/vendor",
            customers: "Content/Customers",
            tests: "../softwrench.sw4.jstest",
            fsscripts: "../softwrench.sw4.firstsolar/web_content/firstsolar/scripts",
            dynforms: "../softwrench.sw4.dynforms/web_content/dynforms/scripts",
            webcommonsoriginal: "../softwrench.sw4.webcommons/web_content/webcommons/scripts",
        },
        //#endregion

        //#region sass
        watch: {
            files: [
                "Content/Customers/**/*.scss",
                "Content/Shared/**/*.scss",
                "Content/styles/**/*.scss"
            ],
            tasks: [
                "sass:dev"
            ]
        },
        sass: {
            dev: {
                options: {
                    sourceMap: true,
                    outputStyle: "compact"
                },
                files: [
                    { expand: true, cwd: "Content/Customers/", dest: "Content/Customers/", src: ["**/*.scss"], ext: ".css" },
                    { expand: true, cwd: "Content/Shared/", dest: "Content/Shared/", src: ["**/*.scss"], ext: ".css" },
                    { expand: true, cwd: "Content/styles/", dest: "Content/styles/", src: ["**/*.scss"], ext: ".css" }
                ]
            }
        },
        //#endregion

        //#region clean
        clean: {
            vendor: [
                "<%= app.vendor %>/css/*",
                "<%= app.vendor %>/scripts/*",
                "<%= app.vendor %>/fonts/*"
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
                    "bootstrap/bootstrap.css": "bootstrap/dist/css/bootstrap.css",
                    "bootstrap/bootstrap-theme.css": "bootstrap/dist/css/bootstrap-theme.css",
                    "angular-ui-grid/ui-grid.css": "angular-ui-grid/ui-grid.min.css",
                    "angular-ui-grid/ui-grid.ttf": "angular-ui-grid/ui-grid.ttf",
                    "angular-ui-grid/ui-grid.woff": "angular-ui-grid/ui-grid.woff",
                    "bootstrap/bootstrap-datetimepicker.css": "eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.min.css",
                    "bootstrap/selectize.css": "selectize/dist/css/selectize.bootstrap3.css",
                    // font-awesome
                    "font-awesome/font-awesome.css": "font-awesome/css/font-awesome.min.css",
                    // angular
                    "angular/textAngular.css": "textAngular/dist/textAngular.css",
                    "angular/angular-ui-select.css": "ui-select/dist/select.min.css",
                    "jquery-ui/jquery-ui.css": "jquery-ui/themes/base/jquery-ui.min.css"
                }
            },
            imgs: {
                options: {
                    destPrefix: "<%= app.vendor %>/css"
                },
                files: {
                    "jquery-ui/images": "jquery-ui/themes/base/images/*"
                }
            },
            fonts: {
                options: {
                    destPrefix: "<%= app.vendor %>/css"
                },
                files: {
                    "fonts": [
                        "font-awesome/fonts/*",
                        "bootstrap/dist/fonts/*",
                    ]
                }
            },
            scripts: {
                options: {
                    destPrefix: "<%= app.vendor %>/scripts"
                },
                files: {
                    // jquery
                    "jquery/jquery.js": "jquery/dist/jquery.js",
                    "jquery/jquery-ui.js": "jquery-ui/jquery-ui.js",
                    "jquery/jquery-file-style.js": "jquery.filestyle/jquery.filestyle.js",
                    "jquery/jquery-file-download.js": "jquery-file-download/src/Scripts/jquery.fileDownload.js",
                    "jquery/jquery-file-upload.js": "blueimp-file-upload/js/jquery.fileupload.js",
                    "jquery/jquery-colorbox.js": "colorbox/jquery.colorbox.js",
                    "jquery/jquery-knob.js": "jquery-knob/js/jquery.knob.js",
                    // bootstrap
                    "bootstrap/bootstrap.js": "bootstrap/dist/js/bootstrap.js",
                    // angular
                    "angular/ui-mask.js": "angular-ui-mask/dist/mask.js",
                    "angular/angular.js": "angular/angular.js",
                    "angular/angular-sanitize.js": "angular-sanitize/angular-sanitize.js",
                    "angular/angular-strap.js": "angular-strap/dist/angular-strap.js",
                    "angular/angular-animate.js": "angular-animate/angular-animate.js",
                    "angular/angular-xeditable.js": "angular-xeditable/dist/js/xeditable.js",
                    "angular-ui-grid/ui-grid.js": "angular-ui-grid/ui-grid.min.js",
                    "angular/angular-file-upload.js": "angular-file-upload/angular-file-upload.js",
                    "angular/angular-bindonce.js": "angular-bindonce/bindonce.js",
                    "angular/angular-drag-and-drop-lists.js": "angular-drag-and-drop-lists/angular-drag-and-drop-lists.js",
                    "angular/sortable.js": "angular-ui-sortable/sortable.js",
                    "angular/clickoutside.directive.js": "angular-click-outside/clickoutside.directive.js",
                    // devextreme
                    "devextreme/globalize.js": "globalize/lib/globalize.js",
                    "devextreme/dx.chartjs.js": "devextreme-web/js/dx.chartjs.js",
                    "devextreme/vectormap/usa.js": "devextreme-web/js/vectormap-data/usa.js",
                    // tinymce
                    "tinymce/angular-ui-tinymce.js" : "angular-ui-tinymce/src/tinymce.js",
                    // ace
                    "ace/ace.js": "ace-builds/src/ace.js",
                    "ace/mode-xml.js": "ace-builds/src/mode-xml.js",
                    "ace/mode-csharp.js": "ace-builds/src/mode-csharp.js",
                    "ace/ui-ace.js": "angular-ui-ace/ui-ace.js",
                    // pdf
                    "pdf/pdf.combined.js": "pdfjs-dist/build/pdf.combined.js",
                    "pdf/angular-pdf.js": "angular-pdf/dist/angular-pdf.js",
                    // utils
                    "utils/a-moment.js": "moment/min/moment.min.js",
                    "utils/moment-tz.js": "moment-timezone/builds/moment-timezone-with-data-2012-2022.min.js",
                    "utils/moment-locale-de.js": "moment/locale/de.js",
                    "utils/moment-locale-es.js": "moment/locale/es.js",
                    "utils/spin.js": "spin.js/spin.js",
                    "utils/lz-string.js": "lz-string/libs/lz-string.js"

                }
            }
        },
        //#endregion

        //#region karma
        karma: {
            options: {
                basePath: "",
                frameworks: ["jasmine"],
                port: 9876,
                colors: true,
                browserNoActivityTimeout: 10000,
                browserDisconnectTolerance: 10,
                browserDisconnectTimeout: 5000,
                reporters: ["progress", "dots"],
                logLevel: "INFO",
                preprocessors: {
                    'Content/Templates/**/*.html': ["ng-html2js"]
                },
                ngHtml2JsPreprocessor: {
                    // If your build process changes the path to your TEMPLATES,
                    // use stripPrefix and prependPrefix to adjust it.
                    prependPrefix: "/",
                    // the name of the Angular module to create
                    moduleName: "sw.templates"
                },
                files: [
                    // vendors
                    "<%= app.vendor %>/scripts/jquery/jquery.js",
                    "<%= app.vendor %>/scripts/jquery/jquery-ui.js",
                    "<%= app.vendor %>/scripts/jquery/*!(jquery).js",
                    "<%= app.vendor %>/scripts/angular/angular.js",
                    "<%= app.vendor %>/scripts/angular/*!(angular).js",
                    "<%= app.vendor %>/scripts/angular-ui-grid/*.js",
                    "<%= app.vendor %>/scripts/utils/**/*.js",
                    "<%= app.vendor %>/scripts/bootstrap/**/*.js",
                    "<%= app.vendor %>/scripts/devextreme/globalize.js",
                    "<%= app.vendor %>/scripts/devextreme/dx.chartjs.js",
                    "<%= app.vendor %>/scripts/devextreme/vectormap/usa.js",
                    "<%= app.vendor %>/scripts/tinymce/angular-ui-tinymce.js",
                    "<%= app.vendor %>/scripts/pdf/**/*.js",

                    // ace
                    "<%= app.vendor %>/scripts/ace/ace.js",
                    "<%= app.vendor %>/scripts/ace/mode-xml.js",
                    "<%= app.vendor %>/scripts/ace/mode-csharp.js",
                    "<%= app.vendor %>/scripts/ace/ui-ace.js",

                    // custom vendors
                    "<%= app.customVendor %>/scripts/**/*.js",

                    // app 
                    // modules
                    "<%= app.webcommons %>/scripts/softwrench/sharedservices_module.js", // webcommons
                    "<%= app.content %>/Scripts/client/crud/aaa_layout.js", // sw
                    
                    // webcommons
                    //TODO: symlinks not working here...
                    //"<%= app.content %>/Shared/
                    "<%= app.webcommonsoriginal %>/**/*.js",
                    "<%= app.content %>/Shared/activitystream/**/*.js",
                    "<%= app.content %>/Shared/audit/**/*.js",
                    "<%= app.content %>/Shared/dashboard/**/*.js",
                    "<%= app.content %>/Shared/problems/**/*.js",
//                    "<%= app.content %>/Shared/webcommons/**/*.js",
//                    "<%= app.webcommonsoriginal %>/**/*.js",

                  

                    // sw
                    "<%= app.content %>/Scripts/client/crud/**/*!(aaa_layout).js",
                    "<%= app.content %>/Scripts/client/services/**/*.js",
                    "<%= app.content %>/Scripts/client/*.js",
                    "<%= app.content %>/Scripts/client/constants/*.js",
                    "<%= app.content %>/Scripts/client/controllers/*.js",
                    "<%= app.content %>/Scripts/client/adminresources/*.js",
                    "<%= app.content %>/Scripts/client/components/*.js",
                    "<%= app.content %>/Scripts/client/util/*.js",
                    "<%= app.content %>/Scripts/client/directives/**/*.js",
                    "<%= app.content %>/Templates/commands/**/*.js",
                    "<%= app.content %>/modules/**/*.js",


                    // base otb
                    "<%= app.content %>/Scripts/customers/otb/*.js",
                    // customers shared
                    "<%= app.content %>/Scripts/customers/shared/*.js",
                    // customers: outer build process guarantees there's only the selected customer in the path
//                    "<%= app.customers %>/**/scripts/**/*.js",
                    "<%= app.fsscripts %>/**/*.js",
                    "<%= app.dynforms %>/**/*.js",

                    // templates
                    "<%= app.content %>/Templates/**/*.html",

                    // tests
                    "<%= app.tests %>/angular_mock.js",
                    "<%= app.tests %>/tests/**/*.js"
                ]
            },
            dev: {
                browsers: ["PhantomJS"],
                singleRun: true,
                options: {
                    babelPreprocessor: {
                        options: {
                            presets: ["latest"],
                            sourceMap: false
                        }
                    },
                    preprocessors: {
                        'Content/Shared/**/*.js': ["babel"],
                        'Content/Scripts/**/*.js': ["babel"],
                        'Content/modules/**/*.js': ["babel"],
                        'Content/Customers/**/scripts/**/*.js': ["babel"],
                        '../softwrench.sw4.jstest/tests/**/*.js': ["babel"],
                        'Content/Templates/**/*.html': ["ng-html2js"]
                    }
                }
            },
            tdd: {
                browsers: ["Chrome"],
                autowatch: true,
                singleRun: false
            }
        }
        //#endregion
    });

    //#region load npm tasks
    grunt.loadNpmTasks("grunt-sass");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-bowercopy");
    grunt.loadNpmTasks("grunt-contrib-watch");
    grunt.loadNpmTasks("grunt-karma");
    //#endregion

    //#region cutom tasks
    grunt.registerTask("copyAll", ["clean:vendor", "bowercopy:css", "bowercopy:imgs", "bowercopy:fonts", "bowercopy:scripts"]);
    grunt.registerTask("default", ["copyAll", "sass:dev"]);
    //#endregion
};
