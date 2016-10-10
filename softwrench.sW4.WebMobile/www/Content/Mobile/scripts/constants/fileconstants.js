(function (mobileServices, ionic) {
    "use strict";

    /**
     * @returns cordova directory constant (https://github.com/apache/cordova-plugin-file#where-to-store-files) 
     *          where the app's files should be created e.g. attachments and log files (changes depending on the platform)
     */
    function getAppFileDirectory() {
        var dir = undefined;
        if (ionic.Platform.isAndroid()) {
            dir = "externalDataDirectory";
        } else if (ionic.Platform.isIOS()) {
            // TODO: test user accessibility
            dir = "dataDirectory";
        } else if (ionic.Platform.isWindowsPhone()) {
            // TODO: test user acceesibility
            dir = "dataDirectory";
        }
        return dir;
    }

    class FilePathWrapper {
        toString() {
            return `{filePath: ${this.filePath}, fileName: ${this.fileName}, dirPath: ${this.dirPath}}`;
        }
        constructor(filePath) {
            if (!filePath || !angular.isString(filePath)) throw new Error("Path is empty or is not a string");
            this.filePath = filePath;
            const nameIndex = filePath.lastIndexOf("/") + 1;
            this.fileName = filePath.substr(nameIndex);
            this.dirPath = filePath.substr(0, nameIndex);
        }
    }

    class FileContentEntry {
        toString() {
            return `{length: ${this.length}, size: ${this.size}B}`;
        }
        constructor(data) {
            if (!data || !angular.isString(data)) throw new Error("Data is empty or is not a string");
            this.value = data;
            this.length = data.length;
            this.size = data.byteSize();
        }
    }

    class FileContentWrapper {
        toString() {
            return `{raw: ${this.data.toString()}, compressed: ${this.compressed.toString()}}`;
        }
        constructor(data) {
            // this.mime = mime;
            this.data = new FileContentEntry(data);
            const compressed = window.LZString.compressToUTF16(data);
            this.compressed = new FileContentEntry(compressed);
        }
    }

    mobileServices.constant("fileConstants", {
        fileEnabled: !isRippleEmulator(), // in ripple we don't use files
        appDirectory: getAppFileDirectory(),
        FilePathWrapper,
        FileContentWrapper
    });


})(mobileServices, ionic);