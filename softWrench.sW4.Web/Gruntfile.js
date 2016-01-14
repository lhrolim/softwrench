/// <binding AfterBuild='sass:dev' ProjectOpened='default' />
module.exports = function (grunt) {

    var path = grunt.option("path") || "";
    var fullPath = !!path ? path + "/" : "";

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
        watch: {
            files: [
                "Content/Customers/**/*.scss",
                "Content/Shared/**/*.scss",
                "Content/styles/**/*.scss"],
            tasks: ["sass:dev"]
        },
        sass: {
            config: {
                files: filesToCompile
            },
            dev: {
                options: {
                    sourceMap: true,
                    outputStyle: "compact",
                },
                files: "<%= sass.config.files %>"
            }
        }
    });

    // load npm tasks
    grunt.loadNpmTasks("grunt-sass");
    grunt.loadNpmTasks("grunt-contrib-watch");

    // define default task
    grunt.registerTask("default", ["sass", "watch"]);
};