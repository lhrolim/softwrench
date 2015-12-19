module.exports = function (grunt) {

    var path = grunt.option("path") || "";
    var fullPath = !!path ? path + "/" : "../softWrench.sW4.Web/";

    var filesToCompile = [
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
        sass: {
            config: {
                files: filesToCompile
            },
            prod: {
                options: {
                    sourceMap: false,
                    outputStyle: "compressed",
                },
                files: "<%= sass.config.files %>"
            }

        }
    });

    // load npm tasks
    grunt.loadNpmTasks("grunt-sass");

    // define default task
    grunt.registerTask("default", ["sass"]);
};