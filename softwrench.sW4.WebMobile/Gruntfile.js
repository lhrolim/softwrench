/// <vs AfterBuild='quick_dev_wrapper' />
module.exports = function (grunt) {

    // Project configuration.

    var commonScripts = [
        // persistence.offline
        "Content/Mobile/scripts/persistence/module.js",
        "Content/Mobile/scripts/persistence/services/**/*.js",
        "Content/Mobile/scripts/persistence/config.entities.js",
        // audit.offline
        "Content/Shared/audit/scripts/offline/audit.js"
    ];

    var sharedScripts = [
        "Content/Shared/webcommons/scripts/softwrench/sharedservices_module.js",
        "Content/Shared/webcommons/scripts/softwrench/services/statuscolor_service.js",
        "Content/Shared/webcommons/scripts/softwrench/util/aa_stringutils.js",
        "Content/Shared/webcommons/scripts/softwrench/util/aa_utils.js",
        "Content/Shared/webcommons/scripts/softwrench/util/object_util.js",
        "Content/Shared/webcommons/scripts/softwrench/services/context_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/tabs_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/i18n_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/schema_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/field_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/validation_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/format_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/event_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/expression_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/dispatcher_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/rest_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/user_service.js",
        "Content/Shared/webcommons/scripts/softwrench/services/composition_commons.js",
        "Content/Shared/webcommons/scripts/softwrench/util/log_enhacer.js",
        "Content/Shared/webcommons/scripts/softwrench/util/clientawareserviceprovider.js",
        "Content/Shared/audit/scripts/offline/audit.js"
    ];

    var appScripts = [
        "Content/Mobile/scripts/controllers/**/*.js",
        "Content/Mobile/scripts/services/**/*.js",
        "Content/Mobile/scripts/directives/**/*.js",
        "Content/Mobile/scripts/maximoservices/**/*.js",
        "Content/Mobile/scripts/utils/**/*.js",
        "Content/Mobile/scripts/filters/**/*.js"
    ];

    var solutionScripts = commonScripts.concat(sharedScripts).concat(appScripts);

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
                    //'angular-cookies.js': 'angular-cookies/angular-cookies.js',

                    'jquery.js': 'jquery/dist/jquery.js',
                    'ng-cordova.js': 'ngCordova/dist/ng-cordova.js',
                    'persistence.js': 'persistence/lib/persistence.js',
                    //'persistence.store.sql.js': 'persistence/lib/persistence.store.sql.js',
                    'persistence.store.websql.js': 'persistence/lib/persistence.store.websql.js',
                    'moment.js': 'moment/moment.js',
                    'ionautocomplete.js': 'ion-autocomplete/dist/ion-autocomplete.js',
                    //'ionic/release/js/ionic.js': 'ionic/release/js/ionic.js',
                    //'ionic/release/js/ionic-angular.js': 'ionic/release/js/ionic-angular.js',
                    //'ionic/release/css/ionic.css': 'ionic/release/css/ionic.css',
                    //'ionic/release/fonts/ionicons.ttf': 'ionic/release/fonts/ionicons.ttf',
                    //'ionic/release/fonts/ionicons.woff': 'ionic/release/fonts/ionicons.woff',
                },


            },

            css: {

                options: {
                    destPrefix: 'Content/Vendor/css'
                },
                files: {
                    'ionautocomplete.css': 'ion-autocomplete/dist/ion-autocomplete.min.css',
                }
            },

            prod: {
                files: {
                    'angular.js': 'angular/angular.min.js',
                    'angular-sanitize.js': 'angular-sanitize/angular-sanitize.min.js',
                    'angular-ui-router.js': 'angular-ui-router/release/angular-ui-router.min.js',
                    'angular-animate.js': 'angular-animate/angular-animate.min.js',
                    //'angular-cookies.js': 'angular-cookies/angular-cookies.min.js',

                    'jquery.js': 'jquery/dist/jquery.min.js',
                    'ng-cordova.js': 'ngCordova/dist/ng-cordova.min.js',
                    'persistence.js': 'persistence/lib/persistence.js',
                    //'persistence.store.sql.js': 'persistence/lib/persistence.store.sql.js',
                    'persistence.store.websql.js': 'persistence/lib/persistence.store.websql.js',
                    'moment.js': 'moment/min/moment.min.js',
                    'ionautocomplete.js': 'ion-autocomplete/dist/ion-autocomplete.min.js',
                    //'ionic/release/js/ionic.js': 'ionic/release/js/ionic.min.js',
                    //'ionic/release/js/ionic-angular.js': 'ionic/release/js/ionic-angular.min.js',
                    //'ionic/release/css/ionic.css': 'ionic/release/css/ionic.min.css',
                    //'ionic/release/fonts/ionicons.ttf': 'ionic/release/fonts/ionicons.ttf',
                    //'ionic/release/fonts/ionicons.woff': 'ionic/release/fonts/ionicons.woff',
                }
            }
        },


        tags: {
            options: {
                openTag: '<!-- start auto template tags, grunt will generate it for dev environment, do not remove this -->',
                closeTag: '<!-- end auto template tags -->'
            },

            build: {
                src: solutionScripts,
                dest: 'layout.html'
            }
        },

        concat: {
            mobileScripts: {
                src: solutionScripts,
                dest: "scripts/dist/mobile_angular.js"
            }
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
    grunt.registerTask('quick_dev', ['bowercopy:dev', 'bowercopy:css', 'tags']);


};