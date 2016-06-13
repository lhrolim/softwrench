(function (angular) {
    "use strict";

    /**
     * Requires LZString for compression and decompression.
     * 
     * @returns {CompressionService} instance 
     */
    function compressionService() {
        //#region Utils
        const compressor = window.LZString;
        //#endregion

        //#region Public methods
        function compress(data) {
            return compressor.compressToUTF16(data);
        }

        function decompress(data) {
            return compressor.decompressFromUTF16(data);
        }
        //#endregion

        //#region Service Instance
        const service = {
            compress,
            decompress
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("webcommons_services").factory("compressionService", [compressionService]);
    //#endregion

})(angular);