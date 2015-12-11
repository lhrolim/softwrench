/// <binding AfterBuild='sass' ProjectOpened='default' />
module.exports = function (grunt) {

    var path = grunt.option("path") || "";
    var customer = grunt.option("customer") || "";

    var filesToCompile = [
        {
            expand: true,
            cwd: "Content/Customers/" + (!!customer ? customer + "/" : ""),
            dest: "Content/Customers/" + (!!customer ? customer + "/" : ""),
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
        file.cwd = path + "/" + file.cwd;
        file.dest = path + "/" + file.dest;
        return file;
    });

    grunt.initConfig({
        watch: {
            files: [
                "Content/Customers/**/*.scss",
                "Content/Shared/**/*.scss",
                "Content/styles/**/*.scss"],
            tasks: ["sass"]
        },
        sass: {
            config: {
                files: filesToCompile
            },
            dev: {
                options: {
                    sourceMap: true
                },
                files: "<%= sass.config.files %>"
            },
            prod: {
                options: {
                    sourceMap: false
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