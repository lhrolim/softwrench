// Karma configuration
// Generated on Wed Apr 15 2015 11:01:15 GMT-0700 (Hora Padrão: Montanhas)

module.exports = function (config) {
    config.set({

        // base path that will be used to resolve all patterns (eg. files, exclude)
        basePath: '',


        // frameworks to use
        // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
        frameworks: ['jasmine'],


        // list of files / patterns to load in the browser
        files: [
            'Content//Vendor/**/angular.min.js',
            'Content//Vendor/**/ionic.min.js',
            'Content//Vendor/**/*.js',
            'Content//Mobile/scripts/mobile_bootstrap.js',
            'Content//Mobile/**/*.js',
            '..//softWrench.sW4.Webcommons//web_content//webcommons//scripts/softwrench/sharedservices_module.js',
            '..//softWrench.sW4.Webcommons//web_content//webcommons//scripts/softwrench/util/aa_stringutils.js',
            '..//softWrench.sW4.Webcommons//web_content//webcommons//scripts/softwrench/util/object_util.js',
            '..//softWrench.sW4.Webcommons//web_content//webcommons//scripts/softwrench/services/context_service.js',
            '..//softWrench.sW4.Webcommons//web_content//webcommons//scripts/softwrench/services/dispatcher_service.js',
            'angular_mock.js',
            'tests//**/*.js',
        ],


        // list of files to exclude
        exclude: [
        ],

        // preprocess matching files before serving them to the browser
        // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
        preprocessors: {
            'Content//Mobile/templates/**/*.html': ['ng-html2js']
        },

        ngHtml2JsPreprocessor: {
            // If your build process changes the path to your TEMPLATES,
            // use stripPrefix and prependPrefix to adjust it.
            //stripPrefix: "(.*)softWrench.sW4.Web",

            // the name of the Angular module to create
            moduleName: "sw.templates"
        },



        // test results reporter to use
        // possible values: 'dots', 'progress'
        // available reporters: https://npmjs.org/browse/keyword/karma-reporter
        reporters: ['progress'],


        // web server port
        port: 9876,


        // enable / disable colors in the output (reporters and logs)
        colors: true,


        // level of logging
        // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
        logLevel: config.LOG_INFO,


        // enable / disable watching file and executing tests whenever any file changes
        autoWatch: true,


        // start these browsers
        // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
        browsers: ['Chrome'],


        // Continuous Integration mode
        // if true, Karma captures browsers, runs the tests and exits
        singleRun: false
    });
};
