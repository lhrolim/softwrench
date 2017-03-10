(function (angular, cordova) {
    "use strict";

    /**
     * Manages rolling log files
     * v1.0.0
     * 
     * @author https://github.com/morinted/angular-cordova-rolling-logs
     * @author rbotti: changed source -> support for varargs (like $log and console.log), formatting errors, strict-mode, es6
     */
    const logModule = angular.module('rollingLog', []);

    // copied and adapted from angular.js's $log.formatError
    function formatToMessage(arg) {
        if (arg instanceof Error) {
            if (arg.stack) {
                arg = (arg.message && arg.stack.indexOf(arg.message) < 0)
                    ? `Error: ${arg.message}\n${arg.stack}`
                    : arg.stack;
            } else if (arg.sourceURL) {
                arg = `${arg.message}\n${arg.sourceURL}:${arg.line}`;
            }
            return arg;
        }
        if (!angular.isString(arg)) {
            try {
                const jsonValue = JSON.stringify(JSON.decycle(arg));
                return jsonValue;
            } catch (err) {
                return arg;
            }
            
        }
        return arg;
    }

    logModule.factory('$roll', ['$q', function ($q) {
            /**
             * CordovaFile is an internal service used to interface with
             * the filesystem while using promises instead of callbacks.
             */
            var cordovaFile = {
                // Instance variables to hold the current directory
                location: null,
                // And the current file writer
                writer: null   
            };

            // "Retrieve a DirectoryEntry or FileEntry using local URL"
            function resolveLocalFileSystemURL(directory) {
                const q = $q.defer();

                window.resolveLocalFileSystemURL(directory,
                     fileSystem => q.resolve(fileSystem),
                     err => q.reject(err)
                );

                return q.promise;
            }

            // "Request a file system in which to store application data."
            function getFilesystem() {
                const q = $q.defer();

                window.requestFileSystem(
                    LocalFileSystem.PERSISTENT, 1024 * 1024,
                    filesystem => q.resolve(filesystem),
                    err => q.reject(err)
                );

                return q.promise;
            }

            // Removes a file by its path
            cordovaFile.removeFile = function (filePath) {
                const q = $q.defer();

                getFilesystem().then(filesystem => {
                    filesystem.root.getFile(filePath, { create: false },
                        fileEntry => 
                            fileEntry.remove(() =>
                                q.resolve()
                            )
                        );
                });

                return q.promise;
            };

            // Moves a file by its current file entry to a new location/name
            cordovaFile.moveFile = function (fileEntry, newName) {
                const q = $q.defer();

                fileEntry.moveTo(cordovaFile.location, newName,
                    success => q.resolve(success),
                    error => q.reject(error)
                );

                return q.promise;
            };

            // Returns a promise that resolves the filesize of the file entry
            cordovaFile.getFileSize = function (fileEntry) {
                const q = $q.defer();

                fileEntry.file(
                    file => q.resolve(file.size),
                    err => q.reject(err)
                );

                return q.promise;
            };

            // Get file returns a file entry given a file path/name
            cordovaFile.getFile = function (fileName) {
                const q = $q.defer();

                if (!cordovaFile.location) {
                    q.reject('Please set location before getting a file.');
                } else {
                    // Get file if it exists, create if not
                    cordovaFile.location.getFile(fileName, { create: true, exclusive: false },
                        aFileEntry => q.resolve(aFileEntry),
                        err => q.reject(err)
                    );
                }

                return q.promise;
            };

            // Sets the current writer of cordovaFile, given a file entry.
            cordovaFile.setWriter = function (file) {
                const q = $q.defer();

                file.createWriter(
                    success => {
                        cordovaFile.writer = success;
                        // Seek to end of file for appending
                        cordovaFile.writer.seek(cordovaFile.writer.length);
                        q.resolve(cordovaFile.writer);
                    },
                    err => q.reject(err)
                );

                return q.promise;
            };

            // Appends 'toWrite' to the end of cordovaFile's current writer
            // See: setWriter
            cordovaFile.write = function (toWrite) {
                const q = $q.defer();

                if (!cordovaFile.writer) {
                    q.reject("No writer defined. Please use setWriter before writing.");
                } else {
                    cordovaFile.writer.onwrite = () => q.resolve('Write to file success.');
                        
                    cordovaFile.writer.onerror = err => q.reject(err);
                        
                    cordovaFile.writer.write(toWrite);
                }

                return q.promise;
            };

            // Sets the cordovaFile directory
            cordovaFile.setLocation = function (path) {
                const q = $q.defer();

                resolveLocalFileSystemURL(path)
                    .then(success => {
                        cordovaFile.location = success;
                        q.resolve(success);
                    },
                    err => q.reject(err));

                return q.promise;
            };

            // Rolling log external service
            return {
                logs: [], // holds logs in memory before writing to file
                logCurr: '.0', // suffix for active log
                logLast: '.1', // suffix for old log
                started: false, // whether logging has started

                /**
                 * Internal use: activate or deactivate pause listener
                 */
                listenForPause: function (listen) {
                    if (listen) {
                        document.addEventListener('pause', this.getOnPause(), false);
                    } else {
                        document.removeEventListener('pause', this.getOnPause(), false);
                    }
                },

                /**
                 * Internal use: Writes logs to file based on config, also performs
                 * the roll.
                 */
                writeToFile: function (logs) {
                    if (!logs || logs.length <= 0) {
                        return $q.when("Success");
                    }
                    var log = "";
                    var logFile;
                    const deferredWrite = $q.defer();
                    const logCurr = this.config.prefix + this.logCurr;
                    const logLast = this.config.prefix + this.logLast;
                    // Get file entry based on log file name
                    cordovaFile.getFile(logCurr)
                        .then(aFileEntry => {
                            logFile = aFileEntry;
                            return cordovaFile.getFileSize(logFile);
                        })
                        .then(size => {
                            if (size > this.config.logSize) {
                                // We then return the original log all over again
                                return cordovaFile.moveFile(logFile, logLast)
                                    .then(success => 
                                        cordovaFile.getFile(logCurr)
                                    )
                                    .then(aFileEntry => {
                                        logFile = aFileEntry;
                                        return cordovaFile.setWriter(logFile);
                                    });
                            }
                            return cordovaFile.setWriter(logFile);
                        })
                        .then(() => { // we don't need the file writer that this returned
                            while (logs.length > 0) {
                                log += `\n${logs.shift()}`;
                            }
                            return cordovaFile.write(log);
                        })
                        .then(success => {
                            deferredWrite.resolve("success");
                        })
                        .catch(err => {
                            console.error(err, "Error writing to log file.. Adding old logs to logs variable for next time.");
                            logs.push(...log.split("\n"));
                            deferredWrite.reject(err);
                        })
                        .finally(() => {
                            this.started = true;
                        });

                    return deferredWrite.promise;
                },


                /**
                 * Internal use: Writes log to console, adds log to in-memory list,
                 * then calls to file if it is time.
                 */
                writeLog: function (level, args) {
                    // no messages to log
                    if (args.length <= 0) {
                        return;
                    }
                    const message = [].slice.call(args).map(formatToMessage).join(" ");
                    this.logs.push(message);
                    // add to queue for writing
                    // Only write to file if queue is large enough
                    // or on error.
                    if (this.started && (level === "error" || this.logs.length > this.config.eventBuffer)) {
                        this.started = false;
                        this.writeToFile(this.logs, this.directory);
                    }
                },

                /**
                * Internal use: Function called on device pause when
                * config.writeOnPause is enabled
                */
                getOnPause: function () {
                    // We need to give context to the callback
                    var logFactory = this;
                    return function () {
                        logFactory.writeNow();
                    };
                },


                // Default config
                config: {
                    logSize: 25600, // Size of files, in bytes
                    eventBuffer: 25, // Number of events before write
                    debug: false, // Write debug messages
                    console: false, // Write to JS console with $log
                    writeOnPause: false,
                    prefix: 'log',
                    directory: 'dataDirectory',
                },

                /**
                 * Use setConfig to change from the default config options above
                 */
                setConfig: function (options) {
                    if (!options) {
                        return this.config;
                    }
                    if (options.logSize !== undefined) {
                        this.config.logSize = parseInt(options.logSize, 10);
                    }
                    if (options.eventBuffer !== undefined) {
                        this.config.eventBuffer = parseInt(options.eventBuffer, 10);
                    }
                    if (options.debug !== undefined) {
                        this.config.debug = !!options.debug;
                    }
                    if (options.console !== undefined) {
                        this.config.console = !!options.console;
                    }
                    if (options.writeOnPause !== undefined) {
                        this.config.writeOnPause = !!options.writeOnPause;
                        this.listenForPause(this.config.writeOnPause);
                    }
                    if (options.prefix !== undefined) {
                        if (options.prefix.length >= 1) {
                            this.config.prefix = options.prefix;
                        }
                    }
                    if (options.directory !== undefined) {
                        // TODO check legality
                        this.config.directory = options.directory;
                    }
                    return this.config;
                },

                /**
                 * Starts logging. User must call once the device is
                 * ready so that the module doesn't start before Cordova initiates
                 */
                start: function () {
                    var deferredStart = $q.defer(), directory;
                    try {
                        if (!window.cordova) {
                            throw "Cordova not found";
                        }
                        directory = cordova.file[this.config.directory];
                        this.debug(`Setting location to Cordova file data directory: ${directory}`);
                        cordovaFile.setLocation(directory)
                            .then(success => {
                                this.started = true;
                                deferredStart.resolve('Rolling logger started.');
                            }, error => {
                                console.error(error);
                                deferredStart.reject('Rolling logger unable to start.');
                            });
                    } catch (e) {
                        console.error(e);
                        console.error('No file logging during this session');
                        deferredStart.reject('Rolling logger unable to start');
                    }
                    return deferredStart.promise;
                },

                /**
                 * Write in-memory logs immediately, disregarding the eventBuffer
                 * Called when device is paused if config.writeOnPause is enabled
                 */
                writeNow: function () {
                    return this.started
                        ? this.writeToFile(this.logs, this.directory)
                        : $q.reject("No file to write to");
                },

                /**
                 * Main log calls, mirroring $log in name.
                 */
                log: function () {
                    this.writeLog('log', arguments);
                },
                info: function () {
                    this.writeLog('info', arguments);
                },
                error: function () {
                    this.writeLog('error', arguments);
                },
                debug: function () {
                    this.writeLog('debug', arguments);
                },
                warn: function() {
                    this.writeLog("warn", arguments);
                },
                trace: function() {
                    this.writeLog("trace", arguments);
                }
            };
        }
    ]);

})(angular, cordova);