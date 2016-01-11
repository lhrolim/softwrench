module.exports = function (grunt) {

    var webProjectRelPath = "../softWrench.sW4.Web/";
    var path = grunt.option("path") || "";
    var fullPath = !!path ? path + "/" : webProjectRelPath;

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

        app: {
            vendor: webProjectRelPath + "Content/vendor",
            tmp: webProjectRelPath + "Content/tmp",
            dist: webProjectRelPath + "Content/dist"
        },

        sass: {
            prod: {
                options: {
                    sourceMap: false,
                    outputStyle: "compressed"
                },
                files: scssFilesToCompile
            }
        },

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
            fontsdev: {
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
            dev: {
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
                    "bootstrap/bootstrap-combobox.js": "bootstrap-combobox/js/bootstrap-combobox.js",
                    "bootstrap/bootstrap-datetimepicker.js": "eonasdan-bootstrap-datetimepicker/src/js/bootstrap-datetimepicker.js",
                    "bootstrap/bootstrap-multiselect.js": "bootstrap-multiselect/dist/js/bootstrap-multiselect.js",
                    "bootstrap/bootbox.js": "bootbox.js/bootbox.js",
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
            },
            prod: {
                options: {
                    clean: true,
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
        }
    });

    // load npm tasks
    grunt.loadNpmTasks("grunt-sass");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-bowercopy");
    grunt.loadNpmTasks("grunt-contrib-concat");
    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks("grunt-contrib-cssmin");
    grunt.loadNpmTasks("grunt-karma"); 

    // define default tasks
    grunt.registerTask("copyAll", ["clean:vendor", "bowercopy:css", "bowercopy:fontsdev", "bowercopy:dev"]);
    grunt.registerTask("default", ["copyAll"]);
};