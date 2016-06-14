(function(mobileServices) {
    "use strict";

    mobileServices.constant("rollingLogFileConstants", {
        // TODO: determine optimal size and buffer
        logFileSize: 10 * 1024 * 1024, // 10MB
        eventBuffer: 20,
        writeOnPause: true,
        logToConsole: false,
        debug: true, // controlled by the caller
        logFileName: "SWOFF"
    });


})(mobileServices);