(function (angular, mobileServices) {
    "use strict";

    function cameraService($cordovaCamera, fileConstants) {
        //#region Utils
        const config = {
            options: {
                quality: 20,
                destinationType: Camera.DestinationType.FILE_URI,
                sourceType: Camera.PictureSourceType.CAMERA,
                mediaType: Camera.MediaType.PICTURE,
                encodingType: Camera.EncodingType.PNG,
                cameraDirection: Camera.Direction.BACK,
                targetWidth: window.innerWidth / 2,
                targetHeight: window.innerHeight / 2,
                allowEdit: false,
                saveToPhotoAlbum: false,
                correctOrientation: true,
                // popoverOptions: new CameraPopoverOptions(300, 300, 100, 100, Camera.PopoverArrowDirection.ARROW_ANY), --> iOS only
            }
        };

        function mergedOptions(options) {
            if (!options) return config.options;
            const merged = angular.copy(config.options);
            angular.forEach(options, (value, key) => merged[key] = value);
            return merged;
        }
        //#endregion

        //#region Public methods
        
        /**
         * Captures data from camera.
         * 
         * @param {cordova.Camera.options} options 
         * @returns {Promise<String>} image's path (cache) | image's base64 encoded content
         */
        function capture(options) {
            const optionsToUse = mergedOptions(options);
            return $cordovaCamera.getPicture(optionsToUse);
        }

        /**
         * Captures camera data as base64 encoded content.
         * 
         * @param {cordova.Camera.options} options 
         * @returns {Promise<fileConstants.FileContentWrapper>} 
         */
        function captureData(options) {
            const optionsToUse = options || {};
            optionsToUse.destinationType = Camera.DestinationType.DATA_URL;
            return capture(optionsToUse).then(data => new fileConstants.FileContentWrapper(`data:image/jpeg;base64,${data}`));
        }

        /**
         * Captures data from camera as file url.
         * 
         * @param {cordova.Camera.options} options 
         * @returns {Promise<fileConstants.FilePathWrapper>} 
         */
        function captureFile(options) {
            const optionsToUse = options || {};
            optionsToUse.destinationType = Camera.DestinationType.FILE_URI;
            return capture(optionsToUse).then(url => new fileConstants.FilePathWrapper(url));
        }

        //#endregion

        //#region Service Instance
        const service = {
            capture,
            captureData,
            captureFile,
            FileContentWrapper,
            FilePathWrapper
        };
        return service;
        //#endregion
    }

    //#region Service registration
    mobileServices.factory("cameraService", ["$cordovaCamera", "fileConstants", cameraService]);
    //#endregion

})(angular, mobileServices);