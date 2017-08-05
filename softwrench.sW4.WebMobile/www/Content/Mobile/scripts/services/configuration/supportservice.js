(function (angular, cordova, _) {
    "use strict";

    function supportService($q, applicationStateService, $roll, $cordovaFile, $cordovaEmailComposer, networkConnectionService, $ionicPopup, $ionicModal, $rootScope, dao) {
        //#region Utils

        const config = {
            logs: {
                emailTo:"support@controltechnologysolutions.com",
                emailConfigKey: "/Offline/Support/Email",
                modalTemplateUrl: "Content/Mobile/templates/support_reportlog_modal.html",
                popup: {
                    title: "Log Reporting", 
                    template: "Do you wish to report to the support team"
                },
                ongoing: false
            }
        };

        /**
         * @returns {Promise<Array<String>>} absolute url of the log files
         */
        function getLogFiles() {
            return $cordovaFile.listDir(cordova.file[$roll.config.directory])
                .then(entries => 
                    entries
                        .filter(e => e.isFile && e.name.startsWith($roll.config.prefix)) // only files that are log files
                        .map(f => f.nativeURL) // absolute path
                );
        }

        function objectToHtml(object) {
            var html = "<ul>";
            angular.forEach(object, (value, key) => {
                if (angular.isFunction(value)) return;
                var valueHtml = value;
                if (!value) {
                    valueHtml = String(value);
                } else if (angular.isArray(value)) {
                    valueHtml = value.toString();
                }
                html += `<li>${key}: ${valueHtml}</li>`;
            });
            html += "</ul>";
            return html;
        }

        function stateToHtml(state) {
            var html = "";
            angular.forEach(state, (value, key) => {
                var valueHtml = "";
                if (key === "user") {
                    valueHtml += "<div>";
                    valueHtml += objectToHtml(_.omit(value, "meta", "properties"));
                    valueHtml += `<b>user.properties</b><br>${objectToHtml(value.properties)}`;
                    valueHtml += "</div>";
                } else if (key === "configs") {
                    valueHtml += "<div>";
                    valueHtml += `<b>configs.server</b><br>${objectToHtml(value.server)}`;
                    valueHtml += `<b>configs.client</b><br>${objectToHtml(value.client)}`;
                    valueHtml += "</div>";
                } else {
                    valueHtml = objectToHtml(value);
                }
                html += `<div><b>${key}</b><br><div>${valueHtml}</div></div>`;
            });
            return html;
        }

        function buildReportEmailMessage(state, userMessage) {
            const user = state.user;
            var message = `<div>softWrench Log Reporting from <b>${user.UserName}</b></div><br>`;
            if (userMessage) {
                message += `<div>${user.UserName} included the following message:<br>${userMessage}</div><br>`;
            }
            const stateHtml = stateToHtml(state);
            message += `
                <div>
                    <h3>Application state during the report<h3>
                    ${stateHtml}
                </div><br>`;
            message += "The log files are attached";
            return message;
        }

        function getSupportEmail() {
            return dao.findSingleByQuery("DataEntry", `application='_configuration' and datamap like '%"fullKey":"${config.logs.emailConfigKey}"%'`)
                .then(emailConfig => {
                    if (!emailConfig) {
                        return config.logs.emailTo;
                    }
                    const datamap = emailConfig.datamap;
                    return datamap.stringValue || datamap.defaultValue || config.logs.emailTo;
                });
        }

        function buildReportEmail({ subject = "", message = "" } = {}) {
            const promises = [
                applicationStateService.getStates(["settings", "configs", "user", "device"]),
                getLogFiles(),
                getSupportEmail()
            ];
            return $q.all(promises).spread((state, logs, supportEmail) => {
                const customer = state.configs.server.client.toUpperCase();
                var mailSubject = `[${customer}][SWOFF][LOG]`;
                if (subject) mailSubject += ` ${subject}`;
                
                const mailMessage = buildReportEmailMessage(state, message);
                const userMail = state.user.Email;
                
                const email = {
                    to: [supportEmail],
                    cc: [userMail],
                    subject: mailSubject,
                    body: mailMessage,
                    isHtml: true,
                    attachments: logs
                };
                return email;
            });
        }

        function getLogReportingModal() {
            // create an isolated $scope to hold the viewmodel
            const $scope = $rootScope.$new(true);
            $scope.email = { subject: null, message: null };
            // $scope method: report and dispose of the modal
            $scope.sendLogReport = function (email) {
                reportLogs(email)
                    .catch(error => $ionicPopup.alert({ title: "Error Sending Log Report", template: error.message }))
                    .finally(() => $scope.modal.remove());
            };
            return $ionicModal
                .fromTemplateUrl(getResourcePath(config.logs.modalTemplateUrl), {
                    scope: $scope,
                    animation: "slide-in-up",
                    hardwareBackButtonClose: true
                })
                // set reference to built modal so it can be disposed and return it
                .then(modal => $scope.modal = modal);
        }
        
        //#endregion

        //#region Public methods

        /**
         * Sends email to support reporting the log files 
         * and the current state of the application.
         * 
         * @param {Object} emailConfig 
         *                 {
         *                  subject: String // subject to be appended
         *                  message: String // message to be prepended
         *                 }
         * @returns {Promise} 
         */
        function reportLogs(emailConfig = {}) {
            if (isRippleEmulator() || !$roll.started) {
                return $q.reject(new Error("Logging is not enabled"));
            }
            if (networkConnectionService.isOffline()) {
                return $q.reject(new Error("Device is not connected to a network"));
            }
            return $roll.writeNow() // flush logs to file
                // check availability
                .then(() => $cordovaEmailComposer.isAvailable())
                // build email dto
                .then(() => buildReportEmail(emailConfig))
                // send email intent
                .then(email => 
                    $cordovaEmailComposer.open(email)
                        .catch(() => $q.when()) // user canceled email -> catch
                );
        }
        
        /**
         * Shows confirm popup requesting permission to report the logs.
         * If it is confirmed will open modal: 
         * - optional email fields
         * - button to send the log report
         * 
         * @param {String?} title confirm popup title
         * @param {String?} template confirm popup template
         * @returns {Promise} 
         */
        function requestLogReporting({ title = config.logs.popup.title, template = config.logs.popup.template } = {}) {
            // prompt user asking confirmation
            if (config.logs.ongoing) return;
            // make sure only a single report can run at a time 
            // --> uncaught $digest loop (e.g. bad expression inside ng-if) exceptions may flood the screen
            config.logs.ongoing = true;
            $ionicPopup.confirm({ title, template })
                // build additional email data form modal
                .then(res => res ? getLogReportingModal() : $q.reject())
                // show built modal
                .then(modal => modal.show())
                // swallow any exceptions
                .catch(e => e)
                // mark finished
                .finally(() => config.logs.ongoing = false);
        }

        //#endregion

        //#region Service Instance
        const service = {
            reportLogs,
            requestLogReporting: _.debounce(requestLogReporting, 500)
        };
        return service;
        //#endregion
    }

    //#region Service registration
    angular.module("sw_mobile_services")
        .factory("supportService", ["$q", "applicationStateService", "$roll", "$cordovaFile", "$cordovaEmailComposer", "networkConnectionService", "$ionicPopup", "$ionicModal", "$rootScope", "swdbDAO", supportService]);
    //#endregion

})(angular, cordova, _);