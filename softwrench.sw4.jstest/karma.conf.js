// Karma configuration
// Generated on Sat Jan 31 2015 15:14:08 GMT-0200 (E. South America Daylight Time)

module.exports = function (config) {
    config.set({

        // base path that will be used to resolve all patterns (eg. files, exclude)
        basePath: '',


        // frameworks to use
        // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
        frameworks: ['jasmine'],


        // list of files / patterns to load in the browser
        files: [
            '..//softWrench.sW4.Web//Content//Scripts//vendor/jquery/**/*.js',
            '..//softWrench.sW4.Web//Content//Scripts//vendor/bootstrap/**/*.js',
            '..//softWrench.sW4.Web//Content//Scripts//vendor/angular/angular.js',
            '..//softWrench.sW4.Web//Content//Scripts//vendor/angular/**/*.js',
            '..//softWrench.sW4.Web//Content//Scripts//vendor/other/**/*.js',
            '..//softWrench.sW4.Web//Content//Scripts//vendor/ace/**/*.js',
            '..//softWrench.sW4.Web//Content//Scripts//client/crud/**/*.js',
            '..//softWrench.sW4.Web//Content//Scripts//client/**/*.js',
            '..//softWrench.sW4.Web//Content//Templates/**/*.js',
            '..//softWrench.sW4.Web//Content//Templates/**/*.html',
            '..//softWrench.sW4.Web//Content//Scripts//customers/**/*.js',
            '..//softWrench.sW4.Webcommons//web_content//webcommons//scripts//softwrench//sharedservices_module.js',
            '..//softWrench.sW4.Webcommons//web_content//webcommons//scripts//softwrench/**/*.js',
            '..//softWrench.sW4.dashboard//web_content//dashboard//scripts//**/*.js',
            'angular_mock.js',
            'tests//**/*.js',
        ],


        // list of files to exclude
        exclude: [
        ],


        // preprocess matching files before serving them to the browser
        // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
        preprocessors: {
            '..//softWrench.sW4.Web//Content//Templates/**/*.html': ['ng-html2js']
        },

        ngHtml2JsPreprocessor: {
            // If your build process changes the path to your TEMPLATES,
            // use stripPrefix and prependPrefix to adjust it.
            stripPrefix: "(.*)softWrench.sW4.Web",

            // the name of the Angular module to create
            moduleName: "sw.templates"
        },

        // test results reporter to use
        // possible values: 'dots', 'progress'
        // available reporters: https://npmjs.org/browse/keyword/karma-reporter
        reporters: ['progress', 'xml', 'dots', 'junit'],

        junitReporter: {
            outputFile: 'jenkinstest-results.xml'
        },


        // web server port
        port: 9876,


        // enable / disable colors in the output (reporters and logs)
        colors: true,

        browserNoActivityTimeout: 140000,

        // level of logging
        // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
        logLevel: config.LOG_INFO,


        // enable / disable watching file and executing tests whenever any file changes
        autoWatch: true,


        // start these browsers
        // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
        //    browsers: ['Chrome', 'IE', 'PhantomJS','Firefox'],
        browsers: ['Chrome'],


        // Continuous Integration mode
        // if true, Karma captures browsers, runs the tests and exits
        singleRun: false
    });
};
