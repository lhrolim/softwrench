/// <vs AfterBuild='quick_dev_wrapper' />
module.exports = function (grunt) {

    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),

        clean: {
            folder: "Content/Vendor/scripts/",
            folder2: "Content/Vendor/styles/"
        },




        bowercopy: {

            options: {
                destPrefix: 'Content/Vendor/scripts'
            },


            dev: {

                files: {
                    'angular-sanitize.js': 'angular-sanitize/angular-sanitize.js',
                    'angular-ui-router.js': 'angular-ui-router/release/angular-ui-router.js',
                    'angular-animate.js': 'angular-animate/angular-animate.js',
                    'angular.js': 'angular/angular.js',

                    'jquery.js': 'jquery/dist/jquery.js',
                    'ng-cordova.js': 'ngCordova/dist/ng-cordova.js',
                    'persistence.js': 'persistence/lib/persistence.js',
//                    'persistence.store.sql.js': 'persistence/lib/persistence.store.sql.js',
                    'persistence.store.websql.js': 'persistence/lib/persistence.store.websql.js',
                    'moment.js': 'moment/moment.js',
                }
            },

            prod: {
                files: {
                    'angular.js': 'angular/angular.min.js',
                    'angular-sanitize.js': 'angular-sanitize/angular-sanitize.min.js',
                    'angular-ui-router.js': 'angular-ui-router/release/angular-ui-router.min.js',
                    'angular-animate.js': 'angular-animate/angular-animate.min.js',

                    'jquery.js': 'jquery/dist/jquery.min.js',
                    'ng-cordova.js': 'ngCordova/dist/ng-cordova.min.js',
                    'persistence.js': 'persistence/lib/persistence.js',
//                    'persistence.store.sql.js': 'persistence/lib/persistence.store.sql.js',
                    'persistence.store.websql.js': 'persistence/lib/persistence.store.websql.js',
                    'moment.js': 'moment/min/moment.min.js',
                }
            }



        },


        tags: {
            options: {
                openTag: '<!-- start auto template tags, grunt will generate it for dev environment, do not remove this -->'
            },

            build: {
                src: ["Content/Mobile/scripts/controllers/**/*.js", "Content/Mobile/scripts/services/**/*.js", "Content/Mobile/scripts/directives/**/*.js", "Content/Mobile/scripts/utils/**/*.js"],
                dest: 'layout.html'
            }
        },

        concat: {
            mobileScripts: {
                src: ["Content/Mobile/scripts/controllers/**/*.js", "Content/Mobile/scripts/services/**/*.js", "Content/Mobile/scripts/directives/**/*.js", "Content/Mobile/scripts/utils/**/*.js"],
                dest: "scripts/dist/mobile_angular.js"
            },
        }


    });


    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-concat');
    grunt.loadNpmTasks('grunt-contrib-clean');
    grunt.loadNpmTasks('grunt-bowercopy');
    grunt.loadNpmTasks('grunt-script-link-tags');

    grunt.option('jssuffix', 'min.js');

    //grunt.option('jssuffix', 'js');




    // Default task(s).

    grunt.registerTask('prod', ['clean', 'bowercopy:prod', 'uglify']);

    grunt.registerTask('fulldev', ['clean', 'bowercopy:dev', 'tags']);
    grunt.registerTask('quick_dev', ['bowercopy:dev','tags']);


};