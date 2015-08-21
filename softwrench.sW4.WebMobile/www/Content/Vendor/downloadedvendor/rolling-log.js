(function (angular, cordova) {
    "use strict";

    /*global $, cordova, LocalFileSystem, FileReader */
    /**
    * Manages rolling log files
    * v1.0.0
     * 
     * @author https://github.com/morinted/angular-cordova-rolling-logs
     * @author rbotti: changed source -> support for varargs (like $log and console.log), formatting errors, strict-mode
    */
    var logModule = angular.module('rollingLog', []);

    // copied and adapted from angular.js's $log.formatError
    function formatToMessage(arg) {
        if (arg instanceof Error) {
            if (arg.stack) {
                arg = (arg.message && arg.stack.indexOf(arg.message) === -1)
                    ? 'Error: ' + arg.message + '\n' + arg.stack
                    : arg.stack;
            } else if (arg.sourceURL) {
                arg = arg.message + '\n' + arg.sourceURL + ':' + arg.line;
            }
            return arg;
        }
        if (!angular.isString(arg)) {
            return angular.toJson(arg);
        }
        return arg;
    }

    logModule.factory('$roll', ['$q', function ($q) {
            /*
            * CordovaFile is an internal service used to interface with
            * the filesystem while using promises instead of callbacks.
            **/
            var cordovaFile = {};

            // Instance variables to hold the current directory
            cordovaFile.location = null;
            // And the current file writer
            cordovaFile.writer = null;

            // "Retrieve a DirectoryEntry or FileEntry using local URL"
            function resolveLocalFileSystemURL(directory) {
                var q = $q.defer();
                window.resolveLocalFileSystemURL(directory,
                    function (fileSystem) {
                        q.resolve(fileSystem);
                    },
                    function (err) {
                        q.reject(err);
                    });

                return q.promise;
            }

            // "Request a file system in which to store application data."
            function getFilesystem() {
                var q = $q.defer();

                window.requestFileSystem(LocalFileSystem.PERSISTENT,
                            1024 * 1024, function (filesystem) {
                                q.resolve(filesystem);
                            },
                    function (err) {
                        q.reject(err);
                    });

                return q.promise;
            }

            // Removes a file by its path
            cordovaFile.removeFile = function (filePath) {
                var q = $q.defer();

                getFilesystem().then(
                    function (filesystem) {
                        filesystem.root.getFile(filePath, { create: false },
                            function (fileEntry) {
                                fileEntry.remove(function () {
                                    q.resolve();
                                });
                            });
                    }
                );

                return q.promise;
            };

            // Moves a file by its current file entry to a new location/name
            cordovaFile.moveFile = function (fileEntry, newName) {
                var q = $q.defer();

                fileEntry.moveTo(cordovaFile.location, newName,
                    function (success) {
                        q.resolve(success);
                    },
                    function (error) {
                        q.reject(error);
                    });

                return q.promise;
            };

            // Returns a promise that resolves the filesize of the file entry
            cordovaFile.getFileSize = function (fileEntry) {
                var q = $q.defer();

                fileEntry.file(
                    function (file) {
                        q.resolve(file.size);
                    },
                    function (err) {
                        q.reject(err);
                    }
                );
                return q.promise;
            };

            // Get file returns a file entry given a file path/name
            cordovaFile.getFile = function (fileName) {
                var q = $q.defer();

                if (!cordovaFile.location) {
                    q.reject('Please set location before getting a file.');
                } else {
                    // Get file if it exists, create if not
                    cordovaFile.location.getFile(fileName,
                        { create: true, exclusive: false },
                        function (aFileEntry) {
                            q.resolve(aFileEntry);
                        },
                        function (err) {
                            q.reject(err);
                        });
                }

                return q.promise;
            };

            // Sets the current writer of cordovaFile, given a file entry.
            cordovaFile.setWriter = function (file) {
                var q = $q.defer();

                file.createWriter(
                    function (success) {
                        cordovaFile.writer = success;
                        // Seek to end of file for appending
                        cordovaFile.writer.seek(cordovaFile.writer.length);
                        q.resolve(cordovaFile.writer);
                    },
                    function (err) {
                        q.reject(err);
                    }
                );

                return q.promise;
            };

            // Appends 'toWrite' to the end of cordovaFile's current writer
            // See: setWriter
            cordovaFile.write = function (toWrite) {
                var q = $q.defer();

                if (!cordovaFile.writer) {
                    q.reject(
                        "No writer defined. Please use setWriter before writing."
                    );
                } else {
                    cordovaFile.writer.onwrite =
                        function () {
                            q.resolve('Write to file success.');
                        };
                    cordovaFile.writer.onerror =
                        function (err) {
                            q.reject(err);
                        };
                    cordovaFile.writer.write(toWrite);
                }

                return q.promise;
            };

            // Sets the cordovaFile directory
            cordovaFile.setLocation = function (path) {
                var q = $q.defer();
                resolveLocalFileSystemURL(path)
                    .then(function (success) {
                        cordovaFile.location = success;
                        q.resolve(success);
                    }, function (err) {
                        q.reject(err);
                    });

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
                    var log = '',
                        logFile,
                        deferredWrite = $q.defer(),
                        logFactory = this, // scope changes inside 'then'
                        logCurr = this.config.prefix + this.logCurr,
                        logLast = this.config.prefix + this.logLast;
                    // Get file entry based on log file name
                    cordovaFile.getFile(logCurr)
                        .then(function (aFileEntry) {
                            logFile = aFileEntry;
                            return cordovaFile.getFileSize(logFile);
                        }).then(function (size) {
                            if (size > logFactory.config.logSize) {
                                //$log.info(
                                //    'Log is over capacity. Moving old log to ' +
                                //        logLast
                                //);
                                // We then return the original log all over again
                                return cordovaFile.moveFile(logFile,
                                    logLast).then(function (success) {
                                        // logFactory.debug(success);
                                        return cordovaFile.getFile(logCurr);
                                    }).then(function (aFileEntry) {
                                        logFile = aFileEntry;
                                        return cordovaFile.setWriter(logFile);
                                    });
                            }
                            return cordovaFile.setWriter(logFile);
                        })
                        .then(// we don't need the file writer that this returned
                            function () {
                                while (logs.length > 0) {
                                    log += '\n' + logs.shift();
                                }
                                return cordovaFile.write(log);
                            }
                        )
                        .then(function (success) {
                            //$log.debug('Wrote logs to log file successfully: ' +
                            //    success);
                            deferredWrite.resolve('success');
                            logFactory.started = true;
                        })
                        .catch(function (err) {
                            console.error(err);
                            console.error(
                                'Error writing to log file.. Adding old' +
                                    ' logs to logs variable for next time.'
                            );
                            logs.push(log);
                            logFactory.started = true;
                            deferredWrite.reject(err);
                        });
                    return deferredWrite.promise;
                },


                /**
                * Internal use: Writes log to console, adds log to in-memory list,
                * then calls to file if it is time.
                */
                writeLog: function (level, args) {
                    //// first argument is log level
                    //var level = arguments[0];
                    //// second argument is a varargs with the messages
                    //var args = 2 <= arguments.length ? [].slice.call(arguments, 1) : [];
                    
                    // no messages to log
                    if (args.length <= 0) {
                        return;
                    }

                    // logging to console
                    //if (this.config.console) {
                    //    if (level === 'log') {
                    //        $log.log(args);
                    //    } else if (level === 'info') {
                    //        $log.info(args);
                    //    } else if (level === 'warn') {
                    //        $log.warn(args);
                    //    } else if (level === 'error') {
                    //        $log.error(args);
                    //    }  else if (level === 'debug' && this.config.debug) {
                    //        $log.debug(args);
                    //    } else {
                    //        return;
                    //    }
                    //}
                    // logging to file

                    //preparing the messages
                    //var displayType = level === "log" ? "" : " - " + level.toUpperCase();
                    //var time = new Date().toISOString();
                    //var logHeader = time + displayType + ": ";
                    //var msgs = [logHeader];
                    var msgs = [];
                    angular.forEach(args, function(arg) {
                        msgs.push(formatToMessage(arg));
                    });
                    this.logs.push(msgs.join(" "));
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
                        logFactory.debug('App pausing');
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
                    if (options === undefined) {
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
                    var deferredStart = $q.defer(), directory,
                        logFactory = this;
                    try {
                        if (!window.cordova) {
                            throw "Cordova not found";
                        }
                        directory = cordova.file[this.config.directory];
                        this.debug('Setting location to Cordova ' +
                                         'file data directory: ' + directory);
                        cordovaFile.setLocation(directory)
                            .then(function (success) {
                                logFactory.started = true;
                                // logFactory.debug('Location set!');
                                // logFactory.debug(success);
                                deferredStart.resolve('Rolling logger started.');
                            }, function (error) {
                                // console.error(error);
                                deferredStart
                                    .reject('Rolling logger unable to start.');
                            });
                    } catch (e) {
                        this.debug(e);
                        this.error('No file logging during this session');
                        deferredStart.reject('Rolling logger unable to start');
                    }
                    return deferredStart.promise;
                },

                /**
                * Write in-memory logs immediately, disregarding the eventBuffer
                * Called when device is paused if config.writeOnPause is enabled
                */
                writeNow: function () {
                    if (this.started) {
                        this.debug('Writing changes to log file');
                        return this.writeToFile(this.logs, this.directory);
                    }
                    return $q.reject('No file to write to');
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