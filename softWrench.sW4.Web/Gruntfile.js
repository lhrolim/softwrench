/// <binding BeforeBuild='copyAll' AfterBuild='sass:dev' ProjectOpened='watch' />
module.exports = function (grunt) {
    grunt.initConfig({
        //#region global app config 
        app: {
            content: "Content",
            vendor:  "Content/vendor"
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
            ],
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
                    destPrefix: "<%= app.vendor %>/css"
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
                    "jquery/jquery.js": "jquery/dist/jquery.js",
                    "jquery/jquery-ui.js": "jquery-ui/ui/jquery-ui.js",
                    "jquery/jquery-file-style.js": "jquery.filestyle/jquery.filestyle.js",
                    "jquery/jquery-file-download.js": "jquery-file-download/src/Scripts/jquery.fileDownload.js",
                    "jquery/jquery-file-upload.js": "blueimp-file-upload/js/jquery.fileupload.js",
                    // bootstrap
                    "bootstrap/bootstrap.js": "bootstrap/dist/js/bootstrap.js",
                    "bootstrap/bootstrap-datetimepicker.js": "eonasdan-bootstrap-datetimepicker/src/js/bootstrap-datetimepicker.js",
                    "bootstrap/bootstrap-multiselect.js": "bootstrap-multiselect/dist/js/bootstrap-multiselect.js",
                    // angular
                    "angular/angular.js": "angular/angular.js",
                    "angular/angular-sanitize.js": "angular-sanitize/angular-sanitize.js",
                    "angular/angular-strap.js": "angular-strap/dist/angular-strap.js",
                    "angular/angular-animate.js": "angular-animate/angular-animate.js",
                    "angular/angular-xeditable.js": "angular-xeditable/dist/js/xeditable.js",
                    "angular/angular-file-upload.js": "angular-file-upload/angular-file-upload.js",
                    "angular/angular-bindonce.js": "angular-bindonce/bindonce.js",
                    // utils
                    "utils/a-moment.js": "moment/min/moment.min.js",
                    "utils/moment-locale-de.js": "moment/locale/de.js",
                    "utils/moment-locale-es.js": "moment/locale/es.js",
                    "utils/spin.js": "spin.js/spin.js",
                    "utils/lz-string.js": "lz-string/libs/lz-string.js",
                }
            }
        }
        //#endregion
    });

    //#region load npm tasks
    grunt.loadNpmTasks("grunt-sass");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-bowercopy");
    grunt.loadNpmTasks("grunt-contrib-watch");
    //#endregion

    //#region cutom tasks
    grunt.registerTask("copyAll", ["clean:vendor", "bowercopy:css", "bowercopy:fonts", "bowercopy:scripts"]);
    grunt.registerTask("default", ["copyAll", "sass:dev"]);
    //#endregion
};
