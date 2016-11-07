(function (angular, $) {
    "use strict";

    var staticvalidFileTypes = ["pdf", "zip", "txt", "doc", "docx", "dwg", "gif", "jpg", "csv", "xls", "xlsx", "pptx", "ppt", "xml", "xsl", "bmp", "html", "las", "avi", "jpeg", "mp3", "mp4", "z7", "rar", "AVI", "eml", "emz", "evt", "evtx", "mp4", "log", "LAS", "MDB", "PNG", "JPEG", "config", "dat", "lic", "ora", "eml", "png", "js", "exe"];

    angular.module("sw_layout").factory("attachmentService", ["$log","$rootScope", "$q", "$timeout", "contextService", "fieldService", "schemaService", "alertService", "i18NService", "searchService", "tabsService", "redirectService", "$http", "userService", "crudContextHolderService", "fileService", attachmentService]);

    function attachmentService($log,$rootScope, $q, $timeout, contextService, fieldService, schemaService, alertService, i18NService, searchService, tabsService, redirectService, $http, userService, crudContextHolderService, fileService) {

        $rootScope.$on("sw.attachment.file.changed", function (event, fileNames) {
            const panelId = crudContextHolderService.isShowingModal() ? "#modal" : null;
            const dm = crudContextHolderService.rootDataMap(panelId);
            const log = $log.get("attachmentService#filechange", ["attachment"]);


            if (!fileNames || !nullOrEmpty(dm.document)) {
                log.warn(`no file name found`);
                return;
            }

            $timeout(() => {
                dm.document = angular.isArray(fileNames) ? fileNames.join(",") : fileNames;
            });
            
        });

        const service = {
            isValid,
            selectAttachment,
            fetchDownloadUrl,
            redirectToAttachmentView,
            createAttachmentFromFile,
            createAttachmentFromElement,
            validateRemoval
        };
        return service;

        //#region Utils
        function downloadErrorMessage(docinfoId) {
            return String.format(i18NService.get18nValue("download.error", "Error downloading file with id {0}. Please, Contact your Administrator"), docinfoId);
        }

        function fetchDownloadUrl(item) {
            const id = item["docinfoid"];
            const params = { id: id };
            const serviceUrl = url(`/Attachment/DownloadUrl?${$.param(params)}`);
            return $http.get(serviceUrl, { avoidspin: true }).then(response => response.data);
        }

        function downloadFile(item) {
            const id = item["docinfoid"];
            const parameters = { id: id, mode: "http" };
            const errorMessage = downloadErrorMessage(id);

            const controllerUrl = url(`/Attachment/Download?${$.param(parameters)}`);
            return fileService.downloadPromise(controllerUrl).catch(() => alertService.alert(errorMessage));
        }

        function getUrl(item) {
            const docinfoid = item.docinfoid.toString();
            const searchData = { docinfoid };
            return searchService.searchWithData("attachment", searchData)
                .then(response => {
                    const data = response.data;
                    const resultObject = data.resultObject;
                    const resultFields = resultObject[0];
                    const resultUrl = resultFields["docinfo_.urlname"];
                    window.open(resultUrl);
                }).catch(error => {
                    const errorMessage = downloadErrorMessage(docinfoid);
                    alertService.alert(errorMessage);
                });
        }

        function readFile(file) {
            var deferred = $q.defer();
            var reader = new FileReader();

            reader.addEventListener("loadend", function (event) {
                deferred.resolve(reader.result);
            });
            reader.addEventListener("error", function (error) {
                deferred.reject(error);
            });

            reader.readAsDataURL(file);
            return deferred.promise;
        }

        function newAttachmentFileName(extension) {
            return "attachment{0}.{1}".format(Date.now().getTime(), extension);
        }

        function broadCastAttachmentLoaded(file) {
            const timer = $timeout(() => {
                $rootScope.$broadcast("sw.attachment.file.load", file);
            }, 500); // empirically determined

            return timer.then(() => file);
        }
        //#endregion


        //#region Public

        /**
         * Returns whether or not the file has a valid type by checking it's extension against a list of valid extensions.
         * If no list is provided, will compare against a list stored in contextService['allowedfiles'].
         * If the list in contextService does not exist, will compare against a static list of validTypes
         * (["pdf", "zip", "txt", "doc", "docx", "dwg", "gif", "jpg", "csv", "xls", "xlsx", "ppt", "xml", "xsl", "bmp", "html", "png", "lic"]).
         * 
         * @param {String} value file extension or filename
         * @param {Array<String>} types optional list of valid extensions
         * @returns {Boolean}
         */
        function isValid(value, types) {
            if (!value) return false;
            // var fileName = value.match(/[^\/\\]+$/);
            const validFileTypes = types || contextService.fetchFromContext("allowedfiles", true) || staticvalidFileTypes;
            const extensionIdx = value.lastIndexOf(".");
            const extension = value.substring(extensionIdx + 1).toLowerCase();
            return $.inArray(extension, validFileTypes) !== -1;
        }

        /**
         * Downloads the contents of the selected attachment item.
         * 
         * @param {Datamap} item 
         * @returns {Boolean} false
         */
        function selectAttachment(item) {
            if (item["docinfo_.urltype"] === "URL") {
                getUrl(item);
            } else {
                downloadFile(item);
            }
            return false;
        }

        /**
         * Redirects the user to the attachment composition tab.
         * 
         * @param {Schema} schema detail schema containing the attachment tab
         * @returns {Promise} 
         */
        function redirectToAttachmentView(schema) {
            // find the attachment view and redirect to it
            const tabs = tabsService.tabsDisplayables(schema);
            if (!tabs) {
                const errorMessage = "no displayable tabs for schema {0}.{1}".format(schema.applicationName, schema.schemaId);
                return $q.reject(new Error(errorMessage));
            }
            const attachmentTab = tabs.find(tab => tab["tabId"].startsWith("attachment"));
            if (!attachmentTab) {
                const errorMessage = "no attachment tab for schema {0}.{1}".format(schema.applicationName, schema.schemaId);
                return $q.reject(new Error(errorMessage));
            }
            // redirect to view
            return redirectService.redirectToTab(attachmentTab["tabId"]);
        }

        /**
         * Resolves a file wrapper instance from a DataTransferItem|File|Blob.
         * - will redirect the view to a visible attachment tab
         * - will fire "sw.attachment.file.load" event with the file wrapper as argument 
         * 
         * @param {DataTransferItem|Blob|File} file
         * @param {Schema} schema attachment's parent schema
         * @returns {Promise} resolved with a file wrapper dto instance: 
         *          {
         *               file: String, // content
         *               name: String, // file's name
         *               type: String, // mime type
         *               size: Number // size in bytes
         *           } 
         */
        function createAttachmentFromFile(file, schema) {
            // file data has to be querried before returning the promise (can't be done inside the callbacks)
            // because the file gets disposed (no content and empty properties) after the function returns
            const blob = (file instanceof Blob || file instanceof File) ? file : file.getAsFile();
            const extension = file.type.split("/")[1];
            const fileName = !!blob.name ? blob.name : newAttachmentFileName(extension);

            return redirectToAttachmentView(schema)
                .then(() => readFile(blob)) // read content
                .then(content => ({ // build dto
                    file: content,
                    name: fileName,
                    type: file.type,
                    size: blob.size
                }))
                .then(broadCastAttachmentLoaded);
        }

        /**
         * Resolves a file wrapper instance from an HTMLNode that will receive a pasted image (FF trick).
         * - will redirect the view to a visible attachment tab
         * - will fire "sw.attachment.file.load" event with the file wrapper as argument
         * 
         * @param HTMLNode contentHolder DOM node with `contenteditable='true'` that will receive the pasted content as an image Node
         * @param {} schema attachment's parent schema
         * @returns Promise resolved with a file wrapper dto instance: 
         *          {
         *               file: String, // content
         *               name: String, // file's name
         *               type: String, // mime type
         *           }  
         */
        function createAttachmentFromElement(contentHolder, schema) {

            var promise = null;

            const waitForImage = function () {
                // polling the Node to see if the image node was appended
                if (!contentHolder.childNodes || contentHolder.childNodes.length <= 0) {
                    promise = $timeout(waitForImage, 20, false);
                    return promise;
                }
                // get the image as base64 encoded from child image Node
                const child = contentHolder.childNodes[0];
                contentHolder.innerHTML = "";
                if (!child) return $q.reject(new Error("image was not pasted"));
                if (child.tagName !== "IMG") return $q.reject(new Error("only support images are supported"));
                return child.src;
            };
            // begin polling for image
            promise = $timeout(waitForImage, 0, false);

            // use timer promise prior to the redirect so the image validation can happen before trying to redirect
            return promise.then(content => {
                // redirect then chain and resolve with the file
                return redirectToAttachmentView(schema).then(() => {
                    return {
                        file: content,
                        name: newAttachmentFileName("png"),
                        type: "image/png"
                    }
                });
            })
            .then(broadCastAttachmentLoaded);
        }


        function validateRemoval(datamap, schema) {
            if (datamap["createby"].toLowerCase() === userService.getPersonId().toLowerCase()) {
                return $q.when();
            }

            alertService.alert("Cannot delete an attachment that was created by another user");
            return $q.reject();
        }

        //#endregion
    }

})(angular, jQuery);
