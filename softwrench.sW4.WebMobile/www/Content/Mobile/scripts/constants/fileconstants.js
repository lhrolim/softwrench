(function (mobileServices, ionic) {
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
            // TODO: test user accessibility
            dir = "dataDirectory";
        } else if (ionic.Platform.isWindowsPhone()) {
            // TODO: test user acceesibility
            dir = "dataDirectory";
        }
        return dir;
    }

    class FileContentWrapper {
        toString() {
            return `{filePath: ${this.filePath}, fileName: ${this.fileName}, dirPath: ${this.dirPath}}`;
        }
        constructor(filePath) {
            this.filePath = filePath;
            const nameIndex = filePath.lastIndexOf("/") + 1;
            this.fileName = filePath.substr(nameIndex);
            this.dirPath = filePath.substr(0, nameIndex);
        }
    }

    class FileContentEntry {
        toString() {
            return `{length: ${this.length}, size: ${this.size}}`;
        }
        constructor(data) {
            if (!data || !angular.isString(data)) throw new Error("Captured camera data is empty");
            this.value = data;
            this.length = data.length;
            this.size = data.byteSize();
        }
    }

    class FilePathWrapper {
        toString() {
            return `{raw: ${this.data.toString()}, compressed: ${this.compressed.toString()}}`;
        }
        constructor(data) {
            // this.mime = mime;
            this.data = new FileContentEntry(data);
            const compressed = window.LZString.compressToUTF16.compress(data);
            this.compressed = new FileContentEntry(compressed);
        }
    }

    mobileServices.constant("fileConstants", {
        fileEnabled: !isRippleEmulator(), // in ripple we don't use files
        appDirectory: getLogFileDirectory(),
        FilePathWrapper,
        FileContentWrapper
    });


})(mobileServices, ionic);