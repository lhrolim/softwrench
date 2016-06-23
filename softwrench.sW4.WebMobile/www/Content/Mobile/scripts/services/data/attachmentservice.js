(function (angular, mobileServices) {
    "use strict";

    function offlineAttachmentService($log, cameraService, $cordovaFile, fileConstants, crudContextHolderService, swdbDAO, offlineSchemaService) {
        //#region Utils
        const config = {
            newAttachmentFieldName: "newattachment",
            newAttachmentPathFieldName: "newattachment_path",
            fileNameFieldName: "document",
            defaultBaseFileName: "camera.jpg"
            //newAttachmentCompressedFlagFieldName: "#is_compressed"
        };

        function newFileName(application, fileName) {
            return `attachment_${application}_${Date.now()}_${fileName || config.defaultBaseFileName}`;
        }

        function createAttachmentEntity(attachment) {
            return swdbDAO.instantiate("Attachment", attachment).then(a => swdbDAO.save(a));
        }
        //#endregion

        //#region Public methods

        /**
         * Finds the attachments of a root entity.
         * 
         * @param {String} application 
         * @param {String} parentId 
         * @returns {Promise<persistence.entity.Attachment>} 
         */
        function getAttachment(application, parentId) {
            Validate.notEmpty(application, "application");
            Validate.notEmpty(parentId, "parentId");
            return swdbDAO.findByQuery("Attachment", `application='${application}' and parentId='${parentId}'`);
        }

        /**
         * Takes picture, moves cached image file to app's external storage and sets the file path in the datamap.
         * 
         * @param {Schema} schema current schema on display
         * @param {Datamap} datamap current datamap on display
         * @returns {Promise<String>} resolved with the file path
         */
        function attachCameraPictureAsFile(schema, datamap) {
            return cameraService.captureFile()
                .then(file => {
                    const application = crudContextHolderService.currentApplicationName();
                    return $cordovaFile.copyFile(file.dirPath, file.fileName, cordova.file[fileConstants.appDirectory], newFileName(application, file.fileName));
                })
                .then(fileEntry => datamap[config.newAttachmentFieldName] = fileEntry.nativeURL);
        }

        /**
         * Takes a picture and sets the base64 encoded content in the datamap.
         * 
         * @param {Schema} schema 
         * @param {Datamap} datamap 
         * @returns {Promise<string>} base64 encoded picture's content 
         */
        function attachCameraPicture(schema, datamap) {
            return cameraService.captureData().then(content => {
                const application = crudContextHolderService.currentApplicationName();
                const field = offlineSchemaService.getFieldByAttribute(schema, config.newAttachmentFieldName);

                field.rendererParameters["showImagePreview"] = true;
                datamap[config.fileNameFieldName] = datamap[config.newAttachmentPathFieldName] = newFileName(application);
                datamap[config.newAttachmentFieldName] = content.data.value;
            });
        }

        /**
         * Creates an Attachment entity from the base64 encoded content present in the datamap.
         * 
         * @param {Schema} schema 
         * @param {Datamap} datamap 
         * @returns {Promise<persistence.Entity.Attachment>} resolved with the saved attachment
         */
        function saveAttachment(schema, datamap) {
            const attachment = {
                application: crudContextHolderService.currentApplicationName(),
                parentId: crudContextHolderService.currentDetailItem().remoteId,
                content: datamap[config.newAttachmentFieldName],
                compressed: false,
                compositionRemoteId: null
            };
            return createAttachmentEntity(attachment);
        }

        /**
         * Creates an Attachment entity from the file path present in the datamap.
         * 
         * @param {Schema} schema 
         * @param {Datamap} datamap 
         * @returns {Promise<persistence.Entity.Attachment>} resolved with the saved Attachment
         */
        function saveAttachmentAsFile(schema, datamap) {
            const attachment = {
                application: crudContextHolderService.currentApplicationName(),
                parentId: crudContextHolderService.currentDetailItem().remoteId,
                path: datamap[config.newAttachmentFieldName]
            };
            return createAttachmentEntity(attachment);
        }

        /**
         * Deletes the file from the attachment file path present in the datamap.
         * 
         * @param {Schema} schema 
         * @param {Datamap} datamap 
         * @returns {Promise<Void>} 
         */
        function deleteAttachmentFile(schema, datamap) {
            const log = $log.get("attachmentService#cancelAttachment", ["attachment"]);
            const attachmentPath = datamap[config.newAttachmentFieldName];
            log.debug(`deleting ${attachmentPath}`);
            const file = new fileConstants.FilePathWrapper(attachmentPath);
            return $cordovaFile.removeFile(file.dirPath, file.fileName)
                .then(() => log.debug(`Successfully deleted ${attachmentPath}`))
                .catch(e => log.warn(`Could not delete ${attachmentPath}`, e));
        }

        //#endregion

        //#region Service Instance
        const service = {
            // general
            getAttachment,
            // file
            attachCameraPictureAsFile,
            saveAttachmentAsFile,
            deleteAttachmentFile,
            // content
            attachCameraPicture,
            saveAttachment
        };
        return service;
        //#endregion
    }

    //#region Service registration
    mobileServices.factory("offlineAttachmentService",
        ["$log", "cameraService", "$cordovaFile", "fileConstants", "crudContextHolderService", "swdbDAO", "offlineSchemaService", offlineAttachmentService]);
    //#endregion

})(angular, mobileServices);