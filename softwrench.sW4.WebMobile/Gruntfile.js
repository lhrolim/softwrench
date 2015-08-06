/// <vs AfterBuild='quick_dev_wrapper' />
module.exports = function (grunt) {

    // Project configuration.

    var commonScripts = [
        // persistence.offline
        "www/Content/Mobile/scripts/persistence/module.js",
        "www/Content/Mobile/scripts/persistence/services/**/*.js",
        "www/Content/Mobile/scripts/persistence/config.entities.js",
        // audit.offline
        "www/Content/Shared/audit_offline/scripts/offline/audit.js"
    ];

    var sharedScripts = [
        "www/Content/Shared/webcommons/scripts/softwrench/sharedservices_module.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/statuscolor_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/aa_stringutils.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/aa_utils.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/object_util.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/context_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/tabs_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/i18n_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/schema_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/field_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/validation_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/format_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/event_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/expression_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/dispatcher_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/rest_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/scannerCommons.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/user_service.js",
        "www/Content/Shared/webcommons/scripts/softwrench/services/composition_commons.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/log_enhacer.js",
        "www/Content/Shared/webcommons/scripts/softwrench/util/clientawareserviceprovider.js"
    ];

    var appScripts = [
        "www/Content/Mobile/scripts/controllers/**/*.js",
        "www/Content/Mobile/scripts/services/**/*.js",
        "www/Content/Mobile/scripts/directives/**/*.js",
        "www/Content/Mobile/scripts/maximoservices/**/*.js",
        "www/Content/Mobile/scripts/utils/**/*.js",
        "www/Content/Mobile/scripts/filters/**/*.js"
    ];

    //TODO: make a client-based build??
    //make it download the customer scripts from the server at runtime?
    var customerScripts = [];
    var customer = grunt.option("customer");
    if (customer) {
        customerScripts = [
            "www/Content/Customers/" + customer + "_offline/scripts/**/*.mobile.js"
        ];
    }

    var solutionScripts = commonScripts.concat(sharedScripts).concat(appScripts).concat(customerScripts);

    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),

        clean: {
            folder: "www/Content/Vendor/scripts/",
            folder2: "www/Content/Vendor/styles/"
        },

        bowercopy: {

            options: {
                destPrefix: 'www/Content/Vendor/scripts'
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
                    destPrefix: 'www/Content/Vendor/css'
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
                dest: 'www/layout.html'
            }
        },

        concat: {
            mobileScripts: {
                src: solutionScripts,
                dest: "www/scripts/dist/mobile_angular.js"
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