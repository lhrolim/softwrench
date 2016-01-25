if (typeof String.prototype.endsWith !== 'function') {
    String.prototype.endsWith = function (suffix) {
        return this.indexOf(suffix, this.length - suffix.length) !== -1;
    };
}

if (typeof String.prototype.startsWith != 'function') {
    String.prototype.startsWith = function (str) {
        if (str == undefined) {
            return false;
        }
        return this.slice(0, str.length) == str;
    };
}

var BrowserDetect =
{
    init: function () {
        this.browser = this.searchString(this.dataBrowser) || "Other";
        this.version = this.searchVersion(navigator.userAgent) || this.searchVersion(navigator.appVersion) || "Unknown";
    },

    searchString: function (data) {
        for (var i = 0 ; i < data.length ; i++) {
            var dataString = data[i].string;
            this.versionSearchString = data[i].subString;

            if (dataString.indexOf(data[i].subString) != -1) {
                return data[i].identity;
            }
        }
    },

    searchVersion: function (dataString) {
        var index = dataString.indexOf(this.versionSearchString);
        if (index == -1) return;
        return parseFloat(dataString.substring(index + this.versionSearchString.length + 1));
    },

    dataBrowser:
    [
        { string: navigator.userAgent, subString: "Chrome", identity: "Chrome" },
        { string: navigator.userAgent, subString: "MSIE", identity: "Explorer" },
        { string: navigator.userAgent, subString: "Firefox", identity: "Firefox" },
        { string: navigator.userAgent, subString: "Safari", identity: "Safari" },
        { string: navigator.userAgent, subString: "Opera", identity: "Opera" }
    ]

};
BrowserDetect.init();




var DeviceDetect =
{
    init: function () {
        this.os = this.searchString(this.dataOS) || 'Unknown';
        this.catagory = this.getType(this.os) || 'Unknown';
    },

    searchString: function (data) {
        for (var i = 0 ; i < data.length ; i++) {
            var dataString = data[i].string;
            this.versionSearchString = data[i].subString;

            if (dataString.indexOf(data[i].subString) != -1) {
                return data[i].identity;
            }
        }
    },

    getType: function (os) {
        if (os === 'Unknown') {
            return;
        }

        var catagory;

        switch (os) {
            case 'Windows':
            case 'Macintosh':
            default:
                catagory = 'Desktop';
                break;
            case 'Android':
            case 'iPhone':
            case 'iPad':
                catagory = 'Mobile';
        }

        return catagory;
    },

    dataOS:
    [
        { string: navigator.userAgent, subString: 'Windows', identity: 'Windows' },
        { string: navigator.userAgent, subString: 'Macintosh', identity: 'Macintosh' },
        { string: navigator.userAgent, subString: 'Android', identity: 'Android' },
        { string: navigator.userAgent, subString: 'iPhone', identity: 'iPhone' },
        { string: navigator.userAgent, subString: 'iPad', identity: 'iPad' }
    ]

};
DeviceDetect.init();


function isMobile() {
    return DeviceDetect.catagory == 'Mobile';
};

function isDesktop() {
    return DeviceDetect.catagory == 'Desktop';
};

function safeCSSselector(name) {

    //if column has an attribute property, else return an empty string
    if (name) {
        //make sure name is all lower case
        var newName = name.toLowerCase();

        //replace invaild characters with underscores

        //TODO: make more robust, add additional invalid characters
        newName = newName.replace('.', '_');
        newName = newName.replace('#', '_');

        return newName;
    } else {
        return "";
    }
}



function instantiateIfUndefined(obj, nullcheck) {
    var shouldNullCheck = nullcheck === undefined || nullcheck == true;
    if (obj === undefined || (shouldNullCheck && obj === null)) {
        obj = {};
    }
    return obj;
}

function FillRelationship(obj, property, valueToSet) {
    property = property.replace(/_/g, '_\.');
    property = property.replace(/\.\./g, '\.');
    return JsonProperty(obj, property, valueToSet, true);
}

function GetRelationshipName(property) {
    property = property.replace(/_/g, '_\.');
    property = property.replace(/\.\./g, '\.');
    return property;
}

function RemoveSpecialChars(id) {
    //remove all occurences of special chars (except _) (to remove # mainly)
    return id.replace(/[^\w\s]/gi, '');
}

