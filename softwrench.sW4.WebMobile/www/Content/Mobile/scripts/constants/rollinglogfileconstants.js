(function(mobileServices, ionic) {
    "use strict";

    /**
     * @returns cordova directory constant (https://github.com/apache/cordova-plugin-file#where-to-store-files) 
     *          where the log file should be created (changes depending on the platform)
     */
    function getLogFileDirectory() {
        var dir = undefined;
        if (ionic.Platform.isAndroid()) {
            dir = "externalApplicationStorageDirectory";
        } else if (ionic.Platform.isIOS()) {
            // TODO: test user acceesibility
            dir = "dataDirectory";
        } else if (ionic.Platform.isWindowsPhone()) {
            // TODO: test user acceesibility
            dir = "dataDirectory";
        }
        return dir;
    }

    mobileServices.constant("rollingLogFileConstants", {
        // TODO: determine optimal size and buffer
        logToFileEnabled: !isRippleEmulator(), // in ripple we don't use files
        logFileSize: 10 * 1024 * 1024, // 10MB
        eventBuffer: 20,
        writeOnPause: true,
        logToConsole: false,
        debug: true, // controlled by the caller
        logFileName: "SWOFF",
        logFileDirectory: getLogFileDirectory()
    });


})(mobileServices, ionic);