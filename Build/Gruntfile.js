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
            vendor: webProjectRelPath + "Content/vendor"  
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

        bowercopy: {
            css: {
                options: {
                    destPrefix: "<%= app.vendor %>" + "/css"
                },
                files: {
                    "bootstrap.css": "bootstrap/dist/css/bootstrap.min.css",
                    "bootstrap-theme.css": "bootstrap/dist/css/bootstrap-theme.min.css",
                    "bootstrap-datetimepicker.css": "eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.min.css",
                    "font-awesome.css": "font-awesome/css/font-awesome.min.css",
                    "fonts": ["font-awesome/fonts/*", "bootstrap/dist/fonts/*"],
                    "angular-ui-select.css": "ui-select/dist/select.min.css",
                    "selectize.css": "selectize/dist/css/selectize.bootstrap3.css",
                }
            },
            fonts: {
                options: {
                    destPrefix: "<%= app.vendor %>" + "/fonts"
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
                    destPrefix: "<%= app.vendor %>" + "/scripts"
                },
                files: {
                }
            },
            prod: {
                options: {
                    clean: true,
                    destPrefix: "<%= app.vendor %>" + "/scripts"
                },
                files: {
                }
            }
        }
    });

    // load npm tasks
    grunt.loadNpmTasks("grunt-sass");
    grunt.loadNpmTasks("grunt-bowercopy");
    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks("grunt-contrib-concat");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-karma");

    // define default tasks
    grunt.registerTask("default", ["sass"]);
};