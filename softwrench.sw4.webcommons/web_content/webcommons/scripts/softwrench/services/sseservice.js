(function (angular, EventSource) {
    "use strict";

    function sseService($log, $timeout) {
        //#region Utils

        /**
         * Wrapper on top of Event.
         * The data will be a parsed content (according to the dataType).
         * Original event can be accessed in originalEvent.
         */ 
        class ServerSentEvent {
            constructor(event, dataType) {
                this.data = event.data ? this. _parseData(event.data, dataType) : null;
                this.origin = event.origin;
                this.type = event.type;
                this.timeStamp = event.timeStamp;
                this.originalEvent = event;
            }
            _parseData(data, dataType) {
                switch (dataType) {
                    case "json":
                        return JSON.parse(data);
                    case "xml":
                        return $.parseXML(data);
                    case "html":
                        return $.parseHTML(data);
                    case "text":
                    default:
                        return String(data);
                }
            }
        }

        /**
         * Wrapper on top of html5's EventSource api with a a fluent api, internal logging and more 
         * descriptive/user-friendly method signatures.
         */
        class EventSourceAdapter {
            constructor(log, timeout, url) {
                this._$log = log;
                this._logger = this._$log.get("sseService#EventSourceAdapter", ["sse"]);
                this._$timeout = timeout;
                this._url = url;
                this._events = new Set();
                this._logger.debug(`Connecting to eventsource '${this._url}'`);
                this._eventSource = new EventSource(this._url, { withCredentials: true });
            }
            _eventHandlerAdapter(handler, config) {
                const eventHandler = !config.runApply ? handler : event => this._$timeout(() => handler(event));
                return event => {
                    this._logger.debug(`Event '${event.type}' received.`);
                    const sse = new ServerSentEvent(event, config.dataType);
                    eventHandler(sse);
                }
            }
            /**
             * Adds an event listener to the connected EventSource.
             * 
             * @param {String} event event's name
             * @param {Function} handler callback receiving the SSE as parameter
             * @param {Object} 
             *          runApply: Boolean whether or not to run in angular's $digest cycle (uses $timeout for safety)
             *          dataType: String 'json'|'text'|'xml'|'html'. SSE.data (in handler) will have the data parsed accordingly.
             * @returns {EventSourceAdapter} this instance 
             */
            on(event, handler, config) {
                config = config || {};
                this._logger.debug(`EventSource '${this._url}' listening to '${event}'`);
                this._events.add(event);
                const eventHandler = this._eventHandlerAdapter(handler, config);
                this._eventSource.addEventListener(event, eventHandler, false);
                return this;
            }
            /**
             * Shortcut to `on("message", handler, config)`.
             * 
             * @param {Function} handler callback receiving the SSE's data as parsed JSON object  
             * @param {Object} config 
             * @returns {EventSourceAdapter} this instance 
             */
            onMessage(handler, config) {
                return this.on("message", handler, config);
            }
            /**
             * Shortcut to `on("error", handler, config)`.
             * 
             * @param {Function} handler callback receiving the SSE's data as parsed JSON object  
             * @param {Object} config 
             * @returns {EventSourceAdapter} this instance 
             */
            onError(handler, config) {
                return this.on("error", handler, config);
            }
            onOpen(handler, runApply) {
                return this.on("open", handler, { runApply });
            }
            onClose(handler, runApply) {
                const disconnectHandler = event => {
                    if (event.readyState === EventSource.CLOSED) handler(event);
                }
                return this.onError(disconnectHandler, { runApply });
            }
            /**
             * Closes the EventSource's connection.
             */
            close() {
                this._logger.debug(`Closing connection to EventSource '${this._url}'`);
                this._eventSource.close();
            }
            toString() {
                const view = { url: this._url, events: Array.from(this._events.values()) };
                return JSON.stringify(view);
            }
        }
        //#endregion

        //#region Public methods

        function connect(controller, action) {
            const sourceUrl = url(`/api/generic/${controller}${action ? `/${action}` : ""}`);
            return new EventSourceAdapter($log, $timeout, sourceUrl);
        }

        //#endregion

        //#region Service Instance
        const service = {
            connect,
        };
        return service;
        //#endregion
    }

    //#region Service registration

    angular.module("webcommons_services").factory("sseService", ["$log", "$timeout", sseService]);

    //#endregion

})(angular, window.EventSource);