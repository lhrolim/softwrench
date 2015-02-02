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
            '..//softWrench.sW4.Web//Content//Scripts//client/**/*.js',
            '..//softWrench.sW4.Web//Content//Scripts//customers/**/*.js',
            'angular_mock.js',
          'tests//**/*.js'
        ],


        // list of files to exclude
        exclude: [
        ],


        // preprocess matching files before serving them to the browser
        // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
        preprocessors: {
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

        browserNoActivityTimeout: 40000,

        // level of logging
        // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
        logLevel: config.LOG_INFO,


        // enable / disable watching file and executing tests whenever any file changes
        autoWatch: true,


        // start these browsers
        // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
        //    browsers: ['Chrome', 'IE', 'PhantomJS','Firefox'],
        browsers: ['PhantomJS'],


        // Continuous Integration mode
        // if true, Karma captures browsers, runs the tests and exits
        singleRun: true
    });
};
