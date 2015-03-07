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

String.prototype.format = String.prototype.f = function () {
    var s = this,
    i = arguments.length;
    while (i--) {
        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return s;
};

String.prototype.in = String.prototype.f = function () {
    var s = this;
    return s.equalsAny(arguments);
};

String.prototype.equalsAny = String.prototype.f = function () {
    var s = this,
    i = arguments.length;
    while (i--) {
        if (arguments[i].isEqual(s, true)) {
            return true;
        }
    }
    return false;
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

String.prototype.equalIc = String.prototype.f = function (other) {
    var s = this;
    if (other == null) {
        return false;
    }
    return s.toLowerCase() == other.toLowerCase();
};


var nullOrEmpty = function (s) {
    return nullOrUndef(s) || String(s).trim().length == 0;
};

var isArrayNullOrEmpty = function (arr) {
    return nullOrUndef(arr) || arr.length == 0;
};

String.format = function () {
    var s = arguments[0];
    for (var i = 0; i < arguments.length - 1; i++) {
        var reg = new RegExp("\\{" + i + "\\}", "gm");
        s = s.replace(reg, arguments[i + 1]);
    }
    return s;
};



