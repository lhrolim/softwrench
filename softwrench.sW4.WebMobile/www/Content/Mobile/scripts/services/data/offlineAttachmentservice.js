(function (angular, mobileServices) {
    "use strict";

    function offlineAttachmentService($log, $q, cameraService, $cordovaFile, fileConstants, crudContextHolderService, swdbDAO, offlineSchemaService, entities, loadingService, swAlertPopup) {
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


        function base64ToBlob(b64Data, contentType, sliceSize) {
            contentType = contentType || "";
            sliceSize = sliceSize || 512;
            const byteCharacters = atob(b64Data);
            const byteArrays = [];
            for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
                const slice = byteCharacters.slice(offset, offset + sliceSize);
                const byteNumbers = new Array(slice.length);
                for (let i = 0; i < slice.length; i++) {
                    byteNumbers[i] = slice.charCodeAt(i);
                }
                const byteArray = new Uint8Array(byteNumbers);
                byteArrays.push(byteArray);
            }
            const blob = new Blob(byteArrays, { type: contentType });
            return blob;
        }

        function createFile(fileName, base64Data, mimeType) {

            var deferred = $q.defer();
            const log = $log.get("attachmentService#createFile", ["attachment"]);

            window.requestFileSystem(window.TEMPORARY, 5 * 1024 * 1024, function (fs) {
                log.debug('file system open directory: ' + fs.name);

                fs.root.getFile(fileName, { create: true, exclusive: false }, function (fileEntry) {

                    fileEntry.createWriter(function (fileWriter) {
                        fileWriter.onwriteend = function () {
                            log.info("Successful file read...");
                            deferred.resolve(fileEntry);
                        };
                        fileWriter.onerror = function (e) {
                            deferred.reject();
                        };
                        fileWriter.write(base64ToBlob(base64Data, mimeType));
                    });

                }, ()=>deferred.reject());

            });

            return deferred.promise;

            // Creates a new file or returns the file if it already exists.

        }

        function createAndLoadTempFile(fileName, attachment, docinfoId) {
            const deferred = $q.defer();

            const mimeType = attachment.mimetype;
            const cachedPath = attachment.path;

            const creationPromise = cachedPath ? $q.when({
                toInternalURL: function () {
                    return attachment.path;
                }
            }) : createFile(fileName, attachment.content, mimeType);
            const log = $log.get("attachmentService#createAndLoadTempFile", ["attachment"]);

            creationPromise.then(fileEntry => {
                var path = fileEntry.toURL();
                
                // if(ionic.Platform.isIOS()) {
                // TODO: might not be the optmimal way to open office docs. Use: https://github.com/pwlin/cordova-plugin-fileopenener2/issues/60
                const ref = window.open(path, "_blank", "location=yes,hidden=no,closebuttoncaption=Close");
                ref.addEventListener("loadstop", evt => deferred.resolve());
                ref.addEventListener("loaderror", evt => deferred.reject(evt));
                    
                /*} else {
                    cordova.plugins.fileOpener2.open(path, mimeType, {
                        error: () => {
                            deferred.reject();
                            log.warn("couldn´t open file {0} at location {1}".format(docinfoId,path));
                        },
                        success: () => {
                            if (cachedPath) {
                                log.info("opening cached file at location {0}".format(path));
                                deferred.resolve();
                                return;
                            }

                            var queryObj = {
                                query: entities.Attachment.UpdateAttachmentPath,
                                args: [path, docinfoId]
                            }
                            swdbDAO.executeQuery(queryObj).then(() => {
                                //updating attachment path so that next time we can (try) load the same file.
                                log.debug("updating cached location {0} for file {1}".format(path, docinfoId));
                                deferred.resolve();
                            });
                        },
                    });
                }*/
            }).catch(
                error=> deferred.reject(error)
            );

            return deferred.promise;

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

        function handleBase64Prefix(content, mimeType) {
            const replacementString = `data:${mimeType};base64,`;
            const indexOf = content.indexOf(replacementString);
            if (indexOf !== -1) {
                //data stored locally need to be 
                return content.substring(replacementString.length);
            }
            return content;
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
                datamap["#mimetype"] = "image/jpeg";
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
                parentId: crudContextHolderService.currentDetailItem().id,
                content: datamap[config.newAttachmentFieldName],
                mimetype: datamap["#mimetype"],
                compressed: false,
                compositionRemoteId: null
            };
            attachment.content = handleBase64Prefix(attachment.content, attachment.mimetype);
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
                parentId: crudContextHolderService.currentDetailItem().id,
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

        function loadRealAttachment(doclinkEntry) {

            const fileName = doclinkEntry.document;
            const docinfoId = doclinkEntry.docinfoid;
            const localHash = doclinkEntry["#offlinehash"];

            const log = $log.get("attachmentService#loadRealAttachment", ["attachment"]);
            const queryObj = {
                query: !!docinfoId ? entities.Attachment.ByDocInfoId : entities.Attachment.ByHashId,
                args: !!docinfoId ? [String(docinfoId)] : [String(localHash)]
            };
            const deferred = $q.defer();
            loadingService.showDefault();
            swdbDAO.executeQuery(queryObj).then(result => {
                if (result.length === 0 || result[0].content==null) {
                    deferred.reject();
                    const error = `Could not open attachment ${docinfoId}, not found on database...`;
                    log.warn(error);
                    swAlertPopup.show({
                        title: error //TODO: maybe create a message for the popup?
                    });
                    loadingService.hide();
                    return;
                }
                var attachment = result[0];

                var mimeType = attachment.mimetype;
                var extension = mimeType.substring(mimeType.indexOf("/") + 1);

                if (isRippleEmulator()) {
                    swAlertPopup.show({
                        title: "File Cannot be openend on Ripple" //TODO: maybe create a message for the popup?
                    });
                    deferred.resolve();
                    loadingService.hide();
                    return;
                }

                if (ionic.Platform.isAndroid()) {
                    const content = handleBase64Prefix(attachment.content, attachment.mimetype);
                    cordova.plugins.fileOpener2.openBase64(fileName, extension, content, mimeType, {
                        error: function (e) {
                            loadingService.hide();
                            deferred.reject();
                        },
                        success: function () {
                            loadingService.hide();
                            deferred.resolve();
                        }
                    });
                } else if (ionic.Platform.isIOS()) {
                    // For IOS we need to create a temp file and load it afterwards, since there´s no plugin support for loading it straight from base64 string
                    //trying to open already cached temp file
                    createAndLoadTempFile(fileName, attachment, docinfoId)
                        .then(() => deferred.resolve())
                        .catch(() => {
                            if (!attachment.path) {
                                //file failed to create/load and
                                log.warn("could not open file load file {0}".format(docinfoId));
                                deferred.reject();
                                return;
                            }
                            //attachment path is no longer valid... cleaning and trying again
                            log.warn("could not open file at path {0}, creating temporary one... ".format(path));
                            attachment.path = null;
                            createAndLoadTempFile(fileName, attachment, docinfoId)
                                .then(() => deferred.resolve())
                                .catch(() => deferred.reject())
                                .finally(() => loadingService.hide());
                        }).finally(() => loadingService.hide());
                }

            });

            return deferred.promise;

        }

        /**
         * Deletes the entities.Attachment related to the composition datamap.
         * 
         * @param {Datamap} composition 
         * @returns {Promise<Array<Void>>} 
         */
        function deleteRelatedAttachment(composition) {
            const attachmentHash = composition["#offlinehash"];
            return swdbDAO.executeStatement(entities.Attachment.DeleteById, [attachmentHash]);
        }

        /**
         * Deletes the entities.Attachments related to the compositions datamaps.
         * 
         * @param {Datamap} compositions 
         * @returns {Promise<Array<Void>>} 
         */
        function deleteRelatedAttachments(compositions) {
            const hashes = compositions.map(c => c["#offlinehash"]).filter(h => !!h).map(h => `'${h}'`);
            if (hashes.length <= 0) return $q.when();

            const query = entities.Attachment.DeleteMultipleByIdsPattern.format(hashes);
            return swdbDAO.executeQuery(query);
        }

        //#endregion

        //#region Service Instance
        const service = {
            // general
            getAttachment,
            loadRealAttachment,
            deleteRelatedAttachment,
            deleteRelatedAttachments,
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
        ["$log", "$q", "cameraService", "$cordovaFile", "fileConstants", "crudContextHolderService", "swdbDAO", "offlineSchemaService", "offlineEntities", "loadingService", "swAlertPopup", offlineAttachmentService]);
    //#endregion

})(angular, mobileServices);