function JsonProperty(obj, property, valueToSet, forceCreation) {

    var prop = property.split('.');
    var value = obj;
    for (var i = 0; i < prop.length; i++) {
        if (value.hasOwnProperty(prop[i])) {
            value = value[prop[i]];
        } else {
            if (valueToSet != undefined || forceCreation) {
                if (i == prop.length - 1) {
                    value[prop[i]] = valueToSet;
                } else {
                    value[prop[i]] = {};
                }
                value = value[prop[i]];
            } else {
                return null;
            }
        }
    }

    return value;
}

Date.prototype.mmddyyyy = Date.prototype.f = function () {
    var yyyy = this.getFullYear().toString();
    var mm = (this.getMonth() + 1).toString(); // getMonth() is zero-based
    var dd = this.getDate().toString();
    return (mm[1] ? mm : "0" + mm[0]) + "/" + (dd[1] ? dd : "0" + dd[0]) + "/" + yyyy; // padding
};

/****************String functions****************************************************/

String.prototype.format = String.prototype.f = function () {
    var s = this,
    i = arguments.length;
    while (i--) {
        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return s;
};

String.prototype.nullOrEmpty = String.prototype.f = function () {
    var s = this;
    return s.trim().length == 0;
};

String.prototype.isEqual = String.prototype.f = function (other, ignoreCase) {
    var s = this;
    if (!ignoreCase) {
        return s == other;
    }
    if (other == null) {
        return false;
    }
    return s.toLowerCase() == other.toLowerCase();
};

String.prototype.contains = function(str) {
    return this.indexOf(str) >= 0;
};

Array.prototype.subarray = function (start, end) {
    if (!end) {
        end = -1;
    }
    return this.slice(start, this.length + 1 - (end * -1));
}

function nullOrCommaSplit(value) {
    if (value == null || value === "") {
        return null;
    }
    return value.split(",");
}

function emptyIfNull(value) {
    if (value == null) {
        return "";
    }
    return value;
}

var nullOrEmpty = function (s) {
    return nullOrUndef(s) || String(s).trim().length == 0;
};

var isArrayNullOrEmpty = function (arr) {
    return nullOrUndef(arr) || arr.length == 0;
};

var hasSingleElement = function (arr) {
    return !nullOrUndef(arr) && arr.length == 1;
};

var safePush = function (baseObject, propertyName, item) {
    if (!baseObject[propertyName]) {
        baseObject[propertyName] = [];
    }
    baseObject[propertyName].push(item);
}

var nullifyProperties = function (baseObject, propertyArray) {
    if (!propertyArray || !baseObject) {
        return;
    }

    for (var i = 0; i < propertyArray.length; i++) {
        var propName = propertyArray[i];
        baseObject[propName] = null;
    }
}

String.format = function () {
    var s = arguments[0];
    for (var i = 0; i < arguments.length - 1; i++) {
        var reg = new RegExp("\\{" + i + "\\}", "gm");
        s = s.replace(reg, arguments[i + 1]);
    }
    return s;
};

$.extend({
    findFirst: function (elems, validateCb) {
        if (elems == null) {
            return undefined;
        }
        var i;
        for (i = 0 ; i < elems.length ; ++i) {
            if (validateCb(elems[i], i))
                return elems[i];
        }
        return undefined;
    }
});



function nullOrUndef(obj) {
    return obj === undefined || obj == null;
}

function insertOrUpdateArray(arr, item, property) {
    property = property || "id";
    var idx = -1;
    for (var i = 0; i < arr.length; i++) {
        if (arr[i][property] == item[property]) {
            idx = i;
            break;
        }
    }
    if (idx == -1) {
        arr.push(item);
    } else {
        arr[idx] = item;
    }
}

function lockCommandBars() {
    var bars = $('[data-class=commandbar]');
    bars.each(function (index, element) {
        var buttons = $(element).find('button');
        for (var i = 0; i < buttons.length; i++) {
            var button = $(buttons[i]);
            if (button.prop('disabled') !== true) {
                //lets disable only those who aren´t already disabled
                button.attr('disabled', 'disabled');
                button.attr('forceddisable', 'disabled');
            }
        }

    });
}

function unLockCommandBars() {
    var bars = $('[data-class=commandbar]');
    bars.each(function (index, element) {
        var buttons = $(element).find('button');
        for (var i = 0; i < buttons.length; i++) {
            var button = $(buttons[i]);
            if (button.attr('forceddisable') === 'disabled') {
                button.removeAttr('disabled');
            }
        }
    });
}

function lockTabs() {
    var tabs = $('[data-toggle=tab]');
    tabs.each(function (index, element) {
        var jquery = $(element);
        jquery.removeAttr('data-toggle');
        jquery.attr('data-toggle', 'tab_inactive');
        //        jquery.css('cursor', 'no-drop');
    });
}

function unLockTabs() {
    var tabs = $('[data-toggle=tab_inactive]');
    tabs.each(function (index, element) {
        var jquery = $(element);
        jquery.removeAttr('data-toggle');
        jquery.attr('data-toggle', 'tab');
        //        jquery.css('cursor', null);
    });
}

function capitaliseFirstLetter(string) {
    var fullstring = string.split(/[ ]+/);
    var returnstring = '';
    for (var i = 0; i < fullstring.length; i++) {
        returnstring += fullstring[i].charAt(0).toUpperCase() + fullstring[i].slice(1);
        returnstring += (i + 1) == fullstring.length ? '' : ' ';
    }
    return returnstring;
}

function isIe9() {
    if ("true" == sessionStorage.mockie9) {
        return true;
    }
    return BrowserDetect.browser == "Explorer" && (BrowserDetect.version == '9' || BrowserDetect.version == '8' || BrowserDetect.version == '7');
};

function isChrome() {
    return BrowserDetect.browser == "Chrome";
};

function isFirefox() {
    return BrowserDetect.browser == "Firefox";
};

$.extend({
    keys: function (obj) {
        var a = [];
        $.each(obj, function (k) { a.push(k) });
        return a;
    }
});


function loadScript(baseurl, callback) {
    // Adding the script tag to the head as suggested before
    var head = document.getElementsByTagName('head')[0];
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = url(baseurl);

    // Then bind the event to the callback function.
    // There are several events for cross browser compatibility.
    script.onreadystatechange = callback;
    script.onload = callback;

    // Fire the loading
    head.appendChild(script);
}

function url(path) {
    if (angular.mock || window.cordova) {
        //this means we´re running under test scenarios or on offline mode
        return path;
    }

    if (path == null) {
        return null;
    }

    var root = location.protocol + '//' + location.hostname +
        (location.port ? ":" + location.port : "");

    if (root.search("localhost:") == -1) {
        root += "/softWrench";
    }

    if (path && path[0] != "/") {
        path = "/" + path;
    }


    var value = $(routes_basecontext)[0].value;
    if (value == "/") {
        return path;
    }
    return value + path;
}

function GetPopUpMode() {
    if (angular.mock) {
        //TODO: extract a service
        return "none";
    }

    var popupMode = $(hddn_popupmode)[0].value;
    if (popupMode === undefined || popupMode == 'null' || popupMode == "") {
        popupMode = 'none';
    }
    return popupMode;
}

function BuildDataObject() {
    var parameters = {};
    var data = $(crud_InitialData)[0].value;
    if (data === undefined || data == null || data == '') {
        return parameters;
    }
    parameters.data = data;
    return parameters;
}

function isString(o) {
    return typeof o == "string" || (typeof o == "object" && o.constructor === String);
}

function platformQS() {
    return "platform=" + platform();
}

function platform() {
    return "web";
}

function detailSchema() {
    return "detail";
}

function listSchema() {
    return "list";
}

function executeFunctionByName(functionName, context /*, args */) {
    var args = Array.prototype.slice.call(arguments).splice(2);
    var namespaces = functionName.split(".");
    var func = namespaces.pop();
    for (var i = 0; i < namespaces.length; i++) {
        context = context[namespaces[i]];
    }
    return context[func].apply(this, args);
}

function addCurrentSchemaDataToJson(json, schema) {
    json["%%currentschema"] = {};
    json["%%currentschema"].schemaId = schema.schemaId;
    json["%%currentschema"].mode = schema.mode;
    json["%%currentschema"].platform = platform();
    return json;
}

function addSchemaDataToJson(json, schema, nextSchemaObj) {
    json = addCurrentSchemaDataToJson(json, schema);
    if (nextSchemaObj) {
        json["%%nextschema"] = {};
        json["%%nextschema"].schemaId = nextSchemaObj.schemaId;
        if (nextSchemaObj.nextSchemaMode != null) {
            json["%%nextschema"].mode = nextSchemaObj.schemaMode;
        }
        json["%%nextschema"].platform = platform();
    }

    else if (schema.properties != null && schema.properties['nextschema.schemaid'] != null) {
        var nextschemaId = schema.properties['nextschema.schemaid'];
        json["%%nextschema"] = {};
        json["%%nextschema"].schemaId = nextschemaId;
        if (schema.properties['nextschema.schemamode'] != null) {
            json["%%nextschema"].mode = schema.properties['nextschema.schemamode'];
        }
        json["%%nextschema"].platform = platform();
    }
    return json;
}



function removeEncoding(crudUrl) {
    //this first one indicates an []
    crudUrl = replaceAll(crudUrl, "%5B%5D", "");
    crudUrl = replaceAll(crudUrl, "%5B", ".");
    crudUrl = replaceAll(crudUrl, "%5D", "");
    return crudUrl;
}

function getCurrentDate() {
    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth() + 1; //January is 0!
    var yyyy = today.getFullYear();

    if (dd < 10) {
        dd = '0' + dd
    }

    if (mm < 10) {
        mm = '0' + mm
    }

    today = mm + '/' + dd + '/' + yyyy;
    return today;
}

function replaceAll(str, find, replace) {
    if (str == null) {
        return null;
    }
    return str.replace(new RegExp(find, 'g'), replace);
}

function imgToBase64(img) {
    var canvas = document.createElement("canvas");
    canvas.width = img.width;
    canvas.height = img.height;

    // Copy the image contents to the canvas
    var ctx = canvas.getContext("2d");
    ctx.drawImage(img, 0, 0);

    // Get the data-URL formatted image
    // Firefox supports PNG and JPEG. You could check img.src to
    // guess the original format, but be aware the using "image/jpg"
    // will re-encode the image.
    var dataURL = canvas.toDataURL("image/png");

    return dataURL;
}







function printWindow() {
    window.print();
}

function isEmpty(obj) {

    // null and undefined are "empty"
    if (obj == null) return true;

    // Assume if it has a length property with a non-zero value
    // that that property is correct.
    if (obj.length > 0) return false;
    if (obj.length === 0) return true;

    // Otherwise, does it have any properties of its own?
    // Note that this doesn't handle
    // toString and valueOf enumeration bugs in IE < 9
    for (var key in obj) {
        if (hasOwnProperty.call(obj, key)) return false;
    }

    return true;
}

/**
* Removes duplicates in object arrays. 
* ex.: var arr = [{id:1}, {id:2}, {id:1}, {id:3}];
* removeDuplicatesOnArray(arr, "id");  -----> [{id:1}, {id:2}, {id:3}]
* 
* @param {Object[]} array 
* @param {string} the name of unique key of the object to verify dulicates
* @returns {Object[]} Array without duplicates
*/
function removeDuplicatesOnArray(array, keyProp) {
    if (!array || !keyProp) {
        return array;
    }
    var indexes = [];
    var withoutDuplicates = [];
    array.forEach(function (option) {
        var index = option[keyProp];
        if (indexes.indexOf(index) >= 0) {
            return;
        }
        indexes.push(index);
        withoutDuplicates.push(option);
    });
    return withoutDuplicates;
}

/* Base64 encode / decode http://www.webtoolkit.info/ */
var Base64 = {
    // private property
    _keyStr: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=",

    // public method for encoding
    encode: function (input) {
        var output = "";
        var chr1, chr2, chr3, enc1, enc2, enc3, enc4;
        var i = 0;

        while (i < input.length) {

            chr1 = input.charCodeAt(i++);
            chr2 = input.charCodeAt(i++);
            chr3 = input.charCodeAt(i++);

            enc1 = chr1 >> 2;
            enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
            enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
            enc4 = chr3 & 63;

            if (isNaN(chr2)) {
                enc3 = enc4 = 64;
            } else if (isNaN(chr3)) {
                enc4 = 64;
            }

            output = output +
            this._keyStr.charAt(enc1) + this._keyStr.charAt(enc2) +
            this._keyStr.charAt(enc3) + this._keyStr.charAt(enc4);

        }

        return output;
    },

    // public method for decoding
    decode: function (input) {
        var output = "";
        var chr1, chr2, chr3;
        var enc1, enc2, enc3, enc4;
        var i = 0;

        input = input.replace(/[^A-Za-z0-9\+\/\=]/g, "");

        while (i < input.length) {

            enc1 = this._keyStr.indexOf(input.charAt(i++));
            enc2 = this._keyStr.indexOf(input.charAt(i++));
            enc3 = this._keyStr.indexOf(input.charAt(i++));
            enc4 = this._keyStr.indexOf(input.charAt(i++));

            chr1 = (enc1 << 2) | (enc2 >> 4);
            chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
            chr3 = ((enc3 & 3) << 6) | enc4;

            output = output + String.fromCharCode(chr1);

            if (enc3 != 64) {
                output = output + String.fromCharCode(chr2);
            }
            if (enc4 != 64) {
                output = output + String.fromCharCode(chr3);
            }

        }

        return output;

    },

    // private method for UTF-8 encoding
    _utf8_encode: function (string) {
        string = string.replace(/\r\n/g, "\n");
        var utftext = "";

        for (var n = 0; n < string.length; n++) {

            var c = string.charCodeAt(n);

            if (c < 128) {
                utftext += String.fromCharCode(c);
            }
            else if ((c > 127) && (c < 2048)) {
                utftext += String.fromCharCode((c >> 6) | 192);
                utftext += String.fromCharCode((c & 63) | 128);
            }
            else {
                utftext += String.fromCharCode((c >> 12) | 224);
                utftext += String.fromCharCode(((c >> 6) & 63) | 128);
                utftext += String.fromCharCode((c & 63) | 128);
            }

        }

        return utftext;
    },

    // private method for UTF-8 decoding
    _utf8_decode: function (utftext) {
        var string = "";
        var i = 0;
        var c = c1 = c2 = 0;

        while (i < utftext.length) {

            c = utftext.charCodeAt(i);

            if (c < 128) {
                string += String.fromCharCode(c);
                i++;
            }
            else if ((c > 191) && (c < 224)) {
                c2 = utftext.charCodeAt(i + 1);
                string += String.fromCharCode(((c & 31) << 6) | (c2 & 63));
                i += 2;
            }
            else {
                c2 = utftext.charCodeAt(i + 1);
                c3 = utftext.charCodeAt(i + 2);
                string += String.fromCharCode(((c & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63));
                i += 3;
            }

        }

        return string;
    }

}


// only create debounce function if underscore is not present
if (typeof (window.debounce !== "function") && (!window._ || typeof (window._.debounce) !== "function")) {
    /**
     * Returns a function, that, as long as it continues to be invoked, will not
     * be triggered. The function will be called after it stops being called for
     * N milliseconds. If `immediate` is passed, trigger the function on the
     * leading edge, instead of the trailing. 
     * (from http://davidwalsh.name/javascript-debounce-function)
     * 
     * @param Function func 
     * @param Long wait milliseconds
     * @param Boolean immediate 
     * @returns debounced function 
     */
    function debounce(func, wait, immediate) {
        var timeout;
        return function () {
            var context = this, args = arguments;
            var later = function () {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    };
}

// only create throttle function if underscore is not present
if (typeof (window.throttle !== "function") && (!window._ || typeof (window._.throttle) !== "function")) {

    /**
     * Creates and returns a new, throttled version of the passed function, that, 
     * when invoked repeatedly, will only actually call the original function at most once per every wait milliseconds. 
     * Useful for rate-limiting events that occur faster than you can keep up with. 
     * By default, throttle will execute the function as soon as you call it for the first time, 
     * and, if you call it again any number of times during the wait period, as soon as that period is over. 
     * If you'd like to disable the leading-edge call, pass {leading: false}, 
     * and if you'd like to disable the execution on the trailing-edge, pass {trailing: false}.
     * (http://underscorejs.org/#throttle)
     * 
     * @param Function fn function to throttle
     * @param Number threshhold throttle interval in milliseconds
     * @param {} options
     * @returns Function throttle function 
     */
    function throttle(func, wait, options) {
        var context, args, result;
        var timeout = null;
        var previous = 0;
        if (!options) options = {};
        var later = function () {
            previous = options.leading === false ? 0 : Date.now();
            timeout = null;
            result = func.apply(context, args);
            if (!timeout) context = args = null;
        };
        return function () {
            var now = Date.now();
            if (!previous && options.leading === false) previous = now;
            var remaining = wait - (now - previous);
            context = this;
            args = arguments;
            if (remaining <= 0 || remaining > wait) {
                if (timeout) {
                    clearTimeout(timeout);
                    timeout = null;
                }
                previous = now;
                result = func.apply(context, args);
                if (!timeout) context = args = null;
            } else if (!timeout && options.trailing !== false) {
                timeout = setTimeout(later, remaining);
            }
            return result;
        };
    }
}