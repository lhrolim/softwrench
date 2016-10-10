(function (angular) {
    "use strict";

    angular.module("softwrench").config(["$provide", function ($provide) {

        function decorator($delegate, $window, $q, $cordovaFileError) {
            //#region Utils

            function toCordovaFileError(error) {
                error.message = $cordovaFileError[error.code] || error.message;
                return error;
            }

            /**
             * List all FileEntries in a directory.
             * 
             * @param {FilePath} path cordova file path (e.g. cordova.file.dataDirectory)
             * @param {String} dirName name of the directory (optional)
             * @returns {Promise<Array<FileEntry>>} fileentries in the directory
             */
            function listDir(path, dirName) {
                const q = $q.defer();
                const dirPath = path + (dirName ? dirName : "");
                try {
                    $window.resolveLocalFileSystemURL(dirPath,
                        fileSystem => {
                            const reader = fileSystem.createReader();
                            reader.readEntries(
                                entries => q.resolve(entries),
                                error => q.reject(toCordovaFileError(error))
                            );
                        },
                        error => q.reject(toCordovaFileError(error))
                    );

                } catch (e) {
                    q.reject(toCordovaFileError(e));
                }

                return q.promise;
            }
            //#endregion

            //#region Decorator

            // adding listDir method to $cordovaFile
            if (!angular.isFunction($delegate.listDir)) {
                $delegate.listDir = listDir.bind($delegate);
            }

            return $delegate;

            //#endregion
        };

        // #region Decorator Registration
        $provide.decorator("$cordovaFile", ["$delegate", "$window", "$q", "$cordovaFileError", decorator]);
        // #endregion
    }]);

})(angular);