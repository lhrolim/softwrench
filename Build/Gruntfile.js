/// <binding AfterBuild='sass' ProjectOpened='default' />
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
                    "bootstrap.css": "bootstrap/dist/css/bootstrap.min.css",
                    "bootstrap-theme.css": "bootstrap/dist/css/bootstrap-theme.min.css",
                    "bootstrap-datetimepicker.css": "eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.min.css",
                    "textAngular.css": "textAngular/dist/textAngular.css",
                    "font-awesome.css": "font-awesome/css/font-awesome.min.css",
                    "angular-ui-select.css": "ui-select/dist/select.min.css",
                    "selectize.css": "selectize/dist/css/selectize.bootstrap3.css",
                }
            },
            fonts: {
                options: {
                    destPrefix: "<%= app.vendor %>/fonts"
                },
                files: {
                    ".": [
                        "font-awesome/fonts/*",
                        "bootstrap/dist/fonts/*"
                    ],
                }
            },
            dev: {
                options: {
                    destPrefix: "<%= app.vendor %>/scripts"
                },
                files: {
                    "jquery.js": "jquery/dist/jquery.js",
                    "jquery-ui.js": "jquery-ui/ui/jquery-ui.js",
                    "jquery-file-style.js": "jquery.filestyle/jquery.filestyle.js",
                    "jquery-file-download.js": "jquery-file-download/src/Scripts/jquery.fileDownload.js",
                    "jquery-file-upload.js": "blueimp-file-upload/js/jquery.fileupload.js",
                    "spin.js": "spin.js/spin.js",
                    "angular-file-upload.js": "angular-file-upload/angular-file-upload.js",
                    "lz-string.js": "lz-string/libs/lz-string.js",
                    "modernizr.js": "modernizr-min/dist/modernizr.min.js",
                    "angular.js": "angular/angular.js",
                    "angular-sanitize.js": "angular-sanitize/angular-sanitize.js",
                    "angular-strap.js": "angular-strap/dist/angular-strap.js",
                    "angular-bindonce.js": "angular-bindonce/bindonce.js",
                    "angular-animate.js": "angular-animate/angular-animate.js",
                    "angular-xeditable.js": "angular-xeditable/dist/js/xeditable.js",
                    "moment.js": "moment/src/moment.js",
                    "moment-locale-de.js": "moment/locale/de.js",
                    "moment-locale-es.js": "moment/locale/es.js",
                    "bootstrap.js": "bootstrap/dist/js/bootstrap.js",
                    "bootstrap-combobox.js": "bootstrap-combobox/js/bootstrap-combobox.js",
                    "bootstrap-datetimepicker.js": "eonasdan-bootstrap-datetimepicker/src/js/bootstrap-datetimepicker.js",
                    "bootstrap-multiselect.js": "bootstrap-multiselect/dist/js/bootstrap-multiselect.js",
                    "bootbox.js": "bootbox.js/bootbox.js",
                }
            },
            prod: {
                options: {
                    clean: true,
                    destPrefix: "<%= app.vendor %>/scripts"
                },
                files: {
                    "jquery.js": "jquery/dist/jquery.min.js",
                    "jquery-ui.js": "jquery-ui/ui/minified/jquery-ui.min.js",
                    "jquery-file-style.js": "jquery.filestyle/jquery.filestyle.js",
                    "jquery-file-download.js": "jquery-file-download/src/Scripts/jquery.fileDownload.js",
                    "jquery-file-upload.js": "blueimp-file-upload/js/jquery.fileupload.js",
                    "spin.js": "spin.js/spin.min.js",
                    "angular-file-upload.js": "angular-file-upload/angular-file-upload.min.js",
                    "lz-string.js": "lz-string/libs/lz-string.min.js",
                    "modernizr.js": "modernizr-min/dist/modernizr.min.js",
                    "angular.js": "angular/angular.min.js",
                    "angular-sanitize.js": "angular-sanitize/angular-sanitize.min.js",
                    "angular-strap.js": "angular-strap/dist/angular-strap.min.js",
                    "angular-bindonce.js": "angular-bindonce/bindonce.min.js",
                    "angular-animate.js": "angular-animate/angular-animate.min.js",
                    "angular-xeditable.js": "angular-xeditable/dist/js/xeditable.min.js",
                    "moment.js": "moment/min/moment.min.js",
                    "moment-locale-de.js": "moment/locale/de.js",
                    "moment-locale-es.js": "moment/locale/es.js",
                    "bootstrap.js": "bootstrap/dist/js/bootstrap.min.js",
                    "bootstrap-combobox.js": "bootstrap-combobox/js/bootstrap-combobox.js",
                    "bootstrap-datetimepicker.js": "eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js",
                    "bootstrap-multiselect.js": "bootstrap-multiselect/dist/js/bootstrap-multiselect.js",
                    "bootbox.js": "bootbox.js/bootbox.js",
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
    grunt.registerTask("default", ["sass"]);
};