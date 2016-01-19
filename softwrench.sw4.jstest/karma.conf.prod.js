// Karma configuration
// Generated on Sat Jan 31 2015 15:14:08 GMT-0200 (E. South America Daylight Time)

module.exports = function (config) {

    config.set({

        // frameworks to use
        // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
        frameworks: ["jasmine"],

        // list of files to exclude
        exclude: [
        ],

        // web server port
        port: 9876,

        // enable / disable colors in the output (reporters and logs)
        colors: true,

        // level of logging
        // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
        logLevel: config.LOG_WARN,

        // start these browsers
        // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
        browsers: ["PhantomJS"],

        browserNoActivityTimeout: 10000,
        browserDisconnectTolerance: 10,
        browserDisconnectTimeout: 5000
    });
};
