(function (angular, mobileServices) {
    "use strict";

    function attachmentService($log, cameraService, $cordovaFile, fileConstants, crudContextService, swdbDAO) {
        //#region Utils
        const config = {
            attachmentFieldName: "newattachment_path"
        };

        function newFileName(application, fileName) {
            return `attachment_${application}_${Date.now()}_${fileName}`;
        }
        //#endregion

        //#region Public methods

        /**
         * Takes picture, moves cached image file to app's external storage and sets the file path in the datamap.
         * 
         * @param {Schema} schema current schema on display
         * @param {Datamap} datamap current datamap on display
         * @returns {Promise<String>} resolved with the file path
         */
        function attachCameraPicture(schema, datamap) {
            const application = crudContextService.currentApplicationName();
            return cameraService.captureFile()
                .then(file =>
                    $cordovaFile.copyFile(file.dirPath, file.fileName, cordova.file[fileConstants.appDirectory], newFileName(application, file.fileName))
                )
                .then(fileEntry => datamap[config.attachmentFieldName] = fileEntry.nativeURL);
        }

        /**
         * Creates Attachment from the file path present in the datamap.
         * 
         * @param {Schema} schema 
         * @param {Datamap} datamap 
         * @returns {Promise<persistence.Entity.Attachment>} resolved with the saved Attachment
         */
        function saveAttachment(schema, datamap) {
            const application = crudContextService.currentApplicationName();
            const parentId = crudContextService.currentDetailItem().id;
            const path = datamap[config.attachmentFieldName];
            return swdbDAO.intantiate("Attachment", { application, parentId, path }).then(attachment => swdbDAO.save(attachment));
        }

        /**
         * Deletes the file from the attachment file path present in the datamap.
         * 
         * @param {Schema} schema 
         * @param {Datamap} datamap 
         * @returns {Promise<Void>} 
         */
        function cancelAttachment(schema, datamap) {
            const log = $log.get("attachmentService#cancelAttachment", ["attachment"]);
            const attachmentPath = datamap[config.attachmentFieldName];
            log.debug(`deleting ${attachmentPath}`);
            const file = new fileConstants.FilePathWrapper(attachmentPath);
            return $cordovaFile.removeFile(file.dirPath, file.fileName)
                .then(() => log.debug(`Successfully deleted ${attachmentPath}`))
                .catch(e => log.warn(`Could not delete ${attachmentPath}`, e));
        }

        //#endregion

        //#region Service Instance
        const service = {
            attachCameraPicture,
            saveAttachment,
            cancelAttachment
        };
        return service;
        //#endregion
    }

    //#region Service registration
    mobileServices.factory("attachmentService", ["$log", "cameraService", "$cordovaFile", "fileConstants", "crudContextService", "swdbDAO", attachmentService]);
    //#endregion

})(angular, mobileServices);