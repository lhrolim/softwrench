$(window).load(function () {
            $("#userTimezoneOffset").val(new Date().getTimezoneOffset());
            delete sessionStorage['schemaCache'];
            $('#btnLogin').click(function () {
                var username = $('#userName');
                var password = $('#password');
                var userNameMessage = $('#userNameMessage');
                var passwordMessage = $('#passwordMessage');

                if (username.val() == '') {
                    passwordMessage.hide();
                    if (userNameMessage.hide()) {
                        userNameMessage.toggle();
                    }
                    return false;
                }

                if (password.val() == '') {
                    userNameMessage.hide();
                    if (passwordMessage.hide()) {
                        passwordMessage.toggle();
                    }
                    return false;
                }
                return true;
            });
        }
);


;
/**
 * Version: 1.0 Alpha-1 
 * Build Date: 13-Nov-2007
 * Copyright (c) 2006-2007, Coolite Inc. (http://www.coolite.com/). All rights reserved.
 * License: Licensed under The MIT License. See license.txt and http://www.datejs.com/license/. 
 * Website: http://www.datejs.com/ or http://www.coolite.com/datejs/
 */
Date.CultureInfo = { name: "en-US", englishName: "English (United States)", nativeName: "English (United States)", dayNames: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"], abbreviatedDayNames: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"], shortestDayNames: ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"], firstLetterDayNames: ["S", "M", "T", "W", "T", "F", "S"], monthNames: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"], abbreviatedMonthNames: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"], amDesignator: "AM", pmDesignator: "PM", firstDayOfWeek: 0, twoDigitYearMax: 2029, dateElementOrder: "mdy", formatPatterns: { shortDate: "M/d/yyyy", longDate: "dddd, MMMM dd, yyyy", shortTime: "h:mm tt", longTime: "h:mm:ss tt", fullDateTime: "dddd, MMMM dd, yyyy h:mm:ss tt", sortableDateTime: "yyyy-MM-ddTHH:mm:ss", universalSortableDateTime: "yyyy-MM-dd HH:mm:ssZ", rfc1123: "ddd, dd MMM yyyy HH:mm:ss GMT", monthDay: "MMMM dd", yearMonth: "MMMM, yyyy" }, regexPatterns: { jan: /^jan(uary)?/i, feb: /^feb(ruary)?/i, mar: /^mar(ch)?/i, apr: /^apr(il)?/i, may: /^may/i, jun: /^jun(e)?/i, jul: /^jul(y)?/i, aug: /^aug(ust)?/i, sep: /^sep(t(ember)?)?/i, oct: /^oct(ober)?/i, nov: /^nov(ember)?/i, dec: /^dec(ember)?/i, sun: /^su(n(day)?)?/i, mon: /^mo(n(day)?)?/i, tue: /^tu(e(s(day)?)?)?/i, wed: /^we(d(nesday)?)?/i, thu: /^th(u(r(s(day)?)?)?)?/i, fri: /^fr(i(day)?)?/i, sat: /^sa(t(urday)?)?/i, future: /^next/i, past: /^last|past|prev(ious)?/i, add: /^(\+|after|from)/i, subtract: /^(\-|before|ago)/i, yesterday: /^yesterday/i, today: /^t(oday)?/i, tomorrow: /^tomorrow/i, now: /^n(ow)?/i, millisecond: /^ms|milli(second)?s?/i, second: /^sec(ond)?s?/i, minute: /^min(ute)?s?/i, hour: /^h(ou)?rs?/i, week: /^w(ee)?k/i, month: /^m(o(nth)?s?)?/i, day: /^d(ays?)?/i, year: /^y((ea)?rs?)?/i, shortMeridian: /^(a|p)/i, longMeridian: /^(a\.?m?\.?|p\.?m?\.?)/i, timezone: /^((e(s|d)t|c(s|d)t|m(s|d)t|p(s|d)t)|((gmt)?\s*(\+|\-)\s*\d\d\d\d?)|gmt)/i, ordinalSuffix: /^\s*(st|nd|rd|th)/i, timeContext: /^\s*(\:|a|p)/i }, abbreviatedTimeZoneStandard: { GMT: "-000", EST: "-0400", CST: "-0500", MST: "-0600", PST: "-0700" }, abbreviatedTimeZoneDST: { GMT: "-000", EDT: "-0500", CDT: "-0600", MDT: "-0700", PDT: "-0800" } };
Date.getMonthNumberFromName = function (name) {
    var n = Date.CultureInfo.monthNames, m = Date.CultureInfo.abbreviatedMonthNames, s = name.toLowerCase(); for (var i = 0; i < n.length; i++) { if (n[i].toLowerCase() == s || m[i].toLowerCase() == s) { return i; } }
    return -1;
}; Date.getDayNumberFromName = function (name) {
    var n = Date.CultureInfo.dayNames, m = Date.CultureInfo.abbreviatedDayNames, o = Date.CultureInfo.shortestDayNames, s = name.toLowerCase(); for (var i = 0; i < n.length; i++) { if (n[i].toLowerCase() == s || m[i].toLowerCase() == s) { return i; } }
    return -1;
}; Date.isLeapYear = function (year) { return (((year % 4 === 0) && (year % 100 !== 0)) || (year % 400 === 0)); }; Date.getDaysInMonth = function (year, month) { return [31, (Date.isLeapYear(year) ? 29 : 28), 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][month]; }; Date.getTimezoneOffset = function (s, dst) { return (dst || false) ? Date.CultureInfo.abbreviatedTimeZoneDST[s.toUpperCase()] : Date.CultureInfo.abbreviatedTimeZoneStandard[s.toUpperCase()]; }; Date.getTimezoneAbbreviation = function (offset, dst) {
    var n = (dst || false) ? Date.CultureInfo.abbreviatedTimeZoneDST : Date.CultureInfo.abbreviatedTimeZoneStandard, p; for (p in n) { if (n[p] === offset) { return p; } }
    return null;
}; Date.prototype.clone = function () { return new Date(this.getTime()); }; Date.prototype.compareTo = function (date) {
    if (isNaN(this)) { throw new Error(this); }
    if (date instanceof Date && !isNaN(date)) { return (this > date) ? 1 : (this < date) ? -1 : 0; } else { throw new TypeError(date); }
}; Date.prototype.equals = function (date) { return (this.compareTo(date) === 0); }; Date.prototype.between = function (start, end) { var t = this.getTime(); return t >= start.getTime() && t <= end.getTime(); }; Date.prototype.addMilliseconds = function (value) { this.setMilliseconds(this.getMilliseconds() + value); return this; }; Date.prototype.addSeconds = function (value) { return this.addMilliseconds(value * 1000); }; Date.prototype.addMinutes = function (value) { return this.addMilliseconds(value * 60000); }; Date.prototype.addHours = function (value) { return this.addMilliseconds(value * 3600000); }; Date.prototype.addDays = function (value) { return this.addMilliseconds(value * 86400000); }; Date.prototype.addWeeks = function (value) { return this.addMilliseconds(value * 604800000); }; Date.prototype.addMonths = function (value) { var n = this.getDate(); this.setDate(1); this.setMonth(this.getMonth() + value); this.setDate(Math.min(n, this.getDaysInMonth())); return this; }; Date.prototype.addYears = function (value) { return this.addMonths(value * 12); }; Date.prototype.add = function (config) {
    if (typeof config == "number") { this._orient = config; return this; }
    var x = config; if (x.millisecond || x.milliseconds) { this.addMilliseconds(x.millisecond || x.milliseconds); }
    if (x.second || x.seconds) { this.addSeconds(x.second || x.seconds); }
    if (x.minute || x.minutes) { this.addMinutes(x.minute || x.minutes); }
    if (x.hour || x.hours) { this.addHours(x.hour || x.hours); }
    if (x.month || x.months) { this.addMonths(x.month || x.months); }
    if (x.year || x.years) { this.addYears(x.year || x.years); }
    if (x.day || x.days) { this.addDays(x.day || x.days); }
    return this;
}; Date._validate = function (value, min, max, name) {
    if (typeof value != "number") { throw new TypeError(value + " is not a Number."); } else if (value < min || value > max) { throw new RangeError(value + " is not a valid value for " + name + "."); }
    return true;
}; Date.validateMillisecond = function (n) { return Date._validate(n, 0, 999, "milliseconds"); }; Date.validateSecond = function (n) { return Date._validate(n, 0, 59, "seconds"); }; Date.validateMinute = function (n) { return Date._validate(n, 0, 59, "minutes"); }; Date.validateHour = function (n) { return Date._validate(n, 0, 23, "hours"); }; Date.validateDay = function (n, year, month) { return Date._validate(n, 1, Date.getDaysInMonth(year, month), "days"); }; Date.validateMonth = function (n) { return Date._validate(n, 0, 11, "months"); }; Date.validateYear = function (n) { return Date._validate(n, 1, 9999, "seconds"); }; Date.prototype.set = function (config) {
    var x = config; if (!x.millisecond && x.millisecond !== 0) { x.millisecond = -1; }
    if (!x.second && x.second !== 0) { x.second = -1; }
    if (!x.minute && x.minute !== 0) { x.minute = -1; }
    if (!x.hour && x.hour !== 0) { x.hour = -1; }
    if (!x.day && x.day !== 0) { x.day = -1; }
    if (!x.month && x.month !== 0) { x.month = -1; }
    if (!x.year && x.year !== 0) { x.year = -1; }
    if (x.millisecond != -1 && Date.validateMillisecond(x.millisecond)) { this.addMilliseconds(x.millisecond - this.getMilliseconds()); }
    if (x.second != -1 && Date.validateSecond(x.second)) { this.addSeconds(x.second - this.getSeconds()); }
    if (x.minute != -1 && Date.validateMinute(x.minute)) { this.addMinutes(x.minute - this.getMinutes()); }
    if (x.hour != -1 && Date.validateHour(x.hour)) { this.addHours(x.hour - this.getHours()); }
    if (x.month !== -1 && Date.validateMonth(x.month)) { this.addMonths(x.month - this.getMonth()); }
    if (x.year != -1 && Date.validateYear(x.year)) { this.addYears(x.year - this.getFullYear()); }
    if (x.day != -1 && Date.validateDay(x.day, this.getFullYear(), this.getMonth())) { this.addDays(x.day - this.getDate()); }
    if (x.timezone) { this.setTimezone(x.timezone); }
    if (x.timezoneOffset) { this.setTimezoneOffset(x.timezoneOffset); }
    return this;
}; Date.prototype.clearTime = function () { this.setHours(0); this.setMinutes(0); this.setSeconds(0); this.setMilliseconds(0); return this; }; Date.prototype.isLeapYear = function () { var y = this.getFullYear(); return (((y % 4 === 0) && (y % 100 !== 0)) || (y % 400 === 0)); }; Date.prototype.isWeekday = function () { return !(this.is().sat() || this.is().sun()); }; Date.prototype.getDaysInMonth = function () { return Date.getDaysInMonth(this.getFullYear(), this.getMonth()); }; Date.prototype.moveToFirstDayOfMonth = function () { return this.set({ day: 1 }); }; Date.prototype.moveToLastDayOfMonth = function () { return this.set({ day: this.getDaysInMonth() }); }; Date.prototype.moveToDayOfWeek = function (day, orient) { var diff = (day - this.getDay() + 7 * (orient || +1)) % 7; return this.addDays((diff === 0) ? diff += 7 * (orient || +1) : diff); }; Date.prototype.moveToMonth = function (month, orient) { var diff = (month - this.getMonth() + 12 * (orient || +1)) % 12; return this.addMonths((diff === 0) ? diff += 12 * (orient || +1) : diff); }; Date.prototype.getDayOfYear = function () { return Math.floor((this - new Date(this.getFullYear(), 0, 1)) / 86400000); }; Date.prototype.getWeekOfYear = function (firstDayOfWeek) {
    var y = this.getFullYear(), m = this.getMonth(), d = this.getDate(); var dow = firstDayOfWeek || Date.CultureInfo.firstDayOfWeek; var offset = 7 + 1 - new Date(y, 0, 1).getDay(); if (offset == 8) { offset = 1; }
    var daynum = ((Date.UTC(y, m, d, 0, 0, 0) - Date.UTC(y, 0, 1, 0, 0, 0)) / 86400000) + 1; var w = Math.floor((daynum - offset + 7) / 7); if (w === dow) { y--; var prevOffset = 7 + 1 - new Date(y, 0, 1).getDay(); if (prevOffset == 2 || prevOffset == 8) { w = 53; } else { w = 52; } }
    return w;
}; Date.prototype.isDST = function () { console.log('isDST'); return this.toString().match(/(E|C|M|P)(S|D)T/)[2] == "D"; }; Date.prototype.getTimezone = function () { return Date.getTimezoneAbbreviation(this.getUTCOffset, this.isDST()); }; Date.prototype.setTimezoneOffset = function (s) { var here = this.getTimezoneOffset(), there = Number(s) * -6 / 10; this.addMinutes(there - here); return this; }; Date.prototype.setTimezone = function (s) { return this.setTimezoneOffset(Date.getTimezoneOffset(s)); }; Date.prototype.getUTCOffset = function () { var n = this.getTimezoneOffset() * -10 / 6, r; if (n < 0) { r = (n - 10000).toString(); return r[0] + r.substr(2); } else { r = (n + 10000).toString(); return "+" + r.substr(1); } }; Date.prototype.getDayName = function (abbrev) { return abbrev ? Date.CultureInfo.abbreviatedDayNames[this.getDay()] : Date.CultureInfo.dayNames[this.getDay()]; }; Date.prototype.getMonthName = function (abbrev) { return abbrev ? Date.CultureInfo.abbreviatedMonthNames[this.getMonth()] : Date.CultureInfo.monthNames[this.getMonth()]; }; Date.prototype._toString = Date.prototype.toString; Date.prototype.toString = function (format) { var self = this; var p = function p(s) { return (s.toString().length == 1) ? "0" + s : s; }; return format ? format.replace(/dd?d?d?|MM?M?M?|yy?y?y?|hh?|HH?|mm?|ss?|tt?|zz?z?/g, function (format) { switch (format) { case "hh": return p(self.getHours() < 13 ? self.getHours() : (self.getHours() - 12)); case "h": return self.getHours() < 13 ? self.getHours() : (self.getHours() - 12); case "HH": return p(self.getHours()); case "H": return self.getHours(); case "mm": return p(self.getMinutes()); case "m": return self.getMinutes(); case "ss": return p(self.getSeconds()); case "s": return self.getSeconds(); case "yyyy": return self.getFullYear(); case "yy": return self.getFullYear().toString().substring(2, 4); case "dddd": return self.getDayName(); case "ddd": return self.getDayName(true); case "dd": return p(self.getDate()); case "d": return self.getDate().toString(); case "MMMM": return self.getMonthName(); case "MMM": return self.getMonthName(true); case "MM": return p((self.getMonth() + 1)); case "M": return self.getMonth() + 1; case "t": return self.getHours() < 12 ? Date.CultureInfo.amDesignator.substring(0, 1) : Date.CultureInfo.pmDesignator.substring(0, 1); case "tt": return self.getHours() < 12 ? Date.CultureInfo.amDesignator : Date.CultureInfo.pmDesignator; case "zzz": case "zz": case "z": return ""; } }) : this._toString(); };
Date.now = function () { return new Date(); }; Date.today = function () { return Date.now().clearTime(); }; Date.prototype._orient = +1; Date.prototype.next = function () { this._orient = +1; return this; }; Date.prototype.last = Date.prototype.prev = Date.prototype.previous = function () { this._orient = -1; return this; }; Date.prototype._is = false; Date.prototype.is = function () { this._is = true; return this; }; Number.prototype._dateElement = "day"; Number.prototype.fromNow = function () { var c = {}; c[this._dateElement] = this; return Date.now().add(c); }; Number.prototype.ago = function () { var c = {}; c[this._dateElement] = this * -1; return Date.now().add(c); }; (function () {
    var $D = Date.prototype, $N = Number.prototype; var dx = ("sunday monday tuesday wednesday thursday friday saturday").split(/\s/), mx = ("january february march april may june july august september october november december").split(/\s/), px = ("Millisecond Second Minute Hour Day Week Month Year").split(/\s/), de; var df = function (n) {
        return function () {
            if (this._is) { this._is = false; return this.getDay() == n; }
            return this.moveToDayOfWeek(n, this._orient);
        };
    }; for (var i = 0; i < dx.length; i++) { $D[dx[i]] = $D[dx[i].substring(0, 3)] = df(i); }
    var mf = function (n) {
        return function () {
            if (this._is) { this._is = false; return this.getMonth() === n; }
            return this.moveToMonth(n, this._orient);
        };
    }; for (var j = 0; j < mx.length; j++) { $D[mx[j]] = $D[mx[j].substring(0, 3)] = mf(j); }
    var ef = function (j) {
        return function () {
            if (j.substring(j.length - 1) != "s") { j += "s"; }
            return this["add" + j](this._orient);
        };
    }; var nf = function (n) { return function () { this._dateElement = n; return this; }; }; for (var k = 0; k < px.length; k++) { de = px[k].toLowerCase(); $D[de] = $D[de + "s"] = ef(px[k]); $N[de] = $N[de + "s"] = nf(de); }
}()); Date.prototype.toJSONString = function () { return this.toString("yyyy-MM-ddThh:mm:ssZ"); }; Date.prototype.toShortDateString = function () { return this.toString(Date.CultureInfo.formatPatterns.shortDatePattern); }; Date.prototype.toLongDateString = function () { return this.toString(Date.CultureInfo.formatPatterns.longDatePattern); }; Date.prototype.toShortTimeString = function () { return this.toString(Date.CultureInfo.formatPatterns.shortTimePattern); }; Date.prototype.toLongTimeString = function () { return this.toString(Date.CultureInfo.formatPatterns.longTimePattern); }; Date.prototype.getOrdinal = function () { switch (this.getDate()) { case 1: case 21: case 31: return "st"; case 2: case 22: return "nd"; case 3: case 23: return "rd"; default: return "th"; } };
(function () {
    Date.Parsing = { Exception: function (s) { this.message = "Parse error at '" + s.substring(0, 10) + " ...'"; } }; var $P = Date.Parsing; var _ = $P.Operators = {
        rtoken: function (r) { return function (s) { var mx = s.match(r); if (mx) { return ([mx[0], s.substring(mx[0].length)]); } else { throw new $P.Exception(s); } }; }, token: function (s) { return function (s) { return _.rtoken(new RegExp("^\s*" + s + "\s*"))(s); }; }, stoken: function (s) { return _.rtoken(new RegExp("^" + s)); }, until: function (p) {
            return function (s) {
                var qx = [], rx = null; while (s.length) {
                    try { rx = p.call(this, s); } catch (e) { qx.push(rx[0]); s = rx[1]; continue; }
                    break;
                }
                return [qx, s];
            };
        }, many: function (p) {
            return function (s) {
                var rx = [], r = null; while (s.length) {
                    try { r = p.call(this, s); } catch (e) { return [rx, s]; }
                    rx.push(r[0]); s = r[1];
                }
                return [rx, s];
            };
        }, optional: function (p) {
            return function (s) {
                var r = null; try { r = p.call(this, s); } catch (e) { return [null, s]; }
                return [r[0], r[1]];
            };
        }, not: function (p) {
            return function (s) {
                try { p.call(this, s); } catch (e) { return [null, s]; }
                throw new $P.Exception(s);
            };
        }, ignore: function (p) { return p ? function (s) { var r = null; r = p.call(this, s); return [null, r[1]]; } : null; }, product: function () {
            var px = arguments[0], qx = Array.prototype.slice.call(arguments, 1), rx = []; for (var i = 0; i < px.length; i++) { rx.push(_.each(px[i], qx)); }
            return rx;
        }, cache: function (rule) {
            var cache = {}, r = null; return function (s) {
                try { r = cache[s] = (cache[s] || rule.call(this, s)); } catch (e) { r = cache[s] = e; }
                if (r instanceof $P.Exception) { throw r; } else { return r; }
            };
        }, any: function () {
            var px = arguments; return function (s) {
                var r = null; for (var i = 0; i < px.length; i++) {
                    if (px[i] == null) { continue; }
                    try { r = (px[i].call(this, s)); } catch (e) { r = null; }
                    if (r) { return r; }
                }
                throw new $P.Exception(s);
            };
        }, each: function () {
            var px = arguments; return function (s) {
                var rx = [], r = null; for (var i = 0; i < px.length; i++) {
                    if (px[i] == null) { continue; }
                    try { r = (px[i].call(this, s)); } catch (e) { throw new $P.Exception(s); }
                    rx.push(r[0]); s = r[1];
                }
                return [rx, s];
            };
        }, all: function () { var px = arguments, _ = _; return _.each(_.optional(px)); }, sequence: function (px, d, c) {
            d = d || _.rtoken(/^\s*/); c = c || null; if (px.length == 1) { return px[0]; }
            return function (s) {
                var r = null, q = null; var rx = []; for (var i = 0; i < px.length; i++) {
                    try { r = px[i].call(this, s); } catch (e) { break; }
                    rx.push(r[0]); try { q = d.call(this, r[1]); } catch (ex) { q = null; break; }
                    s = q[1];
                }
                if (!r) { throw new $P.Exception(s); }
                if (q) { throw new $P.Exception(q[1]); }
                if (c) { try { r = c.call(this, r[1]); } catch (ey) { throw new $P.Exception(r[1]); } }
                return [rx, (r ? r[1] : s)];
            };
        }, between: function (d1, p, d2) { d2 = d2 || d1; var _fn = _.each(_.ignore(d1), p, _.ignore(d2)); return function (s) { var rx = _fn.call(this, s); return [[rx[0][0], r[0][2]], rx[1]]; }; }, list: function (p, d, c) { d = d || _.rtoken(/^\s*/); c = c || null; return (p instanceof Array ? _.each(_.product(p.slice(0, -1), _.ignore(d)), p.slice(-1), _.ignore(c)) : _.each(_.many(_.each(p, _.ignore(d))), px, _.ignore(c))); }, set: function (px, d, c) {
            d = d || _.rtoken(/^\s*/); c = c || null; return function (s) {
                var r = null, p = null, q = null, rx = null, best = [[], s], last = false; for (var i = 0; i < px.length; i++) {
                    q = null; p = null; r = null; last = (px.length == 1); try { r = px[i].call(this, s); } catch (e) { continue; }
                    rx = [[r[0]], r[1]]; if (r[1].length > 0 && !last) { try { q = d.call(this, r[1]); } catch (ex) { last = true; } } else { last = true; }
                    if (!last && q[1].length === 0) { last = true; }
                    if (!last) {
                        var qx = []; for (var j = 0; j < px.length; j++) { if (i != j) { qx.push(px[j]); } }
                        p = _.set(qx, d).call(this, q[1]); if (p[0].length > 0) { rx[0] = rx[0].concat(p[0]); rx[1] = p[1]; }
                    }
                    if (rx[1].length < best[1].length) { best = rx; }
                    if (best[1].length === 0) { break; }
                }
                if (best[0].length === 0) { return best; }
                if (c) {
                    try { q = c.call(this, best[1]); } catch (ey) { throw new $P.Exception(best[1]); }
                    best[1] = q[1];
                }
                return best;
            };
        }, forward: function (gr, fname) { return function (s) { return gr[fname].call(this, s); }; }, replace: function (rule, repl) { return function (s) { var r = rule.call(this, s); return [repl, r[1]]; }; }, process: function (rule, fn) { return function (s) { var r = rule.call(this, s); return [fn.call(this, r[0]), r[1]]; }; }, min: function (min, rule) {
            return function (s) {
                var rx = rule.call(this, s); if (rx[0].length < min) { throw new $P.Exception(s); }
                return rx;
            };
        }
    }; var _generator = function (op) {
        return function () {
            var args = null, rx = []; if (arguments.length > 1) { args = Array.prototype.slice.call(arguments); } else if (arguments[0] instanceof Array) { args = arguments[0]; }
            if (args) { for (var i = 0, px = args.shift() ; i < px.length; i++) { args.unshift(px[i]); rx.push(op.apply(null, args)); args.shift(); return rx; } } else { return op.apply(null, arguments); }
        };
    }; var gx = "optional not ignore cache".split(/\s/); for (var i = 0; i < gx.length; i++) { _[gx[i]] = _generator(_[gx[i]]); }
    var _vector = function (op) { return function () { if (arguments[0] instanceof Array) { return op.apply(null, arguments[0]); } else { return op.apply(null, arguments); } }; }; var vx = "each any all".split(/\s/); for (var j = 0; j < vx.length; j++) { _[vx[j]] = _vector(_[vx[j]]); }
}()); (function () {
    var flattenAndCompact = function (ax) {
        var rx = []; for (var i = 0; i < ax.length; i++) { if (ax[i] instanceof Array) { rx = rx.concat(flattenAndCompact(ax[i])); } else { if (ax[i]) { rx.push(ax[i]); } } }
        return rx;
    }; Date.Grammar = {}; Date.Translator = {
        hour: function (s) { return function () { this.hour = Number(s); }; }, minute: function (s) { return function () { this.minute = Number(s); }; }, second: function (s) { return function () { this.second = Number(s); }; }, meridian: function (s) { return function () { this.meridian = s.slice(0, 1).toLowerCase(); }; }, timezone: function (s) { return function () { var n = s.replace(/[^\d\+\-]/g, ""); if (n.length) { this.timezoneOffset = Number(n); } else { this.timezone = s.toLowerCase(); } }; }, day: function (x) { var s = x[0]; return function () { this.day = Number(s.match(/\d+/)[0]); }; }, month: function (s) { return function () { this.month = ((s.length == 3) ? Date.getMonthNumberFromName(s) : (Number(s) - 1)); }; }, year: function (s) { return function () { var n = Number(s); this.year = ((s.length > 2) ? n : (n + (((n + 2000) < Date.CultureInfo.twoDigitYearMax) ? 2000 : 1900))); }; }, rday: function (s) { return function () { switch (s) { case "yesterday": this.days = -1; break; case "tomorrow": this.days = 1; break; case "today": this.days = 0; break; case "now": this.days = 0; this.now = true; break; } }; }, finishExact: function (x) {
            x = (x instanceof Array) ? x : [x]; var now = new Date(); this.year = now.getFullYear(); this.month = now.getMonth(); this.day = 1; this.hour = 0; this.minute = 0; this.second = 0; for (var i = 0; i < x.length; i++) { if (x[i]) { x[i].call(this); } }
            this.hour = (this.meridian == "p" && this.hour < 13) ? this.hour + 12 : this.hour; if (this.day > Date.getDaysInMonth(this.year, this.month)) { throw new RangeError(this.day + " is not a valid value for days."); }
            var r = new Date(this.year, this.month, this.day, this.hour, this.minute, this.second); if (this.timezone) { r.set({ timezone: this.timezone }); } else if (this.timezoneOffset) { r.set({ timezoneOffset: this.timezoneOffset }); }
            return r;
        }, finish: function (x) {
            x = (x instanceof Array) ? flattenAndCompact(x) : [x]; if (x.length === 0) { return null; }
            for (var i = 0; i < x.length; i++) { if (typeof x[i] == "function") { x[i].call(this); } }
            if (this.now) { return new Date(); }
            var today = Date.today(); var method = null; var expression = !!(this.days != null || this.orient || this.operator); if (expression) {
                var gap, mod, orient; orient = ((this.orient == "past" || this.operator == "subtract") ? -1 : 1); if (this.weekday) { this.unit = "day"; gap = (Date.getDayNumberFromName(this.weekday) - today.getDay()); mod = 7; this.days = gap ? ((gap + (orient * mod)) % mod) : (orient * mod); }
                if (this.month) { this.unit = "month"; gap = (this.month - today.getMonth()); mod = 12; this.months = gap ? ((gap + (orient * mod)) % mod) : (orient * mod); this.month = null; }
                if (!this.unit) { this.unit = "day"; }
                if (this[this.unit + "s"] == null || this.operator != null) {
                    if (!this.value) { this.value = 1; }
                    if (this.unit == "week") { this.unit = "day"; this.value = this.value * 7; }
                    this[this.unit + "s"] = this.value * orient;
                }
                return today.add(this);
            } else {
                if (this.meridian && this.hour) { this.hour = (this.hour < 13 && this.meridian == "p") ? this.hour + 12 : this.hour; }
                if (this.weekday && !this.day) { this.day = (today.addDays((Date.getDayNumberFromName(this.weekday) - today.getDay()))).getDate(); }
                if (this.month && !this.day) { this.day = 1; }
                return today.set(this);
            }
        }
    }; var _ = Date.Parsing.Operators, g = Date.Grammar, t = Date.Translator, _fn; g.datePartDelimiter = _.rtoken(/^([\s\-\.\,\/\x27]+)/); g.timePartDelimiter = _.stoken(":"); g.whiteSpace = _.rtoken(/^\s*/); g.generalDelimiter = _.rtoken(/^(([\s\,]|at|on)+)/); var _C = {}; g.ctoken = function (keys) {
        var fn = _C[keys]; if (!fn) {
            var c = Date.CultureInfo.regexPatterns; var kx = keys.split(/\s+/), px = []; for (var i = 0; i < kx.length; i++) { px.push(_.replace(_.rtoken(c[kx[i]]), kx[i])); }
            fn = _C[keys] = _.any.apply(null, px);
        }
        return fn;
    }; g.ctoken2 = function (key) { return _.rtoken(Date.CultureInfo.regexPatterns[key]); }; g.h = _.cache(_.process(_.rtoken(/^(0[0-9]|1[0-2]|[1-9])/), t.hour)); g.hh = _.cache(_.process(_.rtoken(/^(0[0-9]|1[0-2])/), t.hour)); g.H = _.cache(_.process(_.rtoken(/^([0-1][0-9]|2[0-3]|[0-9])/), t.hour)); g.HH = _.cache(_.process(_.rtoken(/^([0-1][0-9]|2[0-3])/), t.hour)); g.m = _.cache(_.process(_.rtoken(/^([0-5][0-9]|[0-9])/), t.minute)); g.mm = _.cache(_.process(_.rtoken(/^[0-5][0-9]/), t.minute)); g.s = _.cache(_.process(_.rtoken(/^([0-5][0-9]|[0-9])/), t.second)); g.ss = _.cache(_.process(_.rtoken(/^[0-5][0-9]/), t.second)); g.hms = _.cache(_.sequence([g.H, g.mm, g.ss], g.timePartDelimiter)); g.t = _.cache(_.process(g.ctoken2("shortMeridian"), t.meridian)); g.tt = _.cache(_.process(g.ctoken2("longMeridian"), t.meridian)); g.z = _.cache(_.process(_.rtoken(/^(\+|\-)?\s*\d\d\d\d?/), t.timezone)); g.zz = _.cache(_.process(_.rtoken(/^(\+|\-)\s*\d\d\d\d/), t.timezone)); g.zzz = _.cache(_.process(g.ctoken2("timezone"), t.timezone)); g.timeSuffix = _.each(_.ignore(g.whiteSpace), _.set([g.tt, g.zzz])); g.time = _.each(_.optional(_.ignore(_.stoken("T"))), g.hms, g.timeSuffix); g.d = _.cache(_.process(_.each(_.rtoken(/^([0-2]\d|3[0-1]|\d)/), _.optional(g.ctoken2("ordinalSuffix"))), t.day)); g.dd = _.cache(_.process(_.each(_.rtoken(/^([0-2]\d|3[0-1])/), _.optional(g.ctoken2("ordinalSuffix"))), t.day)); g.ddd = g.dddd = _.cache(_.process(g.ctoken("sun mon tue wed thu fri sat"), function (s) { return function () { this.weekday = s; }; })); g.M = _.cache(_.process(_.rtoken(/^(1[0-2]|0\d|\d)/), t.month)); g.MM = _.cache(_.process(_.rtoken(/^(1[0-2]|0\d)/), t.month)); g.MMM = g.MMMM = _.cache(_.process(g.ctoken("jan feb mar apr may jun jul aug sep oct nov dec"), t.month)); g.y = _.cache(_.process(_.rtoken(/^(\d\d?)/), t.year)); g.yy = _.cache(_.process(_.rtoken(/^(\d\d)/), t.year)); g.yyy = _.cache(_.process(_.rtoken(/^(\d\d?\d?\d?)/), t.year)); g.yyyy = _.cache(_.process(_.rtoken(/^(\d\d\d\d)/), t.year)); _fn = function () { return _.each(_.any.apply(null, arguments), _.not(g.ctoken2("timeContext"))); }; g.day = _fn(g.d, g.dd); g.month = _fn(g.M, g.MMM); g.year = _fn(g.yyyy, g.yy); g.orientation = _.process(g.ctoken("past future"), function (s) { return function () { this.orient = s; }; }); g.operator = _.process(g.ctoken("add subtract"), function (s) { return function () { this.operator = s; }; }); g.rday = _.process(g.ctoken("yesterday tomorrow today now"), t.rday); g.unit = _.process(g.ctoken("minute hour day week month year"), function (s) { return function () { this.unit = s; }; }); g.value = _.process(_.rtoken(/^\d\d?(st|nd|rd|th)?/), function (s) { return function () { this.value = s.replace(/\D/g, ""); }; }); g.expression = _.set([g.rday, g.operator, g.value, g.unit, g.orientation, g.ddd, g.MMM]); _fn = function () { return _.set(arguments, g.datePartDelimiter); }; g.mdy = _fn(g.ddd, g.month, g.day, g.year); g.ymd = _fn(g.ddd, g.year, g.month, g.day); g.dmy = _fn(g.ddd, g.day, g.month, g.year); g.date = function (s) { return ((g[Date.CultureInfo.dateElementOrder] || g.mdy).call(this, s)); }; g.format = _.process(_.many(_.any(_.process(_.rtoken(/^(dd?d?d?|MM?M?M?|yy?y?y?|hh?|HH?|mm?|ss?|tt?|zz?z?)/), function (fmt) { if (g[fmt]) { return g[fmt]; } else { throw Date.Parsing.Exception(fmt); } }), _.process(_.rtoken(/^[^dMyhHmstz]+/), function (s) { return _.ignore(_.stoken(s)); }))), function (rules) { return _.process(_.each.apply(null, rules), t.finishExact); }); var _F = {}; var _get = function (f) { return _F[f] = (_F[f] || g.format(f)[0]); }; g.formats = function (fx) {
        if (fx instanceof Array) {
            var rx = []; for (var i = 0; i < fx.length; i++) { rx.push(_get(fx[i])); }
            return _.any.apply(null, rx);
        } else { return _get(fx); }
    }; g._formats = g.formats(["yyyy-MM-ddTHH:mm:ss", "ddd, MMM dd, yyyy H:mm:ss tt", "ddd MMM d yyyy HH:mm:ss zzz", "d"]); g._start = _.process(_.set([g.date, g.time, g.expression], g.generalDelimiter, g.whiteSpace), t.finish); g.start = function (s) {
        try { var r = g._formats.call({}, s); if (r[1].length === 0) { return r; } } catch (e) { }
        return g._start.call({}, s);
    };
}()); Date._parse = Date.parse; Date.parse = function (s) {
    var r = null; if (!s) { return null; }
    try { r = Date.Grammar.start.call({}, s); } catch (e) { return null; }
    return ((r[1].length === 0) ? r[0] : null);
}; Date.getParseFunction = function (fx) {
    var fn = Date.Grammar.formats(fx); return function (s) {
        var r = null; try { r = fn.call({}, s); } catch (e) { return null; }
        return ((r[1].length === 0) ? r[0] : null);
    };
}; Date.parseExact = function (s, fx) { return Date.getParseFunction(fx)(s); };




function getDateFormat() {    
    return 'MM/dd/yyyy HH:mm';
};
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

String.prototype.equalsIc = String.prototype.f = function () {
    return arguments[0].isEqual(this, true);
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





;
//if (!window.console) {
//    var console = {};
//}
//if (!console.log) {
//     console.log = function () { };
//}



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

var nullOrEmpty = function (s) {
    return nullOrUndef(s) || String(s).trim().length == 0;
};

var isArrayNullOrEmpty = function (arr) {
    return nullOrUndef(arr) || arr.length == 0;
};

var isObjectEmpty = function(ob) {
    for (var prop in ob) {
        if (ob.hasOwnProperty(prop)) {
            return false;
        }
    }
    return true;
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

function lockCommandBars() {
    var bars = $('[data-class=commandbar]');
    bars.each(function (index, element) {
        var buttons = $(element).children('button');
        for (var i = 0; i < buttons.length; i++) {
            var button = $(buttons[i]);
            if (button.prop('disabled') != true) {
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
        var buttons = $(element).children('button');
        for (var i = 0; i < buttons.length; i++) {
            var button = $(buttons[i]);
            if (button.attr('forceddisable') == 'disabled') {
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

function isIe() {
    if ("true" == sessionStorage.mockie9) {
        return true;
    }
    return BrowserDetect.browser == "Explorer";
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

function GetBootstrapFormClass(size) {
    var popupmode = GetPopUpMode();
    if (popupmode == "browser" || popupmode == "nomenu") {
        return 'col-xs-' + size;
    }
    return 'col-md-' + size;
}



function GetPopUpMode() {
    var popupMode = $(hddn_popupmode)[0].value;
    if (popupMode === undefined || popupMode == 'null' || popupMode == "") {
        popupMode = 'none';
    }
    return popupMode;
}

function IsPopup() {
    var popup = GetPopUpMode();
    return "browser" == popup || "nomenu" == popup;
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

function addSchemaDataToParameters(parameters, schema, nextSchema) {
    parameters["currentSchemaKey"] = schema.schemaId + "." + schema.mode + "." + platform();
    if (nextSchema != null && nextSchema.schemaId != null) {
        parameters["nextSchemaKey"] = nextSchema.schemaId + ".";
        if (nextSchema.mode != null) {
            parameters["nextSchemaKey"] += nextSchema.mode;
        }
        parameters["nextSchemaKey"] += "." + platform();
    }
    if (schema.properties != null && schema.properties['nextschema.schemaid'] != null) {
        parameters["nextSchemaKey"] = schema.properties['nextschema.schemaid'];
    }

    return parameters;
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
    var HH = today.getHours();
    var mins = today.getMinutes();

    if (dd < 10) {
        dd = '0' + dd
    }

    if (mm < 10) {
        mm = '0' + mm
    }

    today = mm + '/' + dd + '/' + yyyy + " " + HH + ":" + mins;
    return today;
}

function replaceAll(str, find, replace) {
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

function isNumber(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
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

};
var app = angular.module('sw_layout', ['pasvaz.bindonce', 'angularTreeview', 'ngSanitize']).config(function ($controllerProvider) {
//    $controllerProvider.allowGlobals()
});


app.filter('linebreak', function () {
    return function (value) {
        if (value != null) {
            value = value.toString();
            return value.replace(/\n/g, '<br/>');
        }
        return value;
    };
});

app.filter('html', ['$sce', function ($sce) {
        return function (text) {
            return $sce.trustAsHtml(text);
        };
}]);



app.directive('onFinishRender', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    scope.$emit('ngRepeatFinished');
                });
            }
        }
    };
});



app.directive('ngEnter', function () {
    return function (scope, element, attrs) {
        element.bind("keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });

                event.preventDefault();
            }
        });
    };
});

app.directive("ngEnabled", function () {

    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            scope.$watch(attrs.ngEnabled, function (val) {
                if (val)
                    element.removeAttr("disabled");
                else
                    element.attr("disabled", "disabled");
            });
        }
    };
});




function LayoutController($scope, $http, $log, $templateCache, $rootScope, $timeout, fixHeaderService, redirectService, i18NService, menuService, contextService, $location, $window, logoutService, spinService, schemaCacheService) {

    $scope.$name = 'LayoutController';

    schemaCacheService.wipeSchemaCacheIfNeeded();

    $rootScope.$on('sw_ajaxinit', function (ajaxinitevent) {
        var savingMain = true === $rootScope.savingMain;
        spinService.start({ savingDetail: savingMain });
    });

    $rootScope.$on('sw_ajaxend', function (data) {
        spinService.stop();
        $rootScope.savingMain = undefined;

    });

    $rootScope.$on('sw_ajaxerror', function (data) {
        spinService.stop();
        $rootScope.savingMain = undefined;

    });

    $scope.$on('sw_titlechanged', function (titlechangedevent, title) {
        $scope.title = title;
    });

    $scope.$on('ngLoadFinished', function (ngLoadFinishedEvent) {
        $('[rel=tooltip]').tooltip({ container: 'body' });
        menuService.adjustHeight();
    });

    $scope.$on('ngRepeatFinished', function (ngRepeatFinishedEvent) {
        $('[rel=tooltip]').tooltip({ container: 'body' });

        var sidebarWidth = $('.col-side-bar').width();
        if (sidebarWidth != null) {
            $('.col-main-content').css('margin-left', sidebarWidth);
        }
    });

    $scope.contextPath = function (path) {
        return url(path);
    };

    $scope.goToApplicationView = function (applicationName, schemaId, mode, title, parameters, target) {
        menuService.setActiveLeaf(target);
        redirectService.goToApplicationView(applicationName, schemaId, mode, title, parameters);
    };

    $scope.doAction = function (title, controller, action, parameters, target) {
        menuService.setActiveLeaf(target);
        redirectService.redirectToAction(title, controller, action, parameters);
    };

    $rootScope.$on('sw_redirectactionsuccess', function (event, result) {
        var log = $log.getInstance('layoutcontroller#onsw_redirectactionsuccess');
        log.debug("received event");
        $scope.AjaxResult(result);
    });

    $rootScope.$on('sw_redirectapplicationsuccess', function (event, result, mode, applicationName) {
        var log = $log.getInstance('layoutcontroller#onsw_redirectapplicationsuccess');
        //todo: are these 2 parameters really necessary?
        $scope.applicationname = applicationName;
        $scope.requestmode = mode;
        if ($rootScope.popupmode != undefined) {
            $scope.popupmode = $rootScope.popupmode;
            $(hddn_popupmode)[0].value = $rootScope.popupmode;
        } else {
            //keep the current popupmode
            $rootScope.popupmode = $scope.popupmode;
        }
        log.debug("received event");
        $scope.AjaxResult(result);
    });


    $scope.AjaxResult = function (result) {
        if ($scope.resultObject.requestTimeStamp > result.requestTimeStamp) {
            //ignoring intermediary request
            return;
        }

        var log = $log.getInstance('layoutcontroller#AjaxResult');
        var newUrl = url(result.redirectURL);
        if ($scope.includeURL != newUrl) {
            log.debug("redirection detected new:{0} old:{1}".format(newUrl, $scope.includeURL));
            $scope.includeURL = newUrl;
        }
        
        if (result.title != null) {
            log.debug("dispatching title changed event. Title: {0}".format(result.title));
            $scope.$emit('sw_titlechanged', result.title);
        }
        $scope.resultData = result.resultObject;
        $scope.resultObject = result;

        // scroll window to top, in a timeout, in order to give time to the page/grid render
        $timeout(
            function () {
                window.scrollTo(0, 0);
            }, 100, false);               
    };

    $scope.logout = function () {
        logoutService.logout("manual");
    };

    $scope.$on('sw_goToApplicationView', function (event, data) {
        if (data != null) {
            $scope.goToApplicationView(data.applicationName, data.schemaId, data.mode, data.title, data.parameters);
        }
    });

    $scope.$on('sw_indexPageLoaded', function (event, url) {
        if (url != null && $rootScope.menu) {
            menuService.setActiveLeafByUrl($rootScope.menu, url);
        }
    });

    function initController() {
        var configsJSON = $(hddn_configs)[0].value;
        var userJSON = $(hiddn_user)[0].value;
        if (nullOrEmpty(configsJSON) || nullOrEmpty(userJSON)) {
            //this means user tried to hit back button after logout
            logoutService.logout();
            return;
        }

        var config = JSON.parse(configsJSON);
        var user = JSON.parse(userJSON);
        contextService.loadUserContext(user);
        contextService.loadConfigs(config);

        contextService.insertIntoContext("isLocal", config.isLocal);
        contextService.insertIntoContext("allowedfiles", config.allowedFileTypes);
        
        $rootScope.clientName = config.clientName;
        $rootScope.environment = config.environment;
        $rootScope.isLocal = config.isLocal;
        $rootScope.i18NRequired = config.i18NRequired;

        $scope.mainlogo = config.logo;
        $scope.myprofileenabled = config.myProfileEnabled;
        var popupMode = GetPopUpMode();
        $scope.popupmode = popupMode;
        if (popupMode != "none") {
            return;
        }
        $http({
            method: "GET",
            url: url("/api/menu?" + platformQS()),
            cache: $templateCache
        })
        .success(function (menuAndNav) {
            $rootScope.menu = menuAndNav.menu;
            $scope.menu = menuAndNav.menu;
            $scope.isSysAdmin = menuAndNav.isSysAdmin;
            $scope.isClientAdmin = menuAndNav.isClientAdmin;
          
            $('.hapag-body').addClass('hapag-body-loaded');
        })
        .error(function (data) {
            $scope.title = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
        });
    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    initController();
}

;
function AboutController($scope, $http, $templateCache, i18NService) {

    var data = $scope.resultData;
    if (data != null) {
        $scope.aboutData = data;
    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };
};
(function (angular, $) {
    "use strict";

angular.module('sw_layout').config(['$httpProvider', function ($httpProvider) {

    $httpProvider.interceptors.push(function ($q, $rootScope, $timeout, contextService, $log, logoutService, schemaCacheService) {
        var activeRequests = 0;
        var started = function (config) {
            lockCommandBars();
            lockTabs();
            config.headers['currentmodule'] = contextService.retrieveFromContext('currentmodule');
            config.headers['currentmetadata'] = contextService.retrieveFromContext('currentmetadata');
            config.headers['mockerror'] = sessionStorage['mockerror'];
            config.headers['requesttime'] = new Date().getTime();
            config.headers['cachedschemas'] = schemaCacheService.getSchemaCacheKeys();
            config.headers['printmode'] = config.printMode;
            var log = $log.getInstance('sw4.ajaxint#started');
            var spinAvoided = false;
            if (activeRequests <= 0 || config.url.indexOf("/Content/") < 0) {
                if (!log.isLevelEnabled('trace')) {
                    log.info("started request {0}".format(config.url));
                }
                spinAvoided = config.avoidspin;
                if (!spinAvoided) {
                    activeRequests++;
                    $rootScope.$broadcast('sw_ajaxinit');
                }
            }
            log.trace("url: {0} | current module:{1} | current metadata:{2} "
               .format(config.url, config.headers['currentmodule'], config.headers['currentmetadata']));
            activeRequests++;
        };
        var endedok = function (response) {
            //Hiding the tooltip. Workaround for Issue HAP -281 (need proper fix)
            $('[rel=tooltip]').tooltip('hide');
            activeRequests--;
            var log = $log.getInstance('sw4.ajaxint#endedok');
            log.trace("status :{0}, url: {1} ".format(response.status, response.config.url));
            var spinAvoided = false;
            spinAvoided = response.config.avoidspin;
            if (!spinAvoided) {
                activeRequests--;
            }
            if (activeRequests <= 0) {
                unLockCommandBars();
                unLockTabs();
                log.info("Requests ended");
                $rootScope.$broadcast('sw_ajaxend', response.data);
                successMessageHandler(response.data);
            }
        };

        function successMessageHandler(data) {
            var timeOut = contextService.retrieveFromContext('successMessageTimeOut');
            if (data.successMessage != null) {
                var willRefresh = contextService.fetchFromContext("refreshscreen", false, true);
                if (!willRefresh) {
                    //if we´re sure the page will refresh we shall not display the successmessage now, since it would disappear quickly
                    $rootScope.$broadcast('sw_successmessage', data);
                    $timeout(function() {
                        $rootScope.$broadcast('sw_successmessagetimeout', { successMessage: null });
                    }, timeOut);
                } else {
                    contextService.insertIntoContext("onloadMessage", data.successMessage);
                    contextService.deleteFromContext("refreshscreen");
                }
                
            }
        }

        var endederror = function (rejection) {
            //Hiding the tooltip. Workaround for Issue HAP -281 (need proper fix)
            $('[rel=tooltip]').tooltip('hide');
            

            if (rejection.status === 401) {
                window.location = url('');
                return;
            }
            activeRequests--;
            unLockCommandBars();
            unLockTabs();
            if (activeRequests <= 0) {
                $rootScope.$broadcast('sw_ajaxerror', rejection.data,rejection.status);
            }
        };

        return {
            // optional method
            'request': function (config) {
                started(config);

                return config || $q.when(config);
            },

            // optional method
            'response': function (response) {
                endedok(response);
                return response || $q.when(response);
            },

            // optional method
            'responseError': function (rejection) {
                endederror(rejection);
                return $q.reject(rejection);
            }
        };
    });

    $httpProvider.defaults.transformRequest.push(function (data, headers) {
        if (data == undefined) {
            return data;
        }
        if (sessionStorage.mockerror || sessionStorage.mockmaximo) {
            var jsonOb = JSON.parse(data);
            jsonOb['%%mockerror'] = sessionStorage.mockerror === "true";
            jsonOb['%%mockmaximo'] = sessionStorage.mockmaximo === "true";
            return JSON.stringify(jsonOb);
        }
        return data;
    });
}]);


window.onpopstate = function (e) {
    if (e.state) {
        document.getElementById("content").innerHTML = e.state.html;
        document.title = e.state.pageTitle;
    }
};

})(angular, jQuery);;
//var app = angular.module('sw_layout');

var app = angular.module('sw_layout');

app.directive('bodyrendered', function ($timeout, $log, menuService) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.schema.mode !== 'output' && scope.isSelectEnabled) {
                element.data('selectenabled', scope.isSelectEnabled(scope.fieldMetadata));
            }
            if (scope.$last === true) {
                $timeout(function () {
                    var parentElementId = scope.elementid;
                    $log.getInstance('application_dir#bodyrendered').debug('sw_body_rendered will get dispatched');
                    menuService.adjustHeight();
                    scope.$emit('sw_bodyrenderedevent', parentElementId);
                });
            }
        }
    };
});


app.directive('listtablerendered', function ($timeout, $log, menuService) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            var log = $log.getInstance('application_dir#bodyrendered');
            if (scope.$first) {
                log.debug('init list table rendered');
            } else {
                log.trace('list table rendered');
            }
            if (scope.$last === true || scope.datamap.length == 0) {
                $timeout(function () {
                    log.debug('list table rendered will get dispatched');
                    //                    menuService.adjustHeight();
                    scope.$emit('listTableRenderedEvent');
                });
            }
        }
    };
});

app.directive('filterrowrendered', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    scope.$emit('filterRowRenderedEvent');
                });
            }
        }
    };
});

function ApplicationController($scope, $http, $templateCache, $timeout, $log, fixHeaderService, $rootScope, associationService, alertService, contextService, detailService, spinService, schemaCacheService, crudContextHolderService) {
    $scope.$name = 'applicationController';

    function switchMode(mode, scope) {
        if (scope == null) {
            scope = $scope;
        }
        scope.isDetail = mode;
        scope.isList = !mode;
    }

    $scope.toList = function (data, scope) {
        $('#saveBTN').removeAttr('disabled');
        if (data != null && $rootScope.printRequested !== true) {
            //if its a printing operation, then leave the pagination data intact
            scope.paginationData = {};
            scope.searchValues = data.searchValues;
            scope.paginationData.pagesToShow = data.pagesToShow;
            scope.paginationData.pageNumber = data.pageNumber;
            $scope.paginationData.selectedPage = data.pageNumber;
            scope.paginationData.pageCount = data.pageCount;
            scope.paginationData.pageSize = data.pageSize;
            scope.paginationData.paginationOptions = data.paginationOptions;
            scope.paginationData.totalCount = data.totalCount;
            scope.paginationData.hasPrevious = data.hasPrevious;
            scope.paginationData.hasNext = data.hasNext;
            scope.paginationData.filterFixedWhereClause = data.filterFixedWhereClause;
            scope.paginationData.unionFilterFixedWhereClause = data.unionFilterFixedWhereClause;
        }
        switchMode(false, scope);
    };


    function toDetail(scope) {
        switchMode(true, scope);
    };


    $scope.showList = function () {
        $scope.searchData = {};
        $scope.searchOperator = {};
        $scope.searchSort = {};
        $scope.selectPage(1);
    };


    $scope.renderViewWithData = function (applicationName, schemaId, mode, title, dataObject) {
        $scope.applicationname = applicationName;
        $scope.requestmode = mode;
        dataObject.mode = mode;
        $scope.renderData(dataObject);
    };

    //this code will get called when the user is already on a crud page and tries to switch view only.
    $scope.renderView = function (applicationName, schemaId, mode, title, parameters) {
        if (parameters === undefined || parameters == null) {
            parameters = {};
        }
        if (title == null) {
            title = $scope.title;
        }
        $scope.requestpopup = parameters.popupmode ? parameters.popupmode : 'none';
        //change made to prevent popup for incident detail report
        var log = $log.getInstance("applicationController#renderView");
        if ($scope.requestpopup == 'browser' || $scope.requestpopup == 'report') {
            log.debug("calling goToApplicationView for application {0}".format(applicationName));
            $scope.$parent.goToApplicationView(applicationName, schemaId, mode, title, parameters);
            return;
        }

        //remove it, so its not used on server side
        var printMode = parameters.printMode;
        parameters.printMode = null;

        parameters.key = {};
        parameters.key.schemaId = schemaId;
        parameters.key.mode = mode;
        parameters.key.platform = platform();
        parameters.customParameters = {};
        parameters.title = title;

        $scope.applicationname = applicationName;
        $scope.requestmode = mode;
        var urlToCall = url("/api/data/" + applicationName + "?" + $.param(parameters));
        if (printMode == undefined) {
            //avoid the print url to be saved on the sessionStorage, breaking page refresh
            sessionStorage.swGlobalRedirectURL = urlToCall;
        }
        log.info("calling url".format(urlToCall));
        $http.get(urlToCall)
            .success(function (data) {
                if (printMode != undefined) {
                    $rootScope.printRequested = true;
                };
                $scope.renderData(data, printMode);
            });
    };


    $scope.renderSelectedSchema = function () {
        var selectedSchema = $scope.selectedSchema.value;
        if (selectedSchema == null || selectedSchema.schemaId == null) {
            return;
        }
        $scope.isDetail = false;
        $scope.renderView($scope.applicationname, selectedSchema.schemaId, $scope.selectedModeRequest, $scope.title, null);

    };

    function setWindowTitle(scope) {
        var strategy = scope.schema.properties["popup.window.titlestrategy"];
        var id = scope.datamap.fields[scope.schema.idFieldName];
        var titleattribute = scope.schema.properties["popup.window.titleattribute"];
        if (titleattribute != null) {
            var overridenTitle = scope.datamap.fields[titleattribute];
            window.document.title = String.format(overridenTitle, id);
            return;
        }
        if (nullOrEmpty(strategy)) {
            window.document.title = scope.title;
        }
        else if (strategy == "idonly") {
            window.document.title = id;
            if (id == null) {
                window.document.title = scope.title;
            }
        } else if (strategy == "nameandid") {
            window.document.title = scope.schema.applicationName + " " + id;
        } else if (strategy == "schematitle") {
            window.document.title = scope.schema.title;
        }
    }

    $scope.renderData = function renderData(result) {
        $scope.isList = $scope.isDetail = false;
        $scope.crudsubtemplate = null;
        $scope.multipleSchema = false;
        $scope.schemas = null;
        var isModal = $scope.requestpopup && ($scope.requestpopup == 'modal' || $scope.requestpopup == 'multiplemodal');
        if (isModal) {
            $('#crudmodal').modal('show');
            $("#crudmodal").draggable();
            $scope.modal = {};
            $scope.showingModal = true;
        } else {
            /*if ($scope.schemas != null) {
                $scope.previousschema = $scope.schemas;
            } else {*/
            $scope.previousschema = $scope.schema;
            //}
            $scope.previousdata = $scope.datamap;
        }
        var scope = isModal ? $scope.modal : $scope;
        scope.schema = schemaCacheService.getSchemaFromResult(result);

        // resultObject can be null only when SW is pointing to a Maximo DB different from Maximo WS DB
        scope.datamap = instantiateIfUndefined(result.resultObject);
        scope.timeStamp = result.timeStamp;

        scope.mode = result.mode;
        if (scope.schema != null) {
            scope.schema.mode = scope.mode;
            crudContextHolderService.updateCrudContext(scope.schema);
        }
        if (result.title != null) {
            $scope.$emit('sw_titlechanged', result.title);
            if (IsPopup()) {
                setWindowTitle(scope);
            }
        }
        var log = $log.getInstance("applicationcontroller#renderData");
        if (result.type == 'ApplicationDetailResult') {
            log.debug("Application Detail Result handled");
            detailService.fetchRelationshipData(scope, result);
            toDetail(scope);
        } else if (result.type == 'ApplicationListResult') {
            log.debug("Application List Result handled");
            $scope.toList(result, scope);
            fixHeaderService.FixHeader();
            $scope.$broadcast('sw_griddatachanged', scope.datamap, scope.schema);
        } else if (result.crudSubTemplate != null) {

            log.debug("Crud Sub Template handled");
            $scope.crudsubtemplate = url(result.crudSubTemplate);
        }

        //HAP-698 - init scrollpage
        //TODO: find event after menu loads to init (remove 1 second timeout)
        $timeout(function () {

            //HAP-876 - resize the nav, to make sure it is scrollable
            $('.menu-primary').height($(window).height());

            //set the scrollpane
            $('.menu-primary').jScrollPane({ maintainPosition: true });
        }, 1000);

        $scope.requestpopup = null;
    };
    
    //called first time a crud is registered
    function initApplication() {
        $scope.$on('sw_renderview', function (event, applicationName, schemaId, mode, title, parameters) {
            $scope.renderView(applicationName, schemaId, mode, title, parameters);
        });

        $scope.$on('sw_applicationredirected', function (event, parameters) {
            if (parameters.popupmode == "browser" || parameters.popupmode == "modal") {
                return;
            }

            $scope.multipleSchema = false;
            $scope.schemas = null;
            //            fixHeaderService.unfix();
            $scope.isDetail = false;
            contextService.setActiveTab(null);
            //            $scope.isList = false;
        });

        $scope.$on('sw_applicationrenderviewwithdata', function (event, data) {
            var nextSchema = data.schema;
            $scope.renderViewWithData(nextSchema.applicationName, nextSchema.schemaId, nextSchema.mode, nextSchema.title, data);
        });
        window.onbeforeunload = function () {
            spinService.stop();
        };

        doInit();
        $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
            if (oldValue != newValue) {
                $log.getInstance("applicationcontroller#initAplication").info("redirect detected");
                doInit();
            }
        });

    }

    function doInit() {
        if ($scope.resultObject.redirectURL.indexOf("Application.html") == -1) {
            //pog to identify that this result if of the right type.
            return;
        }
        var dataObject = $scope.resultObject;
        var title = $scope.title;

        var mode = dataObject.mode;
        var applicationName = dataObject.applicationName;
        var schema = dataObject.schema;
        var schemaId = schema == null ? null : schema.schemaId;

        $scope.searchData = {};
        $scope.searchOperator = {};
        $scope.searchSort = {};
        

        if (dataObject.schemas == null) {
            if (dataObject.resultObject == null) {
                //this means, most likely a security issue
                alertService.alert('You don´t have enough permissions to see that register. contact your administrator');
                return;
            }
            $scope.renderViewWithData(applicationName, schemaId, mode, title, dataObject);
            return;
        }
        $scope.multipleSchemaHandling(dataObject);

    }

    $scope.multipleSchemaHandling = function (dataObject) {
        $scope.crudsubtemplate = null;
        var title = dataObject.title;
        var applicationName = dataObject.applicationName;
        var mode = dataObject.mode;

        $scope.isDetail = false;
        $scope.isList = false;
        $scope.multipleSchema = true;
        //just don´t know why i had to bound ng-model to selectedSchema.value==> simply selectedSchema didn´t work as expected.
        $scope.selectedSchema = {};
        if (dataObject.placeHolder != null && dataObject.schemas[0].title != dataObject.placeHolder) {
            dataObject.schemas.unshift({ title: dataObject.placeHolder, schemaid: null });
        }
        $scope.selectedSchema.value = dataObject.schemas[0];

        var schemas = [];

        $.each(dataObject.schemas, function (key, value) {
            if (value.cached) {
                schemas.push(schemaCacheService.getCachedSchema(value.applicationName, value.schemaId));
            } else {
                schemas.push(value);
                schemaCacheService.addSchemaToCache(value);
            }
        });

        $scope.schemas = schemas;

        $scope.$emit('sw_titlechanged', title);
        if (GetPopUpMode() === 'browser') {
            window.document.title = title;
        }
        $scope.applicationname = applicationName;
        $scope.selectedModeRequest = mode;
        $scope.schemaSelectionLabel = dataObject.schemaSelectionLabel;
    };

    initApplication();

};
(function (app, angular, $) {
    "use strict";



app.directive('expandedItemOutput', function ($compile) {
    return {
        restrict: "E",
        replace: true,
        scope: {
            displayables: '=',
            schema: '=',
            datamap: '=',
            cancelfn: '&'
        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            if (angular.isArray(scope.displayables)) {
                element.append(
                    "<crud-output schema='schema'" +
                                "datamap='datamap'" +
                                "displayables='displayables'" +
                                "cancelfn='cancelfn()'></crud-output>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});

app.directive('expandedItemInput', function ($compile) {
    return {
        restrict: "E",
        replace: true,
        scope: {
            displayables: '=',
            schema: '=',
            datamap: '=',
            associationOptions: '=',
            savefn: '&',
            cancelfn: '&'
        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            if (angular.isArray(scope.displayables)) {
                element.append(
                    "<crud-input schema='schema'" +
                                "datamap='datamap'" +
                                "displayables='displayables'" +
                                "associationOptions='associationOptions'" +
                                "savefn='savefn()'" +
                                "cancelfn='cancelfn()'></crud-input>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});

app.directive('newItemInput', function ($compile) {

    return {
        restrict: "E",
        replace: true,
        scope: {
            displayables: '=',
            elementid: '=',
            schema: '=',
            datamap: '=',
            associationOptions: '=',
            cancelfn: '&',
            savefn: '&'

        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            if (angular.isArray(scope.displayables)) {

                $.each(scope.displayables, function (key, value) {
                    var target = value.attribute;
                    if (value.defaultValue != undefined && target != undefined) {
                        if (scope.datamap[target] == null) {
                            //TODO: extract a service here, to be able to use @user, @person, @date, etc...
                            scope.datamap[target] = value.defaultValue;
                        }
                    }
                });

                element.append(
                    "<crud-input schema='schema'" +
                                "datamap='datamap'" +
                                "displayables='displayables'" +
                                "elementid='elementid'" +
                                "associationOptions='associationOptions'" +
                                "savefn='savefn()'" +
                                "cancelfn='cancelfn()'></crud-input>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});

app.directive('compositionListWrapper', function ($compile, i18NService, $log, $rootScope, spinService, compositionService) {
    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            metadata: '=',
            parentschema: '=',
            parentdata: '=',
            cancelfn: '&',
            previousschema: '=',
            previousdata: '=',
            inline: '@',
            tabid: '@'
        },
        link: function (scope, element, attrs) {

            var doLoad = function () {
                $log.getInstance('compositionlistwrapper#doLoad').debug('loading composition {0}'.format(scope.tabid));
                var metadata = scope.metadata;
                scope.tabLabel = i18NService.get18nValue(metadata.schema.schemas.list.applicationName + '._title', metadata.label);
                if (scope.parentdata.fields) {
                    scope.compositiondata = scope.parentdata.fields[scope.metadata.relationship];
                } else {
                    scope.compositiondata = scope.parentdata[scope.metadata.relationship];
                }
                scope.compositionschemadefinition = metadata.schema;
                scope.relationship = metadata.relationship;
                element.append("<composition-list title='{{tabLabel}}'" +
                    "compositionschemadefinition='compositionschemadefinition'" +
                    "relationship='{{relationship}}'" +
                    "compositiondata='compositiondata'" +
                    "parentschema='parentschema'" +
                    "parentdata='parentdata'" +
                    "cancelfn='toListSchema(data,schema)'" +
                    "previousschema='previousschema' previousdata='previousdata'/>");
                $compile(element.contents())(scope);
                scope.loaded = true;
            }

            var custom = scope.metadata.schema.renderer.rendererType == 'custom';
            var isInline = scope.metadata.inline;

            if (scope.metadata.type == "ApplicationCompositionDefinition" && isInline && !custom) {
                doLoad();
            }

            scope.$on("sw_lazyloadtab", function (event, tabid) {
                if (scope.tabid != tabid) {
                    //not this tab
                    return;
                }
                if (!compositionService.isCompositionLodaded(scope.tabid)) {
                    spinService.start({ compositionSpin: true });
                }
                if (scope.tabid == tabid && !scope.loaded) {
                    doLoad();
                }
            });

        }



    }
});


app.directive('compositionList', function (contextService, spinService) {

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/composition_list.html'),
        scope: {
            compositionschemadefinition: '=',
            compositiondata: '=',
            parentdata: '=',
            relationship: '@',
            title: '@',
            cancelfn: '&',
            previousschema: '=',
            previousdata: '=',
            parentschema: '='
        },

        controller: ["$scope", "$log", "$filter", "$injector", "$http", "$element", "$rootScope", "i18NService", "tabsService",
            "formatService", "fieldService", "commandService", "compositionService", "validationService", "expressionService", "$timeout",
            function ($scope, $log, $filter, $injector, $http, $element, $rootScope, i18NService, tabsService,
                formatService, fieldService, commandService, compositionService, validationService, expressionService, $timeout) {

            function init() {
                //Extra variables
                $scope.compositionlistschema = $scope.compositionschemadefinition.schemas.list;
                $scope.compositiondetailschema = $scope.compositionschemadefinition.schemas.detail;
                $scope.fetchfromserver = $scope.compositionschemadefinition.fetchFromServer;
                $scope.collectionproperties = $scope.compositionschemadefinition.collectionProperties;
                $scope.inline = $scope.compositionschemadefinition.inline;
                //


                $scope.clonedCompositionData = [];
                jQuery.extend($scope.clonedCompositionData, $scope.compositiondata);
                $scope.isNoRecords = $scope.clonedCompositionData.length > 0 ? false : true;
                $scope.detailData = {};
                $scope.noupdateallowed = !expressionService.evaluate($scope.collectionproperties.allowUpdate, $scope.parentdata);
                $scope.expanded = false;
                $scope.wasExpandedBefore = false;
                $scope.isReadonly = !expressionService.evaluate($scope.collectionproperties.allowUpdate, $scope.parentdata);


                $injector.invoke(BaseController, this, {
                    $scope: $scope,
                    i18NService: i18NService,
                    fieldService: fieldService,
                    commandService: commandService
                });

                if (!$scope.paginationData) {
                    //case the tab is loaded after the event result, the event would not be present on the screen
                    $scope.paginationData = contextService.get("compositionpagination_{0}".format($scope.relationship), true, true);
                    //workaround for hapag
                    if ($scope.paginationData) {
                        $scope.paginationData.selectedPage = $scope.paginationData.pageNumber;
                    }
                }

                $scope.showPagination = !$scope.isNoRecords && // has items to show
                                        !!$scope.paginationData && // has paginationdata
                                        $scope.paginationData.paginationOptions.some(function (option) { // totalCount is bigger than at least one option
                                            return $scope.paginationData.totalCount > option;
                                        });

                // scroll to top on ajax errors
                $scope.$on("sw_ajaxerror", function() {
                    $(document.body).animate({ scrollTop: 0 });
                });

            };

            init();

            $scope.compositionProvider = function () {
                var localCommands = {};
                var toAdd = [];
                localCommands.toAdd = toAdd;
                var log = $log.getInstance('composition_service#compositionProvider');

                localCommands.toKeep = compositionService.getListCommandsToKeep($scope.compositionschemadefinition);

                if (!expressionService.evaluate($scope.collectionproperties.allowInsertion, $scope.parentdata) || $scope.inline) {
                    log.debug('local commands without add. {0} '.format(JSON.stringify(localCommands)));
                    return localCommands;
                }

                var addCommand = {};
                addCommand.label = $scope.i18N($scope.relationship + '.add', 'Add ' + $scope.title);
                addCommand.method = $scope.newDetailFn;
                addCommand.defaultPosition = 'left';
                toAdd.push(addCommand);
                log.debug('local commands: {0} '.format(JSON.stringify(localCommands)));
                return localCommands;
            };


            $scope.newDetailFn = function ($event) {
                $scope.newDetail = true;
                $scope.selecteditem = {};
                $scope.collapseAll();
                // scroll to detail form (500ms for smoothness)
                $timeout(function () {
                    var scrollTarget = $(".js_compositionnewitem");
                    if (!scrollTarget[0]) scrollTarget = $($event.delegateTarget);
                    $(document.body).animate({ scrollTop: scrollTarget.offset().top });
                }, 500, false);
            };

            var doToggle = function (id, item, forcedState) {
                if ($scope.detailData[id] == undefined) {
                    $scope.detailData[id] = {};
                    $scope.detailData[id].expanded = false;
                }
                $scope.detailData[id].data = item;
                var newState = forcedState != undefined ? forcedState : !$scope.detailData[id].expanded;
                $scope.detailData[id].expanded = newState;
            };

            $scope.toggleDetails = function (item, updating) {
                contextService.insertIntoContext("sw:crudbody:scrolltop", false);
                
                var fullServiceName = $scope.compositionlistschema.properties['list.click.service'];
                if (fullServiceName != null) {
                    commandService.executeClickCustomCommand(fullServiceName, item, $scope.compositionlistschema.displayables);
                    return;
                };

                $scope.isReadOnly = !updating;
                var compositionId = item[$scope.compositionlistschema.idFieldName];
                if (!$scope.fetchfromserver) {
                    doToggle(compositionId, item);
                    return;
                } else if ($scope.detailData[compositionId] != undefined) {
                    doToggle(compositionId, $scope.detailData[compositionId].data);
                    return;
                }

                var compositiondetailschema = $scope.compositiondetailschema;
                var applicationName = compositiondetailschema.applicationName;
                var parameters = {};
                var request = {};
                var key = {};
                parameters.request = request;
                request.id = compositionId;
                request.key = key;
                key.schemaId = compositiondetailschema.schemaId;
                key.mode = compositiondetailschema.mode;
                key.platform = "web";
                var urlToCall = url("/api/data/" + applicationName + "?" + $.param(parameters));
                $http.get(urlToCall)
                    .then( function (response) {
                        var result = response.data;
                        doToggle(compositionId, result.resultObject.fields);
                        $rootScope.$broadcast('sw_bodyrenderedevent', $element.parents('.tab-pane').attr('id'));
                    })
                    .then(function () {
                    });
            };

            $scope.getGridColumnStyle = function (column, propertyName) {
                var property = column.rendererParameters[propertyName];

                if (property != null) {
                    return property;
                }

                if (propertyName === 'maxwidth') {
                    var high = $(window).width() > 1199;
                    if (high) {
                        return '135px';
                    }
                    return '100px';
                }
                return null;
            }

            $scope.$on('sw_compositiondataresolved', function (event, compositiondata) {
                if (!compositiondata[$scope.relationship]) {
                    //this is not the data this tab is interested
                    return;
                }
                spinService.stop({ compositionSpin: true });
                $scope.paginationData = compositiondata[$scope.relationship].paginationData;
                if (!!$scope.paginationData) {
                    $scope.paginationData.selectedPage = $scope.paginationData.pageNumber;
                }
                $scope.compositiondata = compositiondata[$scope.relationship].list;
                
                init();
                if (!$scope.$$phase && !$scope.$root.$$phase) {
                    $scope.$digest();
                }
            });

            $scope.cancelComposition = function () {
                $scope.newDetail = false;
                //                $scope.isReadonly = true;
            };

            $scope.cancel = function (previousData, previousSchema) {
                $('#crudmodal').modal('hide');
                if (GetPopUpMode() == 'browser') {
                    close();
                }
                $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
                $scope.$emit('sw_cancelclicked');
            };

            $scope.refresh = function () {
                //TODO: make a composition refresh only --> now it will be samething as F5
                window.location.href = window.location.href;
            };

            $scope.allowButton = function (value) {
                return expressionService.evaluate(value, $scope.parentdata) && $scope.inline;
            };

            $scope.save = function () {
                var selecteditem = $scope.selecteditem;
                //todo:update
                if ($scope.compositiondata == null) {
                    $scope.compositiondata = [];
                }

                var validationErrors = validationService.validate($scope.compositionschemadefinition.schemas.detail, $scope.compositionschemadefinition.schemas.detail.displayables, selecteditem);
                if (validationErrors.length > 0) {
                    //interrupting here, can´t be done inside service
                    return;
                }

                $scope.compositiondata.push(selecteditem);

                if (!$scope.collectionproperties.autoCommit) {
                    return;
                }

                var alwaysrefresh = $scope.compositiondetailschema.properties && "true" == $scope.compositiondetailschema.properties['compositions.alwaysrefresh'];
                if (alwaysrefresh) {
                    //this will disable success message, since we know we´ll need to refresh the screen
                    contextService.insertIntoContext("refreshscreen", true, true);
                }
                $scope.$parent.$parent.save(null, {
                    successCbk: function (data) {

                        if (alwaysrefresh) {
                            window.location.href = window.location.href;
                            return;
                        }
                        var updatedArray = data.resultObject.fields[$scope.relationship];
                        if (updatedArray == null || updatedArray.length == 0) {
                            window.location.href = window.location.href;
                            return;
                        }
                        $scope.clonedCompositionData = updatedArray;
                        $scope.compositiondata = updatedArray;
                        $scope.newDetail = false;
                        $scope.isReadonly = !$scope.collectionproperties.allowUpdate;
                        $scope.selecteditem = {};
                        $scope.collapseAll();

                        var relName = $scope.relationship;
                        var eventData = {};

                        eventData[relName] = {
                            list: data.resultObject.fields[relName],
                            relationship: relName
                        };

                        $scope.$emit("sw_compositiondataresolved", eventData);
                    },
                    failureCbk: function (data) {
                        var idx = $scope.compositiondata.indexOf(selecteditem);
                        if (idx != -1) {
                            $scope.compositiondata.splice(idx, 1);
                        }
                        $scope.isReadonly = !$scope.collectionproperties.allowUpdate;
                    },
                    isComposition: true,
                    nextSchemaObj: { schemaId: $scope.$parent.$parent.schema.schemaId },
                    refresh: alwaysrefresh
                });
            };

            $scope.collapseAll = function () {
                $.each($scope.detailData, function (key, value) {
                    $scope.detailData[key].expanded = false;
                });
            };

            $scope.showListCommands = function () {
                return !$scope.detail || $scope.expanded;
            };

            function buildExpandAllParams() {
                var params = {};
                params.key = {};
                params.options = {};
                params.application = $scope.parentschema.applicationName;
                params.detailRequest = {};
                var key = {};
                params.detailRequest.key = key;

                var parentSchema = $scope.parentschema;
                params.detailRequest.id = fieldService.getId($scope.parentdata, parentSchema);
                key.schemaId = parentSchema.schemaId;
                key.mode = parentSchema.mode;
                key.platform = platform();

                var compositionsToExpand = {};
                compositionsToExpand[$scope.relationship] = { schema: $scope.compositionlistschema, value: true };
                //                var compositionsToExpand = { 'worklog_': true };

                params.options.compositionsToExpand = tabsService.buildCompositionsToExpand(compositionsToExpand, parentSchema,
                    $scope.parentdata, $scope.compositiondetailschema.schemaId, [], false);
                params.options.printMode = true;
                return params;
            }

            $scope.expandAll = function () {
                if ($scope.wasExpandedBefore) {
                    $.each($scope.detailData, function (key, value) {
                        $scope.detailData[key].expanded = true;
                    });
                    return;
                }

                var urlToInvoke = removeEncoding(url("/api/generic/ExtendedData/ExpandCompositions?" + $.param(buildExpandAllParams())));
                $http.get(urlToInvoke).success(function (result) {
                    $.each(result.resultObject[$scope.relationship], function (key, value) {
                        doToggle(value[$scope.compositiondetailschema.idFieldName], value, true);
                    });
                    $scope.wasExpandedBefore = true;
                });
            };

            $scope.getFormattedValue = function (value, column) {
                return formatService.format(value, column);
            };

            $scope.shouldDisplayCommand = function (commandSchema, id) {
                return commandService.shouldDisplayCommand(commandSchema, id);
            };

            $scope.commandLabel = function (schema, id, defaultValue) {
                return commandService.commandLabel(schema, id, defaultValue);
            };

            $scope.isEnabledToExpand = function () {
                return $scope.isReadonly && $scope.compositiondetailschema != null &&
                    ($scope.compositionlistschema.properties.expansible == undefined ||
                    $scope.compositionlistschema.properties.expansible == 'true');
            };
            //overriden function
            $scope.i18NLabel = function (fieldMetadata) {
                return i18NService.getI18nLabel(fieldMetadata, $scope.compositionlistschema);
            };


            /* pagination */

            $scope.selectPage = function (pageNumber, pageSize, printMode) {
                if (pageNumber === undefined || pageNumber <= 0 || pageNumber > $scope.paginationData.pageCount) {
                    $scope.paginationData.pageNumber = pageNumber;
                    return;
                }
                compositionService
                    .getCompositionList($scope.relationship, $scope.parentschema, $scope.parentdata.fields, pageNumber, $scope.paginationData.pageSize)
                    .then(function (result) {
                        $scope.clonedCompositionData = [];
                        // clear lists
                        $scope.compositiondata = result[$scope.relationship].list;
                        $scope.paginationData = result[$scope.relationship].paginationData;
                        //workaround for hapag
                        $scope.paginationData.selectedPage = $scope.paginationData.pageNumber;
                        init();
                    });
            };
        }]
    };
});

})(app, angular, jQuery);;
var app = angular.module('sw_layout');
var CONDITIONMODAL_$_KEY = '[data-class="conditionModal"]';

app.directive('configrendered', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    $('[rel=tooltip]').tooltip({ container: 'body' });
                    scope.$emit('sw_bodyrenderedevent');
                });
            }
        }
    };
});

app.directive('conditionmodal', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/configConditionModal.html'),
        scope: {
            profile: '=',
            module: '=',
            type: '=',
            condition: '=',
            fullkey: '='
        },
        controller: function ($scope, $http, i18NService) {

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.init = function () {
                if ($scope.condition == null) {
                    $scope.condition = {};
                    $scope.condition.appContext = {};
                }
            };

            $scope.saveCondition = function () {
                $scope.condition.fullKey = $scope.fullkey;
                var jsonString = angular.toJson($scope.condition);
                $http.put(url("/api/generic/Configuration/CreateCondition"), jsonString)
                    .success(function (data) {
                        var modal = $(CONDITIONMODAL_$_KEY);
                        modal.modal('hide');
                    });
            };

            $scope.init();


        }
    };
});

function ConfigController($scope, $http, i18NService, alertService) {



    var noneProfile = {
        name: '-- Any --',
        id: null
    };

    var noneCondition = {
        alias: "-- No Specific --",
        id: null
    };

    var noneModule = {
        alias: "-- Any --",
        name: null,
        id: null
    };

    function navigateToCategory(categories, fullKey) {
        var resultValue = null;
        $.each(categories, function (k, value) {
            if (value.fullKey == fullKey) {
                resultValue = value;
            } else if (fullKey.startsWith(value.fullKey)) {
                resultValue = navigateToCategory(value.children, fullKey);
            }
        });
        return resultValue;
    };

    function isNullOrEqualMatching(defValue, scopeValue, property) {
        if (defValue == null) {
            return { mode: scopeValue[property] == null ? true : null };
        }
        return { mode: (defValue.isEqual(scopeValue[property],true)) };
    }

    function isNullOrEqualMatchingNumeric(defValue, scopeValue, property) {
        if (defValue == null) {
            return { mode: scopeValue[property] == null ? true : null };
        }
        return { mode: (defValue == scopeValue[property]) };
    }


    function nullOrEqualBothNulls(defValue, scopeValue, property) {
        if (defValue == null) {
            return scopeValue[property] == null;
        }
        return defValue.isEqual(scopeValue[property],true);
    }

    

    //    function zeroOrEqual(defValue, scopeValue, property) {
    //        if (defValue == 0) {
    //            return true;
    //        }
    //        return defValue == scopeValue[property];
    //    }


    $scope.$name = 'ConfigController';

    $scope.doInit = function () {
        $scope.currentCategory = {};
        $scope.showSave = false;
        $scope.categoryData = $scope.resultData.categories;
        $scope.type = $scope.resultData.type;

        $scope.currentmodule = noneModule;
        $scope.currentprofile = noneProfile;
        $scope.currentcondition = noneCondition;

        $scope.currentValues = {};
        $scope.currentDefaultValues = {};

        if ($scope.resultData.profiles != undefined) {
            $scope.resultData.profiles.unshift(noneProfile);
        }
        if ($scope.resultData.modules != undefined) {
            $scope.resultData.modules.unshift(noneModule);
        }
        if ($scope.resultData.conditions != undefined) {
            $scope.resultData.conditions.unshift(noneCondition);
        }

        $scope.profiles = $scope.resultData.profiles;
        $scope.modules = $scope.resultData.modules;
        $scope.allConditions = $scope.resultData.conditions;
    };

    $scope.restoreDefault = function (definition) {
        alertService.confirm("", "", function (result) {
            $scope.currentValues[definition.fullKey] = null;
        }, "Are you sure you want to restore the default value?");

    };

    $scope.getCurrentCondition = function () {
        if ($scope.currentCondition == null || $scope.currentCondition.id == null) {
            return null;
        }
        return $scope.currentcondition;
    };

    $scope.removeCondition = function (condition) {
        if (condition.id == null) {
            alertService.alert('This condition cannot be deleted');
            return;
        }

        alertService.confirm('condition', condition.alias, function (result) {
            var jsonString = angular.toJson(condition);
            $http.put(url("/api/generic/Configuration/DeleteCondition?currentKey=" + $scope.currentCategory.fullKey), jsonString)
                .success(function (data) {
                    data.conditions = data.resultObject;
                    if (data.conditions != undefined) {
                        data.conditions.unshift(noneCondition);
                    }
                    $scope.allConditions = data.conditions;
                    //clear cache and rebuild data
                    $scope.currentCategory.conditionsToShow = null;
                    $scope.getConditions($scope.currentCategory);
                    $scope.currentcondition = noneCondition;
                });
        });
    };

    $scope.createCondition = function () {
        var modal = $(CONDITIONMODAL_$_KEY);
        $scope.modalcondition = $scope.getCurrentCondition();
        $scope.fullkey = $scope.currentCategory.fullKey;
        modal.draggable();
        modal.modal('show');
    };

    $scope.editCondition = function (condition) {
        if (condition.id == null) {
            alertService.alert('This condition cannot be edited');
            return;
        }

        var modal = $(CONDITIONMODAL_$_KEY);
        $scope.modalcondition = condition;
        $scope.fullkey = $scope.currentCategory.fullKey;
        modal.draggable();
        modal.modal('show');
    };

    $scope.getConditions = function (category) {
        var shouldShow = false;
        if (category.conditionsToShow != null) {
            //cache
            return category.conditionsToShow;
        }

        var conditions = [];
        $.each($scope.allConditions, function (key, condition) {
            if (condition.id == null || condition.global || condition.fullKey == category.fullKey) {
                conditions.push(condition);
            }
        });

        $.each(category.definitions, function (key, definition) {
            if (definition.contextualized) {
                shouldShow = true;
            }
            if (definition.values != null) {
                $.each(definition.values, function (index, value) {
                    if (value.condition != null) {
                        conditions.push(value.condition);
                    }
                });
            }
        });
        //cache
        category.conditionsToShow = {};
        category.conditionsToShow.values = conditions;
        category.conditionsToShow.shouldShow = shouldShow;
        return category.conditionsToShow;
    };

    $scope.doInit();

    $scope.showDefinitionsOfCondition = function (currentcondition, cat) {
        $scope.currentcondition = currentcondition;
        $scope.showDefinitions(cat);
    };

    $scope.showDefinitions = function (cat) {
        $scope.currentCategory = cat;
        if (cat.definitions == null || cat.definitions.length == 0) {
            $scope.showSave = false;
            return;
        }
        $scope.showSave = true;
        //        $scope.currentCondition = noneCondition;
        for (var i = 0; i < cat.definitions.length; i++) {
            var def = cat.definitions[i];
            $scope.currentValues[def.fullKey] = null;
            $scope.currentDefaultValues[def.fullKey] = def.stringValue;
            var values = def.values;
            if (values == null) {
                //sometimes we don´t have any value but the default one
                return;
            }
            var exactMatchSet = false;
            $.each(values, function (key, propertyValue) {
                var moduleMatches = isNullOrEqualMatching(propertyValue.module, $scope.currentmodule, 'id');
                var profileMatches = isNullOrEqualMatchingNumeric(propertyValue.userProfile, $scope.currentprofile, 'id');
                var exactMatch = moduleMatches.mode == true && profileMatches.mode == true;

                if ((moduleMatches.mode == null || moduleMatches.mode == true) &&
                    (profileMatches.mode == null || profileMatches.mode == true) &&
                    nullOrEqualBothNulls(propertyValue.condition, $scope.currentcondition, 'id')) {
                    if (exactMatch) {
                        exactMatchSet = true;
                    }
                    if (exactMatch || !exactMatchSet) {
                        $scope.currentValues[def.fullKey] = propertyValue.stringValue;
                    }
                    if (propertyValue.systemStringValue != null) {
                        $scope.currentDefaultValues[def.fullKey] = propertyValue.systemStringValue;
                    }

                }
            });


        }
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.Alias = function (definition) {
        if (definition.alias != null) {
            return definition.alias;
        }
        return definition.key;
    };

    $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
        if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("Configuration.html") != -1) {
            $scope.doInit();
        }
    });

    $scope.save = function () {
        var currentCategory = $scope.currentCategory;
        currentCategory.valuesToSave = $scope.currentValues;
        currentCategory.module = $scope.currentmodule;
        currentCategory.userProfile = $scope.currentprofile.id;
        currentCategory.condition = $scope.currentcondition.id == null ? null : $scope.currentcondition;
        var jsonString = angular.toJson($scope.currentCategory);
        $http.put(url("/api/generic/Configuration/Put"), jsonString)
            .success(function (data) {
                $scope.categoryData = data.resultObject;
                $scope.currentCategory = navigateToCategory($scope.categoryData, currentCategory.fullKey);
            });
    };

    $scope.$on('sw_bodyrenderedevent', function (ngRepeatFinishedEvent) {
        $("input[type=file]").filestyle({
            image: url("/Content/Images/update_24.png"),
            imageheight: 32,
            imagewidth: 25,
            width: 250
        });
    });


};;
//idea took from  https://www.exratione.com/2013/10/two-approaches-to-angularjs-controller-inheritance/
function BaseController($scope, i18NService, fieldService,commandService) {

    /* i18N functions */
    $scope.i18NLabelTooltip = function (fieldMetadata) {
        return i18NService.getI18nLabelTooltip(fieldMetadata, $scope.schema);
    };
    //to allow overriding
    $scope.i18NLabel = $scope.i18NLabel || function (fieldMetadata) {
        return i18NService.getI18nLabel(fieldMetadata, $scope.schema);
    };

    $scope.i18NOptionField = function (option, fieldMetadata, schema) {
        return i18NService.getI18nOptionField(option, fieldMetadata, schema);
    };


    $scope.i18N = function (key, defaultValue, paramArray,languageToForce) {
        return i18NService.get18nValue(key, defaultValue, paramArray, languageToForce);
    };

    $scope.i18NCommandLabel = function (command) {
        return i18NService.getI18nCommandLabel(command, $scope.schema);
    };

    $scope.i18NTabLabel = function (tab) {
        return i18NService.getTabLabel(tab, $scope.schema);
    };

    $scope.isIE9 = function () {
        return isIe9();
    };

    $scope.isIe9 = function () {
        return isIe9();
    };

    $scope.i18nSectionLabel = function (section) {
        var label = i18NService.getI18nSectionHeaderLabel(section, section.header, $scope.schema);
        if (label != undefined && label != "") {
            label = label.replace(':', '');
        }
        return label;
    };

    $scope.nonTabFields = function (displayables) {
        return fieldService.nonTabFields(displayables);
    };

    $scope.contextPath = function (path) {
        return url(path);
    };

    $scope.isFieldHidden = function (application, fieldMetadata) {
        return fieldService.isFieldHidden($scope.datamap, application, fieldMetadata);
    };

    $scope.shouldDisplayCommand = function (commandSchema, id) {
        return commandService.shouldDisplayCommand(commandSchema, id);
    };

    $scope.commandLabel = function (schema, id, defaultValue) {
        return commandService.commandLabel(schema, id, defaultValue);
    };


    $scope.isCommandHidden = function (schema, command) {
        return commandService.isCommandHidden($scope.datamap, schema, command);
    };

    $scope.isCommandEnabled = function (schema, command) {
        return commandService.isCommandEnabled($scope.datamap, schema, command);
    }

    $scope.doCommand = function (command) {
        commandService.doCommand($scope, command);
    };

    $scope.hasSameLineLabel = function (fieldMetadata) {
        if (!$scope.isVerticalOrientation()) {
            return false;
        }
        if (fieldMetadata.rendererType == "TABLE") {
            //workaround because compositions are appending "" as default label values, but we dont want it!
            return false;
        }
        if (fieldMetadata.rendererType == "label" && "true" == fieldMetadata.rendererParameters["overflow"]) {
            return false;
        }
        return fieldMetadata.label !=null || (fieldMetadata.header != null && fieldMetadata.header.displacement != 'ontop');
    };

    $scope.isVerticalOrientation = function () {
        return $scope.orientation == 'vertical';
    };


    $scope.getSectionClass = function (fieldMetadata) {
        if (fieldMetadata.rendererType == "TABLE") {
            //workaround because compositions are appending "" as default label values, but we dont want it!
            return null;
        }
        if (!$scope.hasSameLineLabel(fieldMetadata)) {
            return 'col-horizontal-orientation';
        }
        return null;
    };

    

};
var app = angular.module('sw_layout');


app.directive('tabsrendered', function ($timeout, $log, $rootScope, contextService, spinService) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            // Do not execute until the last iteration of ng-repeat has been reached,
            // or if $last is undefined (this happens when the tabsrendered directive 
            // is placed on something other than ng-repeat).
            if (scope.$last === false) {
                return;
            }

            var log = $log.getInstance('tabsrendered');
            log.debug("finished rendering tabs of detail screen");
            $timeout(function () {
                var firstTabId = null;
                $('.compositiondetailtab li>a').each(function () {
                    var $this = $(this);
                    if (firstTabId == null) {
                        firstTabId = $(this).data('tabid');
                    }
                    $this.click(function (e) {
                        e.preventDefault();
                        $this.tab('show');
                        var tabId = $(this).data('tabid');

                        log.trace('lazy loading tab {0}'.format(tabId));
                        spinService.stop({ compositionSpin: true });
                        $rootScope.$broadcast('sw_lazyloadtab', tabId);

                    });

                });
                $rootScope.$broadcast("sw_alltabsloaded", firstTabId);

            }, 0, false);
        }
    };
});


app.directive('crudBody', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_body.html'),
        scope: {
            isList: '=',
            isDetail: '=',
            blockedassociations: '=',
            associationOptions: '=',
            associationSchemas: '=',
            schema: '=',
            datamap: '=',
            cancelfn: '&',
            previousschema: '=',
            previousdata: '=',
            paginationdata: '=',
            searchData: '=',
            searchOperator: '=',
            searchSort: '=',
            ismodal: '@',
            timestamp: '=',
            checked: '='
        },

        controller: function ($scope, $http, $rootScope, $filter, $injector,
            formatService, fixHeaderService,
            searchService, tabsService,
            fieldService, commandService, i18NService,
            validationService, submitService, redirectService,
            associationService, $timeout, dispatcherService) {

            $scope.$name = 'crudbody' + ($scope.ismodal == "false" ? 'modal' : '');
            contextService.insertIntoContext("sw:crudbody:scrolltop", true);


            $scope.getFormattedValue = function (value, column) {
                var formattedValue = formatService.format(value, column);
                if (formattedValue == "-666") {
                    //this magic number should never be displayed! 
                    //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                    return null;
                }
                return formattedValue;
            };

            $scope.setActiveTab = function (tabId) {
                contextService.setActiveTab(tabId);
            }

            $scope.hasTabs = function (schema) {
                return tabsService.hasTabs(schema);
            };
            $scope.isCommand = function (schema) {
                if ($scope.schema.properties['command.select'] == "true") {
                    return true;
                }
            };
            $scope.isNotHapagTest = function () {
                return $rootScope.clientName !== 'hapag';
            };
            $scope.tabsDisplayables = function (schema) {
                return tabsService.tabsDisplayables(schema);
            };

            $scope.$on('sw_bodyrenderedevent', function (parentElementId) {
                var tab = contextService.getActiveTab();
                if (tab != null) {
                    redirectService.redirectToTab(tab);
                }
                // make sure we are seeing the top of the grid 
                // unless it is prevented
                var scrollTop = contextService.fetchFromContext("sw:crudbody:scrolltop", false, false);
                if (!!scrollTop && scrollTop !== "false") {
                    window.scrollTo(0, 0);
                } else {
                    // do not scroll and reset to default behaviour
                    contextService.insertIntoContext("sw:crudbody:scrolltop", true);
                }

                var onLoadMessage = contextService.fetchFromContext("onloadMessage", false, false, true);
                if (onLoadMessage) {
                    var data = {
                        successMessage: onLoadMessage
                    }
                    $rootScope.$broadcast('sw_successmessage', data);
                    $timeout(function () {
                        $rootScope.$broadcast('sw_successmessagetimeout', { successMessage: null });
                    }, contextService.retrieveFromContext('successMessageTimeOut'));
                }
            });

            $scope.$on('sw_successmessagetimeout', function (event, data) {
                if (!$rootScope.showSuccessMessage) {
                    fixHeaderService.resetTableConfig($scope.schema);
                }
            });


            $scope.$on('sw_errormessage', function (event, show) {
                fixHeaderService.topErrorMessageHandler(show, $scope.$parent.isDetail, $scope.schema);
            });

            $scope.$on('sw_compositiondataresolved', function (event, compositiondata) {
                for (var ob in compositiondata) {
                    if (!compositiondata.hasOwnProperty(ob)) {
                        continue;
                    }
                    $scope.datamap.fields[ob] = compositiondata[ob].list;
                }
                
            });

            function defaultSuccessFunction(data) {
                $scope.$parent.multipleSchema = false;
                $scope.$parent.schemas = null;
                if (data != null) {
                    if (data.type == 'ActionRedirectResponse') {
                        //we´ll not do a crud action on this case, so totally different workflow needed
                        redirectService.redirectToAction(null, data.controller, data.action, data.parameters);
                    } else {
                        var nextSchema = data.schema;
                        $scope.$parent.renderViewWithData(nextSchema.applicationName, nextSchema.schemaId, nextSchema.mode, nextSchema.title, data);
                    }
                }
            }


            $scope.isEditing = function (schema) {
                var idFieldName = schema.idFieldName;
                var id = $scope.datamap.fields[idFieldName];
                return id != null;
            };


            $scope.shouldShowField = function (expression) {
                if (expression == "true") {
                    return true;
                }
                var stringExpression = '$scope.datamap.' + expression;
                var ret = eval(stringExpression);
                return ret;
            };


            $scope.isHapag = function () {
                return $rootScope.clientName == "hapag";
            };



            $scope.renderListView = function (parameters) {
                $scope.$parent.multipleSchema = false;
                $scope.$parent.schemas = null;
                var listSchema = 'list';
                if ($scope.schema != null && $scope.schema.stereotype.isEqual('list', true)) {
                    //if we have a list schema already declared, keep it
                    listSchema = $scope.schema.schemaId;
                }
                $scope.$parent.renderView($scope.$parent.applicationname, listSchema, 'none', $scope.title, parameters);
            };

            $scope.toListSchema = function (data, schema) {
                /*if (schema instanceof Array) {
                    $scope.$parent.multipleSchemaHandling($scope.$parent.resultObject);
                } else {*/
                $scope.$parent.multipleSchema = false;
                $scope.$parent.schemas = null;
                $('#crudmodal').modal('hide');
                $scope.showingModal = false;
                if (GetPopUpMode() == 'browser') {
                    open(location, '_self').close();
                }
                $scope.schema = schema;
                $scope.datamap = data;
                if ($scope.schema == null || $scope.datamap == null || $scope.schema.stereotype == 'Detail') {
                    $scope.renderListView(null);
                    return;
                } else {
                    $scope.$emit('sw_titlechanged', schema.title);
                    $scope.$parent.toList(null);
                }
                //}
            };

            $scope.delete = function () {

                var schema = $scope.schema;
                var idFieldName = schema.idFieldName;
                var applicationName = schema.applicationName;
                var id = $scope.datamap.fields[idFieldName];

                var parameters = {};
                if (sessionStorage.mockmaximo == "true") {
                    parameters.mockmaximo = true;
                }
                parameters.platform = platform();
                parameters = addSchemaDataToParameters(parameters, $scope.schema);
                var deleteParams = $.param(parameters);

                var deleteURL = removeEncoding(url("/api/data/" + applicationName + "/" + id + "?" + deleteParams));
                $http.delete(deleteURL)
                    .success(function (data) {
                        defaultSuccessFunction(data);
                    });

            };

            $scope.save = function (selecteditem, parameters) {
                if (parameters == undefined) {
                    parameters = {};
                }
                var successCbk = parameters.successCbk;
                var failureCbk = parameters.failureCbk;
                var nextSchemaObj = parameters.nextSchemaObj;
                var applyDefaultSuccess = parameters.applyDefaultSuccess;
                var applyDefaultFailure = parameters.applyDefaultFailure;
                var isComposition = parameters.isComposition;

                //selectedItem would be passed in the case of a composition with autocommit=true. 
                //Otherwise, fetching from the $scope.datamap
                var fromDatamap = selecteditem == null;
                var itemToSave = fromDatamap ? $scope.datamap : selecteditem;
                var fields = fromDatamap ? itemToSave.fields : itemToSave;
                var applicationName = $scope.schema.applicationName;
                var idFieldName = $scope.schema.idFieldName;
                var id = fields[idFieldName];

                //hook for updating doing custom logic before sending the data to the server
                $rootScope.$broadcast("sw_beforeSave", fields);

                if (sessionStorage.mockclientvalidation == undefined) {
                    var validationErrors = validationService.validate($scope.schema, $scope.schema.displayables, fields);
                    if (validationErrors.length > 0) {
                        //interrupting here, can´t be done inside service
                        return;
                    }
                }
                //some fields might require special handling
                submitService.removeNullInvisibleFields($scope.schema.displayables, fields);
                fields = submitService.removeExtraFields(fields, true, $scope.schema);
                submitService.translateFields($scope.schema.displayables, fields);
                associationService.insertAssocationLabelsIfNeeded($scope.schema, fields, $scope.associationOptions);


                if ($scope.schema.properties["oncrudsaveevent.transformservice"]) {
                    var serviceSt = $scope.schema.properties["oncrudsaveevent.transformservice"];
                    var fn = dispatcherService.loadService(serviceSt.split(".")[0], serviceSt.split(".")[1]);
                    fn(
                        {
                            "datamap": fields,
                            "schema": $scope.schema,
                            "associationOptions": $scope.associationOptions,
                        })
                    ;
                }

                var jsonString = angular.toJson(fields);

                parameters = {};
                if (sessionStorage.mockmaximo == "true") {
                    //this will cause the maximo layer to be mocked, allowing testing of workflows without actually calling the backend
                    parameters.mockmaximo = true;
                }
                parameters = addSchemaDataToParameters(parameters, $scope.schema, nextSchemaObj);
                parameters.platform = platform();

                $rootScope.savingMain = !isComposition;

                if (isIe9()) {
                    var formToSubmitId = submitService.getFormToSubmitIfHasAttachement($scope.schema.displayables, fields);
                    if (formToSubmitId != null) {
                        var form = $(formToSubmitId);
                        submitService.submitForm(form, parameters, jsonString, applicationName);
                        return;
                    }
                }

                var saveParams = $.param(parameters);
                var urlToUse = url("/api/data/" + applicationName + "/" + id + "?" + saveParams);
                var command = id == null ? $http.post : $http.put;

                command(urlToUse, jsonString)
                    .success(function (data) {
                        if (successCbk == null || applyDefaultSuccess) {
                            defaultSuccessFunction(data);
                        }
                        if (successCbk != null) {
                            successCbk(data);
                        }
                    })
                    .error(function (data) {
                        if (failureCbk != null) {
                            failureCbk(data);
                        }
                    });
            };



            $injector.invoke(BaseController, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService
            });

        }
    };
});

;
app.directive('crudInputWrapper', function (contextService, $compile, $rootScope) {
    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",

        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            associationOptions: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            cancelfn: '&',
            savefn: '&',
            previousschema: '=',
            previousdata: '=',
            title: '=',
            elementid: '@',
            isMainTab: '@',
            tabid:'@'
        },

        link: function (scope, element, attrs) {
            var doLoad = function () {
                element.append(
                  "<crud-input elementid='crudInputMain' schema='schema' " +
                  "datamap='datamap' association-options='associationOptions' blockedassociations='blockedassociations'" +
                  "association-schemas='associationSchemas'cancelfn='toListSchema(data,schema)' displayables='displayables'" +
                  "savefn='save(selecteditem, parameters)' previousschema='previousschema' previousdata='previousdata' />"
               );
                $compile(element.contents())(scope);
                scope.loaded = true;
            }

            if (scope.schema.mode == "input" && ("true" == scope.isMainTab)) {
                doLoad();
            }

            scope.$on("sw_lazyloadtab", function (event, tabid) {
                if (scope.tabid == tabid && !scope.loaded) {
                    doLoad();
                }
            });

            scope.save = function () {
                scope.savefn();
            };
        }
    }
});


app.directive('crudInput', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_input.html'),
        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            associationOptions: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            cancelfn: '&',
            savefn: '&',
            previousschema: '=',
            previousdata: '=',
            title: '=',
            elementid: '@'
        },

        controller: function ($scope, $http,$injector, $element, printService, compositionService, commandService, fieldService, i18NService) {
            $scope.$name = 'crudinput';

            $scope.cancel = function () {
                $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
                $scope.$emit('sw_cancelclicked');
            };

            $scope.save = function () {
                $scope.savefn();
            };

            $scope.isEditing = function (schema, datamap) {
                var id = datamap[schema.idFieldName];
                return id != null;
            };

            $scope.printDetail = function () {
                var schema = $scope.schema;
                printService.printDetail(schema, $scope.datamap[schema.idFieldName]);
            };

            $injector.invoke(BaseController, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService:commandService
            });
        }
    };
});;
app.directive('configAssociationListInputDatamap', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {

            if (scope.$first) {
                scope.datamap[scope.fieldMetadata.attribute] = [];
            }
            var item = {};
            var displayables = scope.associationSchemas[scope.fieldMetadata.associationKey].displayables;
            for (i = 0; i < displayables.length; i++) {
                var attribute = displayables[i].attribute;
                scope.option.extrafields = scope.option.extrafields || {};
                item[attribute] = scope.option.extrafields[attribute];
            }
            scope.datamap[scope.fieldMetadata.attribute].push(item);
        }
    };
});

app.directive('configUpdateSectionDatamap', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {

            if (scope.$first) {
                scope.datamap[scope.fieldMetadata.id] = [];
            }
            var item = {};
            item["label"] = scope.i18NLabel(scope.field);
            item["value"] = scope.datamap[scope.field.attribute];

            scope.$watch('datamap["' + scope.field.attribute + '"]', function (newValue, oldValue) {
                if (oldValue == newValue) {
                    return;
                }
                scope.datamap[scope.fieldMetadata.id][scope.$index]["value"] = newValue;
            });

            item["#newvalue"] = '';

            scope.datamap[scope.fieldMetadata.id].push(item);
        }
    };
});

app.directive('sectionElementInput', function ($compile) {
    return {
        restrict: "E",
        replace: true,
        scope: {
            schema: '=',
            datamap: '=',
            displayables: '=',
            associationOptions: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            elementid: '@',
            orientation: '@',
            islabelless: '@'
        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            if (angular.isArray(scope.displayables)) {

                element.append(
                    "<crud-input-fields displayables='displayables'" +
                                "schema='schema'" +
                                "datamap='datamap'" +
                                "displayables='displayables'" +
                                "association-options='associationOptions'" +
                                "association-schemas='associationSchemas'" +
                                "blockedassociations='blockedassociations'" +
                                "elementid='{{elementid}}'" +
                                "orientation='{{orientation}}' insidelabellesssection={{islabelless}}></crud-input-fields>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});


app.directive('crudInputFields', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_input_fields.html'),
        scope: {
            schema: '=',
            datamap: '=',
            displayables: '=',
            associationOptions: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            elementid: '@',
            orientation: '@',
            insidelabellesssection: '@',
        },

        controller: function ($scope, $http, $element, $injector, $timeout,
            printService, compositionService, commandService, fieldService, i18NService,
            associationService, expressionService, styleService,
            cmpfacade, cmpComboDropdown, redirectService,dispatcherService) {

            $scope.$name = 'crud_input_fields';

            $scope.handlerTitleInputFile = function (cssclassaux) {
                var title = $scope.i18N('attachment.' + cssclassaux, 'No file selected');
                var fileInput = $('.' + cssclassaux);
                $().ready(function () {
                    fileInput.change(function () {
                        var titleaux = title;
                        if (fileInput != undefined && fileInput.val() != '') {
                            title = fileInput.val();
                        }
                        fileInput.attr('title', title);
                        title = titleaux;
                    });
                });
                return title;
            };

         

            $scope.$on('sw_block_association', function (event, association) {
                $scope.blockedassociations[association] = true;
            });

            $scope.$on('sw_unblock_association', function (event, association) {
                $scope.blockedassociations[association] = false;
            });

            $scope.$on('sw_associationsupdated', function (event, associationoptions) {
                //this in scenarios where a section is compiled before the association has returned from the server... angular seems to get lost in the bindings
                $scope.associationOptions = associationoptions;
            });

            //this will get called when the input form is done rendering
            $scope.$on('sw_bodyrenderedevent', function (ngRepeatFinishedEvent, parentElementId) {

                // Configure input files
                $('#uploadBtn').on('change', function (e) {
                    var fileName = this.value.match(/[^\/\\]+$/);
                    var validFileTypes = contextService.fetchFromContext('allowedfiles', true);
                    var extensionIdx = this.value.lastIndexOf(".");
                    var extension = this.value.substring(extensionIdx +1).toLowerCase();
                    if($.inArray(extension, validFileTypes) == -1) {
                        $('#uploadFile').attr("value", "");
                        if (isIe9()) {
                            //hacky around ie9 -- HAP-894
                            $(this).replaceWith($(this).clone(true));
                        } else {
                            $('#uploadFile').val('');
                        }
                       
                        return;
                    }
                    $('#uploadFile').attr("value", fileName);
                });

                //                var tab = contextService.getActiveTab();
                //                if (tab != null) {
                //                    redirectService.redirectToTab(tab);
                //                }

                var bodyElement = $('#' + parentElementId);
                if (bodyElement.length <= 0) {
                    return;
                }
                // Configure tooltips
                $('[rel=tooltip]', bodyElement).tooltip({ container: 'body' });

                $scope.configureLookupModals(bodyElement);
                cmpfacade.init(bodyElement, $scope);
                // workaround in order to make the <select> comboboxes work properly on ie9

                angular.forEach($("select"), function (currSelect) {
                    if (currSelect.selectedIndex >= 0) {
                        currSelect.options[currSelect.selectedIndex].text += " ";
                    }

                    $(currSelect).change(function () {
                        if (this.selectedIndex >= 0) {
                            this.options[this.selectedIndex].text += " ";
                        }
                    });
                });

                if (parentElementId.equalsAny('crudInputMainCompositionFields', 'crudInputMainFields')) {
                    //to avoid registering these global listeners multiple times, as the page main contain sections.
                    $scope.configureNumericInput();
                    $scope.configureOptionFields();
                    $scope.configureAssociationChangeEvents();
                    if ($scope.schema.properties["oncrudloadevent.detail"]) {
                        var serviceSt = $scope.schema.properties["oncrudloadevent.detail"];
                        var fn =dispatcherService.loadService(serviceSt.split(".")[0], serviceSt.split(".")[1]);
                        fn($scope.datamap, $scope.schema);
                    }
                    
                }

                $('.datetimereadonly').datepicker("remove");


            });

            /* Association (COMBO, AUTOCOMPLETECLIENT) functions */
            $scope.configureAssociationChangeEvents = function () {
                var associations = fieldService.getDisplayablesOfTypes($scope.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
                if (associations == null) {
                    return;
                }
                //reversing to preserve focus order. see https://controltechnologysolutions.atlassian.net/browse/HAP-674
                associations = associations.reverse();
                $.each(associations, function (key, association) {
                    //                    var shouldDoWatch = true;

                    var isMultiValued = association.multiValued;

                    $scope.$watch('datamap["' + association.attribute + '"]', function (newValue, oldValue) {

                        var oldValueIgnoreWatch = oldValue == null ? false : oldValue.indexOf('$ignorewatch') != -1;
                        var newValueIgnoreWatchIdx = newValue == null ? -1 : newValue.indexOf('$ignorewatch');

                        if ((oldValue == newValue || oldValueIgnoreWatch) && !(oldValueIgnoreWatch && newValueIgnoreWatchIdx != -1)) {
                            //if both are ignoring, there´s something wrong, I´ve seen this only on ie9, let´s fix the newvalue removing the $ignorewatch instead of simply returning
                            return;
                        }
                        if (newValue != null) {
                            //this is a hacky thing when we want to change a value of a field without triggering the watch
                            if (newValueIgnoreWatchIdx != -1) {
                                //                                shouldDoWatch = false;
                                newValue = newValue.substring(0, newValueIgnoreWatchIdx);
                                if (newValue == '$null') {
                                    newValue = null;
                                }
                                $scope.datamap[association.attribute] = newValue;
                                cmpfacade.digestAndrefresh(association, $scope);

                                $timeout(function () {
                                    //                                    shouldDoWatch = true;
                                }, 0, false);

                                /*
                                try {
                                    $scope.$digest();
                                    shouldDoWatch = true;
                                } catch (e) {
                                    //nothing to do, just checking if digest was already in place or not
                                    $timeout(function () {
                                        shouldDoWatch = true;
                                    }, 0, false);
                                }*/
                                return;
                            }
                        }

                        var eventToDispatch = {
                            oldValue: oldValue,
                            newValue: newValue,
                            fields: $scope.datamap,
                            displayables: $scope.displayables,
                            scope: $scope,
                            'continue': function () {
                                if (isMultiValued && association.rendererType != 'lookup') {
                                    associationService.updateUnderlyingAssociationObject(association, null, $scope);
                                }
                                var result = associationService.updateAssociations(association, $scope);
                                if (result != undefined && result == false) {
                                    associationService.postAssociationHook(association, $scope, { phase: 'configured', dispatchedbytheuser: true });
                                }
                                try {
                                    $scope.$digest();
                                } catch (ex) {
                                    //nothing to do, just checking if digest was already in place or not
                                }

                            },
                            interrupt: function () {
                                $scope.datamap[association.attribute] = oldValue;
                                //to avoid infinite recursion here.
                                shouldDoWatch = false;
                                cmpfacade.digestAndrefresh(association, $scope);
                                //turn it on for future changes
                                shouldDoWatch = true;
                            }
                        };
                        if (newValueIgnoreWatchIdx == -1) {
                            //let´s avoid this call if we´ve set the $ignorewatch value, otherwise this may lead to unwanted events to be called, for instance when refreshing the data
                            associationService.onAssociationChange(association, isMultiValued, eventToDispatch);
                            cmpfacade.digestAndrefresh(association, $scope);
                        }

                    });

                    $scope.$watchCollection('associationOptions.' + association.associationKey, function (newvalue, old) {
                        $timeout(
                            function () {
                                cmpfacade.digestAndrefresh(association, $scope);
                            }, 0, false);
                    });


                    $scope.$watch('blockedassociations.' + association.associationKey, function (newValue, oldValue) {
                        cmpfacade.blockOrUnblockAssociations($scope, newValue, oldValue, association);
                    });
                });
            }

            $scope.isSelectEnabled = function (fieldMetadata) {
                var key = fieldMetadata.associationKey;
                $scope.enabledassociations = instantiateIfUndefined($scope.enabledassociations);
                if (!$scope.blockunblockparameters) {
                    $scope.blockunblockparameters = [];
                }
                if (key == undefined) {
                    return true;
                }
                //either we don´t have blocked associations at all (server didnt responded yet, or this specific one is marked as false)
                var notblocked = ($scope.blockedassociations == null || !$scope.blockedassociations[key]);
                if (fieldMetadata.enableExpression == "true") {
                    return notblocked;
                }

                var result = notblocked && expressionService.evaluate(fieldMetadata.enableExpression, $scope.datamap);
                //result ==> its not blocked and there´s no expression marking it to be disabled
                var currentAssociationEnabledState = $scope.enabledassociations[key];
                if (currentAssociationEnabledState == undefined) {
                    $scope.enabledassociations[key] = result;
                } else if (result != currentAssociationEnabledState) {
                    //here we´re toggling the status of it, passing the currentstate --> the values refer to block state, so we need to inverse them
                    //we need to call an extra refresh, in the reverse order as of the listeners registered on the screen so that the focus are kept in the correct order
                    $scope.blockunblockparameters.unshift({ result: !result, currentState: !currentAssociationEnabledState, fieldMetadata: fieldMetadata });
                    $scope.enabledassociations[key] = result;
                }
                if ($scope.blockunblockparameters.length == 1) {
                    //register the timeout only once
                    $timeout(function () {
                        for (var i = 0; i < $scope.blockunblockparameters.length; i++) {
                            //this array was built in the reverse order,using unshift, as if the screen was rendered bottom up
                            var param = $scope.blockunblockparameters[i];
                            cmpfacade.blockOrUnblockAssociations($scope, param.result, param.currentState, param.fieldMetadata);
                            cmpfacade.digestAndrefresh(param.fieldMetadata, $scope);
                        }
                        $scope.blockunblockparameters = [];
                    }, 0, false);
                }

                return result;
            };

            $scope.getCalendarTooltip = function (redererParameters) {
                //HAP-1052
                this.i18N('calendar.date_tooltip', 'Open the calendar popup')
            }

            $scope.haslookupModal = function (schema) {
                return fieldService.getDisplayablesOfRendererTypes(schema.displayables, ['lookup']).length > 0;
            }

            $scope.isModifiableEnabled = function (fieldMetadata) {
                var result = expressionService.evaluate(fieldMetadata.enableExpression, $scope.datamap);
                return result;
            };


            /* CHECKBOX functions */

            $scope.isCheckboxSelected = function (option, datamapKey) {
                var model = $scope.datamap[datamapKey];

                if (model == undefined) {
                    return false;
                }
                return model.indexOf(option.value) > -1;
            };

            $scope.toogleCheckboxSelection = function (option, datamapKey) {
                var model = $scope.datamap[datamapKey];
                if (model == undefined) {
                    model = [];
                }
                if (datamapKey == "selectallHLAG") {
                    // This is wrong!!!!
                    $('option', '.multiselect').each(function (element) {
                        $('.multiselect').multiselect('select', 'ADL');
                    });
                }
                var idx = model.indexOf(option.value);
                if (idx > -1) {
                    model.splice(idx, 1);
                } else {
                    model.push(option.value);
                }
                $scope.datamap[datamapKey] = model;
            };


            /* LOOKUP functions */

            $scope.lookupAssociationsCode = {};
            $scope.lookupAssociationsDescription = {};

            $scope.showLookupModal = function (fieldMetadata) {
                if (!$scope.isSelectEnabled(fieldMetadata)) {
                    return;
                }

                $scope.lookupModalSearch = {};
                $scope.lookupModalSearch.descripton = '';
                $scope.lookupModalSearch.fieldMetadata = fieldMetadata;

                var targetValue = $scope.datamap[fieldMetadata.target];
                if (targetValue == null || targetValue == " ") {
                    $scope.lookupModalSearch.code = $scope.lookupAssociationsCode[fieldMetadata.attribute];
                } else {
                    $scope.lookupModalSearch.code = '';
                }
                var modals = $('[data-class="lookupModal"]', $element);
                modals.draggable();
                modals.modal('show');
            };

            $scope.lookupCodeChange = function (fieldMetadata) {
                if ($scope.datamap[fieldMetadata.target] != null) {
                    $scope.datamap[fieldMetadata.target] = " "; // If the lookup value is changed to a null value, set a white space, so it can be updated on maximo WS.
                    $scope.lookupAssociationsDescription[fieldMetadata.attribute] = null;
                    associationService.updateUnderlyingAssociationObject(fieldMetadata, null, $scope);
                }
            };

            $scope.lookupCodeBlur = function (fieldMetadata) {
                var code = $scope.lookupAssociationsCode[fieldMetadata.attribute];
                var targetValue = $scope.datamap[fieldMetadata.target];
                if (code != null && code != '' && (targetValue == null || targetValue == " ")) {
                    $scope.showLookupModal(fieldMetadata);
                }
            };

            $scope.configureLookupModals = function (bodyElement) {
                // Configure lookup modals
                var lookups = fieldService.getDisplayablesOfRendererTypes($scope.schema.displayables, ['lookup']);
                $.each(lookups, function (key, value) {
                    var fieldMetadata = value;
                    if ($scope.associationOptions == null) {
                        //this scenario happens when a composition has lookup-associations on its details, 
                        //but the option list has not been fetched yet
                        $scope.lookupAssociationsDescription[fieldMetadata.attribute] = null;
                        $scope.lookupAssociationsCode[fieldMetadata.attribute] = null;
                    } else {
                        var options = $scope.associationOptions[fieldMetadata.associationKey];

                        var doConfigure = function (optionValue) {

                            $scope.lookupAssociationsCode[fieldMetadata.attribute] = optionValue;
                            if (options == null || options.length <= 0) {
                                //it should always be lazy loaded... why is this code even needed?
                                return;
                            }

                            var optionSearch = $.grep(options, function (e) {
                                return e.value == optionValue;
                            });

                            var valueToSet = optionSearch != null && optionSearch.length > 0 ? optionSearch[0].label : null;
                            $scope.lookupAssociationsDescription[fieldMetadata.attribute] = valueToSet;
                        }

                        doConfigure($scope.datamap[fieldMetadata.target]);
                    }
                });
            };

            $scope.bindEvalExpression = function (fieldMetadata) {
                if (fieldMetadata.evalExpression == null) {
                    return;
                }
                var variables = expressionService.getVariablesForWatch(fieldMetadata.evalExpression);
                $scope.$watchCollection(variables, function (newVal, oldVal) {
                    if (newVal != oldVal) {
                        $scope.datamap[fieldMetadata.attribute] = expressionService.evaluate(fieldMetadata.evalExpression, $scope.datamap);
                    }
                });
            }

            $scope.configureNumericInput = function () {
                for (i in $scope.schema.displayables) {
                    var fieldMetadata = $scope.schema.displayables[i];
                    if (fieldMetadata.rendererType != 'numericinput') {
                        continue;
                    }
                    if ($scope.datamap != null) {
                        var currentValue = $scope.datamap[fieldMetadata.attribute];
                        if (currentValue == null) /* without default value */ {
                            $scope.datamap[fieldMetadata.attribute] = 1;
                        } else if (typeof currentValue == "string") {
                            $scope.datamap[fieldMetadata.attribute] = parseInt(currentValue);
                        }
                    }
                }
            };



            $scope.configureOptionFields = function () {
                //TODO: check field parameter as well, with top priority before schema
                if ($scope.schema.properties["optionfield.donotusefirstoptionasdefault"] == "true") {
                    return;
                }
                if ($scope.displayables != null && $scope.datamap != null) {
                    var optionsFields = fieldService.getDisplayablesOfTypes($scope.displayables, ['OptionField']);
                    for (var i = 0; i < optionsFields.length; i++) {
                        var optionfield = optionsFields[i];
                        if ("false" == optionfield.rendererParameters["setdefaultvalue"]) {
                            continue;
                        }

                        if ($scope.datamap[optionfield.target] == null && optionfield.providerAttribute == null && optionfield.rendererType != 'checkbox') {
                            var values = $scope.GetOptionFieldOptions(optionfield);
                            if (values != null) {
                                $scope.datamap[optionfield.target] = values[0].value;
                            }
                        }
                    }
                }
            };

            $scope.getSelectedTexts = function (fieldMetadata) {
                return cmpComboDropdown.getSelectedTexts(fieldMetadata);
            };

            $scope.opendetails = function (fieldMetadata) {
                if ($scope.enabletoopendetails(fieldMetadata)) {
                    var parameters = { id: $scope.paramstopendetails.idtopendetails, popupmode: 'browser' };
                    redirectService.goToApplicationView($scope.paramstopendetails.application, 'detail', 'output', null, parameters);
                }
            };

            $scope.fillparamstoopendetails = function (fieldMetadata) {
                $scope.paramstopendetails = null;
                if (!nullOrUndef(fieldMetadata.rendererParameters)) {
                    var idtopendetails = fieldMetadata.rendererParameters['idtopendetails'];
                    if (!nullOrUndef(fieldMetadata.applicationTo)) {
                        var application = fieldMetadata.applicationTo.replace('_', '');
                        if (isNumber(application.charAt(0))) {
                            application = application.substring(1);
                        }

                        var id = $scope.datamap[idtopendetails];
                        if (!nullOrUndef(id) && !nullOrUndef(application)) {
                            $scope.paramstopendetails = { idtopendetails: id, application: application };
                        }
                    }
                }
            };

            $scope.enabletoopendetails = function (fieldMetadata) {
                $scope.fillparamstoopendetails(fieldMetadata);
                return $scope.paramstopendetails == null ? false : true;
            };

            $scope.getLengthParam = function (fieldMetadata) {
                var lengthclass = null;
                if (!nullOrUndef(fieldMetadata.rendererParameters)) {
                    var length = fieldMetadata.rendererParameters['length'];
                    if (!nullOrUndef(length)) {
                        switch (length) {
                            case 'full':
                                lengthclass = GetBootstrapFormClass(10);
                                break;
                            case 'half':
                                lengthclass = GetBootstrapFormClass(5);
                                break;
                            case 'quarter':
                                lengthclass = GetBootstrapFormClass(2);
                                break;
                        }
                    }
                }
                return lengthclass;
            };

            $scope.GetAssociationOptions = function (fieldMetadata) {
                if (fieldMetadata.type == "OptionField") {
                    return $scope.GetOptionFieldOptions(fieldMetadata);
                }
                return $scope.associationOptions[fieldMetadata.associationKey];
            }

            $scope.GetOptionFieldOptions = function (optionField) {
                if (optionField.providerAttribute == null) {
                    return optionField.options;
                }
                $scope.associationOptions = instantiateIfUndefined($scope.associationOptions);
                return $scope.associationOptions[optionField.providerAttribute];
            }


            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.isIE = function () {
                //TODO: is this needed for all ieversions or only 9 and, in this case replace function for aa_utils
                return isIe9();
            };

            $scope.isFieldRequired = function (fieldMetadata) {
                return fieldService.isFieldRequired(fieldMetadata, $scope.datamap);
            };

            $scope.getLabelStyle = function (fieldMetadata) {
                var rendererColor = styleService.getLabelStyle(fieldMetadata, 'color');
                var weight = styleService.getLabelStyle(fieldMetadata, 'font-weight');
                var result = {
                    'color': rendererColor,
                    'font-weight': weight
                }
                return result;
            }

            //SM - 08/30 - Start, fix label width
            $scope.getLabelClass = function (fieldMetadata) {
                //return $scope.hasSameLineLabel(fieldMetadata) ? 'col-md-2' : 'col-md-12';
                if (fieldMetadata.resourcepath != undefined && fieldMetadata.header == null) {
                    return null;
                }

                return $scope.hasSameLineLabel(fieldMetadata) ? GetBootstrapFormClass(2) : GetBootstrapFormClass(12);
            }

            $scope.getFieldClass = function (fieldMetadata) {
                if (fieldMetadata.resourcepath != undefined && fieldMetadata.header == null) {
                    return GetBootstrapFormClass(12);;
                }
                return $scope.hasSameLineLabel(fieldMetadata) ? GetBootstrapFormClass(10) : GetBootstrapFormClass(12);
                //                return $scope.hasSameLineLabel(fieldMetadata) ? 'col-md-8' : 'col-md-12';
            }
            //SM - 08/30 - End, fix label width

            ///
            // legendevaluation is boolean indicating the mode we are calling this method, either for an ordinary field or for a header with legend
            ////
            $scope.isLabelVisible = function (fieldMetadata, legendEvaluationMode) {
                if (!$scope.isVerticalOrientation()) {
                    return false;
                }
                var header = fieldMetadata.header;
                if (!header) {
                    return !legendEvaluationMode;
                }
                var isVisible = expressionService.evaluate(header.showExpression, $scope.datamap);
                var isFieldSet = header.parameters != null && "true" == header.parameters['fieldset'];
                //if header is declared as fieldset return true only for the legendEvaluation
                return isVisible && (isFieldSet == legendEvaluationMode);
            }

            $scope.isFieldSet = function (fieldMetadata) {
                if (!$scope.isVerticalOrientation()) {
                    return false;
                }
                var header = fieldMetadata.header;
                if (!header) {
                    return false;
                }
                var isVisible = expressionService.evaluate(header.showExpression, $scope.datamap);
                var isFieldSet = header.parameters != null && "true" == header.parameters['fieldset'];
                //if header is declared as fieldset return true only for the legendEvaluation
                return isVisible && (isFieldSet);
            }


            $scope.isSectionWithoutLabel = function (fieldMetadata) {
                return fieldMetadata.type == 'ApplicationSection' && fieldMetadata.resourcepath == null && fieldMetadata.header == null;
            };



            $scope.sectionHasSameLineLabel = function (fieldMetadata) {
                return $scope.hasSameLineLabel(fieldMetadata) && fieldMetadata.type == 'ApplicationSection' && fieldMetadata.resourcepath == null;
            };

            $scope.getValueColumnClass = function (fieldMetadata) {
                var classes = '';
                if ($scope.sectionHasSameLineLabel(fieldMetadata)) {
                    classes += 'col-sectionsamelineheader ';
                }

                if (fieldMetadata.resourcepath != undefined && fieldMetadata.header == null) {
                    return GetBootstrapFormClass(12);
                }
                if ($scope.isSectionWithoutLabel(fieldMetadata)) {
                    //                    classes += 'col-md-12 ';
                } else if (!$scope.hasSameLineLabel(fieldMetadata)) {
                    classes += GetBootstrapFormClass(12);
                } else if ($scope.isVerticalOrientation()) {
                    //SM - 08/30 - Start, fix label width
                    var lengthparam = $scope.getLengthParam(fieldMetadata);
                    if (lengthparam != null) {
                        classes += lengthparam;
                    } else {
                        classes += GetBootstrapFormClass(10);
                    }
                    //                    classes += 'col-md-8 ';
                    //SM - 08/30 - End, fix label width
                }
                return classes;
            };

            $scope.formatId = function (id) {
                return RemoveSpecialChars(id);
            }

            $scope.nonTabFields = function (displayables) {
                return fieldService.nonTabFields(displayables);
            };

            function init() {

                $injector.invoke(BaseController, this, {
                    $scope: $scope,
                    i18NService: i18NService,
                    fieldService: fieldService
                });

                if (!$scope.isVerticalOrientation()) {
                    var countVisibleDisplayables = fieldService.countVisibleDisplayables($scope.datamap, $scope.schema, $scope.displayables);
                    if (countVisibleDisplayables > 0) {
                        $scope.horizontalWidth = {
                            width: (100 / countVisibleDisplayables) + "%"
                        };
                    }
                }
            }
            init();
        }



    }
});

//No longer used
//app.directive('selectCombo', function () {
//    return {
//        restrict: 'A',
//        link: function (scope, element, attr) {
//            $(element).on('click', 'input', function (e) {
//                console.log('click');

//                $(element).find('[data-dropdown="dropdown"]').click();
//                //return false;
//                //e.stopPropagation();
//            });
//        }
//    };
//});;
var app = angular.module('sw_layout');

app.directive('advancedFilterToogle', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/advanced_filter_toogle.html')
    };
});


app.directive('crudListWrapper', function (contextService, $compile) {
    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            schema: '=',
            datamap: '=',
            previousschema: '=',
            previousdata: '=',
            paginationData: '=',
            searchData: '=',
            searchOperator: '=',
            searchSort: '=',
            ismodal: '@',
            checked: '=',
            isList: "=",
            timestamp: '=',
        },
        link: function (scope, element, attrs) {
            if (scope.isList) {
                element.append(
                    "<crud-list datamap='datamap' schema='schema' pagination-data='paginationData' " +
                    "search-data='searchData' " +
                    "search-operator='searchOperator' " +
                    "search-sort='searchSort'" +
                    "timestamp='{{timestamp}}' />"
                );
                $compile(element.contents())(scope);
            }
        }
    }


});


app.directive('crudList', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_list.html'),
        scope: {
            schema: '=',
            datamap: '=',
            previousschema: '=',
            previousdata: '=',
            paginationData: '=',
            searchData: '=',
            searchOperator: '=',
            searchSort: '=',
            ismodal: '@',
            checked: '=',
            timestamp: '@',
        },

        controller: function ($scope, $http, $rootScope, $filter, $injector, $log, $timeout,
            formatService, fixHeaderService,
            searchService, tabsService,
            fieldService, commandService, i18NService,
            validationService, submitService, redirectService,
            associationService, contextService, schemaCacheService) {

            $scope.$name = 'crudlist';

            fixHeaderService.activateResizeHandler();

            $scope.getFormattedValue = function (value, column) {
                var formattedValue = formatService.format(value, column);
                if (formattedValue == "-666") {
                    //this magic number should never be displayed! 
                    //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                    return "";
                }
                return formattedValue == null ? "" : formattedValue;
            };

            $scope.hasTabs = function (schema) {
                return tabsService.hasTabs(schema);
            };
            $scope.isCommand = function (schema) {
                if ($scope.schema && $scope.schema.properties['command.select'] == "true") {
                    return true;
                }
            };

            $scope.getOpenCalendarTooltip = function (attribute) {
                var currentData = $scope.searchData[attribute];
                if (currentData) {
                    return currentData;
                }
                return this.i18N('calendar.date_tooltip', 'Open the calendar popup');
            }

            $scope.isNotHapagTest = function () {
                return $rootScope.clientName != 'hapag';
            }
            $scope.tabsDisplayables = function (schema) {
                return tabsService.tabsDisplayables(schema);
            };

            $scope.shouldShowSort = function(column,orientation) {
                return column.attribute != null && ($scope.searchSort.field == column.attribute || $scope.searchSort.field == column.rendererParameters['sortattribute']) && $scope.searchSort.order == orientation;
            };

            $scope.getGridHeaderStyle = function (column, propertyName) {
                if (!isIe9()) {
                    return null;
                }

                var property = column.rendererParameters[propertyName];

                if (property != null) {
                    return property;
                }

                if (propertyName == 'maxwidth') {
                    var high = $(window).width() > 1199;
                    if (high) {
                        return '135px';
                    }
                    return '100px';
                }
                return null;
            }


            $scope.$on('filterRowRenderedEvent', function (filterRowRenderedEvent) {
                if ($scope.datamap.length == 0) {
                    // only update filter visibility if there are no results to shown on grid... else the filter visibility will be updated on "listTableRenderedEvent"
                    fixHeaderService.updateFilterZeroOrOneEntries();
                    // update table heigth (for ie9)
                }
            });

            $scope.$on('listTableRenderedEvent', function (listTableRenderedEvent) {
                var log = $log.getInstance('sw4.crud_list_dir#on#listTableRenderedEvent');
                log.debug('init table rendered listener');
                if ($scope.ismodal == 'true' && !(true === $scope.$parent.showingModal)) {
                    return;
                }

                var params = {};

                if (!$rootScope.printRequested) {

                    fixHeaderService.fixThead($scope.schema, params);
                    if ($rootScope.showSuccessMessage) {
                        fixHeaderService.fixSuccessMessageTop(true);
                    }
                    // fix status column height
                    $('.statuscolumncolor').each(function (key, value) {
                        $(value).height($(value).parent().parent().height());
                    });

                    $('[rel=tooltip]').tooltip({ container: 'body' });
                    log.debug('finish table rendered listener');

                    //make sure we are seeing the top of the grid 
                    window.scrollTo(0, 0);

                    //add class to last visible th in the filter row
                    $('#listgrid .filter-row').find('th').filter(':visible:last').addClass('last');
                }
            });

            $scope.$on('sw_successmessagetimeout', function (event, data) {
                if (!$rootScope.showSuccessMessage) {
                    fixHeaderService.resetTableConfig($scope.schema);
                }
            });

            $scope.$on('sw_errormessage', function (event, show) {
                fixHeaderService.topErrorMessageHandler(show, $scope.$parent.isDetail, $scope.schema);
            });

            $scope.isEditing = function (schema) {
                var idFieldName = schema.idFieldName;
                var id = $scope.datamap.fields[idFieldName];
                return id != null;
            };


            $scope.shouldShowField = function (expression) {
                if (expression == "true") {
                    return true;
                }
                var stringExpression = '$scope.datamap.' + expression;
                var ret = eval(stringExpression);
                return ret;
            };


            $scope.isHapag = function () {
                return $rootScope.clientName == "hapag";
            };


            $scope.showDetail = function (rowdm, column) {

                var mode = $scope.schema.properties['list.click.mode'];
                var popupmode = $scope.schema.properties['list.click.popupmode'];
                var schemaid = $scope.schema.properties['list.click.schema'];
                var fullServiceName = $scope.schema.properties['list.click.service'];

                if (column.rendererType == "checkbox") {
                    return;
                }

                if (popupmode == "report") {
                    return;
                }

                if (fullServiceName != null) {
                    commandService.executeClickCustomCommand(fullServiceName, rowdm.fields, column);
                    return;
                };

                var id = rowdm.fields[$scope.schema.idFieldName];
                if (id == null || id == "-666") {
                    window.alert('error id is null');
                    return;
                }

                var applicationname = $scope.schema.applicationName;
                if (schemaid == '') {
                    return;
                }
                if (schemaid == null) {
                    schemaid = detailSchema();
                }
                contextService.insertIntoContext("currentmodulenewwindow", contextService.retrieveFromContext('currentmodule'));
                $scope.$emit("sw_renderview", applicationname, schemaid, mode, $scope.title, { id: id, popupmode: popupmode });
            };

            $scope.renderListView = function (parameters) {
                $scope.$parent.multipleSchema = false;
                $scope.$parent.schemas = null;
                var listSchema = 'list';
                if ($scope.schema != null && $scope.schema.stereotype.isEqual('list', true)) {
                    //if we have a list schema already declared, keep it
                    listSchema = $scope.schema.schemaId;
                }
                $scope.$emit("sw_renderview", $scope.schema.applicationName, listSchema, 'none', $scope.title, parameters);
            };

            $scope.selectPage = function (pageNumber, pageSize, printMode) {
                if (pageNumber === undefined || pageNumber <= 0 || pageNumber > $scope.paginationData.pageCount) {
                    $scope.paginationData.pageNumber = pageNumber;
                    return;
                }
                var totalCount = 0;
                var filterFixedWhereClause = null;
                var unionFilterFixedWhereClause = null;
                if ($scope.paginationData != null) {
                    totalCount = $scope.paginationData.totalCount;
                    //if pageSize is specified, use it... this is used for printing function
                    //TODO: refactor calls to always pass pageSize
                    if (pageSize == undefined) {
                        pageSize = $scope.paginationData.pageSize;
                    }
                    filterFixedWhereClause = $scope.paginationData.filterFixedWhereClause;
                    unionFilterFixedWhereClause = $scope.paginationData.unionFilterFixedWhereClause;
                }
                if (pageSize === undefined) {
                    //if it remains undefined, use 100
                    pageSize = 100;
                }

                var searchDTO;

                //TODO Improve this solution
                var reportDto = contextService.retrieveReportSearchDTO($scope.schema.schemaId);
                if (reportDto != null) {
                    reportDto = $.parseJSON(reportDto);
                    searchDTO = searchService.buildReportSearchDTO(reportDto, $scope.searchData, $scope.searchSort, $scope.searchOperator, filterFixedWhereClause);
                } else {
                    searchDTO = searchService.buildSearchDTO($scope.searchData, $scope.searchSort, $scope.searchOperator, filterFixedWhereClause, unionFilterFixedWhereClause);
                }

                searchDTO.pageNumber = pageNumber;
                searchDTO.totalCount = totalCount;
                searchDTO.pageSize = pageSize;
                searchDTO.paginationOptions = $scope.paginationData.paginationOptions;

                //avoids table flickering
                fixHeaderService.unfix();

                $scope.renderListView({ SearchDTO: searchDTO, printMode: printMode });
            };

            $scope.getSearchIcon = function (columnName) {
                var showSearchIcon = $scope.schema.properties["list.advancedfilter.showsearchicon"] == "true";
                var operator = $scope.getOperator(columnName);

                if (showSearchIcon && operator.symbol != "") {
                    return operator.symbol;
                } else {
                    return "";
                }
            }

            $scope.searchOperations = function () {
                return searchService.searchOperations();
            }

            $scope.getDefaultOperator = function () {
                return searchService.defaultSearchOperation();
            };

            $scope.selectOperator = function (columnName, operator) {
                var searchOperator = $scope.searchOperator;
                var searchData = $scope.searchData;

                searchOperator[columnName] = operator;

                if (operator.id == "") {
                    searchData[columnName] = '';
                    $scope.selectPage(1);
                } else if (searchData[columnName] != null && searchData[columnName] != '') {
                    $scope.selectPage(1);
                } else if (operator.id == "BLANK") {
                    searchData[columnName] = '';
                    $scope.selectPage(1);
                }
            };

            $scope.getOperator = function (columnName) {
                var searchOperator = $scope.searchOperator;
                if (searchOperator != null && searchOperator[columnName] != null) {
                    return searchOperator[columnName];
                }
                return searchService.getSearchOperation(0);
            };

            $scope.filterSearch = function (columnName, event) {
                var currentOperator = $scope.searchOperator[columnName];
                // has no operator selected or has a noop operator selected
                if (!currentOperator || !currentOperator.id) {
                    $scope.searchOperator[columnName] = searchService.defaultSearchOperation();
                }

                var searchString = $scope.searchData[columnName];
                
                if (!!searchString) {
                    $scope.selectPage(1);
                } else {
                    $scope.searchOperator[columnName] = searchService.noFilter();
                    $scope.selectPage(1);
                }

                // workaround to remove the focus from the filter textbox
                // on ie9, if we dont took the focus out of the textbox, the page breaks something on the rendering 
                // that prevents the click on the grid to show the details
                $("#listgrid").focus();
                window.scrollTo(0, 0);
            };

            $scope.shouldShowFilter = function (column) {
                return column.type === "ApplicationFieldDefinition" && column.rendererType !== "color";
            }



            $scope.sort = function (column) {
                if (!$scope.shouldShowFilter(column)) {
                    return;
                }
                var columnName = column.attribute;
                if (column.rendererParameters && column.rendererParameters.sortattribute) {
                    columnName = column.rendererParameters.sortattribute;
                }
                var sorting = $scope.searchSort;
                if (sorting.field != null && sorting.field == columnName) {
                    sorting.order = sorting.order == 'desc' ? 'asc' : 'desc';
                } else {
                    sorting.field = columnName;
                    sorting.order = 'asc';
                }
                $scope.selectPage(1);
            };

            $scope.collapse = function (selector) {
                if ($(selector).is(':visible')) {
                    $(selector).hide();
                } else {
                    $(selector).show();
                }
                fixHeaderService.fixTableTop($(".fixedtable"));
            };


            $injector.invoke(BaseController, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService
            });

        }
    };
});

;
app.directive('crudOutputWrapper', function (contextService, $compile, $rootScope) {
    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            cancelfn: '&',
            previousschema: '=',
            previousdata: '=',
            hasError: '=',
            tabid: '@',
            isMainTab:"@"
        },
        link: function (scope, element, attrs) {

            var doLoad = function () {
                element.append(
                    "<crud-output schema='schema'" +
                    "datamap='datamap'" +
                    "displayables='displayables'" +
                    "orientation='{{orientation}}'></crud-output-fields>"
                );
                $compile(element.contents())(scope);
                scope.loaded = true;
            }

            if (scope.schema.mode == "output" && ("true" == scope.isMainTab)) {
                doLoad();
            }

            scope.$on("sw_lazyloadtab", function(event,tabid) {
                if (scope.tabid == tabid && !scope.loaded) {
                    doLoad();
                }
            });

        }
    }
});


app.directive('crudOutput', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_output.html'),
        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            cancelfn: '&',
            previousschema: '=',
            previousdata: '=',
            hasError: '='
        },

        controller: function ($scope,$injector, formatService, printService, tabsService, fieldService, commandService, redirectService, i18NService) {
            $scope.$name = 'crudoutput';

            $scope.cancel = function (previousdata,previousschema) {
                $('#crudmodal').modal('hide');
                if (GetPopUpMode() == 'browser') {
                    close();
                }
                $scope.cancelfn({ data: previousdata, schema: previousschema });
                $scope.$emit('sw_cancelclicked');
            };

            //TODO:RemoveThisGambi
            $scope.redirectToHapagHome = function () {
                redirectService.redirectToAction(null, 'HapagHome', null, null);
            };

            $scope.redirectToAction = function (title, controller, action, parameters) {
                redirectService.redirectToAction(title, controller, action, parameters);
            };

            $scope.printDetail = function () {
                printService.printDetail($scope.schema, $scope.datamap[$scope.schema.idFieldName]);
            };


            $scope.nonTabFields = function (displayables) {
                return fieldService.nonTabFields(displayables);
            };

            $injector.invoke(BaseController, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService
            });

        }

    };
});;
app.directive('sectionElementOutput', function ($compile) {
    return {
        restrict: "E",
        replace: true,
        scope: {
            schema: '=',
            datamap: '=',
            displayables: '=',
            orientation: '@'
        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            if (angular.isArray(scope.displayables)) {
                element.append(
                    "<crud-output-fields schema='schema'" +
                                    "datamap='datamap'" +
                                    "displayables='displayables'" +
                                    "orientation='{{orientation}}'></crud-output-fields>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});

app.directive('crudOutputFields', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_output_fields.html'),
        scope: {
            schema: '=',
            datamap: '=',
            displayables: '=',
            orientation: '@'
        },

        controller: function ($scope, $injector, formatService, printService, tabsService, fieldService, commandService, redirectService, i18NService, expressionService) {
            $scope.$name = 'crud_output_fields';

            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.i18NLabel = $scope.i18NLabel || function (fieldMetadata) {
                var label = i18NService.getI18nLabel(fieldMetadata, $scope.schema);
                if (label != undefined && label != "") {
                    label = label.replace(':', '');
                }
                return label;
            };



            $scope.getFormattedValue = function (value, column) {
                return formatService.format(value, column);
            };


            $scope.getChildrenExpanded = function (attribute) {
                var root = datamap[attribute];
                var result = [];
                if (!root.children) {
                    return result;
                }
                for (var i = 0; i < root.children.length; i++) {

                }
            },




            $scope.getSectionStyle = function (fieldMetadata) {
                var style = {};

                if (fieldMetadata.parameters != null) {
                    for (i in fieldMetadata.parameters) {
                        style[i] = fieldMetadata.parameters[i];
                    }
                }

                if (fieldMetadata.rendererParameters != null) {
                    for (i in fieldMetadata.rendererParameters) {
                        style[i] = fieldMetadata.rendererParameters[i];
                    }
                }

                if (style.width == null && !$scope.isVerticalOrientation() && $scope.countVisibleDisplayables > 0) {
                    style.width = (100 / $scope.countVisibleDisplayables) + '%';
                }

                return style;
            };

     


            $scope.getFieldClass = function (fieldMetadata) {
                if ($scope.hasSameLineLabel(fieldMetadata)) {
                    return 'col-xs-9';
                }

                if (fieldMetadata.rendererType== "TABLE") {
                    //workaround because compositions are appending "" as default label values, but we dont want it!
                    return null;
                }
                return 'col-xs-12';
            };




            $scope.bindEvalExpression = function (fieldMetadata) {
                if (fieldMetadata.evalExpression == null) {
                    return;
                }
                var variables = expressionService.getVariablesForWatch(fieldMetadata.evalExpression);
                $scope.$watchCollection(variables, function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        $scope.datamap[fieldMetadata.attribute] = expressionService.evaluate(fieldMetadata.evalExpression, $scope.datamap);
                    }
                });
            }

            $scope.getHeaderStyle = function (fieldMetadata) {
                var style = {};

                if (fieldMetadata.header != null && fieldMetadata.header.parameters != null) {
                    for (i in fieldMetadata.header.parameters) {
                        style[i] = fieldMetadata.header.parameters[i];
                    }
                }

                return style;
            };

            $scope.hasLabelOrHeader = function (fieldMetadata) {
                return fieldMetadata.header || fieldMetadata.label;
            }


            function init() {
                $scope.countVisibleDisplayables = fieldService.countVisibleDisplayables($scope.datamap, $scope.schema, $scope.displayables);
                $injector.invoke(BaseController, this, {
                    $scope: $scope,
                    i18NService: i18NService,
                    fieldService: fieldService
                });
            }

            init();

            $scope.selectNodeLabel = function () {
                //workaround to avoid treeview node to be selected
            };
        }

    };
});;
var app = angular.module('sw_layout');

function griditemclick(rowNumber, columnNumber, element) {
    //this is a trick to call a angular scope function from an ordinary onclick listener (same used by batarang...)
    //with this, we can generate the table without compiling it to angular, making it faster
    //first tests pointed a 100ms gain, but need to gather more data.
    var scope = angular.element(element).scope();
    if (!scope.showDetail) {
        //workaround for HAP-1006, if there are checkboxes present, then the crudlist scope is the parent
        scope = scope.$parent;
    }

    if (scope.showDetail) {
        scope.showDetail(scope.datamap[rowNumber], scope.schema.displayables[columnNumber]);
    }
}

function buildStyle(minWidth, maxWidth, width,isdiv) {
    if (minWidth == undefined && maxWidth == undefined && width == undefined) {
        return "";
    }
    var style = "style=\"";
    if (minWidth != undefined) {
        style += 'min-width:' + minWidth + ";";
    }
    if (isdiv && maxWidth != undefined) {
        //we cannot set max-widths for the tds
        style += 'max-width:' + maxWidth + ";";
    }
    if (width != undefined) {
        style += 'width:' + width + ";";
    }
    return style + " \"";
};

app.directive('crudtbody', function (contextService, $compile, $parse, formatService, i18NService, fieldService, commandService, $injector, $timeout, $log) {
    return {
        restrict: 'A',
        replace: false,
        scope: {
            datamap: '=',
            schema: '=',
        },
        template: "",
        link: function (scope, element, attrs) {

            scope.getFormattedValue = function (value, column) {
                var formattedValue = formatService.format(value, column);
                if (formattedValue == "-666") {
                    //this magic number should never be displayed! 
                    //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                    return "";
                }
                return formattedValue == null ? "" : formattedValue;
            };

            scope.getGridColumnStyle = function (column, propertyName, highResolution) {
                if (column.rendererParameters != null) {
                    //sections for instance dont have it
                    var property = column.rendererParameters[propertyName];

                    if (property != null) {
                        return property;
                    }
                }

                //HAP-716 - SM - don't force maxwidth here, let th/td control width of column
                //if (propertyName == 'maxwidth') {
                //    if (highResolution) {
                //        return '135px';
                //    }
                //    return '100px';
                //}
                return null;
            }

            scope.statusColor = function (status, gridname) {
                /* in case of change grid colors might be different */
                if (gridname != null && gridname == "change") {
                    if (status.equalsAny("INPRG"))
                        return "blue";
                }

                /* otherwise use general coloring */
                if (status.equalsAny("NEW", "WAPPR", "WSCH")) {
                    return "orange";
                }
                if (status.equalsAny("QUEUED", "INPROG", "INPRG", "PENDING", "null")) {
                    return "yellow";
                }

                if (status.equalsAny("CANCELLED", "FAIL", "FAILED", "CAN", "FAILPIR", "REJECTED", "NOTREQ")) {
                    return "red";
                }

                if (status.equalsAny("RESOLVED", "SLAHOLD", "SCHED", "APPR", "AUTHORIZED", "AUTH", "HOLDINPRG", "PLANNED", "ACC_CAT", "ASSESSES", "PENDAPPR", "RCACOMP", "WMATL", "INFOPEND", "ASSESS")) {
                    return "blue";
                }
                if (status.equalsAny("CLOSED", "RESOLVCONF", "IMPL", "IMPLEMENTED", "REVIEW", "CLOSE", "HISTEDIT", "COMP")) {
                    return "green";
                }
                if (status.equalsAny("DRAFT")) {
                    return "white";
                }
                return "transparent";
            }

            scope.refreshGrid = function (datamap, schema) {
                scope.datamap = datamap;
                scope.schema = schema;
                var t0 = new Date().getTime();;
                var columnarray = scope.columnarray = [];
                var hiddencolumnArray = [];
                for (var j = 0; j < schema.displayables.length; j++) {
                    var column = schema.displayables[j];
                    columnarray.push(column);
                    hiddencolumnArray.push(scope.isFieldHidden(schema, column));
                }
                var hasCheckBox = false;

                var html = '';

                var highResolution = $(window).width() > 1199;

                for (var i = 0; i < datamap.length; i++) {
                    var rowst = "datamap[{0}]".format(i);
                    html += "<tr style='cursor: pointer' listtablerendered rel='hideRow'>";
                    var dm = datamap[i];
                    for (j = 0; j < schema.displayables.length; j++) {
                        var columnst = "columnarray[{0}]".format(j);
                        column = schema.displayables[j];
                        var formattedText = scope.getFormattedValue(datamap[i].fields[column.attribute], column);

                        if (!column.rendererParameters) {
                            column.rendererParameters = {};
                        }

                        var minwidthDiv = scope.getGridColumnStyle(column, 'minwidth', highResolution);
                        var maxwidthDiv = scope.getGridColumnStyle(column, 'maxwidth', highResolution);
                        var widthDiv = scope.getGridColumnStyle(column, 'width', highResolution);
                        
                        var minWidth = column.rendererParameters['minwidth'];
                        var maxWidth = column.rendererParameters['maxwidth'];
                        var width = column.rendererParameters['width'];
                        var isHidden = hiddencolumnArray[j];
                        html += "<td {2} onclick='griditemclick({0},{1},this)'".format(i, j, isHidden ? 'style="display:none"' : '');
                        if (!isHidden) {
                            html += buildStyle(minWidth, maxWidth, width,false);
                        } 
                        html += ">";
                        if (column.rendererType == 'color') {
                            var color = scope.statusColor(dm.fields[column.rendererParameters['column']] || 'null', schema.applicationName);
                            html += "<div class='statuscolumncolor' style='background-color:{0}'>".format(color);
                        } else if (column.rendererType == 'checkbox') {
                            var name = column.attribute;
                            html += "<div>";
                            html += "<input type='checkbox' class='check' name='{0}' ".format(name);
                            html += "ng-model=\"{0}.fields['checked']\"".format(rowst);
                            html += "ng-init=\"{0}.fields['checked']=false\" >".format(rowst);
                            html += "</div>";
                            hasCheckBox = true;
                        }else if (column.type == 'ApplicationFieldDefinition') {
                            html += "<div class='gridcolumnvalue'".format(columnst);
                            if (!isHidden) {
                                html += buildStyle(minwidthDiv, maxwidthDiv, widthDiv, true);
                            }
                            html += ">";
                            html += formattedText;
                        }
                     
                        html += "</div>";
                        html += "</td>";
                    }
                    html += "</tr>";
                }
                element.html(html);
                if (hasCheckBox) {
                    $compile(element.contents())(scope);
                }
                var t1 = new Date().getTime();
                $log.getInstance('crudtbody#link').debug('grid compilation took {0} ms'.format(t1 - t0));
                $timeout(function (key, value) {
                    scope.$emit('listTableRenderedEvent');
                    if (!hasCheckBox) {
                        scope.$$watchers = null;
                    }
                });
            }

          

            scope.$on('sw_griddatachanged', function (event, datamap, schema) {
                scope.refreshGrid(datamap, schema);
            });




            $injector.invoke(BaseController, this, {
                $scope: scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService
            });

            //first call when the directive is linked (listener was not yet in place)
            scope.refreshGrid(scope.datamap, scope.schema);

        }
    }


});


;

app.directive('dashboardsdone', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    $('[rel=tooltip]').tooltip({ container: 'body' });
                });
            }
        }
    };
});

function DashboardController($scope, $http, $templateCache, $rootScope, formatService, i18NService, contextService, schemaService,fixHeaderService) {

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.i18NLabel = function (fieldMetadata) {
        return i18NService.getI18nLabel(fieldMetadata, $scope.schema);
    };


    $scope.renderDashboard = function (index, fetchData) {

        $scope.schema = null;
        $scope.datamap = null;
        $scope.totalCount = null;
        $scope.currentDashboardIndex = index;


        if (index < 0 && index >= $scope.dashboards.length) {
            $scope.$emit('sw_titlechanged', $scope.i18N('general.error', 'Error'));
            return;
        }

        var currentDashboard = $scope.dashboards[index];
        var parameters = {};
        parameters.key = {};
        parameters.key.schemaId = currentDashboard.schemaId;
        parameters.key.mode = currentDashboard.mode;
        parameters.key.platform = platform();
        parameters.SearchDTO = {};
        parameters.SearchDTO.pageSize = currentDashboard.pageSize;
        parameters.SearchDTO.searchParams = currentDashboard.searchParams;
        parameters.SearchDTO.searchValues = currentDashboard.searchValues;
        parameters.SearchDTO.Context = {};
        parameters.SearchDTO.Context.MetadataId = currentDashboard.id;
        parameters.currentmetadata= currentDashboard.id;


        $http.get(url("/api/data/" + currentDashboard.applicationName + "?" + $.param(parameters)))
            .success(function (result) {
                $scope.schema = result.schema;
                $scope.datamap = result.resultObject;
                $scope.totalCount = result.totalCount;
                $scope.pageSize = result.pageSize;
                $scope.loaded = true;
                $scope.dashboards[index].totalCount = result.totalCount;
            });

    };

    $scope.format = function (value, column) {
        return formatService.format(value, column);
    };

    $scope.viewAll = function (index) {
        var dashboard;
        if (index < 0 && index >= $scope.dashboards.length) {
            $scope.$emit('sw_titlechanged', $scope.i18N('general.error', 'Error'));
            return;
        } else {
            dashboard = $scope.dashboards[index];
        }

        var searchDTO = {
            searchParams: dashboard.searchParams,
            searchValues: dashboard.searchValues,
            pageSize:100
        };
        //this is used for keeping the same whereclause on the viewAll grid
        contextService.insertIntoContext('currentmetadata', dashboard.id);
        var schemaObj = schemaService.parseAppAndSchema(dashboard.viewAllSchema);
        var applicationToUse = schemaObj.app == null ? dashboard.applicationName : schemaObj.app;
        var schemaId = schemaObj.schemaId;
        $scope.goToApplicationView(applicationToUse, schemaId, dashboard.mode, dashboard.title, { SearchDTO: searchDTO });
    };



    $scope.showDetail = function (datamap) {


        var dashboard = $scope.dashboards[$scope.currentDashboardIndex];
        var schemaObj = schemaService.parseAppAndSchema(dashboard.detailSchema);
        var applicationToUse = schemaObj.app == null ? dashboard.applicationName : schemaObj.app;
        var id = dashboard.idFieldName != "" ? datamap[dashboard.idFieldName] : datamap[$scope.schema.idFieldName];
        contextService.insertIntoContext("currentmodulenewwindow", contextService.retrieveFromContext('currentmodule'));
        var parameters = {
            id: id,
            popupmode: $scope.schema.properties['list.click.popupmode'],
            key: {
                schemaId: schemaObj.schemaId,
                mode: $scope.schema.properties['list.click.mode'],
                platform: "web"
            }
        };

        if (parameters.popupmode == 'browser') {
            $scope.$parent.goToApplicationView(applicationToUse, parameters.key.schemaId, parameters.key.mode, $scope.schema.title, parameters);
            return;
        }

        $scope.applicationname = applicationToUse;

        $http.get(url("/api/data/" + applicationToUse + "?" + $.param(parameters)))
            .success(function renderData(result) {

                $('#crudmodal').modal('show');
                $("#crudmodal").draggable();

                $scope.modal = {};
                $scope.modal.schema = result.schema;
                $scope.modal.datamap = result.resultObject;

                if (result.schema.title != null && $scope.modal.title == null) {
                    $scope.modal.title = result.schema.title;
                }

                $scope.modal.associationOptions = result.associationOptions;
                $scope.modal.associationSchemas = result.associationSchemas;
                $scope.modal.compositions = result.Compositions;
                $scope.modal.isDetail = true;
                $scope.modal.isList = false;
            });

    };

    function initDashboard() {
        //        $scope.$emit('sw_titlechanged', "Home");
        $scope.dashboards = $scope.resultData;

        $scope.renderDashboard(0,false);
        $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
            if (oldValue != newValue && $scope.resultObject.crudSubTemplate != null && $scope.resultObject.crudSubTemplate.indexOf("HapagHome.html") != -1) {
                $scope.dashboards = $scope.resultData;
                $scope.renderDashboard(0,false);
            }
        });

    }


    initDashboard();



};
(function (angular, $) {
    // "use strict";
    // [IMPORTANT!] Do not go back to strict mode, it causes a production error:
    // 'Uncaught SyntaxError: In strict mode code, functions can only be declared at top level or immediately within another function' 
    // for the `watchForStartDate` function

    var app = angular.module("sw_layout");

    app.directive("dateTime", ["$timeout", "formatService", "expressionService", "$q", function ($timeout, formatService, expressionService, $q) {

        function parseBooleanValue(attrValue) {
            return !attrValue ? true : attrValue.toLowerCase() === "true";
        }

        function datetimeclassHandler(timeOnly) {
            var datetime = $(".datetime-class").last();
            var calendar = "glyphicon glyphicon-calendar";
            var time = "glyphicon glyphicon-time";
            datetime.removeClass(calendar);
            datetime.removeClass(time);
            var classToAdd = timeOnly ? time : calendar;
            datetime.addClass(classToAdd);
        }

        // 01/jan/1970 00:00:00
        var defaultStartDate = new Date(1970, 0, 1, 0, 0, 0);

        return {
            restrict: "A",
            require: "?ngModel",
            link: function (scope, element, attrs, ngModel) {
                // no model, not required to do anything fancy
                if (!ngModel) {
                    return;
                }
                // block usage of ng-enter: use date-enter instead
                if (!!attrs.ngEnter) {
                    throw new Error("ng-enter directive is not supported. Use date-enter instead.");
                }

                var showTime = parseBooleanValue(attrs.showTime);
                var showIgnoreTime = attrs.showIgnoretime === "true";
                var originalAttribute = attrs.originalAttribute;
                var showDate = parseBooleanValue(attrs.showDate);
                var dateFormat = formatService.adjustDateFormatForPicker(attrs.dateFormat, showTime);
                var originalDateFormat = attrs.dateFormat;
                if (!attrs.language) {
                    attrs.language = userLanguage || "en-US";
                }
                var showMeridian = attrs.showAmpm == undefined ? undefined : attrs.showAmpm.toLowerCase() === "true";
                var istimeOnly = showTime && !showDate;
                var isReadOnly = attrs.readonly == undefined ? false : (attrs.readonly);
                var datamap = scope.datamap;
                datetimeclassHandler(istimeOnly);

                $timeout(function () {
                    if (!!scope.fieldMetadata) {
                        var value = formatService.formatDate(ngModel.$modelValue, attrs.dateFormat);
                        ngModel.$setViewValue(value);
                        element.val(value);
                        if (!!originalAttribute) {
                            //this is useful on sections, like samelinepickers.html
                            datamap[originalAttribute] = value;
                        } else if (!!datamap && !!scope.fieldMetadata) {
                            datamap[scope.fieldMetadata.attribute] = value;
                        }
                        ngModel.$render();
                    }
                });

                if (!!dateFormat) {
                    var allowfuture = parseBooleanValue(attrs.allowFuture);
                    var allowpast = parseBooleanValue(attrs.allowPast);
                    var startDate = allowpast ? defaultStartDate : "+0d";
                    var endDate = allowfuture ? Infinity : "+0d";
                    var minStartDateExpression = attrs.minDateexpression;

                    // watch on variables that need to cause a change in the picker's 'startDate' property
                    function watchForStartDate(picker) {
                        var localStartDate = expressionService.evaluate(minStartDateExpression, datamap);
                        localStartDate = formatService.formatDate(localStartDate, originalDateFormat);
                        var variablesToWatch = expressionService.getVariablesForWatch(minStartDateExpression);
                        scope.$watchCollection(variablesToWatch, function (newVal, oldVal) {
                            if (newVal !== oldVal) {
                                localStartDate = expressionService.evaluate(minStartDateExpression, datamap);
                                localStartDate = formatService.formatDate(localStartDate, originalDateFormat);
                                var datePicker = element.data(picker);
                                startDate = Date.parse(localStartDate);
                                datePicker.startDate = startDate;
                                datePicker.initialDate = datePicker.startDate;
                            }
                        });
                        return Date.parse(localStartDate);
                    }

                    /**
                     * Checks if a given date is before the startDate.
                     * 
                     * @param Date date 
                     * @returns Boolean 
                     */
                    function isInvalidValidDate(date) {
                        return angular.isDate(startDate) && date.getTime() < startDate.getTime();
                    }

                    /**
                     * Sets the ngModelController's $viewValue and $render's it.
                     *
                     * @param {} value
                     * @returns Promise
                     */
                    function renderValue(value) {
                        var deferred = $q.defer();
                        scope.$apply(function () {
                            ngModel.$setViewValue(value);
                            ngModel.$render();
                            deferred.resolve(value);
                        });
                        return deferred.promise;
                    }

                    /**
                     * Intercepts the date the user set and checks if it's an invalid date. 
                     * In this case renders formattted starDate.
                     * 
                     * @param Bootstrap.DateTimePicker.Event<changeDate> event 
                     */
                    function changeDateHandler(event) {
                        var date = event.date;
                        if (!date) return;
                        if (isInvalidValidDate(date)) renderValue(formatService.formatDate(startDate, originalDateFormat));
                    };

                    /**
                     * Validates the date (both the time and format) and returns a value that can be rendered:
                     * - if format is wrong and has stardate render formatted startdate
                     * - if format is wrong and has no startdate render empty string
                     * - if format is right but date is invalid render formatted start date
                     * 
                     * @param String value 
                     * @returns String 
                     */
                    function getRenderableDateValue(value) {
                        try {
                            var dateFromUser = $.fn.datetimepicker.DPGlobal.parseDate(value, $.fn.datepicker.DPGlobal.parseFormat(dateFormat), attrs.language);
                            // attempt a regular parse just in case:
                            // this may be necessary when the user causes the parse of an already formatted date (e.g. multiple clicking enter)
                            if (isInvalidValidDate(dateFromUser)) {
                                dateFromUser = Date.parse(value);
                            }
                            var dateToUse = isInvalidValidDate(dateFromUser) ? startDate : dateFromUser;
                            return formatService.formatDate(dateToUse, originalDateFormat);
                        } catch (e) {
                            return angular.isDate(startDate) ? formatService.formatDate(startDate, originalDateFormat) : "";
                        }
                    }

                    /**
                     * Handles the blur event on a datepicker field.
                     * It's useful for when the user manually writes the date (doesn't trigger DateTimePicker events nor JQuery.Event<change>).
                     * Renders the value obtained from getRenderableDateValue(<input_value>).
                     * 
                     * @param JQuery.Event<Blur> event 
                     */
                    function blurHandler(event) {
                        var inputValue = $(this).val();
                        if (!inputValue) return;
                        var dateToRender = getRenderableDateValue(inputValue);
                        renderValue(dateToRender);
                    }

                    /**
                     * Handles the pressing of the 'enter' key on datepicker input.
                     * Renders the value obtained from getRenderableDateValue(<input_value>) then executes any ng-enter expression.
                     * Cancels the event.
                     * 
                     * @param JQuery.Event<keypress> event 
                     */
                    function enterHandler(event) {
                        if (event.which !== 13) return;
                        var value = $(this).val();
                        if (!value) return;
                        // render value
                        var date = getRenderableDateValue(value);
                        var promise = renderValue(date);
                        // schedule to execute date-enter callback after rendering the value and cancelling the 'enter' event 
                        if (!!attrs.dateEnter) {
                            promise.then(function () {
                                scope.$eval(attrs.dateEnter);
                            });
                        }
                        // cancelling event
                        event.preventDefault();
                        event.stopPropagation();
                        event.stopImmediatePropagation();
                    }

                    var initialDate;
                    if (showTime) {
                        // var futureOnly = (attrs.futureOnly != undefined && attrs.futureOnly.toLowerCase() === "true");
                        // attrs.startDate = futureOnly ? '+0d' : -Infinity;

                        //if the date format starts with dd --> we don´t have the AM/PM thing, which is just an american thing where the dates starts with months
                        if (!showMeridian) {
                            showMeridian = dateFormat.startsWith("MM");
                        }

                        if (!!attrs.minDateexpression) startDate = watchForStartDate("datetimepicker");
                        initialDate = startDate === defaultStartDate ? null : startDate;

                        element.datetimepicker({
                            format: dateFormat,
                            autoclose: true,
                            language: attrs.language,
                            todayBtn: false,
                            showMeridian: showMeridian,
                            startDate: startDate,
                            initialDate: initialDate,
                            formatViewType: "time",
                            startView: istimeOnly ? 1 : 2,
                            maxView: istimeOnly ? 1 : 3,
                            readonly: isReadOnly,
                            ignoretimeBtn: showIgnoreTime,
                            forceParse: !showIgnoreTime
                        }).on("blur", blurHandler).on("changeDate", changeDateHandler).keypress(enterHandler);

                    } else {

                        if (!!attrs.minDateexpression) startDate = watchForStartDate("datepicker");
                        initialDate = startDate === defaultStartDate ? null : startDate;

                        element.datepicker({
                            initialDate: initialDate,
                            startDate: startDate,
                            endDate: endDate,
                            format: dateFormat,
                            autoclose: true,
                            language: attrs.language,
                            maxView: 3,
                            showMeridian: false,
                            readonly: isReadOnly
                        }).on("blur", blurHandler).on("changeDate", changeDateHandler).keypress(enterHandler);
                    }
                }
            }
        };
    }]);

})(angular, jQuery);;
function DownloadController($scope, i18NService, fieldService, alertService, formatService) {


    $scope.getFormattedValue = function (value, format) {
        var column = {};
        if (format) {
            column.rendererType = 'datetime';
            column.rendererParameters = {
                format: format
            };
        }

        return formatService.format(value, column);
    };

    $scope.download = function (controller, action, id, mode) {
        var controllerToUse = controller == undefined ? "Attachment" : controller;
        var actionToUse = controller == undefined ? "Download" : action;
        
        var parameters = {};
        parameters.id = id;
        parameters.mode = mode == undefined ? "http" : mode;
        parameters.parentId = fieldService.getId(this.parentdata.fields, this.parentschema);
        parameters.parentApplication = this.parentschema.applicationName;
        parameters.parentSchemaId = this.parentschema.schemaId;
        
        var rawUrl = url("/" + controllerToUse + "/" + actionToUse + "?" + $.param(parameters));

        $.fileDownload(rawUrl, {

            failCallback: function (html, url) {
                alertService.alert(String.format(i18NService.get18nValue('download.error', 'Error downloading file with id {0}. Please, Contact your Administrator'), id));
            }
        });
    };

};
/**
 * ##ux.each##
 * Like angular.forEach except that you can pass additional arguments to it that will be available
 * in the iteration function. It is optimized to use while loops where possible instead of for loops for speed.
 * Like Lo-Dash.
 * @param {Array\Object} list
 * @param {Function} method
 * @param {*=} data _additional arguments passes are available in the iteration function_
 * @returns {*}
 */
//_example:_
//
//      function myMethod(item, index, list, arg1, arg2, arg3) {
//          console.log(arg1, arg2, arg3);
//      }
//      ux.each(myList, myMethod, arg1, arg2, arg3);
function each(list, method, data) {
    var i = 0, len, result, extraArgs;
    if (arguments.length > 2) {
        extraArgs = exports.util.array.toArray(arguments);
        extraArgs.splice(0, 2);
    }
    if (list && list.length) {
        len = list.length;
        while (i < len) {
            result = method.apply(null, [list[i], i, list].concat(extraArgs));
            if (result !== undefined) {
                return result;
            }
            i += 1;
        }
    } else if (list && Object.prototype.hasOwnProperty.apply(list, ['0'])) {
        while (Object.prototype.hasOwnProperty.apply(list, [i])) {
            result = method.apply(null, [list[i], i, list].concat(extraArgs));
            if (result !== undefined) {
                return result;
            }
            i += 1;
        }
    } else if (!(list instanceof Array)) {
        for (i in list) {
            if (Object.prototype.hasOwnProperty.apply(list, [i])) {
                result = method.apply(null, [list[i], i, list].concat(extraArgs));
                if (result !== undefined) {
                    return result;
                }
            }
        }
    }
    return list;
};
function ErrorController($scope, i18NService) {

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

};;
function FaqController($scope, i18NService, redirectService, $timeout) {
    var isOpen = 0;
    $scope.showDefinitions = function (data) {
        if (data != null && data.solutionId != null) {
            redirectService.goToApplicationView("solution", "detail", "output", "FAQ", { id: data.solutionId, faqid: data.faqId, lang: data.lang, popupmode: 'browser' });
        }
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.doInit = function () {
        var data = $scope.resultData;
        var lang = data.lang;

        $scope.faqLanguageFilter = lang == null ? 'en' : lang;
        $scope.faqTextFilter = data.search;

        var dataObject = JSON.parse(data.faqModelResponseJson);
        if (dataObject != null) {
            $scope.isFaqOk = true;
            $scope.roleList = dataObject;
            $scope.isSearchOk = !jQuery.isEmptyObject($scope.roleList);
        }
    };

    $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
        if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("Faq.html") != -1) {
            $scope.doInit();
        }
    });

    $scope.filterFaq = function () {
        if ($scope.faqTextFilter == null) {
            $scope.faqTextFilter = "";
        }
        var parameters = {};
        parameters.lang = $scope.faqLanguageFilter;
        parameters.search = $scope.faqTextFilter;
        $scope.$parent.doAction(null, "FaqApi", "Index", parameters);
    };

    $scope.doInit();
};;
function HomeController($scope, $http, $templateCache, $rootScope, $timeout,$log, $compile, contextService, menuService, i18NService, alertService) {

    $scope.$name = 'HomeController';

    function initController() {
        var log =$log.getInstance('home.js#initController');

        var redirectUrl = url(homeModel.Url);
        i18NService.load(homeModel.I18NJsons, userLanguage);


        var sessionRedirectURL = sessionStorage.swGlobalRedirectURL;
        if (sessionRedirectURL != null && ((redirectUrl.indexOf("popupmode=browser") == -1) && (redirectUrl.indexOf("MakeSWAdmin") == -1))) {
            redirectUrl = sessionRedirectURL;
        }
        var currentModule = contextService.currentModule();
        if (currentModule == undefined || currentModule == "null") {
            //sessionstorage is needed in order to avoid F5 losing currentmodule
            log.info('switching to home module {0}'.format(homeModel.InitialModule));
            contextService.insertIntoContext("currentmodule", homeModel.InitialModule, false);
        }
        $http({
            method: "GET",
            url: redirectUrl,
            cache: $templateCache
        })
        .success(function (result) {
            $scope.$parent.includeURL = contextService.getResourceUrl(result.redirectURL);
            $scope.$parent.resultData = result.resultObject;
            $scope.$parent.resultObject = result;
            //            if (nullOrUndef($rootScope.currentmodule) && !nullOrUndef(currentModule) && currentModule != "") {
            //                $rootScope.currentmodule = currentModule;
            //            }

            $scope.$emit('sw_indexPageLoaded', redirectUrl);
            $scope.$emit('sw_titlechanged', result.title);

            if (homeModel.Message != undefined) {
                /*if (homeModel.MessageType == 'error') {
                    var error = { errorMessage: homeModel.Message }

                    $timeout(function () {                        
                        $rootScope.$broadcast('sw_ajaxerror', error);
                    }, 1000);
                    
                } else {*/
                alertService.success(homeModel.Message, false);
                //}
                homeModel.Message = null;
            }
        });
    }



    $scope.onTemplateLoad = function (event) {
        $timeout(function () {
            $scope.$emit('ngLoadFinished');
            var windowTitle = homeModel.WindowTitle;
            if (!nullOrUndef(windowTitle)) {
                window.document.title = windowTitle;
            }
            var content=angular.element('#headerline');
            $compile(content.contents())($scope);
        });
    };

    initController();
};
var app = angular.module('sw_layout');


app.directive('inlineCompositionListWrapper', function ($compile) {

    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            parentdata: '=',
            metadata: '=',
            iscollection : '='
        },

        link: function (scope, element, attrs) {

            var doLoad = function () {
                scope.compositionschemadefinition = scope.metadata.schema;
                scope.compositiondata = scope.parentdata[scope.metadata.relationship];
                element.append(
                    "<inline-composition-list parentdata='parentdata'" +
                             "metadata='metadata' iscollection='iscollection' compositionschemadefinition='compositionschemadefinition' compositiondata='compositiondata'/>"
                );
                $compile(element.contents())(scope);
                scope.loaded = true;
            }


            doLoad();

            scope.$on("sw_lazyloadtab", function (event, tabid) {
                if (scope.tabid == tabid && !scope.loaded) {
                    doLoad();
                }
            });

        }
    };
});


app.directive('inlineCompositionList', function (contextService, commandService) {

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/inline_composition_list.html'),
        scope: {
            parentdata: '=',
            metadata: '=',
            iscollection: '=',
            compositionschemadefinition: '=',
            compositiondata:'=',
        },

        controller: function ($scope, $filter, $http, $element, $rootScope, tabsService) {

            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.handleItemClick = function (item, schema) {
                if (!nullOrUndef(schema.rendererParameters) && !nullOrEmpty(schema.rendererParameters.onClickService)) {
                    commandService.executeClickCustomCommand(schema.rendererParameters.onClickService, item, schema);
                }
            }

        }
    };
});;
function LogAdminController($scope, $http, i18NService, redirectService) {

    $scope.filter = function (data) {
        var logname = data.logname;
        $scope.logs = $.grep($scope.initiallogs, function (element) {
            return element.name.toLowerCase().indexOf(logname.toLocaleLowerCase()) != -1;
        });
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.changeLevel = function (logName, level) {
        var parameters = {
            logName: logName,
            newLevel: level,
            pattern: ''
        };
        var urlToInvoke = redirectService.getActionUrl('LogAdmin', 'ChangeLevel', parameters);
        $http.post(urlToInvoke).success(function (data) {
            init(data.resultObject);
        });
    };

    $scope.changeAll = function (level) {
        var parameters = {
            pattern: $scope.logname,
            newLevel: level,
            logName: ''
        };
        var urlToInvoke = redirectService.getActionUrl('LogAdmin', 'ChangeLevel', parameters);
        $http.post(urlToInvoke).success(function (data) {
            init(data.resultObject);
        });
    };

    $scope.viewAppenderContent = function (selectedappender) {
        $scope.selectedappender = selectedappender;
        var parameters = {
            value: selectedappender.value
        };
        var urlToInvoke = redirectService.getActionUrl('LogAdmin', 'GetAppenderTxtContent', parameters);
        $http.get(urlToInvoke).
        success(function (data, status, headers, config) {
            $scope.appendercontent = data.resultObject;
        }).
        error(function (data, status, headers, config) {
            $scope.appendercontent = "Error " + status;
        });
    };

    $scope.downloadFile = function (selectedappender) {
        var parameters = {};
        parameters.fileName = selectedappender.name + ".txt";
        parameters.contentType = 'text/plain';
        parameters.path = selectedappender.value;
        parameters.setFileNameWithDate = true;
        window.location = removeEncoding(url("/Application/DownloadFile" + "?" + $.param(parameters)));
    };

    function init(data) {
        var logs = data.logs;
        var appenders = data.appenders;
        $scope.logs = logs;
        $scope.appenders = appenders;
        $scope.initiallogs = logs;
        $scope.logname = "";
        $scope.chooselog = 'changeviewlog';
        $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
            if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("LogAdmin.html") != -1) {
                init($scope.resultData);
            }
        });
    };

    $scope.setDefaultAppender = function () {
        if (nullOrUndef($scope.selectedappender)) {
            for (var i = 0; i < $scope.appenders.length; i++) {
                if ($scope.appenders[i].name == 'MaximoAppender') {
                    $scope.selectedappender = $scope.appenders[i];
                    $scope.viewAppenderContent($scope.selectedappender);
                    break;
                }
            }
        }
    };

    init($scope.resultData);
};
var app = angular.module('sw_layout');

app.directive('lookupModalWrapper', function ($compile) {
    return {
        restrict: "E",
        replace: true,
        scope: {
            lookupAssociationsCode: '=',
            lookupAssociationsDescription: '=',
            lookupObj: '=',
            schema: '=',
            datamap: '='
        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
            element.append(
            "<lookup-modal lookup-obj='lookupObj'" +
                "lookup-associations-code='lookupAssociationsCode'" +
                "lookup-associations-description='lookupAssociationsDescription'" +
                "schema='schema' datamap='datamap'>" +
            "</lookup-modal>"
            );
            $compile(element.contents())(scope);
        },

    }
}),

app.directive('lookupModal', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/lookupModal.html'),
        scope: {
            lookupAssociationsCode: '=',
            lookupAssociationsDescription: '=',
            lookupObj: '=',
            schema: '=',
            datamap: '='
        },

        controller: function ($scope, $http, $element, searchService, i18NService, associationService) {

            $scope.lookupModalSearch = function (pageNumber) {
                var schema = $scope.schema;
                var fields = $scope.datamap;
                var lookupObj = $scope.lookupObj;

                var parameters = {};
                parameters.application = schema.applicationName;
                parameters.key = {};
                parameters.key.schemaId = schema.schemaId;
                parameters.key.mode = schema.mode;
                parameters.key.platform = platform();
                parameters.associationFieldName = lookupObj.fieldMetadata.associationKey;

                var lookupApplication = lookupObj.fieldMetadata.schema.rendererParameters["application"];
                var lookupSchemaId = lookupObj.fieldMetadata.schema.rendererParameters["schemaId"];
                if (lookupApplication != null && lookupSchemaId != null) {
                    parameters.associationApplication = lookupApplication;
                    parameters.associationKey = {};
                    parameters.associationKey.schemaId = lookupSchemaId;
                    parameters.associationKey.platform = platform();
                }

                var totalCount = 0;
                var pageSize = 30;
                if ($scope.modalPaginationData != null) {
                    totalCount = $scope.modalPaginationData.totalCount;
                    pageSize = $scope.modalPaginationData.pageSize;
                }
                if (pageNumber === undefined) {
                    pageNumber = 1;
                }

                if (lookupObj.schema != null) {
                    var defaultLookupSearchOperator = searchService.getSearchOperationById("CONTAINS");
                    var searchValues = $scope.searchObj;
                    var searchOperators = {}
                    for (var field in searchValues) {
                        searchOperators[field] = defaultLookupSearchOperator;
                    }
                    parameters.SearchDTO = searchService.buildSearchDTO(searchValues, {}, searchOperators);
                    parameters.SearchDTO.pageNumber = pageNumber;
                    parameters.SearchDTO.totalCount = totalCount;
                    parameters.SearchDTO.pageSize = pageSize;
                } else {
                    parameters.valueSearchString = lookupObj.code;
                    parameters.labelSearchString = lookupObj.description;
                    parameters.SearchDTO = {
                        pageNumber: pageNumber,
                        totalCount: totalCount,
                        pageSize: pageSize
                    };
                }

                var urlToUse = url("/api/generic/Data/UpdateAssociation?" + $.param(parameters));
                var jsonString = angular.toJson(fields);
                $http.post(urlToUse, jsonString).success(function (data) {
                    var result = data.resultObject;
                    for (association in result) {
                        if (lookupObj.fieldMetadata.associationKey == association) {

                            lookupObj.options = result[association].associationData;
                            lookupObj.schema = result[association].associationSchemaDefinition;

                            $scope.modalPaginationData = {};
                            $scope.modalPaginationData.pageCount = result[association].pageCount;
                            $scope.modalPaginationData.pageNumber = result[association].pageNumber;
                            $scope.modalPaginationData.pageSize = result[association].pageSize;
                            $scope.modalPaginationData.totalCount = result[association].totalCount;
                            $scope.modalPaginationData.selectedPage = result[association].pageNumber;
                        }
                    }
                }).error(function data() {
                });
            };

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.i18NLabel = function (fieldMetadata) {
                return i18NService.getI18nLabel(fieldMetadata, $scope.lookupObj.schema);
            };

            $scope.lookupModalSelect = function (option) {

                var fieldMetadata = $scope.lookupObj.fieldMetadata;

                $scope.datamap[fieldMetadata.target] = option.value;
                $scope.lookupAssociationsCode[fieldMetadata.attribute] = option.value;
                $scope.lookupAssociationsDescription[fieldMetadata.attribute] = option.label;

                associationService.updateUnderlyingAssociationObject(fieldMetadata, option, $scope);


                $element.modal('hide');
            };

            $element.on('hide.bs.modal', function (e) {

                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();

                if ($scope.lookupObj == null) {
                    return;
                }

                var fieldMetadata = $scope.lookupObj.fieldMetadata;
                if ($scope.datamap != null && ($scope.datamap[fieldMetadata.target] == null || $scope.datamap[fieldMetadata.target] == " ")) {
                    $scope.$apply(function () {
                        $scope.lookupAssociationsCode[fieldMetadata.attribute] = null;
                        $scope.lookupAssociationsDescription[fieldMetadata.attribute] = null;
                    });
                }

            });

            $element.on('shown.bs.modal', function (e) {
                $scope.searchObj = {};
                if ($scope.lookupObj != undefined) {
                    $scope.lookupModalSearch();
                }
            });
        }
    };
});;
function MakeSWAdminController($scope, $http, $timeout, redirectService) {

    $scope.submit = function () {
        var parameters = {
            password: $scope.password
        };
        var urlToInvoke = redirectService.getActionUrl('MakeSWAdmin', 'Submit', parameters);
        $http.get(urlToInvoke).
        success(function (data, status, headers, config) {
            if (data.resultObject == true) {
                $scope.msg = "";
                $scope.msgsuccess = "Successfully Authorized";
                $timeout(function () {
                    location.reload();
                }, 1500);
            } else {
                $scope.msg = "Unauthorized";
            }
        }).
        error(function (data, status, headers, config) {
            $scope.msg = "Error";
        });
    };

    function init() {
        $scope.password = "";
        $scope.msg = "";
        $scope.msgsuccess = "";
        $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
            if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("MakeSWAdmin.html") != -1) {
                init($scope.resultData);
            }
        });
    };

    init();
};
var app = angular.module('sw_layout');

app.directive('messagesection', function (contextService,$timeout,logoutService) {
    return {
        restrict: 'E',
        templateUrl: contextService.getResourceUrl('Content/Templates/message_section.html'),
        controller: function ($scope, i18NService, $rootScope, fixHeaderService) {

            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.$on('sw_checksuccessmessage', function (event, data) {
                if (!nullOrUndef(data.schema)) {
                    var schema = data.schema;
                    if (schema == 'list' && $rootScope.showSuccessMessage && $scope.successMsg != null) {
                        $scope.hasSuccessList = true;
                        $scope.hasSuccessDetail = false;
                    }
                }
            });

            $scope.$on('sw_successmessage', function (event, data) {
                if (!nullOrUndef(data.successMessage) && allowSuccessMessageDisplay(data)) {
                    if (nullOrUndef(data.schema)) {
                        $scope.hasSuccessDetail = true;
                    } else {
                        if (data.schema.stereotype == "List") {
                            $scope.hasSuccessList = true;
                        } else {
                            $scope.hasSuccessDetail = true;
                        }
                    }
                    $scope.successMsg = data.successMessage;
                    $rootScope.showSuccessMessage = true;

                } else {
                    hideSuccessMessage();
                }
            });

            function allowSuccessMessageDisplay(data) {
                var allow = true;
                if (!nullOrUndef(data.schema)) {
                    if (data.schema.schemaId.indexOf("Summary") > -1) {
                        allow = false;
                    }
                }
                return allow;
            }

            function hideSuccessMessage() {
                $scope.hasSuccessList = false;
                $scope.hasSuccessDetail = false;
                $scope.successMsg = null;
                $rootScope.showSuccessMessage = false;
            }

            $scope.$on('sw_successmessagetimeout', function (event, data) {
                hideSuccessMessage();
            });

            $scope.$on('sw_ajaxerror', function (event, errordata, status) {
                if (status == 403) {
                    logoutService.logout();
                    return;
                }

                if ($scope.errorMsg) {
                    return;
                }

                $scope.errorMsg = errordata.errorMessage;
                $scope.errorStack = errordata.errorStack;
                $scope.$broadcast('sw_errormessage', true);

                if (nullOrUndef($rootScope.hasErrorDetail) && nullOrUndef($rootScope.hasErrorList)) {
                    $rootScope.hasErrorDetail = true;
                }
            });

            $scope.$on('sw_validationerrors', function (event, validationArray) {
                $scope.hasValidationError = true;
                $scope.validationArray = validationArray;
                $('html, body').animate({ scrollTop: 0 }, 'fast');
            });

            $scope.$on('sw_ajaxinit', function (event, errordata) {
                $scope.removeAlert();
                $scope.removeValidationAlert();
            });

            $scope.$on('sw_cancelclicked', function (event, errordata) {
                $scope.removeAlert();
                $scope.removeValidationAlert();
            });

            $scope.removeAlert = function () {
                $rootScope.hasErrorDetail = false;
                $rootScope.hasErrorList = false;
                $scope.errorMsg = null;
                $scope.errorStack = null;
                $scope.$broadcast('sw_errormessage', false);
            };

            $scope.removeValidationAlert = function () {
                $scope.hasValidationError = false;
                $scope.validationArray = null;
            };

            $scope.openModal = function () {
                $('#errorModal').modal('show');
                $("#errorModal").draggable();
            };

            $scope.hideModal = function () {
                $('#errorModal').modal('hide');
            };

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.closeSuccessMessage = function() {
                hideSuccessMessage();
            }
        }
    };
});



;
function MyProfileController($scope, $http, $templateCache, i18NService, $rootScope, redirectService) {

    init($scope.resultData);

    function init(data) {
        if (data != null) {
            $scope.currentUser = data.user;
            i18NService.changeCurrentLanguage($scope.currentUser.language);
            $scope.$emit('sw_titlechanged', i18NService.get18nValue('general.profiledetails', 'Profile Details'));
            $scope.isMyProfile = true;
            $scope.isHapag = $rootScope.clientName == 'hapag' ? true : false;
            $scope.canChangeLanguage = "true" == sessionStorage.mocklanguagechange || ($scope.isHapag && data.canChangeLanguage);
            $scope._rolesandfunctions = data.rolesAndFunctions;
            fillRestrictions(data);
            fillLanguage();
        }
    }

    function fillRestrictions(data) {
        $scope.restrictions = {};
        $scope.restrictions = data.restrictions;
        $scope.canViewRestrictions = $rootScope.clientName == 'hapag' && data.canViewRestrictions;
    }

    function fillLanguage() {
        $scope.languages = [
               { value: 'EN', text: $scope.i18N('language.english', 'English') },
               { value: 'DE', text: $scope.i18N('language.german', 'German') },
               { value: 'ES', text: $scope.i18N('language.spanish', 'Spanish') }
        ];
        $scope.language = { selected: null };
        for (var i = 0; i < $scope.languages.length; i++) {
            if ($scope.languages[i].value == $scope.currentUser.language) {
                $scope.language = { selected: $scope.languages[i] };
                break;
            }
        }
    }

    $scope.rolesAndFunctions=function() {
        return $scope._rolesandfunctions;
    }

    $scope.fixselecttext = function () {
        angular.forEach($("#userLanguage"), function (currSelect) {
            currSelect.options[currSelect.selectedIndex].text += ' ';
        });
    };

    $scope.$watch('languages', function (newValue, oldValue) {
        if (newValue !== oldValue) {
            $scope.languages = newValue;
            angular.forEach($("#userLanguage"), function (currSelect) {
                currSelect.options[currSelect.selectedIndex].text = newValue[currSelect.selectedIndex].text;
            });
        }
    });

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.saveMyProfile = function () {
        $http({
            method: "GET",
            url: url("api/security/User/" + $scope.currentUser.dbId)
        })
        .success(function (user) {
            fillUserToSave(user);
            $('#saveMyProfileBTN').prop('disabled', 'disabled');

            $http.post(url("api/security/User"), user)
                .success(function () {
                    $('#saveMyProfileBTN').removeAttr('disabled');
                    resetuserinfo();
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                })
                .error(function (data) {
                    $('#saveMyProfileBTN').removeAttr('disabled');
                    $scope.title = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
                });
        });
    };

    function resetuserinfo() {
        var parameters = {};
        var urlToInvoke = redirectService.getActionUrl('User', 'MyProfile', parameters);
        $http.get(urlToInvoke).
        success(function (data, status, headers, config) {
            init(data.resultObject);
        }).
        error(function (data, status, headers, config) {
            var error = "Error " + status;
        });
    }

    function fillUserToSave(user) {
        var password = $scope.currentUser.password;
        if (!nullOrUndef(password)) {
            user.password = password;
        } else {
            user.password = null;
        }
        user.firstName = $scope.currentUser.firstName;
        user.lastName = $scope.currentUser.lastName;
        user.siteId = $scope.currentUser.siteId;
        user.email = $scope.currentUser.email;
        user.department = $scope.currentUser.department;
        user.phone = $scope.currentUser.phone;
        if (!nullOrUndef($scope.language.selected)) {
            $scope.currentUser.language = $scope.language.selected.value;
        }
        user.language = $scope.currentUser.language;
    }
};
app.directive('pagination', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/pagination.html'),
        scope: {
            renderfn: "&",
            paginationData: '=',
            showactions: "@",
            schemaId: '@',
            applicationName: '@',
            mode: '@',
            disablePrint: '@',
            disableExport: '@'
        },

        controller: function ($scope,
            $http,
            $rootScope,
            $timeout,
            printService,
            searchService,
            i18NService,
            redirectService,
            contextService,
            excelService) {

            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.isHapag = function () {
                return $rootScope.clientName == "hapag";
            };

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.previousPage = function () {
                var pageNumber = $scope.paginationData.pageNumber;
                if (pageNumber > 1) {
                    $scope.selectPage(pageNumber - 1);
                }
            };

            $scope.nextPage = function () {
                var pageNumber = $scope.paginationData.pageNumber;
                if (pageNumber < $scope.paginationData.pageCount) {
                    $scope.selectPage(pageNumber + 1);
                }
            };

            $scope.exportToExcel = function (schemaId) {
                excelService.exporttoexcel($scope.$parent.schema, $scope.$parent.searchData, $scope.$parent.searchSort, $scope.$parent.searchOperator, $scope.paginationData);
            };

         

       

            var printPageSize;

            $scope.exportToPrint = function () {
                printService.printList($scope.paginationData.totalCount,
                    $scope.paginationData.pageSize, $scope.renderfn, $scope.$parent.schema);
            };

            $scope.selectPage = function (page) {
                printPageSize = null;
                $scope.renderfn({ pageNumber: page });
            };

            if ($scope.disablePrint === undefined) {
                $scope.disablePrint = false;
            }
            if ($scope.disableExport === undefined) {
                $scope.disableExport = false;
            }

            $scope.adjustMargin = function (language) {
                if (!$scope.isHapag()) {
                    return;
                }
                var marginLeft = '30px';
                if (language.toLowerCase() == 'en') {
                    marginLeft = '60px';
                }
                $('.pagination-pager').css({ 'margin-left': marginLeft });
            }

            $scope.adjustMargin(i18NService.getCurrentLanguage());
        }
    };
});;
$(function () {

    if (typeof jScrollPane === 'undefined') {
        return;
    }

    var api = $('.menu-primary').jScrollPane({ maintainPosition: true }).data('jsp');
    var throttleTimeout;

    $(window).bind(
        'resize',
        function () {
            // IE fires multiple resize events while you are dragging the browser window which
            // causes it to crash if you try to update the scrollpane on every one. So we need
            // to throttle it to fire a maximum of once every 50 milliseconds...
            if (typeof api !== 'undefined') {
                if (!throttleTimeout) {
                    throttleTimeout = setTimeout(
                        function () {

                            //HAP-876 - resize the nav, to make sure it is scrollable
                            $('.menu-primary').height($(window).height());

                            api.reinitialise();
                            throttleTimeout = null;
                        },
                        50
                    );
                }
            }
        }
    );

    //prevent window scrolling after reaching end of navigation pane 
    $(document).on('mousewheel', '.menu-primary',
      function (e) {
          var delta = e.originalEvent.wheelDelta;
          this.scrollTop += (delta < 0 ? 1 : -1) * 30;
          e.preventDefault();
      });
});;
function RoleController($scope, $http, $templateCache, i18NService) {

    function bind(application) {
        $scope.application = application;
        $scope.title = application.title;
        $scope.showList();
    };


    function toDetail() {
        switchMode(true);
    };

    function toList(data) {
        if (data != null) {
            $scope.roles = data;
        }
        switchMode(false);
    };

    function switchMode(mode) {

        $scope.isDetail = mode;
        $scope.isList = !mode;
    }

    $scope.editRole = function (id, name, active) {
        $scope.role = {};
        $scope.role.id = id;
        $scope.role.name = name;
        $scope.role.active = active;
        toDetail();
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.showList = function () {
        $scope.searchData = {};
        $scope.selectPage(1);
    };

    $scope.cancel = function () {
        toList(null);
    };

    $scope.delete = function () {
        $('#saveBTN').prop('disabled', 'disabled');
        $http.put(url("api/security/Role"), JSON.stringify($scope.role))
            .success(function (data) {
                $('#saveBTN').removeAttr('disabled');
                toList(data.resultObject);
            })
            .error(function (data) {
                $('#saveBTN').removeAttr('disabled');
                $scope.title = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
            });
    };

    $scope.save = function () {
        $('#saveBTN').prop('disabled', 'disabled');
        $http.post(url("api/security/Role"), JSON.stringify($scope.role))
          .success(function (data) {
              $('#saveBTN').removeAttr('disabled');
              toList(data.resultObject);
          })
          .error(function (data) {
              $('#saveBTN').removeAttr('disabled');
              $scope.title = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
          });
    };

    $scope.new = function () {
        toDetail(true);
    };

    function init() {
        $scope.roles = $scope.resultData;
        toList(null);
    }

    init();

};
var app = angular.module('sw_layout');

function SamelinePickersController($scope, $rootScope, formatService) {

    var getCurrentDateString = function() {
        var date = new Date();
        return date.mmddyyyy();
    }
    
    $scope.formClass = function () {
        return GetBootstrapFormClass(6);
    }

    var joinDates = function (fields) {
        if ($scope.date1 == undefined && $scope.date2 == undefined) {
            return;
        }
        var date1St = $scope.date1 == undefined ? getCurrentDateString() : $scope.date1; 
        var date2St = $scope.date2 == undefined ? "" : " " + $scope.date2;
        fields[$scope.fieldMetadata.parameters['joinattribute']] = date1St + date2St;
    };

    function doInit() {
        var fieldMetadata = $scope.fieldMetadata;
        var defaultValue = fieldMetadata.parameters['default'];
        if (fieldMetadata.parameters['timeformat'] == undefined) {
            fieldMetadata.parameters['timeformat'] = 'HH:mm';
        }
        if (fieldMetadata.parameters['dateformat'] == undefined) {
            fieldMetadata.parameters['dateformat'] = 'MM/dd/yyyy';
        }
        var targetDate = $scope.datamap[fieldMetadata.parameters['joinattribute']];
        var valueToUse = targetDate == undefined ? defaultValue : targetDate;

        $scope.date1 = valueToUse;
        $scope.date2 = valueToUse;
            
        $scope.$on("sw_beforeSave", function (event, fields) {
            joinDates(fields);
        });
    };

    doInit();
}
;
function SchedulerSetupController($scope, $http, $templateCache, i18NService) {

    function toList(data) {
        if (data != null) {
            $scope.listObjects = data;
        }
        switchMode(false);
    };



    function toDetail() {
        switchMode(true);
    };

    $scope.contextPath = function (path) {
        return url(path);
    };

    $scope.edit = function (data) {
        $scope.scheduler = data;
        toDetail();
    };

    $scope.cancel = function () {
        toList(null);
    };

    $scope.pause = function (data) {
        callApi(data, "Pause");
    };

    $scope.schedule = function (data) {
        callApi(data, "Schedule");
    };

    $scope.execute = function (data) {
        callApi(data, "Execute");
    };

    $scope.changeCron = function (data) {
        callApi(data, "ChangeCron");
    };

    function callApi(data, jobCommand) {
        if (data != null) {
            var urlAux = "api/scheduler?name=" + data.name + "&jobCommand=" + jobCommand;
            if (jobCommand == "ChangeCron" && data.cron != null)
                urlAux += "&cron=" + data.cron;

            $http({
                method: "GET",
                url: url(urlAux)
            })
            .success(function (dataAux) {
                toList(dataAux.resultObject);
            })
            .error(function (dataAux) {
                $scope.title = dataAux || i18NService.get18nValue('general.requestfailed', 'Request failed');
            });
        }
    }

    function switchMode(mode) {
        $scope.isDetail = mode;
        $scope.isList = !mode;
    }

    function initSchedulerSetup() {
        var data = $scope.resultData;
        if (data != null) {
            $scope.listObjects = data;
            toList(null);
        }
    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    initSchedulerSetup();
};;
function AceController($scope, $http, $templateCache, $window, i18NService) {

    $scope.save = function () {
        var urlToUse = $scope.type == 'menu' ? "/api/generic/EntityMetadata/SaveMenu" : "/api/generic/EntityMetadata/SaveMetadata";
        $http({
            method: "PUT",
            url: url(urlToUse),
            headers: { "Content-Type": "application/xml" },
            data: ace.edit("editor").getValue()
        })
            .success(function () {
                $window.location.href = url("/stub/reset");
            });

    };

    $scope.contextPath = function (path) {
        return url(path);
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    function init() {
        var editor = ace.edit("editor");
        editor.getSession().setMode("ace/mode/xml");
        var data = $scope.resultData;
        $scope.type = data.type;
        editor.setValue(data.content);
        editor.gotoLine(0);
    }

    loadScript("/Content/Scripts/ace/ace.js", init);

    $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
        if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("EntityMetadataEditor.html") != -1) {
            init();
        }
    });
};


function UserController($scope, $http, $templateCache, i18NService) {

    var app = angular.module('plunker', ['ui.multiselect']);

    $scope.addSelectedProfiles = function (availableprofilesselected) {
        $scope.user.profiles = $scope.user.profiles.concat(availableprofilesselected);
        var availableProfilesArr = $scope.availableprofiles;
        $scope.availableprofiles = availableProfilesArr.filter(function (item) {
            return availableprofilesselected.indexOf(item) === -1;
        });
    };

    $scope.removeSelectedProfiles = function (selectedProfiles) {
        $scope.availableprofiles = $scope.availableprofiles.concat(selectedProfiles);
        var userProfiles = $scope.user.profiles;
        $scope.user.profiles = userProfiles.filter(function (item) {
            return selectedProfiles.indexOf(item) === -1;
        });
    };

    $scope.addConstraint = function () {
        $scope.profile.dataConstraints.push({ id: null, Entity: "", whereClause: "", isactive: true });
    };

    $scope.removeConstraint = function (constraint) {
        var dataConstraints = $scope.profile.dataConstraints;
        var idx = dataConstraints.indexOf(constraint);
        if (idx > -1) {
            dataConstraints.splice(idx, 1);
        }
    };

    function toDetail() {
        switchMode(true);
    };

    function toList(data) {
        $scope.availableroles = $scope.availablerolesOriginal;
        $scope.availableprofiles = $scope.availableprofilesOriginal;

        if (data != null) {
            $scope.listObjects = data;
        }
        switchMode(false);
    };

    function switchMode(mode) {

        $scope.isDetail = mode;
        $scope.isList = !mode;

    }

    $scope.edit = function (id) {
        $http({
            method: "GET",
            url: url("api/security/User/" + id),
        })
            .success(function (data) {
                $scope.user = data;
                $scope.user.password = null;
                $scope.availableprofilesselected = {};
                $scope.selectedProfiles = {};
                $scope.selectedroles = {};
                $scope.availablerolesselected = {};

                var availableProfilesArr = $scope.availableprofiles;
                $scope.availableprofiles = availableProfilesArr.filter(function (item) {
                    var profiles = data.profiles;
                    for (var i = 0; i < profiles.length; i++) {
                        if (item["id"] == profiles[i]["id"]) {
                            return false;
                        }
                    }
                    return true;
                });
                toDetail();
            });

    };
    
    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.showList = function () {
        $scope.searchData = {};
        $scope.selectPage(1);
    };

    $scope.cancel = function () {
        toList(null);
    };

    function doSaveOrDelete(method) {
        $('#saveBTN').prop('disabled', 'disabled');
        $http[method](url("api/security/User"), JSON.stringify($scope.user))
            .success(function (data) {
                $('#saveBTN').removeAttr('disabled');
                toList(data);
            })
            .error(function (data) {
                $('#saveBTN').removeAttr('disabled');
                //                $scope.title = data || "Request failed";
            });
    }

    $scope.delete = function () {
        doSaveOrDelete("put");
    };

    $scope.save = function () {
        $('#saveBTN').prop('disabled', 'disabled');
        $http.post(url("api/security/User"), JSON.stringify($scope.user))
            .success(function (data) {
                $('#saveBTN').removeAttr('disabled');
                toList(data.resultObject);
                $('html, body').animate({ scrollTop: 0 }, 'fast');
            })
            .error(function (data) {
                $('#saveBTN').removeAttr('disabled');
                //                $scope.title = data || "Request failed";
            });
    };

    $scope.new = function () {
        $scope.user = {};
        $scope.user.profiles = [];
        $scope.user.customRoles = [];
        $scope.user.customConstraints = [];
        toDetail(true);
    };

    function initUser() {
        var data = $scope.resultData;
        $scope.listObjects = data.users;
        $scope.$emit('sw_titlechanged', i18NService.get18nValue('general.usersetup','User Setup'));
        $scope.availableprofiles = data.profiles;
        $scope.availableprofilesselected = {};
        $scope.selectedProfiles = {};
        $scope.availableprofilesOriginal = data.profiles;

        $scope.availableroles = data.roles;
        $scope.availablerolesOriginal = data.roles;
        $scope.selectedroles = {};
        $scope.availablerolesselected = {};

        toList(null);
    };

    initUser();
}


;


function UserProfileController($scope, $http, $templateCache, i18NService) {

    var app = angular.module('plunker', ['ui.multiselect']);

    $scope.addSelectedRoles = function (availablerolesselected) {
        $scope.profile.roles = $scope.profile.roles.concat(availablerolesselected);
        var availableRolesArr = $scope.availableroles;
        $scope.availableroles = availableRolesArr.filter(function (item) {
            return availablerolesselected.indexOf(item) === -1;
        });
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.removeSelectedRoles = function (selectedRoles) {
        $scope.availableroles = $scope.availableroles.concat(selectedRoles);
        var profileRoles = $scope.profile.roles;
        $scope.profile.roles = profileRoles.filter(function (item) {
            return selectedRoles.indexOf(item) === -1;
        });
    };

    $scope.addConstraint = function () {
        $scope.profile.dataConstraints.push({ id: null, Entity: "", whereClause: "", isactive: true });
    };

    $scope.removeConstraint = function (constraint) {
        var dataConstraints = $scope.profile.dataConstraints;
        var idx = dataConstraints.indexOf(constraint);
        if (idx > -1) {
            dataConstraints.splice(idx, 1);
        }
    };


    function toDetail() {
        switchMode(true);
    };

    function toList(data) {
        $scope.availableroles = $scope.availablerolesOriginal;
        if (data != null) {
            $scope.listObjects = data;
        }
        switchMode(false);
    };

    function switchMode(mode) {
        $scope.errorMessage = null;
        $scope.isDetail = mode;
        $scope.isList = !mode;
    }

    $scope.editProfile = function (profile) {
        $scope.profile = profile;
        var availableRolesArr = $scope.availableroles;
        $scope.availableroles = availableRolesArr.filter(function (item) {
            var roles = profile.roles;
            for (var i = 0; i < roles.length; i++) {
                if (item["id"] == roles[i]["id"]) {
                    return false;
                }
            }
            return true;
        });
        toDetail();
    };



    $scope.showList = function () {
        $scope.searchData = {};
        $scope.selectPage(1);
    };

    $scope.cancel = function () {
        toList(null);
    };

    $scope.delete = function () {
        $('#saveBTN').prop('disabled', 'disabled');
        $http.put(url("api/security/UserProfile"), JSON.stringify($scope.profile))
            .success(function (data) {
                $('#saveBTN').removeAttr('disabled');
                toList(data);
            })
            .error(function (data) {
                $('#saveBTN').removeAttr('disabled');
                $scope.errorMessage = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
            });
    };

    $scope.save = function () {
        $('#saveBTN').prop('disabled', 'disabled');
        $http.post(url("api/security/UserProfile"), JSON.stringify($scope.profile))
          .success(function (data) {
              $('#saveBTN').removeAttr('disabled');
              toList(data);
          })
          .error(function (data) {
              $('#saveBTN').removeAttr('disabled');
              $scope.errorMessage = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
          });
    };

    $scope.new = function () {
        $scope.profile = {};
        $scope.profile.roles = [];
        $scope.profile.dataConstraints = [];
        toDetail(true);
    };

    function init() {
        var data = $scope.resultData;
        $scope.listObjects = data.profiles;
        $scope.availableroles = data.roles;
        $scope.availablerolesOriginal = data.roles;
        $scope.selectedroles = {};
        $scope.availablerolesselected = {};
        toList(null);
    }

    init();

};
var app = angular.module('sw_layout');

app.factory('alertService', function ($rootScope, $timeout, i18NService) {

    return {

        confirm: function (applicationName, applicationId, callbackFunction, msg, cancelcallback) {
            var defaultConfirmMsg = "Are you sure you want to delete {0} {1}?".format(applicationName, applicationId);
            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            var defaultDeleteMsg = i18NService.get18nValue('general.defaultcommands.delete.confirmmsg', defaultConfirmMsg, [applicationName, applicationId]);
            bootbox.confirm({
                message: msg == null ? defaultDeleteMsg : msg,
                title: i18NService.get18nValue('general.defaultcommands._confirmationtitle', 'Confirmation'),
                className: 'smallmodal',
                callback: function (result) {
                    if (result == false) {
                        if (cancelcallback != undefined) {
                            cancelcallback();
                            return;
                        }
                        return;
                    }
                    callbackFunction();
                }
            });
        },
        confirmCancel: function (scope, prevdata, prevschema, applicationName, applicationId, callbackFunction, msg, cancelcallback) {
            var defaultConfirmMsg = "Are you sure you want to cancel {0} {1}?".format(applicationName, applicationId);
            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            var defaultDeleteMsg = i18NService.get18nValue('general.defaultcommands.delete.confirmmsg', defaultConfirmMsg, [applicationName, applicationId]);
            bootbox.confirm({
                message: msg == null ? defaultDeleteMsg : msg,
                title: i18NService.get18nValue('general.defaultcommands._confirmationtitle', 'Confirmation'),
                className: 'smallmodal',
                callback: function (result) {
                
                    if (result == false) {
                        if (cancelcallback != undefined) {
                            cancelcallback();
                            return;
                        }
                        return;
                    }
                    
                        scope.cancelfn({ data: scope.previousdata, schema: scope.previousschema });
                        scope.$emit('sw_cancelclicked');
                        return;
                }
                
            });
        },

        alert: function (msg) {
            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            bootbox.alert({
                message: msg,
                title: i18NService.get18nValue('general.defaultcommands._alert', 'Alert'),
                className: 'smallmodal',
            });
        },

        success: function (message, autoHide) {
            var data = { successMessage: message };
            $rootScope.$broadcast('sw_successmessage', data);
            if (autoHide) {
                $timeout(function () {
                    data.successMessage = null;
                    $rootScope.$broadcast('sw_successmessage', data);
                }, 5000);
            }
        },

        error: function (message, autoHide) {
            var data = { errorMessage: message };
            $rootScope.$broadcast('sw_errormessage', data);
            if (autoHide) {
                $timeout(function () {
                    data.errorMessage = null;
                    $rootScope.$broadcast('sw_errormessage', data);
                }, 5000);
            }
        }
    };
});


;
var app = angular.module('sw_layout');

/*
/Just a holder for multiple inner services
*/
app.factory('applicationFacade', function (i18NService,compositionService,printService,tabsService) {



    return {
        tabsService: function () {
            return tabsService;
        },

        compositionService: function () {
            return compositionService;
        },
        printService: function () {
            return printService;
        },

    };

});


;
var app = angular.module('sw_layout');

app.factory('associationService', function ($injector, $http, $timeout, $log, $rootScope, submitService, fieldService) {

    var doUpdateExtraFields = function (associationFieldMetadata, underlyingValue, datamap) {
        var log = $log.getInstance('associationService#doUpdateExtraFields');
        var key = associationFieldMetadata.associationKey;
        datamap[key] = {};
        datamap.extrafields = instantiateIfUndefined(datamap.extrafields);
        if (associationFieldMetadata.extraProjectionFields == null) {
            return;
        }
        for (var i = 0; i < associationFieldMetadata.extraProjectionFields.length; i++) {
            var extrafield = associationFieldMetadata.extraProjectionFields[i];
            var valueToSet = null;
            if (underlyingValue != null) {
                underlyingValue.extrafields = underlyingValue.extrafields || {};
                valueToSet = underlyingValue.extrafields[extrafield];  
            }
            log.debug('updating extra field {0}.{1} | value={2}'.format(key, extrafield, valueToSet));
            if (extrafield.indexOf(key) > -1) {
                datamap[extrafield] = valueToSet;
            } else {
                var appendDot = extrafield.indexOf('.') == -1;
                var fullKey = key;
                if (appendDot) {
                    fullKey += ".";
                }
                fullKey += extrafield;
                FillRelationship(datamap, fullKey, valueToSet);
                datamap[GetRelationshipName(fullKey)] = valueToSet;
                datamap[key][extrafield] = valueToSet;

                FillRelationship(datamap.extrafields, fullKey, valueToSet);
                datamap.extrafields[GetRelationshipName(fullKey)] = valueToSet;
                datamap.extrafields[key][extrafield] = valueToSet;
            }
        };
    }






    var doGetFullObject = function (associationFieldMetadata, associationOptions, selectedValue) {
        if (selectedValue == null) {
            return null;
        } else if (Array.isArray(selectedValue)) {         // for 'checkbox' option fields (multi value selection)
            var ObjectArray = [];

            // Extract each item into an array object
            for (var i = 0; i < selectedValue.length; i++) {
                var Object = doGetFullObject(associationFieldMetadata, associationOptions, selectedValue[i]);
                ObjectArray = ObjectArray.concat(Object);
            }

            // Return results for multi-value selection
            return ObjectArray;
        }

        //we need to locate the value from the list of association options
        // we only have the "value" on the datamap 
        var key = associationFieldMetadata.associationKey;
        var listToSearch = associationOptions[key];
        if (listToSearch == null) {
            //if the list is lazy (ex: lookups, there´s nothing we can do, except for static option field )
            if (associationFieldMetadata.options != undefined) {
                //this means this is an option field with static options
                var resultArr = $.grep(associationFieldMetadata.options, function (option) {
                    return selectedValue.equalIc(option.value);
                });
                return resultArr == null ? null : resultArr[0];
            }

            return null;
        }
        for (var i = 0; i < listToSearch.length; i++) {
            var objectWithProjectionFields = listToSearch[i];
            if ((typeof selectedValue === 'string') && selectedValue.equalIc(objectWithProjectionFields.value)) {
                //recovers the whole object in which the value field is equal to the datamap
                return objectWithProjectionFields;
            }
        }
        return null;
    };

    return {

        cleanupDependantAssociationChain: function (triggerFieldName, scope) {
            var schema = scope.schema;
            var datamap = scope.datamap;
            var associationOptions = scope.associationOptions;
            var depFields = schema.dependantFields;
            var firstLevelDeps = depFields[triggerFieldName];
            if (firstLevelDeps == null) {
                return;
            }
            for (var i = 0; i < firstLevelDeps.length; i++) {
                var value = firstLevelDeps[i];
                var associatedDisplayable = fieldService.getDisplayablesByAssociationKey(schema, value);
                var target = associatedDisplayable[0].target;
                $log.getInstance('associationService#cleanupDependantAssociationChain').debug('cleaning up dependant association of {0} {1}'.format(triggerFieldName, target));
                associationOptions[value] = [];
                if (isIe9()) {
                    //due to a crazy ie9-angular bug, we need to do it like this
                    // taken from https://github.com/angular/angular.js/issues/2809
                    var element = $("select[data-comboassociationkey='" + value + "']");
                    element.hide();
                    element.show();
                }
                datamap[target] = "$null$ignorewatch";
                this.cleanupDependantAssociationChain(target, scope);
            }
        },


        getFullObjectByAttribute: function (attribute, schema, datamap, associationOptions) {
            var associationFieldMetadata = fieldService.getDisplayableByKey(schema, attribute);
            return this.getFullObject(associationFieldMetadata, datamap, associationOptions);
        },

        getFullObject: function (associationFieldMetadata, datamap, associationOptions) {
            //we need to locate the value from the list of association options
            // we only have the "value" on the datamap 
            var target = associationFieldMetadata.target;
            var selectedValue = datamap[target];
            if (selectedValue == null) {
                return null;
            }
            //TODO: Return full object.
            if (typeof selectedValue == "string") {
                selectedValue = selectedValue.replace("$ignorewatch", "");
            }

            var resultValue = doGetFullObject(associationFieldMetadata, associationOptions, selectedValue);
            if (resultValue == null) {
                $log.getInstance('associationService#getFullObject').warn('value not found in association options for {0} '.format(associationFieldMetadata.associationKey));
            }
            return resultValue;
        },

        ///this method is used to update associations which depends upon the projection result of a first association.
        ///it only makes sense for associations which have extraprojections
        updateUnderlyingAssociationObject: function (associationFieldMetadata, underlyingValue, scope) {

            if (associationFieldMetadata.extraProjectionFields.length == 0) {
                //do nothing because it has no extraprojection fields
                return;
            }

            var key = associationFieldMetadata.associationKey;
            var i;
            if (underlyingValue == null) {
                //we need to locate the value from the list of association options
                // we only have the "value" on the datamap
                underlyingValue = this.getFullObject(associationFieldMetadata, scope.datamap, scope.associationOptions);
            }
            if (underlyingValue == null && scope.associationOptions[key] == null) {
                //the value remains null, but this is because the list of options is lazy loaded, nothing to do
                return;
            }

            doUpdateExtraFields(associationFieldMetadata, underlyingValue, scope.datamap);
        },

        ///
        ///
        ///dispatchedbyuser: the method could be called after a user action (changing a field), or internally after a value has been set programatically 
        postAssociationHook: function (associationMetadata, scope, triggerparams) {
            if (associationMetadata.events == undefined) {
                return;
            }
            var afterChangeEvent = associationMetadata.events['afterchange'];
            if (afterChangeEvent == undefined) {
                return;
            }
            var service = $injector.get(afterChangeEvent.service);
            if (service == undefined) {
                //this should not happen, it indicates a metadata misconfiguration
                return;
            }
            //now let´s invoke the service
            var fn = service[afterChangeEvent.method];
            if (fn == undefined) {
                //this should not happen, it indicates a metadata misconfiguration
                return;
            }
            var fields = scope.datamap;
            if (scope.datamap.fields != undefined) {
                fields = scope.datamap.fields;
            }

            var afterchangeEvent = {
                fields: fields,
                scope: scope,
                triggerparams: instantiateIfUndefined(triggerparams)
            };
            $log.getInstance('associationService#postAssociationHook').debug('invoking post hook service {0} method {1}'.format(afterChangeEvent.service, afterChangeEvent.method));
            fn(afterchangeEvent);
        },

        //Callback of the updateAssociations call, in which the values returned from the server would update the scope variables, 
        //to be shown on screen
        ///It would be called at the first time the detail screen is opened as well
        updateAssociationOptionsRetrievedFromServer: function (scope, serverOptions, datamap) {
            var log = $log.getInstance('associationService#updateAssociationOptionsRetrievedFromServer');
            scope.associationOptions = instantiateIfUndefined(scope.associationOptions);
            scope.blockedassociations = instantiateIfUndefined(scope.blockedassociations);
            scope.associationSchemas = instantiateIfUndefined(scope.associationSchemas);
            scope.disabledassociations = instantiateIfUndefined(scope.disabledassociations);
            for (var dependantFieldName in serverOptions) {

                //this iterates for list of fields which were dependant of a first one. 
                var array = instantiateIfUndefined(serverOptions[dependantFieldName]);

                log.debug('updating association from server {0} length {1}'.format(dependantFieldName, array.associationData == null ? 0 : array.associationData.length));

                scope.associationOptions[dependantFieldName] = array.associationData;
                scope.blockedassociations[dependantFieldName] = (array.associationData == null || array.associationData.length == 0);
                scope.associationSchemas[dependantFieldName] = array.associationSchemaDefinition;
            }
        },

        restorePreviousValues: function (scope, serverOptions, datamap) {
            var log = $log.getInstance('associationService#restorePrevious');
            for (var dependantFieldName in serverOptions) {

                //this iterates for list of fields which were dependant of a first one. 
                var array = instantiateIfUndefined(serverOptions[dependantFieldName]);

                log.debug('restoring previous values for {0}'.format(dependantFieldName));

                var associationFieldMetadatas = fieldService.getDisplayablesByAssociationKey(scope.schema, dependantFieldName);
                if (associationFieldMetadatas == null) {
                    //should never happen, playing safe here
                    continue;
                }
                var fn = this;
                $.each(associationFieldMetadatas, function (index, value) {
                    if (value.target == null) {
                        return;
                    }
                    if (isIe9()) {
                        //due to a crazy ie9-angular bug, we need to do it like this
                        // taken from https://github.com/angular/angular.js/issues/2809
                        var element = $("select[data-comboassociationkey='" + value.associationKey + "']");
                        element.hide();
                        element.show();
                        //this workaround, fixes a bug where only the fist charachter would show...
                        //taken from http://stackoverflow.com/questions/5908494/select-only-shows-first-char-of-selected-option
                        element.css('width', 0);
                        element.css('width', '');
                    }
                    //                    clear datamap for the association updated -->This is needed due to a IE9 issue
                    var previousValue = datamap[value.target];

                    if (array.associationData == null) {
                        //if no options returned from the server, nothing else to do
                        return;
                    }

                    //
                    if (previousValue != null) {
                        datamap[value.target] = "$null$ignorewatch";
                        previousValue = previousValue.replace("$ignorewatch", "");
                        for (var j = 0; j < array.associationData.length; j++) {
                            if (array.associationData[j].value == previousValue) {
                                var fullObject = array.associationData[j];
                                $timeout(function () {
                                    log.debug('restoring {0} to previous value {1}. '.format(value.target, previousValue));
                                    //if still present on the new list, setting back the value which was 
                                    //previous selected, but after angular has updadted the list properly
                                    //using $ignorewatch so that any eventual afterchange doesnt get influentiated by this
                                    datamap[value.target] = previousValue + "$ignorewatch";
                                    doUpdateExtraFields(value, fullObject, datamap);
                                    if (fn.postAssociationHook) {
                                        fn.postAssociationHook(value, scope, { phase: 'initial', dispatchedbytheuser: false });
                                    }
                                }, 0, false);
                                break;
                            }
                        }
                    }

                    //check if options is selected by default
                    //TODO: this name (isSelected) is a bit dangerous, place a # to avoid collisions with a column called isSelected
                    var selectedOptions = $.grep(array.associationData, function (option) {
                        if (option.extrafields != null && option.extrafields['isSelected'] != null) {
                            return option;
                        }
                    });

                    if (selectedOptions.length > 0) {
                        //TODO: why checkboxes aren´t the same (target)?
                        var targetString = value.rendererType == 'checkbox' ? value.associationKey : value.target;
                        // forces to clean previous association selection
                        datamap[targetString] = [];
                        for (var i = 0; i < selectedOptions.length; i++) {
                            if (selectedOptions[i].extrafields['isSelected'] == true) {
                                datamap[targetString].push(selectedOptions[i].value);
                            }
                        }
                    }
                }
                );
            }
        },



        getEagerAssociations: function (scope,options) {
            var associations = fieldService.getDisplayablesOfTypes(scope.schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            if (associations == undefined || associations.length == 0) {
                //no need to hit server in that case
                return;
            }

            scope.associationOptions = instantiateIfUndefined(scope.associationOptions);
            scope.blockedassociations = instantiateIfUndefined(scope.blockedassociations);
            scope.associationSchemas = instantiateIfUndefined(scope.associationSchemas);
            return this.updateAssociations({ attribute: "#eagerassociations" }, scope, options);
        },

        //
        // This method is called whenever an association value changes, in order to update all the dependant associations 
        //of this very first association.
        // This would only affect the eager associations, not the lookups, because they would be fetched at the time the user opens it.
        //Ex: An asset could be filtered by the location, so if a user changes the location field, the asset should be refetched.
        updateAssociations: function (association, scope, options) {
            options = options || {};

            var triggerFieldName = association.attribute;
            var schema = scope.schema;
            if (triggerFieldName != "#eagerassociations" && $.inArray(triggerFieldName, schema.fieldWhichHaveDeps) == -1) {
                //no other asociation depends upon this first association, return here.
                //false is to indicate that no value has been updated
                return false;
            }

            this.cleanupDependantAssociationChain(triggerFieldName, scope);

            var updateAssociationOptionsRetrievedFromServer = this.updateAssociationOptionsRetrievedFromServer;
            var restorePreviousValues = this.restorePreviousValues;
            var postAssociationHook = this.postAssociationHook;

            var applicationName = schema.applicationName;
            var fields = scope.datamap;
            if (scope.datamap.fields) {
                fields = scope.datamap.fields;
            }

            var parameters = {
                key: {
                    schemaId: schema.schemaId,
                    mode: schema.mode,
                    platform: platform(),
                },
                triggerFieldName: triggerFieldName,
                id: fields[schema.idFieldName]
            };
            var fieldsTosubmit = submitService.removeExtraFields(fields, true, scope.schema);
            var urlToUse = url("/api/data/" + applicationName + "?" + $.param(parameters));
            var jsonString = angular.toJson(fieldsTosubmit);
            var log = $log.getInstance('associationService#updateAssociations');
            log.info('going to server for dependat associations of {0}'.format(triggerFieldName));
            log.debug('Content: \n {0}'.format(jsonString));

            var config = {};
            if (options.avoidspin) {
                config.avoidspin = true;
            }

            $http.post(urlToUse, jsonString, config).success(function (data) {
                var options = data.resultObject;
                log.info('associations returned {0}'.format($.keys(options)));
                updateAssociationOptionsRetrievedFromServer(scope, options, fields);
                if (association.attribute != "#eagerassociations") {
                    //this means we´re getting the eager associations, see method above
                    postAssociationHook(association, scope, { dispatchedbytheuser: true, phase: 'configured' });
                } else {
                    $rootScope.$broadcast("sw_associationsupdated", scope.associationOptions);
                }
                //let´s restore after the hooks have been runned to avoid any stale data
                restorePreviousValues(scope, options, fields);
            }).error(
            function data() {
            });
        },

        onAssociationChange: function (fieldMetadata, updateUnderlying, event) {

            if (fieldMetadata.events == undefined) {
                event.continue();
                return;
            }
            var beforeChangeEvent = fieldMetadata.events['beforechange'];
            if (beforeChangeEvent == undefined) {
                event.continue();
            } else {
                var service = $injector.get(beforeChangeEvent.service);
                if (service == undefined) {
                    //this should not happen, it indicates a metadata misconfiguration
                    event.continue();
                    return;
                }
                //now let´s invoke the service
                var fn = service[beforeChangeEvent.method];
                if (fn == undefined) {
                    //this should not happen, it indicates a metadata misconfiguration
                    event.continue();
                    return;
                }
                var result = fn(event);
                //sometimes the event might be syncrhonous, returning either true of false
                if (result != undefined && result == false) {
                    event.interrupt();
                }
                event.continue();
            }
        },

        insertAssocationLabelsIfNeeded: function (schema, datamap, associationoptions) {
            if (schema.properties['addassociationlabels'] != "true") {
                return;
            }
            var associations = fieldService.getDisplayablesOfTypes(schema.displayables, ['OptionField', 'ApplicationAssociationDefinition']);
            var fn = this;
            $.each(associations, function (key, value) {
                var targetName = value.target;
                var labelName = "#" + targetName + "_label";
                if (datamap[labelName] && datamap[labelName]!="") {
                    //already filled, let´s not override it
                    return;
                }
                var realValue = fn.getFullObject(value, datamap, associationoptions);
                if (realValue != null && Array.isArray(realValue)) {
                    datamap[labelName] = "";
                    // store result into a string with newline delimitor
                    for (var i = 0; i < realValue.length; i++) {
                        if (realValue[i] != null) {
                            datamap[labelName] += "\\n" + realValue[i].label;
                        }
                    }
                }
                else if (realValue != null) {
                    datamap[labelName] = realValue.label;
                }
            });
        },


        lookupAssociation: function (displayables, associationTarget) {
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                if (displayable.target != undefined && displayable.target == associationTarget) {
                    return displayable;
                }
            }
            return null;
        },


    };

});


;
var app = angular.module('sw_layout');

app.factory('commandService', function (i18NService, $injector, expressionService) {



    return {
        commandLabel: function (schema, id, defaultValue) {
            var commandSchema = schema.commandSchema;
            if (schema.properties != null && id == "cancel" && schema.properties['detail.cancel.lbl'] != null) {
                var value = schema.properties['detail.cancel.lbl'];
                return i18NService.get18nValue('general.' + value.toLowerCase(), value);
            }
            if (!commandSchema.hasDeclaration) {
                return defaultValue;
            }
            var idx = $.inArray(id, commandSchema.toInclude);
            if (idx == -1) {
                return defaultValue;
            }
            return commandSchema.toInclude[idx].label;
        },

        shouldDisplayCommand: function (commandSchema, id) {
            if (commandSchema.ignoreUndeclaredCommands) {
                return false;
            }
            return $.inArray(id, commandSchema.toExclude) == -1 && $.inArray(id, commandSchema.toInclude) == -1;
        },
        //tabId parameter can be used in showexpression, do not remove it
        isCommandHidden: function (datamap, schema, command, tabId) {
            if (command.remove) {
                return true;
            }
            var expression = command.showExpression;
            if (expression == undefined || expression == "") {
                return false;
            }
            var expressionToEval = expressionService.getExpression(expression,datamap);
            return !eval(expressionToEval);
        },

        isCommandEnabled: function (datamap, schema, command, tabId) {
            var expression = command.enableExpression;
            if (expression == undefined || expression == "") {
                return true;
            }
            var expressionToEval = expressionService.getExpression(expression, datamap);
            var returnValue = eval(expressionToEval);
            return returnValue;
        },

        doCommand: function (scope, command) {
            var clientFunction = command.method;
            if (typeof (clientFunction) === 'function') {
                clientFunction();
                return;
            }
            if (nullOrUndef(clientFunction)) {
                clientFunction = command.id;
            }
            if (command.service == undefined) {
                return;
            }

            var service = $injector.get(command.service);
            if (service == undefined) {
                //this should not happen, it indicates a metadata misconfiguration
                return;
            }

            var method = service[clientFunction];

            var args = [];
            if (command.scopeParameters != null) {
                $.each(command.scopeParameters, function (key, parameterName) {
                    args.push(scope[parameterName]);
                });
            }

            method.apply(this, args);
            return;

        },
        //TODO: make it generic
        executeClickCustomCommand: function (fullServiceName, rowdm,column) {
            var idx = fullServiceName.indexOf(".");
            var serviceName = fullServiceName.substring(0, idx);
            var methodName = fullServiceName.substring(idx + 1);

            var service = $injector.get(serviceName);
            if (service == undefined) {
                var errost = "missing clicking service".format(serviceName);
                throw new Error(errost);
            }

            var method = service[methodName];

            var args = [];
            args.push(rowdm);
            args.push(column);

            method.apply(this, args);
            return;

        }

    };

});


;
(function (angular) {
    "use strict";

    var app = angular.module('sw_layout');

    app.factory('compositionService',
        ["$log", "$http", "$rootScope", "$timeout", "contextService", "submitService", "schemaService", "searchService", "$q", "fieldService", "compositionCommons",
        function ($log, $http, $rootScope, $timeout, contextService, submitService, schemaService, searchService, $q, fieldService, compositionCommons) {

            var config = {
                defaultPageSize: 10,
                defaultOptions: [10, 30, "all"],
                defaultRequestOptions: [0, 10, 30]
            };

            //stores the context of the current detail loaded compositions
            var compositionContext = {};

            var api = {
                locatePrintSchema: locatePrintSchema,
                getTitle: getTitle,
                getListCommandsToKeep: getListCommandsToKeep,
                hasEditableProperty: hasEditableProperty,
                buildMergedDatamap: buildMergedDatamap,
                populateWithCompositionData: populateWithCompositionData,
                getCompositionList: getCompositionList,
                isCompositionLodaded: isCompositionLodaded
            };

            return api;


            //#region private methods

            function nonInlineCompositionsDict(schema) {
                if (schema.nonInlineCompositionsDict != undefined) {
                    //caching
                    return schema.nonInlineCompositionsDict;
                }
                var resultDict = {};
                for (var i = 0; i < schema.nonInlineCompositionIdxs.length; i++) {
                    var idx = schema.nonInlineCompositionIdxs[i];
                    var composition = schema.displayables[idx];
                    resultDict[composition.relationship] = composition;
                }
                schema.nonInlineCompositionsDict = resultDict;
                return resultDict;
            };

            function buildPaginatedSearchDTO(pageNumber, pageSize) {
                var dto = searchService.buildSearchDTO();
                dto.pageNumber = pageNumber || 1;
                dto.pageSize = pageSize === "all" ? 0 : pageSize || config.defaultPageSize;
                dto.totalCount = 0;
                dto.paginationOptions = config.defaultRequestOptions;
                return dto;
            };

            function getLazyCompositions(schema, datamap) {
                if (!schema || !schema["cachedCompositions"]) {
                    return null;
                }
                var compositions = [];
                var cachedCompositions = schema.cachedCompositions;
                for (var composition in cachedCompositions) {
                    if (!cachedCompositions.hasOwnProperty(composition)) {
                        continue;
                    }
                    if ("lazy".equalsIc(cachedCompositions[composition].fetchType)) {
                        compositions.push(composition);
                    } else if ("eager".equalsIc(cachedCompositions[composition].fetchType)) {
                        compositionContext[composition] = datamap[composition];
                    }
                }
                return compositions;
            };

            function fetchCompositions(requestDTO, datamap, showLoading) {
                var log = $log.getInstance('compositionservice#fetchCompositions');
                var urlToUse = url("/api/generic/ExtendedData/GetCompositionData");
                return $http.post(urlToUse, requestDTO, { avoidspin: !showLoading })
                    .then(function (response) {
                        var data = response.data;
                        var parentModifiedFields = data.parentModifiedFields;
                        if (parentModifiedFields) {
                            //server has replied that some fields should change on parent datamap as well
                            for (var field in parentModifiedFields) {
                                if (parentModifiedFields.hasOwnProperty(field)) {
                                    datamap[field] = parentModifiedFields[field];
                                }
                            }
                        }
                        var compositionArray = data.resultObject;
                        var result = {};
                        for (var composition in compositionArray) {

                            if (!compositionArray.hasOwnProperty(composition)) {
                                continue;
                            }
                            var resultList = compositionArray[composition].resultList;
                            log.info('composition {0} returned with {1} entries'.format(composition, resultList.length));
                            //this datamap entry is bound to the whole screen, so we need to set it here as well
                            datamap[composition] = resultList;

                            var paginationData = compositionArray[composition].paginationData;
                            // enforce composition pagination options
                            // need to check existence: pagination disabled in metadata
                            if (!!paginationData) {
                                paginationData.paginationOptions = config.defaultOptions;
                            }
                            //setting this case the tabs have not yet been loaded so that they can fetch from here
                            contextService.insertIntoContext("compositionpagination_{0}".format(composition), paginationData, true);
                            compositionContext[composition] = compositionArray[composition];
                            result[composition] = {
                                relationship: composition,
                                list: resultList,
                                paginationData: paginationData
                            };
                        }
                        return result;
                    });
            };

            function doPopulateWithCompositionData(requestDTO, datamap) {

                return fetchCompositions(requestDTO, datamap)
                    .then(function (result) {

                        $timeout(function () {
                            $rootScope.$broadcast("sw_compositiondataresolved", result);
                        });
                        return result;
                    });
            };

            function buildFetchRequestDTO(schema, datamap, compositions, paginatedSearch) {
                var applicationName = schema.applicationName;
                // sanitizing data to submit
                var fieldsTosubmit = submitService.removeExtraFields(datamap, true, schema);
                var compositionNames = getLazyCompositions(schema, datamap);
                angular.forEach(compositionNames, function (composition) {
                    if (!fieldsTosubmit[composition] || !fieldsTosubmit.hasOwnProperty(composition)) {
                        return;
                    }
                    delete fieldsTosubmit[composition];
                });
                var parameters = {
                    key: {
                        schemaId: schema.schemaId,
                        mode: schema.mode,
                        platform: platform()
                    },
                    id: schemaService.getId(datamap, schema),
                    paginatedSearch: paginatedSearch || buildPaginatedSearchDTO()
                };
                parameters.compositionList = compositionNames;

                if (compositions && compositions.length > 0) {
                    parameters.compositionList = compositions;
                }
                return {
                    application: applicationName,
                    request: parameters,
                    data: fieldsTosubmit
                };
            };

            //#endregion

            //#region Public methods

            function isCompositionLodaded(relationship) {
                return compositionContext[relationship] != null;
            };

            function locatePrintSchema(baseSchema, compositionKey) {
                var schemas = nonInlineCompositionsDict(baseSchema);
                var thisSchema = schemas[compositionKey];

                if (thisSchema.schema.schemas.print != null) {
                    return thisSchema.schema.schemas.print;
                } else if (thisSchema.schema.schemas.list != null) {
                    return thisSchema.schema.schemas.list;
                } else {
                    return thisSchema.schema.schemas.detail;
                }
            };

            function getTitle(baseSchema, compositionKey) {
                var schemas = nonInlineCompositionsDict(baseSchema);
                var thisSchema = schemas[compositionKey];
                return thisSchema.label;
            };

            function getListCommandsToKeep(compositionSchema) {
                var listSchema = compositionSchema.schemas.list;
                if (listSchema == null) {
                    return null;
                }
                var toKeepProperty = listSchema.properties["composition.mainbuttonstoshow"];
                if (!nullOrEmpty(toKeepProperty)) {
                    return toKeepProperty.split(';');
                }
                return [];
            };

            /**
             * @deprecated use schemaService#hasEditableProperty instead
             */
            function hasEditableProperty(listSchema) {
                return schemaService.hasEditableProperty(listSchema);
            };

            /**
             * @deprecated use compositionCommons#buildMergedDatamap instead 
             */
            function buildMergedDatamap(datamap, parentdata) {
                return compositionCommons.buildMergedDatamap(datamap, parentdata);
            };

            /*
            * this method will hit the server to fetch associated composition data on a second request making the detail screens faster
            *
            */
            function populateWithCompositionData(schema, datamap) {
                var applicationName = schema.applicationName;
                var log = $log.getInstance('compositionservice#populateWithCompositionData');
                log.info('going to server fetching composition data of {0}, schema {1}.'.format(applicationName, schema.schemaId));
                compositionContext = {};
                // fetching all compositions in a single http request:
                // browser limits simultaneous client requests (usually 6).
                // doing in a single request so it doesn't impact static files fetching and page loading              
                var dto = buildFetchRequestDTO(schema, datamap);
                return doPopulateWithCompositionData(dto, datamap);
            };

            /**
             * Fetches a paginated list of associated compositions with relationship name matching composition
             * 
             * @param String composition name of the composition relationship
             * @param Object schema composition's parent's schema
             * @param Object datamap composition's parent's datamap
             * @param Integer pageNumber number of the requested page
             * @param Integer pageSize number of items per page
             * @returns Promise 
             *              resolved with parent's datamap populated with the fetched composition list 
             *              and pagination data (datamap[composition] = { list: [Object], paginationData: Object });
             *              rejected with HTTP error 
             */
            function getCompositionList(composition, schema, datamap, pageNumber, pageSize) {
                var pageRequest = buildPaginatedSearchDTO(pageNumber, pageSize);
                var dto = buildFetchRequestDTO(schema, datamap, [composition], pageRequest);
                return fetchCompositions(dto, datamap, true);
            }

            //#endregion

        }]);

})(angular);;
(function (angular) {
    "use strict";

    // service.$inject = [];

    var service = function () {

        var buildMergedDatamap = function (datamap, parentdata) {
            var toClone = parentdata;
            if (parentdata.fields) {
                toClone = parentdata.fields;
            }

            var clonedDataMap = angular.copy(toClone);
            if (datamap) {
                var item = datamap;
                for (var prop in item) {
                    if (item.hasOwnProperty(prop)) {
                        clonedDataMap[prop] = item[prop];
                    }
                }
            }
            return clonedDataMap;
        };

        var api = {
            buildMergedDatamap: buildMergedDatamap
        };

        return api;
    };

    angular.module("sw_layout").factory("compositionCommons", service);

})(angular);
;
var app = angular.module('sw_layout');

app.factory('contextService', function ($rootScope) {

    return {
        //using sessionstorage instead of rootscope, as the later would be lost upon F5.
        //see SWWEB-239
        insertIntoContext: function (key, value, userootscope) {
            if (userootscope) {
                $rootScope['ctx_' + key] = value;
            } else {
                if (value != null && !isString(value)) {
                    value = JSON.stringify(value);
                }
                sessionStorage['ctx_' + key] = value;
            }



        },

        get: function (key, isJson, userootscope) {
            return this.fetchFromContext(key, isJson, userootscope);
        },

        fetchFromContext: function (key, isJson, userootscope, removeentry) {
            //shortcut method
            var value = this.retrieveFromContext(key, userootscope, removeentry);
            if (value == "undefined") {
                return undefined;
            }
            if (value != null && isJson == true && isString(value)) {
                return JSON.parse(value);
            }
            return value;
        },

        //shortcut method
        getFromContext: function (key, isJson, userootscope) {
            return this.fetchFromContext(key, isJson, userootscope);
        },

        retrieveFromContext: function (key, userootscope, removeentry) {
            if (userootscope) {
                var object = $rootScope['ctx_' + key];
                if (removeentry) {
                    delete $rootScope['ctx_' + key];
                }
                return object;
            }
            var sessionContextValue = sessionStorage['ctx_' + key];
            if (removeentry) {
                delete sessionStorage['ctx_' + key];
            }
            if (sessionContextValue == "null") {
                return null;
            }
            return sessionContextValue;
        },

        isLocal: function () {
            return this.retrieveFromContext('isLocal');
        },

        getUserData: function () {
            if ($rootScope.user != null) {
                //caching
                return $rootScope.user;
            }
            var userData = this.retrieveFromContext('user');
            if (userData == null) {
                return null;
            }
            var user = JSON.parse(userData);
            $rootScope.user = user;
            return user;
        },

        InModule: function (moduleArray) {
            if (moduleArray == null) {
                return false;
            }
            var result = false;
            var currModule = this.currentModule();
            if (nullOrUndef(currModule)) {
                return false;
            }
            $.each(moduleArray, function (key, value) {
                if (value.equalIc(currModule)) {
                    result = true;
                    return;
                }
            });
            return result;

        },

        //determines whether the current user has one of the roles specified on the array
        HasRole: function (roleArray) {
            if (roleArray == null) {
                return true;
            }
            var user = this.getUserData();
            var userroles = user.roles;
            var result = false;
            $.each(roleArray, function (key, value) {
                $.each(userroles, function (k, v) {
                    if (v.name == value) {
                        result = true;
                        return;
                    }
                });
            });
            return result;
        },

        loadUserContext: function (userData) {
            //clear cache
            $rootScope.user = null;
            this.insertIntoContext('user', JSON.stringify(userData));
        },

        loadConfigs: function (config) {
            this.insertIntoContext('clientName', config.clientName);
            this.insertIntoContext('environment', config.environment);
            this.insertIntoContext('isLocal', config.isLocal);
            this.insertIntoContext('i18NRequired', config.i18NRequired);
            this.insertIntoContext('systeminittime', config.initTimeMillis);
            this.insertIntoContext('successMessageTimeOut', config.successMessageTimeOut);
            if (!config.clientSideLogLevel.equalsAny('warn', 'debug', 'info', 'error', 'none')) {
                //to avoid the change of server side setting it to invalid
                //TODO: config should allow list of options
                config.clientSideLogLevel = 'warn';
            }
            this.insertIntoContext('defaultlevel', config.clientSideLogLevel.toLowerCase());
        },

        getResourceUrl: function (path) {
            var baseURL = url(path);
            if (!this.isLocal()) {
                var initTime = this.getFromContext("systeminittime");
                if (baseURL.indexOf("?") == -1) {
                    return baseURL + "?" + initTime;
                }
                return baseURL + "&" + initTime;
            }
            return baseURL;
        },


        currentModule: function () {
            return this.retrieveFromContext('currentmodule');
        },

        clearContext: function () {
            $.each(sessionStorage, function (key, value) {
                if (key.startsWith('ctx_')) {
                    delete sessionStorage[key];
                }
            });
            $(hiddn_user)[0].value = null;
            $(hddn_configs)[0].value = null;
        },

        deleteFromContext: function (key) {

            delete sessionStorage["ctx_" + key];
            delete $rootScope["ctx_" + key];

        },

        insertReportSearchDTO: function (reportSchemaId, searchDTO) {
            this.insertIntoContext('repSearchDTO_' + reportSchemaId, searchDTO);
        },

        retrieveReportSearchDTO: function (reportSchemaId) {
            return this.retrieveFromContext('repSearchDTO_' + reportSchemaId);
        },

        client: function () {
            return this.retrieveFromContext('clientName');
        },

        isClient: function (name) {
            if (name == null) {
                $log.getInstance('contextService#isClient').warn("asked for null client name");
                return false;
            }
            var clientName = this.client();
            if (name == clientName) {
                return true;
            }
            if (typeof (name) === 'array') {
                if (jQuery.inArray(clientName, name) != -1) {
                    return true;
                }
            }
            return false;
        },

        setActiveTab: function (tabId) {
            this.insertIntoContext('currenttab', tabId);
        },

        getActiveTab: function () {
            return this.fetchFromContext('currenttab');
        }

    };



});


;
var app = angular.module('sw_layout');

app.factory('crudextraService', function ($http, $rootScope, printService, alertService) {

    return {

        deletefn: function (schema, datamap) {
            var idFieldName = schema.idFieldName;
            var applicationName = schema.applicationName;
            var id = datamap[idFieldName];
            alertService.confirm(applicationName, id, function () {
                var parameters = {};
                if (sessionStorage.mockmaximo == "true") {
                    parameters.mockmaximo = true;
                }
                parameters.platform = platform();
                parameters = addSchemaDataToParameters(parameters, schema);
                var deleteParams = $.param(parameters);

                var deleteURL = removeEncoding(url("/api/data/" + applicationName + "/" + id + "?" + deleteParams));
                $http.delete(deleteURL)
                    .success(function (data) {
                        $rootScope.$broadcast('sw_applicationrenderviewwithdata', data);
                    });
            });
        },

        printDetail: function (schema, datamap) {
            printService.printDetail(schema, datamap[schema.idFieldName]);
        }

    };

});


;

(function () {
    "use strict";

    //#region Service registration

    

    //#endregion

    function crudContextHolderService(contextService, schemaCacheService) {

        //#region Utils



        //TODO: continue implementing this methods, removing crud_context object references from the contextService
        // ReSharper disable once InconsistentNaming
        var _crudContext = {
            currentSchema: null,
            currentApplicationName: null,
            //TODO: below is yet to be implemented/refactored
            detail_previous: "0",
            detail_next: "0",
            list_elements: [],
            previousData: null,
            paginationData: null,
        };

        //#endregion

        //#region Public methods

        function getActiveTab() {
            return contextService.getActiveTab();
        }

        function setActiveTab(tabId) {
            contextService.setActiveTab(tabId);
        }

        function currentApplicationName() {
            return _crudContext.currentApplicationName;
        }

        function currentSchema() {
            return _crudContext.currentSchema;
        }


        function updateCrudContext(schema) {
            _crudContext = {};
            _crudContext.currentSchema = schema;
            _crudContext.currentApplicationName = schema.applicationName;
            schemaCacheService.addSchemaToCache(schema);
        }



        //#endregion

        //#region Service Instance

        var service = {
            getActiveTab: getActiveTab,
            setActiveTab: setActiveTab,
            currentSchema: currentSchema,
            currentApplicationName: currentApplicationName,
            updateCrudContext: updateCrudContext,
        };

        return service;

        //#endregion
    }

    angular.module("sw_layout").factory("crudContextHolderService", ["contextService", "schemaCacheService", crudContextHolderService]);

})();;
(function (angular) {
    "use strict";


    detailService.$inject = ["$log", "$q", "$timeout", "$rootScope", "associationService", "compositionService", "fieldService", "schemaService", "contextService"];

    angular.module("sw_layout").factory("detailService", detailService);

    function detailService($log, $q, $timeout, $rootScope, associationService, compositionService, fieldService, schemaService, contextService) {

        var api = {
            fetchRelationshipData: fetchRelationshipData,
            isEditDetail: isEditDetail
        };

        return api;

        function fetchRelationshipData(scope, result) {


            var associationPromise = handleAssociations(scope, result);
            var compositionPromise = handleCompositions(scope, result);
            $q.all([associationPromise, compositionPromise]).then(function (results) {
                //ready to listen for dirty watchers
                $log.get("detailService#fetchRelationshipData").info("associations and compositions fetched");
                scope.$broadcast("sw_configuredirtywatcher");
            });
        };

        function isEditDetail(schema, datamap) {
            return fieldService.getId(datamap, schema) != undefined;
        };

        function handleAssociations(scope, result) {
            var shouldFetchAssociations = !result.allassociationsFetched;

            //some associations might already been retrieved
            associationService.updateAssociationOptionsRetrievedFromServer(scope, result.associationOptions, scope.datamap.fields);
            associationService.restorePreviousValues(scope, result.associationOptions, scope.datamap.fields);

            if (shouldFetchAssociations) {
                return $timeout(function () {
                    //why this timeout?
                    $log.get("#detailService#fetchRelationshipData").info('fetching eager associations of {0}'.format(scope.schema.applicationName));
                    associationService.getEagerAssociations(scope, { avoidspin: true });

                });
            } else {
                //they are all resolved already
                contextService.insertIntoContext("associationsresolved", true, true);

                return $q.when();
            }
        }

        function handleCompositions(scope, result) {
            var datamap = scope.datamap.fields;
            var schema = scope.schema;
            var isEdit = isEditDetail(schema, datamap);

            if (!isEdit) {
                return $q.when();
            }

            //fetch composition data only for edit mode
            var shouldFetchCompositions = !schemaService.isPropertyTrue(result.schema, "detail.prefetchcompositions");

            if (!shouldFetchCompositions) {
                scope.compositions = result.compositions;
            }

            return $timeout(function () {
                if (shouldFetchCompositions) {
                    $log.get("#detailService#fetchRelationshipData").info('fetching compositions of {0}'.format(scope.schema.applicationName));
                    return compositionService.populateWithCompositionData(scope.schema, scope.datamap.fields);
                }
                return $q.when();
            });

        }

    };





})(angular);



;
/*
 */
var app = angular.module('sw_layout');

app.factory('dispatcherService', function ($injector, $log) {
    var loadService = function (service, method) {
        var log = $log.getInstance('dispatcherService#loadService');

        if (service === undefined || method === undefined) {
            return null;
        }

        var dispatcher = $injector.get(service);
        if (!dispatcher) {
            log.warn('Service {0} missing '.format(service));
            return null;
        }
        var fn = dispatcher[method];
        if (!fn) {
            log.warn('Method {0} not found on service {1}'.format(method, service));
            return null;
        }
        return fn;
    };
    return {
        loadService: function (service, method) {
            return loadService(service, method);
        }
    };

});

;
var app = angular.module('sw_layout');

//var PRINTMODAL_$_KEY = '[data-class="printModal"]';


app.factory('excelService', function ($rootScope, $http, $timeout, $log, tabsService, fixHeaderService,
    i18NService,userService,
    redirectService, searchService, contextService, fileService, alertService,restService) {

    function needsRegionSelection(mode) {
        if (mode != "assetlistreport") {
            return false;
        }
        var isInAllAccessModules = contextService.InModule(["tom", "itom", "purchase", "assetcontrol"]);
        if (isInAllAccessModules) {
            return true;
        }
        

        var isXitc = contextService.InModule(["xitc"]);
        return isXitc && userService.InGroup("C-HLC-WW-AR-WW");
    }

    function regionSelectionRequired(searchData,region) {
        var hasSearchInUrl = sessionStorage.swGlobalRedirectURL && sessionStorage.swGlobalRedirectURL.indexOf("searchValues")!=-1;
        return isObjectEmpty(searchData) && !region && !hasSearchInUrl;
    }


    return {
        showModalExportToExcel: function (fn, schema, searchData, searchSort, searchOperator, paginationData) {
            //TODO: fix this crap
            var modalTitle = i18NService.get18nValue('_exportotoexcel.export', 'Export') + " " + schema.applicationName +
                " " + i18NService.get18nValue('_exportotoexcel.toexcel', 'to excel');
            var selectRegionText = i18NService.get18nValue('_exportotoexcel.selectregion', 'Select Region');

            var selectText = i18NService.get18nValue('_exportotoexcel.selectexportmode', 'Select export mode');
            var exportModeGridOnlyText = i18NService.get18nValue('_exportotoexcel.gridonly', 'Grid only');
            var exportModeAllTheColumnsText = i18NService.get18nValue('_exportotoexcel.allthecolumns', 'Asset List');
            var exportModeCategoriesText = i18NService.get18nValue('_exportotoexcel.categories', 'Categories');

            bootbox.dialog({
                message: "<form id='infos' action=''>" +
                    "<label class='control-label'>" + selectText + ":</label><br>" +
                    "<label class='control-label' for='gridonlyid'>" +
                    "<input type='radio' name='exportMode' id='gridonlyid' value='list' checked='checked' /> "
                    + exportModeGridOnlyText +
                    "</label><br>" +
                    "<label class='control-label' for='allthecolumnsid'>" +
                    "<input type='radio' name='exportMode' id='allthecolumnsid' value='assetlistreport' /> "
                    + exportModeAllTheColumnsText +
                    "</label>" +
                    "</form>",
                title: modalTitle,
                buttons: {
                    main: {
                        label: i18NService.get18nValue('_exportotoexcel.export', 'Export'),
                        className: "btn-primary",
                        callback: function (result) {
                            if (result) {
                                var exportModeSelected = $('input[name=exportMode]:checked', '#infos').val();
                                if (needsRegionSelection(exportModeSelected)) {
                                    //HAP-944
                                    handleAssetListReportRegionFilter(fn);
                                    return;
                                }

                                fn(schema, searchData, searchSort, searchOperator, paginationData, exportModeSelected);
                            }
                        }
                    },
                    cancel: {
                        label: i18NService.get18nValue('_exportotoexcel.cancel', 'Cancel'),
                        className: "btn btn-default",
                        callback: function () {
                            return null;
                        }
                    }

                },
                className: "hapag-modal-exporttoexcel"
            });

            function handleAssetListReportRegionFilter(finalCbk) {



                bootbox.dialog({
                    message: "<form id='region' action=''>" +
                        "<label class='control-label'>" + selectRegionText + ":</label><br>" +
                        "<label class='control-label' for='gridonlyid'>" +
                        "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-NAMERICA'  /> "
                            + "Region North America" +
                        "</label><br>" +
                        "<label class='control-label' for='gridonlyid'>" +
                        "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-SAMERICA' /> "
                            + "Region South America" +
                        "</label><br>" +
                        "<label class='control-label' for='gridonlyid'>" +
                        "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-EUROPE'  /> "
                            + "Region Europe" +
                        "</label><br>" +
                       "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-ASIA'  /> "
                            + "Region Asia" +
                        "</label><br>" +
                        "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-GSC'  /> "
                            + "Region GSC" +
                        "</label><br>" +
                          "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-HQ'  /> "
                            + "Region HeadQuarters" +
                        "</label><br>" +
                        "</form>",
                    title: selectRegionText,
                    buttons: {
                        main: {
                            label: i18NService.get18nValue('_exportotoexcel.export', 'Export'),
                            className: "btn-primary",
                            callback: function (result) {

                                if (result) {
                                    var region = $('input[name=region]:checked', '#region').val();
                                    if (regionSelectionRequired(searchData, region)) {
                                        alertService.alert("Please select either a Region or a filter on the grid");
                                        return;
                                    }
                                    var metadataParameter = region ? "region={0}".format(region) : null;
                                    finalCbk(schema, searchData, searchSort, searchOperator, paginationData, 'assetlistreport', metadataParameter);
                                }
                            }
                        },
                        cancel: {
                            label: i18NService.get18nValue('_exportotoexcel.cancel', 'Cancel'),
                            className: "btn btn-default",
                            callback: function () {
                                return null;
                            }
                        }

                    },
                    className: "hapag-modal-exporttoexcel"
                });
            }
        },


        exporttoexcel: function (schema, searchData, searchSort, searchOperator, paginationData, schemaSelected, parameterQuery) {
            var application = schema.applicationName;
            if (contextService.isClient("hapag") && application == "asset" && !schemaSelected) {
                var fn = this.exporttoexcel;
                this.showModalExportToExcel(fn, schema, searchData, searchSort, searchOperator, paginationData);
                return null;
            }
            var schemaToUse = schemaSelected ? schemaSelected : schema.schemaId;

            var parameters = {};
            parameters.key = {};
            parameters.key.schemaId = schemaToUse;
            parameters.key.mode = schema.mode;
            parameters.key.platform = "web";
            parameters.application = application;
            parameters.currentmodule = contextService.retrieveFromContext('currentmodule');
            parameters.currentmetadata = contextService.retrieveFromContext('currentmetadata');
            parameters.currentmetadataparameter = parameterQuery;
            var searchDTO;
            var reportDto = contextService.retrieveReportSearchDTO(schema.schemaId);
            if (reportDto != null) {
                reportDto = $.parseJSON(reportDto);
                searchDTO = searchService.buildReportSearchDTO(reportDto, searchData, searchSort, searchOperator, paginationData.filterFixedWhereClause, paginationData.unionFilterFixedWhereClause);
            } else {
                searchDTO = searchService.buildSearchDTO(searchData, searchSort, searchOperator, paginationData.filterFixedWhereClause, paginationData.unionFilterFixedWhereClause);
            }

            var currentModule = contextService.currentModule();
            if (currentModule != null) {
                parameters.module = currentModule;
            }
            searchDTO.pageNumber = 1;
            searchDTO.totalCount = paginationData.totalCount;
            searchDTO.pageSize = paginationData.pageSize;

            parameters.searchDTO = searchDTO;
            //this quick wrapper ajax call will validate if the user is still logged in or not
            restService.getPromise("ExtendedData", "PingServer").then(function() {
                fileService.download(url("/Excel/Export" + "?" + $.param(parameters)), function(html, url) {}, function(html, url) {
                    alertService.alert("Error generating the {0}.{1} report. Please contact your administrator".format(application, schemaToUse));
                });
            });

        }
    }

});

;
var app = angular.module('sw_layout');

app.factory('expressionService', function ($rootScope,contextService) {

    var preCompiledReplaceRegex = /(?:^|\W)@(\#*)(\w+)(?!\w)/g;

    return {
        getExpression: function (expression, datamap) {
            if (datamap.fields != undefined) {
                expression = expression.replace(/\@/g, 'datamap.fields.');
            } else {
                expression = expression.replace(/\@/g, 'datamap.');
            }
            expression = expression.replace(/ctx:/g, 'contextService.');
            return expression;
        },


        getVariables: function (expression) {
            var variables = expression.match(preCompiledReplaceRegex);
            if (variables != null) {
                for (var i = 0; i < variables.length; i++) {
                    variables[i] = variables[i].replace(/[\@\(\)]/g, '').trim();
                }
            }
            return variables;
        },

        getVariablesForWatch: function (expression) {
            var variables = this.getVariables(expression);
            var collWatch = '[';
            for (var i = 0; i < variables.length; i++) {
                collWatch += 'datamap.' + variables[i];
                if (i != variables.length-1) {
                    collWatch += ",";
                }
            }

            collWatch += ']';
            return collWatch;
        },


        evaluate: function (expression, datamap) {
            if (expression == "true") {
                return true;
            }
            if (expression == "false") {
                return false;
            }
            var expressionToEval = this.getExpression(expression, datamap);
            try {
                return eval(expressionToEval);
            } catch (e) {
                if ($rootScope.isLocal) {
                    console.log(e);
                }
                return true;
            }
        },


    };

});


;
var app = angular.module('sw_layout');

app.factory('fieldService', function (expressionService,$log) {

    var isFieldHidden = function (datamap, application, fieldMetadata) {
        fieldMetadata.jscache = instantiateIfUndefined(fieldMetadata.jscache);
        if (fieldMetadata.jscache.isHidden != undefined) {
            return fieldMetadata.jscache.isHidden;
        }
        var baseHidden = fieldMetadata.isHidden || (fieldMetadata.type != "ApplicationSection" &&
              (fieldMetadata.attribute == application.idFieldName && application.stereotype == "Detail"
              && application.mode == "input" && !fieldMetadata.isReadOnly));
        var isTabComposition = fieldMetadata.type == "ApplicationCompositionDefinition" && !fieldMetadata.inline;
        if (baseHidden || isTabComposition) {
            fieldMetadata.jscache.isHidden = true;
            return true;
        } else if (fieldMetadata.type == "ApplicationSection" && fieldMetadata.resourcepath == null &&
            (fieldMetadata.displayables.length == 0 ||
             $.grep(fieldMetadata.displayables, function (e) {
                  return !isFieldHidden(datamap, application, e);
        }).length == 0)) {

            //            fieldMetadata.jscache.isHidden = true;
            return true;
        }
        //the opposite of the expression: showexpression --> hidden
        var result = !expressionService.evaluate(fieldMetadata.showExpression, datamap);
        if (application.stereotype == "List") {
            //list schemas can be safely cached since if the header is visible the rest would be as well
            fieldMetadata.jscache.isHidden = result;
        }
        return !expressionService.evaluate(fieldMetadata.showExpression, datamap);
    };


    return {
        isFieldHidden: function (datamap, application, fieldMetadata) {
            return isFieldHidden(datamap, application, fieldMetadata);
        },

        isFieldRequired: function (fieldMetadata, datamap) {
            if (fieldMetadata.type === "ApplicationSection" && fieldMetadata.parameters) {
                return "true" === fieldMetadata.parameters["required"];
            }
            var requiredExpression = fieldMetadata.requiredExpression;
            if (requiredExpression != undefined) {
                return expressionService.evaluate(requiredExpression, datamap);
            }
            return requiredExpression;
        },

        nonTabFields: function (displayables) {
            var result = [];
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                var type = displayable.type;
                var isTabComposition = this.isTabComposition(displayable);
                if (!isTabComposition && type != "ApplicationTabDefinition") {
                    result.push(displayable);
                }
            }
            return result;
        },

        isTabComposition: function (displayable) {
            var type = displayable.type;
            return type == "ApplicationCompositionDefinition" && !displayable.inline;
        },

        isTab: function (displayable) {
            if (displayable == null) {
                return;
            }
            else {
                var type = displayable.type;
                return type == "ApplicationTabDefinition";
            }
        },

        getDisplayableByKey: function (schema, key) {
            schema.jscache = instantiateIfUndefined(schema.jscache);
            schema.jscache.fieldsByKey = instantiateIfUndefined(schema.jscache.fieldsByKey);
            if (schema.jscache.fieldsByKey[key] != undefined) {
                return schema.jscache.fieldsByKey[key];
            }

            var displayables = schema.displayables;
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                if ((displayable.attribute && displayable.attribute == key) || (displayable.tabId && displayable.tabId == key)) {
                    schema.jscache.fieldsByKey[key] = displayable;
                    return displayable;
                }
                if (displayable.displayables != undefined) {
                    var innerDisplayables = this.getDisplayableByKey(displayable, key);
                    if (innerDisplayables != undefined && ((innerDisplayables.attribute && innerDisplayables.attribute == key) || (innerDisplayables.tabId && innerDisplayables.tabId == key))) {
                        schema.jscache.fieldsByKey[key] = innerDisplayables;
                        return innerDisplayables;
                    }
                }
            }
            return null;
        },

        getDisplayablesByAssociationKey: function (schema, associationKey) {
            schema.jscache = instantiateIfUndefined(schema.jscache);
            var cacheEntry = schema.jscache.displayablesByAssociation = instantiateIfUndefined(schema.jscache.displayablesByAssociation);
            if (cacheEntry[associationKey] != undefined) {
                return cacheEntry[associationKey];
            }

            var result = [];
            var fn = this;
            $.each(schema.displayables, function (index, value) {
                if (value.associationKey == associationKey) {
                    result.push(value);
                } else if (value.displayables != undefined) {
                    var innerDisplayables = fn.getDisplayablesByAssociationKey(value, associationKey);
                    if (innerDisplayables != null) {
                        result = result.concat(innerDisplayables);
                    }
                }
            });
            cacheEntry[associationKey] = result;
            return result;
        },

        countVisibleDisplayables: function (datamap, application, displayables) {
            var count = 0;
            for (var i = 0; i < displayables.length; i++) {
                if (!this.isFieldHidden(datamap, application, displayables[i])) {
                    count++;
                }
            }
            return count;
        },

        getId: function (datamap, schema) {
            return datamap[schema.idFieldName];
        },

        getDisplayablesOfTypes: function (displayables, types) {
            var result = [];
            var fn = this;
            $.each(displayables, function (key, value) {
                var type = value.type;
                if ($.inArray(type, types) != -1) {
                    result.push(value);
                }
                if (value.displayables != undefined) {
                    var innerDisplayables = fn.getDisplayablesOfTypes(value.displayables, types);
                    result = result.concat(innerDisplayables);
                }
            });

            return result;
        },

        getDisplayablesOfRendererTypes: function (displayables, types) {
            var result = [];
            var fn = this;
            $.each(displayables, function (key, value) {
                var type = value.rendererType;
                if ($.inArray(type, types) != -1) {
                    result.push(value);
                }
                if (value.displayables != undefined) {
                    result = result.concat(fn.getDisplayablesOfRendererTypes(value.displayables, types));
                }
            });

            return result;

        },

        getFilterDisplayables: function (displayables) {
            var result = [];
            var fn = this;
            $.each(displayables, function (key, value) {
                if (value.filter != null && value.filter.operation != null) {
                    result.push(value);
                }
                if (value.displayables != undefined) {
                    result = result.concat(fn.getFilterDisplayables(value.displayables));
                }
            });

            return result;

        },

        ///return if a field which is not on screen (but is not a hidden instance), and whose value is null from the datamap, avoiding sending useless (and wrong) data
        isNullInvisible: function (displayable, datamap) {
            if (displayable.showExpression == undefined || displayable.showExpression == "true" || displayable.isHidden) {
                return false;
            }
            return !expressionService.evaluate(displayable.showExpression, datamap);
        }
    };

});


;
/*Used to download files via a child class of FileDownloadController instead of the wondow.local reload. 
*This gives us access to the callbacks for success and failure.
*/
var app = angular.module('sw_layout');

app.factory('fileService', function ($rootScope, contextService) {

    return {
        download: function (url, successCallback, failCallback) {
            //needed since this is non-ajax call
            url = removeEncoding(url);

            //this is for emulating the busy cursor, since this is not an ajax call
            $rootScope.$broadcast('sw_ajaxinit');


            $.fileDownload(url, {
                successCallback: function (html, url) {
                    //this is for removing the busy cursor
                    $rootScope.$broadcast('sw_ajaxend');
                    if (successCallback) {
                        successCallback(html, url);
                    }
                },
                failCallback: function (html, url,formURL) {
                    //this is for removing the busy cursor
                    $rootScope.$broadcast('sw_ajaxend');
                    if (failCallback) {
                        if (formURL.indexOf("SignIn") != -1) {
                            sessionStorage.removeItem("swGlobalRedirectURL");
                            contextService.clearContext();
                            //this means that the server wanted a redirection to login page (302), due to session expiration.
                            //Since we´re using an inner iframe the contents of the signin might not be show. Let´s redirect manually, and there´s no way to override that.
                            window.location.reload();
                        } else {
                            failCallback(html, url);
                        }
                        
                    }
                }
            });
        }
    }
});;
var app = angular.module('sw_layout');

app.factory('fixHeaderService', function ($rootScope, $log, $timeout, contextService, fieldService) {

    var addClassErrorMessageListHander = function (showerrormessage) {
        var affixpaginationid = $("#affixpagination");
        var listgridtheadid = $("#listgridthread");
        var listgridid = $("#listgrid");
        var paginationerrormessageclass = "pagination-errormessage";
        var listtheaderrormessageclass = "listgrid-thead-errormessage";
        var listgriderrormessageclass = "listgrid-table-errormessage";

        if (showerrormessage) {
            affixpaginationid.addClass(paginationerrormessageclass);
            listgridtheadid.addClass(listtheaderrormessageclass);
            listgridid.addClass(listgriderrormessageclass);
        } else {
            affixpaginationid.removeClass(paginationerrormessageclass);
            listgridtheadid.removeClass(listtheaderrormessageclass);
            listgridid.removeClass(listgriderrormessageclass);
        }
    };

    var addClassSuccessMessageListHander = function (showerrormessage) {
        var affixpaginationid = $("#affixpagination");
        var listgridtheadid = $("#listgridthread");
        var listgridid = $("#listgrid");

        var paginationsuccessrmessageclass = "pagination-successmessage";
        var listtheadsuccessmessageclass = "listgrid-thead-successmessage";
        var listgridsuccessmessageclass = "listgrid-table-successmessage";
        var listgridtheadreset = "listgrid-thead-reset";
        var listgridtablereset = "listgrid-table-reset";

        var listtheadsuccessmessageieclass = "listgrid-thead-successmessage-ie";
        var listgridsuccessmessageieclass = "listgrid-table-successmessage-ie";

        if (showerrormessage) {
            affixpaginationid.addClass(paginationsuccessrmessageclass);
            if (isIe9() && $rootScope.clientName == 'hapag') {
                listgridtheadid.addClass(listtheadsuccessmessageieclass);
                listgridid.addClass(listgridsuccessmessageieclass);
            } else {
                listgridtheadid.removeClass(listgridtheadreset);
                listgridtheadid.addClass(listtheadsuccessmessageclass);
                listgridid.removeClass(listgridtablereset);
                listgridid.addClass(listgridsuccessmessageclass);
            }
        } else {
            affixpaginationid.removeClass(paginationsuccessrmessageclass);
            if (isIe9() && $rootScope.clientName == 'hapag') {
                listgridtheadid.removeClass(listtheadsuccessmessageieclass);
                listgridid.removeClass(listgridsuccessmessageieclass);
            } else {
                listgridtheadid.removeClass(listtheadsuccessmessageclass);
                listgridtheadid.addClass(listgridtheadreset);
                listgridid.removeClass(listgridsuccessmessageclass);
                listgridid.addClass(listgridtablereset);
            }
        }
    };

    var topMessageAddClass = function (div) {
        div.addClass("affix-thead");
        div.addClass("topMessageAux");
    };

    var topMessageRemoveClass = function (div) {
        div.removeClass("affix-thead");
        div.removeClass("topMessageAux");
        $('html, body').animate({ scrollTop: 0 }, 'fast');
    };

    var buildTheadArray = function (log, table, emptyGrid) {
        var thead = [];
        //if the grid is empty, let´s just keep the th array
        var classToUse = emptyGrid ? 'thead tr:eq(0) th' : 'tbody tr:eq(0) td';

        $(classToUse, table).each(function (i, firstrowIterator) {
            var firstTd = $(firstrowIterator);
            if (!(firstTd.css("display") == "none")) {
                //let´s push only the visible entries
                var width = firstTd.width();
                thead.push(width);
            } else {
                thead.push(0);
            }
        });
        log.trace('thead array: ' + thead);
        var total = 0;
        for (var i = 0; i < thead.length; i++) {
            total += thead[i] << 0;
        }
        log.trace('total ' + total);
        return thead;
    }

    return {

        updateFilterZeroOrOneEntries: function () {
            /// <summary>
            /// 
            /// </summary>
            this.fixThead(null, { empty: true });
        },



        updateFilterVisibility: function (schema, theadArray) {
            /// <summary>
            ///  updates the fiter visibility for the grid, adjusting input layouts properly.
            /// </summary>
            /// <param name="schema">the schema is used to determine when we should show the advanced allowtoggle options</param>
            //update which filter row will be displayed and input text width
            var table = $(".listgrid-table");
            var showAdvancedFilter = false;

            if (schema != null) {
                var estimatedNeededSize = schema.displayables.length * 150;
                var allowtoggle = "true" == schema.properties['list.advancedfilter.allowtoggle'];
                showAdvancedFilter = allowtoggle && estimatedNeededSize > table.width();
            }

            if (showAdvancedFilter) {
                $('.hidden-too-much-columns', table).hide();
                $('.hidden-few-columns', table).show();
                return;
            }


            if (!isIe9() || !theadArray) {
                return;
            }


            //                        thead tr:eq(1) th ==> picks all the elements of the first line of the thead of the table, i.e the filters
            $('thead tr:eq(1) th', table).each(function (i, v) {
                var trWidth = theadArray[i];
                if (trWidth == 0) {
                    //hidden fields
                    return;
                }

                var inputGroupElements = $('.input-group', v).children();
                //filtering only the inputs (ignoring divs...)
                var addonWidth = 0;
                for (var j = 1; j < inputGroupElements.length; j++) {
                    //sums both the filter input + the filter button
                    addonWidth += inputGroupElements.eq(j).outerWidth();
                }

                if (addonWidth == 0) {
                    //there´s no filter to update
                    return;
                }


                //first element will be the filter input itself
                var input = inputGroupElements.eq(0);
                var width = input.width();
                var inputPaddingAndBorder = input.outerWidth() - width;
                
                var resultValue = trWidth - addonWidth - inputPaddingAndBorder;
                $log.getInstance('fixheaderService#updateFilterVisibility').debug("result:{0} | Previous:{1} | tr:{2} | addon:{3} | Padding{4}".format(resultValue, width, trWidth, addonWidth, inputPaddingAndBorder));
                input.width(resultValue);
            });
        },


        fixTableTop: function (tableElement, params) {
            var thead = $('thead', tableElement);
            params = instantiateIfUndefined(params);
            $(".listgrid-table").addClass("affixed");
            $(".listgrid-table").removeClass("unfixed");
            var theadHeight = thead.height();
            $log.getInstance("fixheaderService#fixTableTop").debug("head height: " + theadHeight);
            if (isIe9() && "true" != sessionStorage.mockie9) {
                //if mocking ie9, lets keep default behaviour, otherwise will break all the grids
                tableElement.css('margin-top', theadHeight + 19);
                thead.css('top', 111 - (theadHeight + 19));
            } else {
                tableElement.css('margin-top', theadHeight + 23);
                thead.css('top', 114);
            }
        },

        fixThead: function (schema, params) {
            var log = $log.getInstance('fixheaderService#fixThead');
            log.debug('starting fix Thead');

            if (!params || !params.resizing) {
                this.unfix();
            }
            var table = $(".listgrid-table");
            var thead = buildTheadArray(log, table, params.empty);

            $('thead tr:eq(0) th', table).each(function (i, v) {
                $(v).width(thead[i]);
            });
            $('thead tr:eq(1) th', table).each(function (i, v) {
                $(v).width(thead[i]);
            });

            // set the columns width back
            $('tbody tr:eq(0) td', table).each(function (i, v) {
                $(v).width(thead[i]);
            });

            log.debug('updating filter visibility');
            this.updateFilterVisibility(schema, thead);
            contextService.insertIntoContext('currentgridarray', thead);
            log.debug('updated filter visibility');

            //update the style, to fixed
            this.fixTableTop(table, params);

            //hack to fix HAP-610 T-ITOM-015
            $('#pagesize').width($('#pagesize').width());

            log.debug('finishing fix Thead');
        },

        activateResizeHandler: function () {
            var resolutionBarrier = 1200;
            var width = $(window).width();
            var highResolution = width >= resolutionBarrier;
            var fn = this;
            $(window).resize(function () {
                var newWidth = $(this).width();

                //SM - HAP-393, resize regardless of screen size
                //var isNewHighResolution = newWidth > resolutionBarrier + 15; // lets add some margin to give the browser time to render the new table...
                //var isNewLowResolution = newWidth < resolutionBarrier - 15; // lets add some margin to give the browser time to render the new table...
                //if ((isNewHighResolution && !highResolution) || (isNewLowResolution && highResolution)) {
                $log.getInstance("fixheaderService#resize").debug('switching resolutions');
                fn.fixThead(null, {
                    resizing: true
                });
                //width = newWidth;
                //highResolution = width >= resolutionBarrier;
                //}
            });
        },

        FixHeader: function () {
            var table;
            var originalOffset;
            $(window).scroll(function () {
                if (table == null) {
                    table = $("#listgrid");
                    originalOffset = $("thead", table).top;
                }
                var windowTop = $(window).scrollTop();
                $("thead", table).css("top", windowTop + originalOffset);
            });
        },

        unfix: function () {
            var log = $log.getInstance('fixheaderService#unfix');
            log.debug('unfix started');
            var table = $(".listgrid-table");
            table.removeClass("affixed");
            table.addClass("unfixed");
            $('[rel=tooltip]').tooltip('hide');
            log.debug('unfix finished');
        },

        fixSuccessMessageTop: function (isList) {
            if (isList) {
                addClassSuccessMessageListHander(true);
            }
        },

        topErrorMessageHandler: function (show, isDetail, schema) {
            if (show) {
                if (!isDetail) {
                    addClassErrorMessageListHander(true);
                    $rootScope.hasErrorList = true;
                } else {
                    $rootScope.hasErrorDetail = true;
                }
            } else {
                addClassErrorMessageListHander(false);
            }
        },

        resetTableConfig: function (schema) {
            if ($(".listgrid-table").position() != undefined) {
                addClassSuccessMessageListHander(false);
                if (!nullOrUndef(schema)) {
                    var params = {
                    };
                    this.fixThead(schema, params);
                }
                $(window).trigger('resize');
            }
        }
    };

});


// In chrome you have to fire the window.location.reload event to fire a print event when a socket event is in progress.. 
// http://stackoverflow.com/questions/18622626/chrome-window-print-print-dialogue-opens-only-after-page-reload-javascript
//if (navigator.userAgent.toLowerCase().indexOf('chrome') > -1) {
//                    window.location.reload();
//};
var app = angular.module('sw_layout');

app.factory('formatService', function ($filter, i18NService) {

    var doFormatDate = function (value, dateFormat, forceConversion) {
        if (value == null) {
            return null;
        }

        if (value === "@currentdatetime" || value === "@currentdate") {
            return $filter('date')(new Date(), dateFormat);
        }

        if (forceConversion) {
            //this would be needed for applying the time formats
            var date = Date.parse(value);
            if (date== null || isNaN(date)) {
                return $filter('date')(value, dateFormat);
            } else {
                return $filter('date')(date, dateFormat);
            }
        }

        try {
            return $filter('date')(value, dateFormat);
        } catch (e) {
            value = new Date(value);
            return $filter('date')(value, dateFormat);
        }
    };

    var descriptionDataHandler = function (value, column) {
        if (value == null) {
            return "";
        }
        try {
            var unformatteddate = value.split('Logged Date:')[1];
            if (!nullOrUndef(unformatteddate)) {
                var unformatteddatenospaces = unformatteddate.replace(/ /g, '');
                var dateFormat = column.rendererParameters['format'];
                var formatteddate = doFormatDate(unformatteddatenospaces, dateFormat, false);
                value = value.replace(unformatteddatenospaces, formatteddate);
            }
        } catch (e) {
            return value;
        }
        return value;
    };

    return {
        format: function (value, column) {
            if (column == undefined) {
                return value;
            }
            var dateFormat;
            if (column.rendererType == "datetime") {
                if (column.rendererParameters['format'] != null && value != null) {
                    dateFormat = column.rendererParameters['format'];
                    return doFormatDate(value, dateFormat, false);
                }
            } else if (column.type == "ApplicationSection" && column.parameters['format']) {
                if (column.parameters['format'] != null && value != null) {
                    dateFormat = column.parameters['format'];
                    return doFormatDate(value, dateFormat, false);
                }
            } else if (column.rendererParameters != undefined && column.rendererParameters['formatter'] != null) {
                if (column.rendererParameters['formatter'] == 'numberToBoolean') {
                    value = value == 1 ? i18NService.get18nValue('general.yes', 'Yes') : i18NService.get18nValue('general.no', 'No');
                }
                else if (column.rendererParameters['formatter'] == 'doubleToTime') {
                    if (value == null) {
                        return "";
                    }
                    //Converting to hh:mm
                    var time = value.toString();
                    if (time.length > 0 && time.indexOf('.') == -1) {
                        return value + " : 00";
                    }
                    var hours = time.split('.')[0];
                    var tempMins = time.split('.')[1];
                    hours = parseInt(hours);
                    var mins = Math.round(Math.round(parseFloat(0 + '.' + tempMins) * 60 * 100) / 100);
                    return (hours < 10 ? "0" + hours : hours) + " : " + (mins < 10 ? "0" + mins : "" + mins);
                }
                else if (column.rendererParameters['formatter'] == 'descriptionDataHandler') {
                    return descriptionDataHandler(value, column);
                }
            }

            return value;
        },

        formatDate: function (value, dateFormat) {
            return doFormatDate(value, dateFormat, true);
        },

        adjustDateFormatForPicker: function (dateFormat, showTime) {
            if (dateFormat == undefined || dateFormat == '') {
                //default ==> should be client specific
                return showTime ? "yyyy-MM-dd hh:ii" : "yyyy-MM-dd";
            } else {
                dateFormat = dateFormat.replace('mm', 'ii');
                dateFormat = dateFormat.replace('MM', 'mm');
                dateFormat = dateFormat.replace('HH', 'hh');
                if (!showTime) {
                    //the format and the showtime flag are somehow conflitant, let´s adjust the format
                    dateFormat = dateFormat.replace('hh:ii', '');
                }
                return dateFormat.trim();
            }
        }


    };

});;
var app = angular.module('sw_layout');

app.factory('i18NService', function ($rootScope, contextService) {

    var verifyKeyInAllCatalogsButEn = function (key) {
        var catalogs = $rootScope['sw_i18ncatalogs'];
        var all = true;
        $.each(catalogs, function (language, catalog) {
            if (language == 'en') {
                return;
            }
            all = all && hasKey(key, catalog);
        });
        return all;
    }

    var hasKey = function (key, catalog) {
        var catalogValue = null;
        if (catalog != null) {
            catalogValue = JsonProperty(catalog, key);
        }
        return catalogValue != null;
    };

    var doGetValue = function (key, defaultValue, isMenu, languageToForce) {
        if (!languageToForce && !nullOrUndef(contextService.retrieveFromContext('currentmodule')) && !isMenu) {
            return defaultValue;
        }
        var catalog = $rootScope['sw_currentcatalog'];
        if (languageToForce && languageToForce != '') {
            catalog = $rootScope['sw_i18ncatalogs'][languageToForce];
        } 
        var catalogValue = null;
        if (catalog != null) {
            catalogValue = JsonProperty(catalog, key);
        }
        if (catalogValue != null) {
            return catalogValue;
        }
        if ($rootScope.isLocal && $rootScope.i18NRequired == true) {
            if (defaultValue == null) {
                return null;
            }

            if (sessionStorage['ignorei18n'] == undefined && !verifyKeyInAllCatalogsButEn(key)) {
                return "??" + defaultValue + "??";
            }
        }
        return defaultValue;
    };

    var valueConsideringSchemas = function (value, schema) {
        if (value == undefined) {
            return null;
        }

        if (typeof value == "string") {
            //single option goes here
            return value;
        }
        //but we can have multiple definitions, one for each schema...
        if (value.hasOwnProperty(schema.schemaId)) {
            return value[schema.schemaId];
        }
        //default declaration
        return value["_"];
    };

    //??
    function fillattr(fieldMetadata) {
        var attr;
        if (fieldMetadata.attribute == null && fieldMetadata.label != undefined) {
            attr = fieldMetadata.label.replace(":", "");
        } else {
            attr = fieldMetadata.attribute;
        }
        return attr;
    }

    return {

        getI18nLabel: function (fieldMetadata, schema) {
            if (fieldMetadata.type == "ApplicationCompositionDefinition" || fieldMetadata.type == "ApplicationSection") {
                var headerLabel = this.getI18nSectionHeaderLabel(fieldMetadata, fieldMetadata.header, schema);
                if (headerLabel != null && headerLabel != "") {
                    return headerLabel;
                }
            }
            var applicationName = schema.applicationName;

            var attr = fillattr(fieldMetadata);
            var key = applicationName + "." + attr;
            if (fieldMetadata.type == "OptionField") {
                key += "._label";
            }
            var value = doGetValue(key, fieldMetadata.label);
            return valueConsideringSchemas(value, schema);
        },

        getI18nLabelTooltip: function (fieldMetadata, schema) {
            var applicationName = schema.applicationName;
            var attr = fillattr(fieldMetadata);
            var key = applicationName + "." + attr + "._tooltip";
            if (!hasKey(key, $rootScope['sw_currentcatalog'])) {
                //fallbacks to default label strategy
                key = applicationName + "." + attr;
                if (fieldMetadata.type == "OptionField") {
                    key += "._label";
                }
            }
            var defaultValue = fieldMetadata.toolTip;
            if (defaultValue == undefined) {
                defaultValue = fieldMetadata.label;
            }
            var value = doGetValue(key, defaultValue);
            return valueConsideringSchemas(value, schema);
        },


        getI18nOptionField: function (option, fieldMetadata, schema) {
            if (fieldMetadata.type != "OptionField" || fieldMetadata.providerAttribute != null) {
                //case there´s a providerattribute, 118N makes no sense
                return option.label;
            }
            var applicationName = schema.applicationName;
            var attr = fieldMetadata.attribute;
            var val = option.value == '' ? option.label : option.value;
            var key = applicationName + "." + attr + "." + val;
            var value = doGetValue(key, option.label);
            return valueConsideringSchemas(value, schema);
        },

        getI18nCommandLabel: function (command, schema) {
            var applicationName = schema.applicationName;
            var key = applicationName + "._commands." + command.id;
            var value = doGetValue(key, command.label);
            return valueConsideringSchemas(value, schema);
        },

        getI18nMenuLabel: function (menuitem, tooltip) {
            if (nullOrUndef(menuitem.id)) {
                return tooltip ? menuitem.tooltip : menuitem.title;
            }
            var defaultValue = menuitem.title;
            var key = "_menu." + menuitem.id;
            if (tooltip) {
                key += "_tooltip";
                defaultValue = menuitem.tooltip;
            }
            return doGetValue(key, defaultValue, true);
        },

        getI18nTitle: function (schema) {
            var applicationName = schema.applicationName;
            var key = applicationName + "._title." + schema.schemaId;
            return doGetValue(key, schema.title);
        },

        get18nValue: function (key, defaultValue, paramArray, languageToForce) {
            var isHeaderMenu = (key.indexOf("_headermenu") > -1) ? true : false;
            var unformatted = doGetValue(key, defaultValue, isHeaderMenu, languageToForce);
            if (paramArray == undefined) {
                return unformatted;
            }
            var formatFn = unformatted.format;
            return formatFn.apply(unformatted, paramArray);
        },

        getI18nSectionHeaderLabel: function (section, header, schema) {
            if (header == undefined) {
                return "";
            }
            var applicationName = schema.applicationName;
            section = !nullOrUndef(section.id) ? section.id : section.relationship;
            var key = applicationName + "." + section + "._header";
            var value = doGetValue(key, header.label);
            return valueConsideringSchemas(value, schema);
        },

        getTabLabel: function (tab, schema) {
            var applicationName = tab.applicationName;
            var key = applicationName + "." + tab.id + "._title";
            var value = doGetValue(key, tab.label);
            return valueConsideringSchemas(value, schema);
        },

        load: function (jsonString, language) {
            var languages = JSON.parse(jsonString);
            var catalogs = {};
            $.each(languages, function (key, value) {
                catalogs[key] = value;
            });
            $rootScope['sw_i18ncatalogs'] = catalogs;
            this.changeCurrentLanguage(language);
        },

        getCurrentLanguage: function () {
            return $rootScope['sw_userlanguage'];
        },

        changeCurrentLanguage: function (language) {
            if (language == null) {
                language = "en";
            }
            $rootScope['sw_userlanguage'] = language;
            $rootScope['sw_currentcatalog'] = $rootScope['sw_i18ncatalogs'][language.toLowerCase()];
            //broadcast language changed event to update filter label translations.
            $rootScope.$broadcast("sw_languageChanged", language.toLowerCase());
        }


    };

});


;
(function () {
    /* Start angularLocalStorage */
    'use strict';
    var app = angular.module('sw_layout');
    //var angularLocalStorage = angular.module('LocalStorageModule', []);

    app.provider('localStorageService', function () {

        // You should set a prefix to avoid overwriting any local storage variables from the rest of your app
        // e.g. localStorageServiceProvider.setPrefix('youAppName');
        // With provider you can use config as this:
        // myApp.config(function (localStorageServiceProvider) {
        //    localStorageServiceProvider.prefix = 'yourAppName';
        // });
        this.prefix = 'ls';

        // You could change web storage type localstorage or sessionStorage
        this.storageType = 'localStorage';

        // Cookie options (usually in case of fallback)
        // expiry = Number of days before cookies expire // 0 = Does not expire
        // path = The web path the cookie represents
        this.cookie = {
            expiry: 30,
            path: '/'
        };

        // Send signals for each of the following actions?
        this.notify = {
            setItem: true,
            removeItem: false
        };

        // Setter for the prefix
        this.setPrefix = function (prefix) {
            this.prefix = prefix;
        };

        // Setter for the storageType
        this.setStorageType = function (storageType) {
            this.storageType = storageType;
        };

        // Setter for cookie config
        this.setStorageCookie = function (exp, path) {
            this.cookie = {
                expiry: exp,
                path: path
            };
        };

        // Setter for cookie domain
        this.setStorageCookieDomain = function (domain) {
            this.cookie.domain = domain;
        };

        // Setter for notification config
        // itemSet & itemRemove should be booleans
        this.setNotify = function (itemSet, itemRemove) {
            this.notify = {
                setItem: itemSet,
                removeItem: itemRemove
            };
        };



        this.$get = ['$rootScope', '$window', '$document', function ($rootScope, $window, $document) {

            var prefix = this.prefix;
            var cookie = this.cookie;
            var notify = this.notify;
            var storageType = this.storageType;
            var webStorage;

            // When Angular's $document is not available
            if (!$document) {
                $document = document;
            } else if ($document[0]) {
                $document = $document[0];
            }

            // If there is a prefix set in the config lets use that with an appended period for readability
            if (prefix.substr(-1) !== '.') {
                prefix = !!prefix ? prefix + '.' : '';
            }
            var deriveQualifiedKey = function (key) {
                return prefix + key;
            }
            // Checks the browser to see if local storage is supported
            var browserSupportsLocalStorage = (function () {
                try {
                    var supported = (storageType in $window && $window[storageType] !== null);

                    // When Safari (OS X or iOS) is in private browsing mode, it appears as though localStorage
                    // is available, but trying to call .setItem throws an exception.
                    //
                    // "QUOTA_EXCEEDED_ERR: DOM Exception 22: An attempt was made to add something to storage
                    // that exceeded the quota."
                    var key = deriveQualifiedKey('__' + Math.round(Math.random() * 1e7));
                    if (supported) {
                        webStorage = $window[storageType];
                        webStorage.setItem(key, '');
                        webStorage.removeItem(key);
                    }

                    return supported;
                } catch (e) {
                    storageType = 'cookie';
                    $rootScope.$broadcast('LocalStorageModule.notification.error', e.message);
                    return false;
                }
            }());



            // Directly adds a value to local storage
            // If local storage is not available in the browser use cookies
            // Example use: localStorageService.add('library','angular');
            var addToLocalStorage = function (key, value) {

                // If this browser does not support local storage use cookies
                if (!browserSupportsLocalStorage || this.storageType === 'cookie') {
                    $rootScope.$broadcast('LocalStorageModule.notification.warning', 'LOCAL_STORAGE_NOT_SUPPORTED');
                    if (notify.setItem) {
                        $rootScope.$broadcast('LocalStorageModule.notification.setitem', { key: key, newvalue: value, storageType: 'cookie' });
                    }
                    return addToCookies(key, value);
                }

                // Let's convert undefined values to null to get the value consistent
                if (typeof value === "undefined") {
                    value = null;
                }

                try {
                    if (angular.isObject(value) || angular.isArray(value)) {
                        value = angular.toJson(value);
                    }
                    if (webStorage) { webStorage.setItem(deriveQualifiedKey(key), value) };
                    if (notify.setItem) {
                        $rootScope.$broadcast('LocalStorageModule.notification.setitem', { key: key, newvalue: value, storageType: this.storageType });
                    }
                } catch (e) {
                    $rootScope.$broadcast('LocalStorageModule.notification.error', e.message);
                    return addToCookies(key, value);
                }
                return true;
            };

            // Directly get a value from local storage
            // Example use: localStorageService.get('library'); // returns 'angular'
            var getFromLocalStorage = function (key) {

                if (!browserSupportsLocalStorage || this.storageType === 'cookie') {
                    $rootScope.$broadcast('LocalStorageModule.notification.warning', 'LOCAL_STORAGE_NOT_SUPPORTED');
                    return getFromCookies(key);
                }

                var item = webStorage ? webStorage.getItem(deriveQualifiedKey(key)) : null;
                // angular.toJson will convert null to 'null', so a proper conversion is needed
                // FIXME not a perfect solution, since a valid 'null' string can't be stored
                if (!item || item === 'null') {
                    return null;
                }

                if (item.charAt(0) === "{" || item.charAt(0) === "[") {
                    return angular.fromJson(item);
                }

                return item;
            };

            // Remove an item from local storage
            // Example use: localStorageService.remove('library'); // removes the key/value pair of library='angular'
            var removeFromLocalStorage = function (key) {
                if (!browserSupportsLocalStorage) {
                    $rootScope.$broadcast('LocalStorageModule.notification.warning', 'LOCAL_STORAGE_NOT_SUPPORTED');
                    if (notify.removeItem) {
                        $rootScope.$broadcast('LocalStorageModule.notification.removeitem', { key: key, storageType: 'cookie' });
                    }
                    return removeFromCookies(key);
                }

                try {
                    webStorage.removeItem(deriveQualifiedKey(key));
                    if (notify.removeItem) {
                        $rootScope.$broadcast('LocalStorageModule.notification.removeitem', { key: key, storageType: this.storageType });
                    }
                } catch (e) {
                    $rootScope.$broadcast('LocalStorageModule.notification.error', e.message);
                    return removeFromCookies(key);
                }
                return true;
            };

            // Return array of keys for local storage
            // Example use: var keys = localStorageService.keys()
            var getKeysForLocalStorage = function () {

                if (!browserSupportsLocalStorage) {
                    $rootScope.$broadcast('LocalStorageModule.notification.warning', 'LOCAL_STORAGE_NOT_SUPPORTED');
                    return false;
                }

                var prefixLength = prefix.length;
                var keys = [];
                for (var key in webStorage) {
                    // Only return keys that are for this app
                    if (key.substr(0, prefixLength) === prefix) {
                        try {
                            keys.push(key.substr(prefixLength));
                        } catch (e) {
                            $rootScope.$broadcast('LocalStorageModule.notification.error', e.Description);
                            return [];
                        }
                    }
                }
                return keys;
            };

            // Remove all data for this app from local storage
            // Also optionally takes a regular expression string and removes the matching key-value pairs
            // Example use: localStorageService.clearAll();
            // Should be used mostly for development purposes
            var clearAllFromLocalStorage = function (regularExpression) {

                regularExpression = regularExpression || "";
                //accounting for the '.' in the prefix when creating a regex
                var tempPrefix = prefix.slice(0, -1);
                var testRegex = new RegExp(tempPrefix + '.' + regularExpression);

                if (!browserSupportsLocalStorage) {
                    $rootScope.$broadcast('LocalStorageModule.notification.warning', 'LOCAL_STORAGE_NOT_SUPPORTED');
                    return clearAllFromCookies();
                }

                var prefixLength = prefix.length;

                for (var key in webStorage) {
                    // Only remove items that are for this app and match the regular expression
                    if (testRegex.test(key)) {
                        try {
                            removeFromLocalStorage(key.substr(prefixLength));
                        } catch (e) {
                            $rootScope.$broadcast('LocalStorageModule.notification.error', e.message);
                            return clearAllFromCookies();
                        }
                    }
                }
                return true;
            };

            // Checks the browser to see if cookies are supported
            var browserSupportsCookies = function () {
                try {
                    return navigator.cookieEnabled ||
                      ("cookie" in $document && ($document.cookie.length > 0 ||
                      ($document.cookie = "test").indexOf.call($document.cookie, "test") > -1));
                } catch (e) {
                    $rootScope.$broadcast('LocalStorageModule.notification.error', e.message);
                    return false;
                }
            };

            // Directly adds a value to cookies
            // Typically used as a fallback is local storage is not available in the browser
            // Example use: localStorageService.cookie.add('library','angular');
            var addToCookies = function (key, value) {

                if (typeof value === "undefined") {
                    return false;
                }

                if (!browserSupportsCookies()) {
                    $rootScope.$broadcast('LocalStorageModule.notification.error', 'COOKIES_NOT_SUPPORTED');
                    return false;
                }

                try {
                    var expiry = '',
                        expiryDate = new Date(),
                        cookieDomain = '';

                    if (value === null) {
                        // Mark that the cookie has expired one day ago
                        expiryDate.setTime(expiryDate.getTime() + (-1 * 24 * 60 * 60 * 1000));
                        expiry = "; expires=" + expiryDate.toGMTString();
                        value = '';
                    } else if (cookie.expiry !== 0) {
                        expiryDate.setTime(expiryDate.getTime() + (cookie.expiry * 24 * 60 * 60 * 1000));
                        expiry = "; expires=" + expiryDate.toGMTString();
                    }
                    if (!!key) {
                        var cookiePath = "; path=" + cookie.path;
                        if (cookie.domain) {
                            cookieDomain = "; domain=" + cookie.domain;
                        }
                        $document.cookie = deriveQualifiedKey(key) + "=" + encodeURIComponent(value) + expiry + cookiePath + cookieDomain;
                    }
                } catch (e) {
                    $rootScope.$broadcast('LocalStorageModule.notification.error', e.message);
                    return false;
                }
                return true;
            };

            // Directly get a value from a cookie
            // Example use: localStorageService.cookie.get('library'); // returns 'angular'
            var getFromCookies = function (key) {
                if (!browserSupportsCookies()) {
                    $rootScope.$broadcast('LocalStorageModule.notification.error', 'COOKIES_NOT_SUPPORTED');
                    return false;
                }

                var cookies = $document.cookie && $document.cookie.split(';') || [];
                for (var i = 0; i < cookies.length; i++) {
                    var thisCookie = cookies[i];
                    while (thisCookie.charAt(0) === ' ') {
                        thisCookie = thisCookie.substring(1, thisCookie.length);
                    }
                    if (thisCookie.indexOf(deriveQualifiedKey(key) + '=') === 0) {
                        return decodeURIComponent(thisCookie.substring(prefix.length + key.length + 1, thisCookie.length));
                    }
                }
                return null;
            };

            var removeFromCookies = function (key) {
                addToCookies(key, null);
            };

            var clearAllFromCookies = function () {
                var thisCookie = null, thisKey = null;
                var prefixLength = prefix.length;
                var cookies = $document.cookie.split(';');
                for (var i = 0; i < cookies.length; i++) {
                    thisCookie = cookies[i];

                    while (thisCookie.charAt(0) === ' ') {
                        thisCookie = thisCookie.substring(1, thisCookie.length);
                    }

                    var key = thisCookie.substring(prefixLength, thisCookie.indexOf('='));
                    removeFromCookies(key);
                }
            };

            var getStorageType = function () {
                return storageType;
            };

            var bindToScope = function (scope, key, def) {
                var value = getFromLocalStorage(key);

                if (value === null && angular.isDefined(def)) {
                    value = def;
                } else if (angular.isObject(value) && angular.isObject(def)) {
                    value = angular.extend(def, value);
                }

                scope[key] = value;

                scope.$watchCollection(key, function (newVal) {
                    addToLocalStorage(key, newVal);
                });
            };

            return {
                isSupported: browserSupportsLocalStorage,
                getStorageType: getStorageType,
                set: addToLocalStorage,
                add: addToLocalStorage, //DEPRECATED
                get: getFromLocalStorage,
                keys: getKeysForLocalStorage,
                remove: removeFromLocalStorage,
                clearAll: clearAllFromLocalStorage,
                bind: bindToScope,
                deriveKey: deriveQualifiedKey,
                cookie: {
                    set: addToCookies,
                    add: addToCookies, //DEPRECATED
                    get: getFromCookies,
                    remove: removeFromCookies,
                    clearAll: clearAllFromCookies
                }
            };
        }];
    });
}).call(this);

;
var app = angular.module('sw_layout');

app.factory('logoutService', function (contextService, i18NService, $window) {

    return {

        logout: function (mode) {
            if (contextService.getUserData() == null) {
                //this can only be achived if the user hits a back browser button after a logout suceeded
                $window.location.href = url('/SignOut/SignOut');
                return;
            }

            var title = "manual"== mode ? "Logout!" : "Automatic Logout!";
            var message =  "manual" == mode ? "You have successfully logged out of ServiceIT.</br>To completely logout, please close your browser. To log back into ServiceIT, click the button provided below." : ' Auf Grund zu langer Inaktivität wurden Sie aus Sicherheitsgründen automatisch ausgeloggt <br\> You have been automatically logged out due to inactivity. <br\> Usted ha sido cerrada automáticamente debido a la inactividad.';

            bootbox.dialog({
                message: message,
                title: title,
                className: 'logoutmodal',
                closeButton: false,
                buttons: {
                    success: {
                        label: "Close Browser",
                        className: "commandButton btn loginbutton",
                        callback: function () {
                            sessionStorage.removeItem("swGlobalRedirectURL");
                            contextService.clearContext();

                            $window.location.href = isIe() ? url('/SignOut/SignOut') : url('/SignOut/SignOutClosePage');
                            window.open('', '_self', '');
                            window.close();
                        }
                    },
                    danger: {
                        label: "Login to ServiceIT",
                        className: "commandButton btn loginbutton",
                        callback: function () {
                            sessionStorage.removeItem("swGlobalRedirectURL");
                            contextService.clearContext();
                            $window.location.href = url('/SignOut/SignOut');
                        }
                    },
                }
            });
        },
     
    };
});


;
var app = angular.module('sw_layout');

app.factory('menuService', function ($rootScope, redirectService, contextService, i18NService, $log) {

    var cleanSelectedLeaf = function () {
        var menu = $("#applicationmenu");

        $("button", menu).removeClass("selected");
        $("a", menu).removeClass("selected");
    }

    var toggleSelectedLeaf = function (leaf) {

        // look for parent container of the new active menu item and its button
        var parentMenuContainer = $(leaf).parents('.dropdown-container').last();
        var menuContainerToggle = $('button', parentMenuContainer).first();

        if (menuContainerToggle.length == 0) {
            $(leaf).addClass("selected");
        } else {
            menuContainerToggle.addClass("selected");
        }
    }

    var locateLeafById = function (leafs, id) {

        if (id == null) {
            throw new Error("id cannot be null");
        }

        if (leafs == null) {
            return null;
        }

        for (var i = 0; i < leafs.length; i++) {
            var leaf = leafs[i];
            if (id == leaf.id) {
                return leaf;
            }
            if (leaf.type == "MenuContainerDefinition") {
                var result = locateLeafById(leaf.leafs, id);
                if (result != null) {
                    return result;
                }
            }
        }
        return null;
    };

    var searchMenuLeafByUrl = function (leafs, url) {

        var leafUrl;

        if (leafs != null) {
            for (var i = 0; i < leafs.length; i++) {

                if (leafs[i].type == "ApplicationMenuItemDefinition") {
                    leafUrl = redirectService.getApplicationUrl(leafs[i].application, leafs[i].schema, leafs[i].mode, i18NService.getI18nMenuLabel(leafs[i].title, false));
                } else if (leafs[i].type == "ActionMenuItemDefinition") {
                    leafUrl = redirectService.getActionUrl(leafs[i].title, leafs[i].controller, leafs[i].action, leafs[i].parameters);
                } else if (leafs[i].type == "MenuContainerDefinition") {
                    var leaf = searchMenuLeafByUrl(leafs[i].leafs, url);
                    if (leaf != null) {
                        return leaf;
                    }
                }

                if (leafUrl != null && decodeURI(leafUrl) == decodeURI(url)) {
                    return leafs[i];
                }
            }
        }
    };

    return {



        executeById: function (menuId) {
            var leafs = $rootScope.menu.leafs;
            var leaf = locateLeafById(leafs, menuId);
            if (leaf.type == "ApplicationMenuItemDefinition") {
                this.goToApplication(leaf, null);
            } else if (leaf.type == "ActionMenuItemDefinition") {
                this.doAction(leaf, null);
            }
        },


        doAction: function (leaf, target) {
            if (target != undefined) {
                this.setActiveLeaf(target);
            }
            contextService.insertIntoContext('currentgridarray', null);
            if (leaf.parameters && leaf.parameters.popupmode == "browser") {
                //HAP-813 --> since we´ll open that into a new browser window, let´s make sure we don´t change the main window module
                contextService.insertIntoContext("currentmodulenewwindow", leaf.module);
            } else {
                contextService.insertIntoContext("currentmodule", leaf.module);
                contextService.insertIntoContext('currentmetadata', null);
            }
            $log.getInstance('sw4.menu').info("current module: " + leaf.module);

            var title = ''; /* 'New ' +  leaf.parameters['application'] + " - " + leaf.title;*/
            redirectService.redirectToAction(title, leaf.controller, leaf.action, leaf.parameters, leaf.target);
        },

        goToApplication: function (leaf, target) {

            $rootScope.$broadcast('sw_checksuccessmessage', leaf);
            if (target != undefined) {
                this.setActiveLeaf(target);
            }
            $log.getInstance('sw4.menu').info("current module: " + leaf.module);
            
            contextService.insertIntoContext('currentgridarray', null);

            if (leaf.parameters && leaf.parameters.popupmode == "browser") {
                //HAP-813 --> since we´ll open that into a new browser window, let´s make sure we don´t change the main window module
                contextService.insertIntoContext("currentmodulenewwindow", leaf.module);
            } else {
                contextService.insertIntoContext("currentmodule", leaf.module);
                contextService.insertIntoContext('currentmetadata', null);
            }


            var parameters = {};

            if (leaf.parameters != null) {
                parameters.popupmode = leaf.parameters['popupmode'];
            }

            redirectService.goToApplicationView(leaf.application, leaf.schema, leaf.mode, this.getI18nMenuLabel(leaf, null), parameters);
        },


        adjustHeight: function (callbackFunction) {
            var menu = $("#applicationmenu");
            if (($rootScope.clientName != undefined && $rootScope.clientName == 'hapag') || menu.data('displacement') != 'vertical') {
                return;
            }
            var bodyHeight = $(".hapag-body").height();
            menu.children().first().css('min-height', bodyHeight + 4);
        },

        setActiveLeaf: function (leaf) {
            var menu = $("#applicationmenu");
            if (menu.data('displacement') == 'horizontal') {
                cleanSelectedLeaf();
                if ($(leaf).parents('#applicationmenu').length > 0) {
                    toggleSelectedLeaf(leaf);
                }
            }
        },

        getI18nMenuLabel: function (menuItem, tooltip) {

            if (menuItem.module != null) {
                return menuItem.title;
            }
            return i18NService.getI18nMenuLabel(menuItem, tooltip);
        },

        setActiveLeafByUrl: function (menu, url) {
            if (menu.displacement == 'horizontal') {
                var leaf = searchMenuLeafByUrl(menu.leafs, url);

                if (leaf != null) {
                    var leafId = leaf.type + '_' + leaf.id;
                    var menuItem = $('#' + leafId);

                    if (menuItem.length > 0) {
                        cleanSelectedLeaf();
                        toggleSelectedLeaf(menuItem);
                    }
                }
            }
        }
    };

});


;
var app = angular.module('sw_layout');

app.factory('mockService', function (contextService) {

    return {

        //avoids opening dashboard upon container click
        isMockedContainerDashBoard: function () {
            return contextService.isLocal() && sessionStorage.mockdash == "true";
        },

        isMockMaximo: function () {
            return contextService.isLocal() && sessionStorage.mockmaximo == "true";
        },


    };

});


;
var app = angular.module('sw_layout');

//var PRINTMODAL_$_KEY = '[data-class="printModal"]';

app.factory('printService', function ($rootScope, $http, $timeout,$log, tabsService, fixHeaderService, redirectService, searchService) {

   var mergeCompositionData = function (datamap, nonExpansibleData, expansibleData) {
        var resultObj = {};
        if (expansibleData != null) {
            resultObj = expansibleData;
        }
        if (nonExpansibleData == undefined) {
            return resultObj;
        }
        for (var i = 0; i < nonExpansibleData.length; i++) {
            var value = nonExpansibleData[i];
            if (value.tabObject != undefined) {
                //this would happen in case of a tab being printed wherease we don´t have the fields on datamap
                resultObj[value.key] = value.tabObject;
            } else {
                resultObj[value.key] = datamap.fields[value.key];
            }
            
        }
        return resultObj;
   };

    var doPrintGrid = function(schema) {
        fixHeaderService.unfix();
        innerDoPrint();
        fixHeaderService.fixThead(schema,{empty:false});
    };

    var innerDoPrint = function() {
        window.print();
    };

    return {
        doPrint: function() {
            innerDoPrint();
        },

        printList: function (totalCount, printPageSize, gridRefreshFunction, schema) {
            var log = $log.getInstance('printService#printList');
            $rootScope.printRequested = false;
            if ($rootScope.$$listeners['listTableRenderedEvent'] != undefined) {
                //avoids multiple listener registration
                $rootScope.$$listeners['listTableRenderedEvent'] = null;
            }

            // to add the crud_body grid in printing page
            var listgrid = $('#listgrid');
            listgrid.removeClass('hiddenonprint');

            $rootScope.$on('listTableRenderedEvent', function () {
                if (!$rootScope.printRequested) {
                    return;
                }
                $rootScope.printRequested = false;
                var rows = $('[rel=hideRow]');
                var index;
                if (rows.length <= printPageSize) {
                    return;
                }
                for (index = 1; index < rows.length; ++index) {
                    if (index > printPageSize) {
                        rows[index].className += ' hideRows';
                    }
                }                
                if (navigator.userAgent.toLowerCase().indexOf('firefox') > -1) {
                    listgrid.addClass('listgrid-firefox');
                }
                doPrintGrid(schema);
            });

            if (totalCount <= printPageSize) {
                //If all the data is on the screen, just print it
                doPrintGrid(schema);
                return;
            }

            //otherwise, hit the server asking for the full grid to be print
            var invokeObj = {
                pageNumber: 1,
                pageSize: totalCount,
                printMode: true,
            };

            log.info('fetching print data on server');
            gridRefreshFunction(invokeObj);
            
        },

        printDetail: function (schema, datamap, printOptions) {
            var log = $log.getInstance("print_service#printDetail");
            if (schema.hasNonInlineComposition && printOptions === undefined) {
                //this case, we have to choose which extra compositions to choose, so we will open the print modal
                //open print modal...
                $rootScope.$broadcast("sw_showprintmodal", schema);
                return;
            }
            var params = {};
            params.key = {};

            params.options = {};

            params.application = schema.applicationName;
            params.key.schemaId = schema.schemaId;
            params.key.mode = schema.mode;
            params.key.platform = platform();

            var notExpansibleCompositions = [];
            var expansibleCompositions = {};
            if (printOptions != null) {
                params.options.compositionsToExpand = tabsService.buildCompositionsToExpand(printOptions.compositionsToExpand, schema, datamap, 'print', notExpansibleCompositions,true);
            }
            params.options.printMode = true;
            var shouldPageBreak = printOptions == undefined ? true : printOptions.shouldPageBreak;
            var shouldPrintMain = printOptions == undefined ? true : printOptions.shouldPrintMain;

                var emptyCompositions = {};
            //TODO: check whether printOptions might or not be null
            if (printOptions != null) {
                $.each(printOptions.compositionsToExpand, function (key, obj) {
                    if (obj.value == true) {
                        var compositionData = datamap.fields[key];
                            //TODO: CompositionData might be undefined or not
                            if (compositionData == undefined || compositionData.length == 0) {
                            emptyCompositions[key] = [];
                        }
                    }
                });
            }
            if (params.options.compositionsToExpand == undefined || params.options.compositionsToExpand == "") {
                //no need to hit the server, just print the main detail
                log.debug('sw_readytoprintevent dispatched');
                $rootScope.$broadcast("sw_readytoprintevent", mergeCompositionData(datamap, notExpansibleCompositions, emptyCompositions), shouldPageBreak, shouldPrintMain);
                return;
            }

            log.info('calling expanding compositions on service; params: {0}'.format(params));
            var urlToInvoke = removeEncoding(url("/api/generic/ExtendedData/ExpandCompositions?" + $.param(params)));
            $http.get(urlToInvoke, { printMode: true }).success(function (result) {

                var compositions = result.resultObject;
                $.each(emptyCompositions, function (key, obj) {
                    compositions[key] = obj;
                });
                
                log.debug('sw_readytoprintevent dispatched after server return');
                var compositionsToPrint = mergeCompositionData(datamap, notExpansibleCompositions, compositions);
                $rootScope.$broadcast("sw_readytoprintevent", compositionsToPrint, shouldPageBreak, shouldPrintMain);
            });
        },
        
        printDetailedList: function (schema, datamap,searchSort, printOptions) {

            searchSort = searchSort || {};
            var printSchema = schema.printSchema != null ? schema.printSchema : schema;

            if (printSchema.hasNonInlineComposition && printOptions === undefined) {
                //this case, we have to choose which extra compositions to choose, so we will open the print modal
                //open print modal...
                $rootScope.$broadcast("sw_showprintmodal", printSchema, searchSort);
                return;
            }


            var ids = [];
            
            $.grep(datamap, function (element) {
                if (element.fields['checked'] == true) {
                    ids.push(element.fields[printSchema.idFieldName]);
                }
            });
                        
            var applicationName = printSchema.name;
            var printSchemaId = printSchema.schemaId;
            var parameters = {};
            var searchData = {};
            var searchOperator = {};

            searchData[printSchema.idFieldName] =  ids.join(',');
            searchOperator[printSchema.idFieldName] = searchService.getSearchOperationById('EQ');

            parameters.searchDTO = searchService.buildSearchDTO(searchData, searchSort, searchOperator, {});
            parameters.searchDTO.pageSize = ids.length;

            parameters.searchDTO.compositionsToFetch = [];
            var compositionsToFetch = {};
            $.each(printOptions.compositionsToExpand, function (key, obj) {
                if (obj.value == true) {
                    parameters.searchDTO.compositionsToFetch.push(key);
                    compositionsToFetch[key] = obj;
                }
            });

            parameters.printmode = true;
            
            var getDetailsUrl = redirectService.getApplicationUrl(applicationName, printSchemaId, '', '', parameters);

            var shouldPageBreak = printOptions == undefined ? true : printOptions.shouldPageBreak;
            var shouldPrintMain = printOptions == undefined ? true : printOptions.shouldPrintMain;

            $http.get(getDetailsUrl).success(function (data) {
                $rootScope.$broadcast("sw_readytoprintdetailedlistevent", data.resultObject, compositionsToFetch, shouldPageBreak, shouldPrintMain);
            });

            // to remove the crud_body grid from printing page
            $('#listgrid').addClass('hiddenonprint');
        },


        hidePrintModal: function () {
            $rootScope.$broadcast("sw_hideprintmodal");
        }
    };

});


;
var app = angular.module('sw_layout');

app.factory('redirectService', function ($http, $rootScope, $log, contextService, fixHeaderService, restService) {

    var adjustCurrentModuleForNewWindow = function (currentModule) {
        var currentModuleNewWindow = contextService.retrieveFromContext('currentmodulenewwindow');
        if (currentModuleNewWindow != "null" && currentModuleNewWindow != "") {
            //this currentmodulenewwindow is used to avoid that the module of the menu changes if a leaf on another module context that opens a browser gets clicked
            //HAP-813
            currentModule = currentModuleNewWindow;
            if (currentModule == null) {
                currentModule = "null";
            }
            contextService.deleteFromContext('currentmodulenewwindow');
        }
        return currentModule;
    };

    var buildApplicationURLForBrowser = function (applicationName, parameters) {
        var crudUrl = $(routes_homeurl)[0].value;
        var currentModule = contextService.retrieveFromContext('currentmodule');
        currentModule = adjustCurrentModuleForNewWindow(currentModule);
        var currentMetadata = parameters.currentmetadata = "null";
        parameters.currentmodule = currentModule;
        var params = $.param(parameters);
        params = replaceAll(params, "=", "$");
        params = replaceAll(params, "&", "@");
        crudUrl = crudUrl + "?application=" + applicationName + "&popupmode=browser";
        if (!nullOrUndef(currentModule)) {
            crudUrl += "&currentModule=" + currentModule;
        }
        if (!nullOrUndef(currentMetadata)) {
            crudUrl += "&currentMetadata=" + currentMetadata;
        }
        crudUrl = crudUrl + "&querystring=" + params;
        return removeEncoding(crudUrl);
    };

    var buildActionURLForBrowser = function (controller, action, parameters) {
        var crudUrl = $(routes_homeurl)[0].value;
        var currentModule = contextService.retrieveFromContext('currentmodule');
        var currentMetadata = parameters.currentmetadata = "null";
        currentModule = adjustCurrentModuleForNewWindow(currentModule);
        parameters.currentmodule = currentModule;
        parameters.currentmetadata = currentMetadata;
        var params = $.param(parameters);
        params = replaceAll(params, "=", "$");
        params = replaceAll(params, "&", "@");
        crudUrl = crudUrl + "?controllerToRedirect=" + controller + "&actionToRedirect=" + action + "&popupmode=browser";
        if (!nullOrUndef(currentModule)) {
            crudUrl += "&currentModule=" + currentModule;
        }
        if (!nullOrUndef(currentMetadata)) {
            crudUrl += "&currentMetadata=" + currentMetadata;
        }
        crudUrl = crudUrl + "&querystring=" + params;
        return removeEncoding(crudUrl);
    };

    return {

        getActionUrl: function (controller, action, parameters) {
            if (parameters.popupmode == "browser") {
                return buildActionURLForBrowser(controller, action, parameters);
            } else {
                action = (action === undefined || action == null) ? 'get' : action;
                var params = parameters == null ? {} : parameters;
                return url("/api/generic/" + controller + "/" + action + "?" + $.param(params));
            }
        },

        redirectToAction: function (title, controller, action, parameters, target) {
            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            if (title != null) {
                parameters.title = title;
            }

            if (target == 'new') {
                var redirectURL = url(controller + "/" + action + "?" + $.param(parameters));
                var w = window.open(redirectURL);
                w.moveTo(0, 0);
            } else {
                var redirectURL = this.getActionUrl(controller, action, parameters);

                if (parameters.popupmode == "browser") {
                    if ($rootScope.isLocal && "true" != sessionStorage.defaultpopupsize) {
                        //easier to debug on chrome like this
                        var w = window.open(redirectURL);
                        //                    w.moveto(0, 0);
                    } else {
                        openPopup(redirectURL);
                    }
                    return;
                }

                sessionStorage.swGlobalRedirectURL = redirectURL;
                $http.get(redirectURL).success(
                    function (data) {
                        $rootScope.$broadcast("sw_redirectactionsuccess", data);
                    }).error(
                    function (data) {
                        var errordata = {
                            errorMessage: "error opening action {0} of controller {1} ".format(action, controller),
                            errorStack: data.message
                        }
                        $rootScope.$broadcast("sw_ajaxerror", errordata);
                    });
            }
        },

        getApplicationUrl: function (applicationName, schemaId, mode, title, parameters, jsonData) {
            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            parameters.key = {
                schemaId: schemaId,
                mode: mode,
                platform: platform()
            };


            if (parameters.popupmode == "browser") {
                return buildApplicationURLForBrowser(applicationName, parameters);
            } else {
                if (title != null && title.trim() != "") {
                    parameters.title = title;
                }
                if (jsonData == undefined) {
                    return url("/api/data/" + applicationName + "?" + $.param(parameters));
                } else {
                    parameters.application = applicationName;
                    if (title != null && title.trim() != "") {
                        parameters.title = title;
                    }
                    return url("/api/generic/ExtendedData/OpenDetailWithInitialData" + "?" + $.param(parameters));
                }
            }
        },

        goToApplicationView: function (applicationName, schemaId, mode, title, parameters, jsonData) {
            var log = $log.getInstance('redirectService#goToApplication');

            if (parameters === undefined || parameters == null) {
                parameters = {};
            }
            $rootScope.$broadcast('sw_applicationredirected', parameters);

            var redirectURL = this.getApplicationUrl(applicationName, schemaId, mode, title, parameters, jsonData);
            var popupMode = parameters.popupmode;
            if (popupMode == "report") {
                //does not popup any window for incident detail report
                //TODO: is this really necessary?
                return;
            }

            if (popupMode == "browser") {
                restService.getPromise("ExtendedData", "PingServer").then(function () {
                    if ($rootScope.isLocal && "true" != sessionStorage.defaultpopupsize) {
                        //easier to debug on chrome like this
                        var w = window.open(redirectURL);
                        //                    w.moveto(0, 0);
                    } else {
                        openPopup(redirectURL);
                    }
                });
                return;
            }

            //this code will get called when the user is already on a crud page and tries to switch view only.
            $rootScope.popupmode = popupMode;
            fixHeaderService.unfix();
            if (jsonData == undefined) {
                sessionStorage.swGlobalRedirectURL = redirectURL;

                log.info('invoking get on datacontroller for {0}'.format(applicationName));
                $http.get(redirectURL).success(function (data) {
                    $rootScope.$broadcast("sw_redirectapplicationsuccess", data, mode, applicationName);
                });
            } else {
                var jsonString = angular.toJson(jsonData);
                log.info('invoking post on datacontroller for {0} | content: '.format(applicationName, jsonString));
                $http.post(redirectURL, jsonString).success(function (data) {
                    $rootScope.$broadcast("sw_redirectapplicationsuccess", data, mode, applicationName);
                });
            }
        },

        redirectToTab: function (tabId) {
            var tab = $('a[href="#' + tabId + '"]');
            tab.trigger('click');
        },


        redirectNewWindow: function (newWindowURL, needReload, initialData) {
            restService.getPromise("ExtendedData", "PingServer").then(function () {
                if ($rootScope.isLocal) {
                    //easier to debug on chrome like this
                    var w = window.open(newWindowURL);
                    w.moveTo(0, 0);
                    return;
                }

                var cbk = function (view) {
                    var x = openPopup('');

                    x.document.open();
                    x.document.write(view);
                    x.document.close();

                    if (needReload == true) {
                        x.location.reload();
                    }
                    x.focus();
                    if ($rootScope.isLocal) {
                        x.moveTo(0, 0);
                    }
                };
                if (initialData == undefined) {
                    $http.post(newWindowURL).success(cbk);
                } else {
                    var jsonString = angular.toJson(initialData);
                    $http.post(newWindowURL, jsonString).success(cbk);
                }
            });
        }
    };

});

function openPopup(redirectURL) {
    var width = 1024;
    var height = 768;

    var x = screen.width / 2 - width / 2;
    var y = screen.height / 2 - height / 2;

    var w = window.open(redirectURL, '_blank', 'height=' + height + 'px,width=' + width + 'px,left=' + x + ',top=' + y + ',resizable=yes,scrollbars=yes', false);
    w.focus();

    return w;
}
;
angular.module('sw_layout').factory('restService', function ($http, $log, contextService) {


    return {




        getActionUrl: function (controller, action, parameters) {
            action = (action === undefined || action == null) ? 'get' : action;
            var params = parameters == null ? {} : parameters;
            var serverUrl = contextService.getFromContext("serverurl");
            if (serverUrl) {
                return serverUrl + "/api/generic/" + controller + "/" + action + "?" + $.param(params, true);
            }
            return url("/api/generic/" + controller + "/" + action + "?" + $.param(params, true));
        },

        invokePost: function (controller, action, queryParameters, json, successCbk, failureCbk) {
            this.postPromise(controller, action, queryParameters, json)
                .success(function (data) {
                    if (successCbk != null) {
                        successCbk(data);
                    }
                })
                .error(function (data) {
                    if (failureCbk != null) {
                        failureCbk(data);
                    }
                });
        },

        postPromise: function (controller, action, queryParameters, json) {
            var url = this.getActionUrl(controller, action, queryParameters);
            var log = $log.getInstance("restService#invokePost");
            log.info("invoking post on url {0}".format(url));
            return $http.post(url, json);
        },

        getPromise: function (controller, action, queryParameters) {
            var url = this.getActionUrl(controller, action, queryParameters);
            var log = $log.getInstance("restService#invokeGet");
            log.info("invoking get on url {0}".format(url));
            return $http.get(url);
        },

        invokeGet: function (controller, action, queryParameters, successCbk, failureCbk) {
            var url = this.getActionUrl(controller, action, queryParameters);
            var log = $log.getInstance("restService#invokeGet");
            log.info("invoking get on url {0}".format(url));
            var getPromise = this.getPromise(controller, action, queryParameters);
            getPromise
                .success(function (data) {
                    if (successCbk != null) {
                        successCbk(data);
                    }
                })
                .error(function (data) {
                    if (failureCbk != null) {
                        failureCbk(data);
                    }
                });
        }




    };

});


;

(function () {
    "use strict";

    //#region Service registration



    //#endregion

    function schemaCacheService($log, contextService) {

        //#region Utils

        var schemaCache = {

        }

        function restore() {
            var urlContext = url("");
            var schemaCacheJson = sessionStorage[urlContext + ":schemaCache"];
            if (schemaCacheJson) {
                schemaCache = JSON.parse(schemaCacheJson);
            }
        }

        restore();

        //#endregion

        //#region Public methods


        function getSchemaCacheKeys() {
            var result = ";";
            for (var key in schemaCache) {
                if (!schemaCache.hasOwnProperty(key)) {
                    continue;
                }
                result += key + ";";
            }
            $log.get("schemaCacheService#getSchemaCacheKeys").debug("schema keys in cache {0}".format(result));
            return result;
        }

        function getSchemaFromResult(result) {
            if (result.cachedSchemaId) {
                $log.get("schemaCacheService#getSchemaFromResult").info("schema {0}.{1} retrieved from cache".format(result.applicationName, result.cachedSchemaId));
                return this.getCachedSchema(result.applicationName, result.cachedSchemaId);
            }
            return result.schema;

        }

        function getCachedSchema(applicationName, schemaId) {
            return schemaCache[applicationName + "." + schemaId];
        }

        function addSchemaToCache(schema) {
            if (schema == null) {
                return;
            }
            schemaCache = schemaCache || {};
            var schemaKey = schema.applicationName + "." + schema.schemaId;
            if (!schemaCache[schemaKey]) {
                var systeminitMillis = contextService.getFromContext("systeminittime");
                $log.get("schemaCacheService#addSchemaToCache").info("adding schema {0} retrieved to cache".format(schemaKey));
                schemaCache[schemaKey] = schema;
                schemaCache.systeminitMillis = systeminitMillis;
                var urlContext = url("");
                sessionStorage[urlContext + ":schemaCache"] = JSON.stringify(schemaCache);
            }
        }

        function wipeSchemaCacheIfNeeded(forceClean) {
            var systeminitMillis = contextService.getFromContext("systeminittime");
            if (forceClean || (schemaCache && schemaCache.systeminitMillis !== systeminitMillis)) {
                $log.get("schemaCacheService#wipeSchemaCacheIfNeeded").info("wiping out schema cache");
                delete sessionStorage["schemaCache"];
                schemaCache = {
                    systeminitMillis: systeminitMillis
                };
            }
        }

        //#endregion

        //#region Service Instance

        var service = {
            getSchemaCacheKeys: getSchemaCacheKeys,
            addSchemaToCache: addSchemaToCache,
            getCachedSchema: getCachedSchema,
            getSchemaFromResult: getSchemaFromResult,
            wipeSchemaCacheIfNeeded: wipeSchemaCacheIfNeeded
        };

        return service;

        //#endregion
    }
    angular.module("sw_layout").factory("schemaCacheService", ["$log", "contextService", schemaCacheService]);


})();;
var app = angular.module('sw_layout');

app.factory('schemaService', function () {

 

    return {
        parseAppAndSchema: function (schemaKey) {            
            var keys = schemaKey.split('.');
            if (keys.length == 1) {
                //in this case we are passing only the schemaId  
                return { app: null, schemaId: schemaKey, mode: null };
            }
            var mode = null;
            var application = keys[0];
            var schemaId = keys[1];
            if (keys.length == 3) {
                 mode = keys[2];
            }
            return { app: application, schemaId: schemaId, mode: mode };
        },

        getProperty: function (schema, propertyName) {
            if (!schema) {
                return false;
            }
            schema.properties = schema.properties || {};
            return schema.properties[propertyName];
        },


        isPropertyTrue: function (schema, propertyName) {
            if (!schema) {
                return false;
            }
            return schema.properties && "true" == schema.properties[propertyName];
        },

        getId: function (datamap, schema) {
            if (datamap.fields) {
                return datamap.fields[schema.idFieldName];
            }
            return datamap[schema.idFieldName];
        },

        nonTabFields: function (schema) {
            return fieldService.nonTabFields(schema.displayables, true);
        },

        
    };

});


;
var app = angular.module('sw_layout');

app.factory('screenshotService', function ($rootScope, $timeout, i18NService, $log) {

    return {

        init: function (bodyElement, datamap) {
            $log.getInstance('screenshotservice#init').debug('init screenshot service');
            var fn = this;
            // Configure image holders
            $('.image-holder', bodyElement).bind('paste', function (event) {
                fn.handleImgHolderPaste(this, event.originalEvent);
            });
            $('.image-holder', bodyElement).bind('blur', function (event) {
                fn.handleImgHolderBlur(this, datamap);
            });
            $('.richtextbox', bodyElement).parents('form:first').bind('submit', function (event) {
                fn.handleRichTextBoxSubmit($(this), event.originalEvent);
            });
        },

        hasScreenshotData: function () {
            $('.richtextbox', form).each(function () {
                return this.contentWindow.asciiData() != null && this.contentWindow.asciiData() != "";
            });
        },

        handleRichTextBoxSubmit: function (form, event) {

            $('.richtextbox', form).each(function () {
                //this is for ie9
                if (this.contentWindow.asciiData() == null || this.contentWindow.asciiData() == "") {
                    //if nothing pasted, do nothing
                    return;
                }

                var rtbAttribute = this.id.substring('richTextBox_'.length);
                var log = $log.getInstance('screenshotservice#ie9conversion');

                var t0 = performance.now();
                var binaryData = this.contentWindow.binaryData();
                var t1 = performance.now();
                log.debug("get binary data took {0} ms".format(t1 - t0));

                form.append("<input type='hidden' name='" + rtbAttribute + "' value='" + binaryData + "' />");
                var now = new Date();
                var timestamp = '' + now.getFullYear() + (now.getMonth() + 1) + now.getDate();
                form.append("<input type='hidden' name='" + rtbAttribute + "_path' value='" + "Screen" + timestamp + ".rtf" + "' />");
            });
        },


        handleImgHolderBlur: function (imgHolder, datamap) {
            var t0 = performance.now();
            var imgAttribute = imgHolder.id.substring('imgHolder_'.length);
            datamap[imgAttribute] = Base64.encode(imgHolder.innerHTML);
            var t1 = performance.now();
            $log.getInstance('screenshotservice#handleImgHolderBlur').debug('base64 converstion took {0}'.format(t1 - t0));
            var now = new Date();
            var timestamp = '' + now.getFullYear() + (now.getMonth() + 1) + now.getDate();
            datamap[imgAttribute + "_path"] = "Screen" + timestamp + ".html";
        },

        handleImgHolderPaste: function (imgHolder, e) {

            var imgAttribute = imgHolder.id.substring('imgHolder_'.length);

            // Chrome: check if clipboardData is available
            if (e.clipboardData != undefined && e.clipboardData.items != undefined) {

                var items = e.clipboardData.items;
                for (var i = 0; i < items.length; i++) {

                    // Check if an image was pasted
                    if (items[i].type.indexOf("image") !== -1) {
                        var blob = items[i].getAsFile();
                        var url = window.URL || window.webkitURL;
                        var source = url.createObjectURL(blob);
                        this.createImage(imgAttribute, imgHolder, source);
                        return;
                    }
                }
                imgHolder.innerHTML = '';
                e.preventDefault();
            } else {
                // Firefox: the pasted object will be automaticaly included on imgHolder, so do nothing
            }
        },

        createImage: function (imgAttribute, imgHolder, source) {
            var pastedImage = new Image();

            pastedImage.onload = function () {
                var img = new Image();
                img.src = imgToBase64(this);
                imgHolder.appendChild(img);
            };
            pastedImage.src = source;
        }


    };
});


;
var app = angular.module('sw_layout');

app.factory('searchService', function (i18NService, $rootScope, contextService) {

    var objCache = {};

    //Update the Filter lables upon change of language.
    function buildArray() {
        var filtertype = {};
        var filterkeyaux = '_grid.filter.filtertype.';
        filtertype.nofilter = i18NService.get18nValue(filterkeyaux + 'nofilter', 'No Filter');
        filtertype.filter = i18NService.get18nValue(filterkeyaux + 'filter', 'Filter');
        filtertype.contains = i18NService.get18nValue(filterkeyaux + 'contains', 'Contains');
        filtertype.ncontains = i18NService.get18nValue(filterkeyaux + 'ncontains', 'Does Not Contain');
        filtertype.startwith = i18NService.get18nValue(filterkeyaux + 'startwith', 'Starts With');
        filtertype.endwith = i18NService.get18nValue(filterkeyaux + 'endwith', 'Ends With');
        filtertype.eq = i18NService.get18nValue(filterkeyaux + 'eq', 'Equal To');
        filtertype.noteq = i18NService.get18nValue(filterkeyaux + 'noteq', 'Not Equal To');
        filtertype.blank = i18NService.get18nValue(filterkeyaux + 'blank', 'Blank');
        filtertype.gt = i18NService.get18nValue(filterkeyaux + 'gt', 'Greater Than');
        filtertype.lt = i18NService.get18nValue(filterkeyaux + 'lt', 'Less Than');
        filtertype.gte = i18NService.get18nValue(filterkeyaux + 'gte', 'Greater Than Or Equal To');
        filtertype.lte = i18NService.get18nValue(filterkeyaux + 'lte', 'Less Than Or Equal To');

        var searchOperations = [
        { id: "", symbol: "", title: filtertype.nofilter, tooltip: filtertype.filter, begin: "", end: "", renderType: ["default", "datetime"] },
        { id: "CONTAINS", symbol: "C", title: filtertype.contains, tooltip: filtertype.contains, begin: "%", end: "%", renderType: ["default"] },
        { id: "NCONTAINS", symbol: "!C", title: filtertype.ncontains, tooltip: filtertype.ncontains, begin: "!%", end: "%", renderType: ["default"] },
        { id: "STARTWITH", symbol: "ST", title: filtertype.startwith, tooltip: filtertype.startwith, begin: "", end: "%", renderType: ["default"] },
        { id: "ENDWITH", symbol: "END", title: filtertype.endwith, tooltip: filtertype.endwith, begin: "%", end: "", renderType: ["default"] },
        { id: "EQ", symbol: "=", title: filtertype.eq, tooltip: filtertype.eq, begin: "=", end: "", renderType: ["default", "datetime"] },
        { id: "BTW", symbol: "-", title: filtertype.btw, tooltip: filtertype.btw, begin: ">=", end: "<=", renderType: [] },
        { id: "NOTEQ", symbol: "!=", title: filtertype.noteq, tooltip: filtertype.noteq, begin: "!=", end: "", renderType: ["default", "datetime"] },
        { id: "BLANK", symbol: "BLANK", title: filtertype.blank, tooltip: filtertype.blank, begin: "", end: "", renderType: ["default", "datetime"] },
        { id: "GT", symbol: ">", title: filtertype.gt, tooltip: filtertype.gt, begin: ">", end: "", renderType: ["default", "datetime"] },
        { id: "LT", symbol: "<", title: filtertype.lt, lt: filtertype.lt, begin: "<", end: "", renderType: ["default", "datetime"] },
        { id: "GTE", symbol: ">=", title: filtertype.gte, tooltip: filtertype.gte, begin: ">=", end: "", renderType: ["default", "datetime"] },
        { id: "LTE", symbol: "<=", title: filtertype.lte, tooltip: filtertype.lte, begin: "<=", end: "", renderType: ["default", "datetime"] }
        ];
        return searchOperations;
    };



    var buildSearchParamsString = function (searchData, searchOperator) {
        var resultString = "";
        for (var data in searchData) {
            if (data == "lastSearchedValues") {
                //exclude this field which is used only to control the  needsCountUpdate flag
                continue;
            }

            if ((searchData[data] != null && searchData[data] != '') ||
                (searchOperator[data] != null && searchOperator[data].id == "BLANK")) {

                if (data.indexOf('___') != -1) {
                    // this case is only for "BETWEEN" operator
                    data = data.substring(0, data.indexOf('___'));
                    if (resultString.indexOf(data) != -1) {
                        resultString += data + "&&";
                    } else {
                        resultString += data + "___";
                    }
                    continue;
                }

                resultString += data + "&&";
            }
        }
        return resultString.substring(0, resultString.lastIndexOf("&&"));
    };

    var buildSearchSortString = function (searchSort) {
        //            var searchSort = scope.searchSort;
        var resultString = "";

        if (searchSort.field != null && searchSort.field != '') {
            resultString = searchSort.field;
        }
        return resultString;
    };



    return {


        buildSearchValuesString: function (searchData, searchOperator) {
            var resultString = "";
            var value = "";
            var beginAlreadySet = false;
            for (var data in searchData) {
                if ((searchData[data] == null || searchData[data] == '' || data == "lastSearchedValues") &&
                    (searchOperator[data] == null || searchOperator[data].id != "BLANK")) {
                    continue;
                }

                value = searchData[data];
                if (data.indexOf('___') != -1) {
                    data = data.substring(0, data.indexOf('___'));
                }
                if (searchOperator[data] == null) {
                    searchOperator[data] = this.defaultSearchOperation();
                }
                if (searchOperator[data].begin != '' && !beginAlreadySet) {
                    value = searchOperator[data].begin + value;
                    if (searchOperator[data].id == 'BTW') {
                        beginAlreadySet = true;
                        resultString += value + "___";
                        continue;
                    }
                }
                if (searchOperator[data].end != '') {
                    if (searchOperator[data].id == 'BTW') {
                        value = searchOperator[data].end + value;
                        beginAlreadySet = false;
                    }
                    else {
                        value = value + searchOperator[data].end;
                    }
                }
                if (searchOperator[data] != null && searchOperator[data].id == 'BLANK') {
                    value = 'IS NULL';
                }

                resultString += value + ",,,";
            }
            resultString = resultString.substring(0, resultString.lastIndexOf(",,,"));

            return resultString;
        },

        buildSearchDTO: function (searchData, searchSort, searchOperator, filterFixedWhereClause, unionFilterFixedWhereClause) {



            var btwFlag = false;
            var searchDto = {};
            searchData = searchData || {};
            searchSort = searchSort || {};
            searchOperator = searchOperator || {};


            searchDto.searchParams = buildSearchParamsString(searchData, searchOperator);
            searchDto.searchValues = this.buildSearchValuesString(searchData, searchOperator);
            searchDto.searchSort = buildSearchSortString(searchSort);
            searchDto.SearchAscending = searchSort.order == "asc";
            searchDto.filterFixedWhereClause = filterFixedWhereClause;
            searchDto.unionFilterFixedWhereClause = unionFilterFixedWhereClause;
            searchDto.needsCountUpdate = true;
            searchData.lastSearchedValues = searchDto.searchValues;
            return searchDto;

        },

        buildReportSearchDTO: function (searchDto, searchData, searchSort, searchOperator, filterFixedWhereClause, unionFilterFixedWhereClause) {
            if (searchDto == null || searchDto == undefined) {
                var searchDto = {};
                searchDto.searchParams = buildSearchParamsString(searchData, searchOperator);
                searchDto.searchValues = this.buildSearchValuesString(searchData, searchOperator);
            }
            else {
                var extraParams = buildSearchParamsString(searchData, searchOperator);
                var extraValues = this.buildSearchValuesString(searchData, searchOperator);

                if (extraParams != null && extraParams != '' && extraValues != null && extraValues != '') {
                    searchDto.searchParams += "&&" + extraParams;
                    searchDto.searchValues += ",,," + extraValues;
                }
            }
            searchDto.searchSort = buildSearchSortString(searchSort);
            searchDto.SearchAscending = searchSort.order == "asc";
            searchDto.filterFixedWhereClause = filterFixedWhereClause;
            searchDto.unionFilterFixedWhereClause = unionFilterFixedWhereClause;
            searchDto.needsCountUpdate = searchDto.searchValues != searchData.lastSearchedValues;
            searchData.lastSearchedValues = searchDto.searchValues;
            return searchDto;

        },

        getSearchOperation: function (idx) {
            return this.searchOperations()[idx];
        },

        getSearchOperationById: function (id) {
            var op = $.grep(this.searchOperations(), function (e) { return e.id.toUpperCase() == id.toUpperCase() });
            if (op.length > 0) {
                return op[0];
            }
        },

        searchOperations: function () {
            var language = i18NService.getCurrentLanguage();
            var module = contextService.retrieveFromContext('currentmodule');
            if (!nullOrUndef(module)) {
                //if inside a module language should be always english
                language = 'EN';
            }
            if (objCache[language] != undefined) {
                return objCache[language];
            }
            objCache[language] = buildArray();
            return objCache[language];
        },

        defaultSearchOperation: function () {
            return this.searchOperations()[1];
        },

        noFilter: function () {
            return this.searchOperations()[0];
        }

    };


});;

(function (angular) {
    'use strict';

    var defaultOptions = {
        lines: 13, // The number of lines to draw
        length: 20, // The length of each line
        width: 10, // The line thickness
        radius: 30, // The radius of the inner circle
        corners: 1, // Corner roundness (0..1)
        rotate: 0, // The rotation offset
        direction: 1, // 1: clockwise, -1: counterclockwise
        color: '#000', // #rgb or #rrggbb or array of colors
        speed: 1, // Rounds per second
        trail: 60, // Afterglow percentage
        shadow: false, // Whether to render a shadow
        hwaccel: false, // Whether to use hardware acceleration
        className: 'spinner', // The CSS class to assign to the spinner
        zIndex: 2e9, // The z-index (defaults to 2000000000)
        top: 'auto', // Top position relative to parent in px
        left: 'auto', // Left position relative to parent in px,
        opacity: 1 / 4
    };

    var smallOpts = {
        lines: 13, // The number of lines to draw
        length: 10, // The length of each line
        width: 5, // The line thickness
        radius: 15, // The radius of the inner circle
        corners: 1, // Corner roundness (0..1)
        rotate: 0, // The rotation offset
        direction: 1, // 1: clockwise, -1: counterclockwise
        color: '#000', // #rgb or #rrggbb or array of colors
        speed: 1, // Rounds per second
        trail: 60, // Afterglow percentage
        shadow: false, // Whether to render a shadow
        hwaccel: false, // Whether to use hardware acceleration
        className: 'spinner', // The CSS class to assign to the spinner
        zIndex: 2e9, // The z-index (defaults to 2000000000)
        top: 'auto', // Top position relative to parent in px
        left: 'auto', // Left position relative to parent in px,
        opacity: 1 / 4
    };

    angular.module('sw_layout').factory('spinService', ['$rootScope', spinService]);

    function spinService($rootScope) {

        var ajaxspin;

        var compositionspin;

        var service = {
            start: start,
            stop: stop,
        };

        return service;

        function start(parameters) {
            if ($rootScope.showingspin) {
                //if already showing no action needed
                return;
            }
            parameters = parameters || {};
            var savingDetail = parameters.savingDetail || false;
            var isComposition = parameters.compositionSpin || false;

            var spinDivId = savingDetail ? 'detailspinner' : 'mainspinner';
            var optsToUse = savingDetail ? smallOpts : defaultOptions;
            var spinner = document.getElementById(spinDivId);
            $rootScope.showingspin = true;
            if (isComposition) {
                compositionspin = new Spinner(optsToUse).spin(spinner);;
            } else {
                ajaxspin = new Spinner(optsToUse).spin(spinner);;
            }
        };

        function stop(parameters) {

            parameters = parameters || {};
            var isComposition = parameters.compositionSpin || false;

            var spinToUse = isComposition ? compositionspin : ajaxspin;

            if (spinToUse) {
                $rootScope.showingspin = false;
                spinToUse.stop();
            }
        }

    }
})(angular);
;
var app = angular.module('sw_layout');

app.factory('styleService', function ($rootScope, $timeout, i18NService) {

    return {

        getLabelStyle: function (fieldMetadata,key) {
            var parameters = fieldMetadata.rendererParameters;
            if (fieldMetadata.header != null) {
                parameters = fieldMetadata.header.parameters;
            }
            if (parameters == null) {
                return null;
            }
            return parameters[key];

        },

      
    };

});


;
var app = angular.module('sw_layout');

app.factory('submitService', function ($rootScope, fieldService,contextService,restService,spinService) {


    return {
        ///used for ie9 form submission
        submitForm: function (formToSubmit, parameters, jsonString, applicationName) {

            //this quick wrapper ajax call will validate if the user is still logged in or not
            restService.getPromise("ExtendedData", "PingServer").then(function () {
                

                // remove from session the redirect url... the redirect url will be returned when the form submit response comes from server
                sessionStorage.removeItem("swGlobalRedirectURL");

                for (var i in parameters) {
                    formToSubmit.append("<input type='hidden' name='" + i + "' value='" + parameters[i] + "' />");
                }
                if (sessionStorage.mockmaximo == "true") {
                    formToSubmit.append("<input type='hidden' name='%%mockmaximo' value='true'/>");
                }


                formToSubmit.append("<input type='hidden' name='currentmodule' value='" + contextService.retrieveFromContext('currentmodule') + "' />");

                formToSubmit.append("<input type='hidden' name='application' value='" + applicationName + "' />");
                formToSubmit.append("<input type='hidden' name='json' value='" + replaceAll(jsonString, "'", "&apos;") + "' />");

                // start spin befor submitting form
                var savingMain = true === $rootScope.savingMain;
                spinService.start(savingMain);

                // submit form
                formToSubmit.attr("action", url("/Application/Input"));
                formToSubmit.submit();
            });
        },

        ///return if a field which is not on screen (but is not a hidden instance), and whose value is null from the datamap, avoiding sending useless (and wrong) data
        removeNullInvisibleFields: function (displayables, datamap) {
            var fn = this;
            $.each(displayables, function (key, value) {
                if (fieldService.isNullInvisible(value, datamap)) {
                    delete datamap[value.attribute];
                }
                if (value.displayables != undefined) {
                    fn.removeNullInvisibleFields(value.displayables, datamap);
                }

            });
        },

        getFormToSubmitIfHasAttachement: function (displayables, datamap) {

            var form = $("#crudbodyform");
            var formId = null;
            $('.richtextbox', form).each(function () {

                if (this.contentWindow.asciiData != null && this.contentWindow.asciiData() != undefined && this.contentWindow.asciiData() != "") {
                    formId = $(this).closest('form');
                }
            });

            var isValidfn = this.isValidAttachment;

            $('input[type="file"]', form).each(function () {
                if (this.value != null && this.value != "" && isValidfn(this.value)) {
                    formId = $(this).closest('form');
                }
            });
            return formId;
        },

        isValidAttachment: function(value){
            var fileName = value.match(/[^\/\\]+$/);
            var validFileTypes = contextService.fetchFromContext('allowedfiles', true);
            var extensionIdx = value.lastIndexOf(".");
            var extension = value.substring(extensionIdx + 1).toLowerCase();
            if ($.inArray(extension, validFileTypes) == -1) {
                return false;
            }
            return true;
        },




        removeExtraFields: function (datamap, clone, schema) {
            if (!datamap.extrafields) {
                if (clone) {
                    return jQuery.extend(true, {}, datamap);
                }
                return datamap;
            }

            var data;
            if (clone) {
                data = jQuery.extend(true, {}, datamap);
            } else {
                data = datamap;
            }
            $.each(data, function (key, value) {
                //TODO: replace this gambi
                var isAssociatedData = key.indexOf(".") === -1;
                var isSafeKeyNeeded = key === "asset_.primaryuser_.personid";
                if (data.extrafields[key] != undefined) {

                    if (fieldService.getDisplayableByKey(schema, key) == undefined && !isSafeKeyNeeded) {
                        delete data[key];
                    }

                }
            });
            delete data.extrafields;
            return data;
        },

        translateFields: function (displayables, datamap) {
            var fieldsToTranslate = $.grep(displayables, function (e) {
                return e.attributeToServer != null;
            });
            for (var i = 0; i < fieldsToTranslate.length; i++) {
                var field = fieldsToTranslate[i];
                datamap[field.attributeToServer] = datamap[field.attribute];
                delete datamap[field.attribute];
            }
        }

    };

});;
var app = angular.module('sw_layout');

app.factory('tabsService', function (fieldService,i18NService) {

    var nonInlineCompositionsDict = function (schema) {
        if (schema.nonInlineCompositionsDict != undefined) {
            //caching
            return schema.nonInlineCompositionsDict;
        }
        var resultDict = {};
        for (var i = 0; i < schema.nonInlineCompositionIdxs.length; i++) {
            var idx = schema.nonInlineCompositionIdxs[i];
            var composition = schema.displayables[idx];
            resultDict[composition.relationship] = composition;
        }
        schema.nonInlineCompositionsDict = resultDict;
        return resultDict;
    };

    var getCompositionSchema = function (baseSchema, compositionKey, schemaId) {
        var schemas = nonInlineCompositionsDict(baseSchema);
        var thisSchema = schemas[compositionKey];
        schemas = thisSchema.schema.schemas;
        return schemaId == "print" ? schemas.print : schemas.list;
    };

    var getCompositionIdName = function (baseSchema, compositionKey, schemaId) {
        return getCompositionSchema(baseSchema, compositionKey, schemaId).idFieldName;
    };


    var buildTabObjectForPrint = function (datamap, tabSchema, schema) {
        var result = {};        

        result.items = [];
        result.items.push(datamap);
        result.schema = tabSchema;
        result.title = i18NService.getTabLabel(tabSchema, schema);

        return result;
    };

    return {

        hasTabs: function (schema) {
            if (schema.hasTabs != undefined) {
                //cache
                return schema.hasTabs;
            }
            var length = this.tabsDisplayables(schema).length;
            schema.hasTabs = length > 0;
            return length;
        },

        tabsDisplayablesForPrinting: function (schema, datamap) {
            if (schema.tabsDisplayablesForPrinting != undefined) {
                //cache
                return schema.tabsDisplayablesForPrinting;
            }
            var resultList = [];
            var displayables = this.tabsDisplayables(schema);
            $.each(displayables, function (key, displayable) {
                var value = datamap.fields[displayable.relationship];
                if (value != undefined && value.length > 0) {
                    resultList.push(displayable);
                }
            });
            schema.tabsDisplayablesForPrinting = resultList;
            return resultList;
        },

        ///Returns a list of all the tabs of the passed schema
        tabsDisplayables: function (schema) {
            if (schema.tabsDisplayables != undefined) {
                //cache
                return schema.tabsDisplayables;
            }
            var resultList = [];
            var idxArray = [];
            idxArray = idxArray.concat(schema.nonInlineCompositionIdxs);
            idxArray = idxArray.concat(schema.tabsIdxs);
            idxArray.sort(function (a, b) {
                return a - b;
            });

            for (var i = 0; i < idxArray.length; i++) {
                var idx = idxArray[i];
                var displayable = schema.displayables[idx];
                if (!displayable.isHidden) {
                    resultList.push(displayable);
                }
            }
            //cache
            schema.tabsDisplayables = resultList;
            return resultList;
        },

        nonInlineCompositionsDict: function (schema) {
            return nonInlineCompositionsDict(schema);
        },

        /*
        * param notExpansible = array of compositions that we do not need to hit the server, since they are not expandable
        *
        *
        * Returns a string in the same format the server expects for expanding the compositions on the ExtendedDataController.ExpandCompositions method
        *
        */
        buildCompositionsToExpand: function (compositionsToExpandObj, schema, datamap, schemaId, notExpansible, printMode) {
            var resultString = "";
            if (compositionsToExpandObj == null) {
                return "";
            }
            $.each(compositionsToExpandObj, function (key, obj) {
                if (obj.value == false) {
                    return;
                }

                var displayable = fieldService.getDisplayableByKey(schema, key);

                if (fieldService.isTab(displayable)) {
                    notExpansible.push({ key: key, tabObject: buildTabObjectForPrint(datamap, displayable, schema) });
                    return;
                }

                var compositionData = datamap.fields[key];

                if (compositionData == undefined) {
                    //this happens when the composition data has not been fetch yet,due to a lazy strategy
                    resultString += key + "=lazy,,,";
                    return;
                }

                //now, we are retrieving data for printing
                var currentSchema = getCompositionSchema(schema, key, obj.schema);
                if (currentSchema.properties.expansible != undefined && currentSchema.properties.expansible == "false") {
                    if (notExpansible != undefined && compositionData.length > 0) {
                        //only adding if there´s actual at least one element of this nonExpansible composition
                        notExpansible.push({ key: key, schema: currentSchema });
                    }
                    return;
                }
                var printSchema = getCompositionSchema(schema, key, schemaId);
                if (printMode && printSchema.schemaId == currentSchema.schemaId) {
                    notExpansible.push({ key: key, schema: currentSchema });
                    return;
                }


                var compositionIdField = getCompositionIdName(schema, key, schemaId);
                var compositionIdArray = [];

                for (var i = 0; i < compositionData.length; i++) {
                    var composition = compositionData[i];
                    compositionIdArray.push(composition[compositionIdField]);
                }
                if (compositionIdArray.length > 0) {
                    resultString += key + "=" + compositionIdArray.join(",") + ",,,";
                }
            });
            if (resultString != "") {
                resultString = resultString.substring(0, resultString.length - 3);
            }
            return resultString;
        },

        locatePrintSchema: function (baseSchema, compositionKey) {
            var schemas = nonInlineCompositionsDict(baseSchema);
            var thisSchema = schemas[compositionKey];
            return thisSchema.schema.schemas.print;
        },

        getTitle: function (baseSchema, compositionKey) {
            var schemas = nonInlineCompositionsDict(baseSchema);
            var thisSchema = schemas[compositionKey];
            return thisSchema.label;
        }

    };
});


;
var app = angular.module('sw_layout');

app.factory('userService', function (contextService) {

    return {
        //using sessionstorage instead of rootscope, as the later would be lost upon F5.
        //see SWWEB-239


        //determines whether the current user has one of the roles specified on the array
        HasRole: function (roleArray) {
            if (roleArray == null) {
                return true;
            }
            var user = contextService.getUserData();
            var userroles = user.roles;
            var result = false;
            $.each(roleArray, function (key, value) {
                $.each(userroles, function (k, v) {
                    if (v.name == value) {
                        result = true;
                        return;
                    }
                });
            });
            return result;
        },

        InGroup: function (groupName) {
            if (groupName == null) {
                return true;
            }
            var user = contextService.getUserData();
            var personGroups = user.personGroups;
            for (var i = 0; i < personGroups.length; i++) {
                var userGroup = personGroups[i];
                if (userGroup.personGroup.name == groupName) {
                    return true;
                }
            }
            return false;
        },



    };



});


;
var app = angular.module('sw_layout');

app.factory('validationService', function (i18NService, fieldService, $rootScope, dispatcherService, expressionService) {



    return {
        validate: function (schema, displayables, datamap, innerValidation) {
            var validationArray = [];
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                var label = displayable.label;
                if (fieldService.isNullInvisible(displayable, datamap)) {
                    continue;
                }

                var isRequired = !!displayable.requiredExpression ? expressionService.evaluate(displayable.requiredExpression, datamap) : false;

                if (isRequired && nullOrEmpty(datamap[displayable.attribute])) {
                    var applicationName = i18NService.get18nValue(displayable.applicationName + ".name", displayable.applicationName);
                    if (label.endsWith('s') || label.endsWith('S')) {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.isrequired', 'Field {0} for {1} are required', [label, applicationName]));
                    } else {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.isrequired', 'Field {0} for {1} is required', [label, applicationName]));
                    }
                }
                if (displayable.displayables != undefined) {
                    //validating section
                    var innerArray = this.validate(schema, displayable.displayables, datamap, true);
                    validationArray = validationArray.concat(innerArray);
                }
            }
            if (!innerValidation) {
                var validationService = schema.properties['oncrudsaveevent.validationservice'];
                if (validationService != null) {
                    var service = validationService.split('.')[0];
                    var method = validationService.split('.')[1];
                    var fn = dispatcherService.loadService(service, method);
                    var customErrorArray = fn(schema, datamap);
                    if (customErrorArray != null && Array.isArray(customErrorArray)) {
                        validationArray = validationArray.concat(customErrorArray);
                    }
                }
                if (validationArray.length > 0) {
                    $rootScope.$broadcast('sw_validationerrors', validationArray);
                }

            }
            return validationArray;
        },



    };

});


;
app.directive('commandbar', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/commandBar.html'),
        scope: {
            applicationschema: '=',
            schema: '=',
            mode: '=',
            localcommandprovider: '&',
            localschema: '=',
            cancelfn: '&',
            savefn: '&',
            datamap: '=',
            searchSort: '='
        },

        controller: function ($scope, $http, $element, $log, $rootScope, printService, i18NService, commandService, redirectService, alertService) {

            $scope.defaultCommands = function () {

                return {
                    save: {
                        id: "save",
                        method: $scope.save,
                        showExpressionFn: $scope.shouldDisplaySave,
                        icon: 'glyphicon-ok',
                        label: 'Submit',
                        'default': true
                    },
                    print: {
                        id: "print",
                        method: $scope.printDetail,
                        showExpressionFn: $scope.shouldDisplayPrint,
                        icon: 'glyphicon-print',
                        label: 'Print',
                        'default': true
                    },

                    cancel: {
                        id: "cancel",
                        method: $scope.cancel,
                        showExpressionFn: $scope.shouldDisplayCancel,
                        icon: 'glyphicon-backward',
                        label: GetPopUpMode().equalsAny("browser", "nomenu") ? 'Close Window' : 'Cancel',
                        'default': true
                    },
                }

            }

            function addDefault(resultCommands) {

                if ($scope.schema.commandSchema == null) {
                    return resultCommands;
                }

                var schemaDeclaredCommands = $scope.schema.commandSchema.commands;
                var ignoreUndeclared = $scope.schema.commandSchema.ignoreUndeclaredCommands;
                if (!ignoreUndeclared) {
                    $.each($scope.defaultCommands(), function (key, defaultCommand) {
                        //each of the default commands might have been overriden on the schema. If so, change their declarations for the schema´s
                        var overridenCommand = $.findFirst(schemaDeclaredCommands, function (elem) {
                            return elem.id == defaultCommand.id;
                        });

                        if (nullOrUndef(overridenCommand)) {
                            resultCommands.push(defaultCommand);
                        } else {
                            if (overridenCommand.icon == undefined) {
                                overridenCommand.icon = defaultCommand.icon;
                            }
                            resultCommands.push(overridenCommand);
                        }
                    });
                }

                $.each(schemaDeclaredCommands, function (key, command) {
                    var defaultCommand = $scope.defaultCommands()[command.id];
                    if (!ignoreUndeclared && defaultCommand != undefined) {
                        //default commands have already been handled, unless we´re completely redeclaring everything
                        return;
                    }
                    if (command.defaultPosition == "left") {
                        resultCommands.unshift(command);
                    } else {
                        resultCommands.push(command);
                    }
                });

                $scope.cachedCommands = resultCommands;
                return resultCommands;
            }

            $scope.redirectToHapagHome = function () {
                redirectService.redirectToAction(null, 'HapagHome', null, null);
            };

            $scope.shouldDisplaySave = function () {
                return $scope.shouldDisplayCommand($scope.schema.commandSchema, 'save') && $scope.mode == 'input';
            };

            $scope.shouldDisplayCancel = function () {
                return $scope.shouldDisplayCommand($scope.schema.commandSchema, 'cancel');
            };

            $scope.shouldDisplayPrint = function () {
                return $scope.shouldDisplayCommand($scope.schema.commandSchema, 'print') && $scope.isEditing($scope.schema, $scope.datamap);
            };

            $scope.save = function () {
                $scope.savefn();
            };

            $scope.cancel = function () {
                if (GetPopUpMode() == "browser" || GetPopUpMode() == "nomenu") {
                    window.close();
                    return;
                }

                $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
                $scope.$emit('sw_cancelclicked');
            };

            $scope.printDetail = function () {
                var schema = $scope.schema;
                printService.printDetail(schema, $scope.datamap[schema.idFieldName]);
            };


            $scope.getCommands = function () {
                var log = $log.getInstance('commandbar#getCommands');
                if ($scope.cachedCommands) {
                    log.trace('returning from cache '.format(JSON.stringify($scope.cachedCommands)));
                    return $scope.cachedCommands;
                }

                var resultCommands = [];
                if ($scope.localcommandprovider == undefined) {
                    log.debug('adding default commands only');
                    //no external provider, just add the schema commands
                    return addDefault(resultCommands);
                }

                //this is for compositions or tabs, where we can have custom commands declared
                var localCommands = $scope.localcommandprovider();
                if (localCommands == undefined) {
                    log.debug('provider returned null: adding default commands only');
                    return addDefault(resultCommands);
                }

                if (localCommands.toAdd != undefined) {
                    //let´s add specific commands of the localcontext to the default bar (ex: composition add Item)
                    resultCommands = resultCommands.concat(localCommands.toAdd);
                }
                addDefault(resultCommands);

                if (isArrayNullOrEmpty(localCommands.toKeep) || localCommands.toKeep[0] == "all") {
                    resultCommands = resultCommands.concat($scope.schema.commandSchema.commands);
                    $scope.cachedCommands = resultCommands;
                    return resultCommands;
                }

                //let´s see the non-default commands, i.e commands that are declared on the schema, 
                //but may be overriden by specific tab or composition_list
                $.each($scope.schema.commandSchema.commands, function (index, value) {
                    if ($.inArray(value.id, localCommands.toKeep) != -1) {
                        resultCommands.push(value);
                    } else {
                        if ($scope.localschema != undefined) {
                            $scope.localschema.commandSchema.toExclude.push(value.id);
                        }
                    }
                });
                if (!isArrayNullOrEmpty(localCommands.toKeep)) {
                    //let´s see the default commands now cancel,print,save
                    $.each($scope.defaultCommands(), function (index, value) {
                        log.debug('checking command {0}'.format(value.id));
                        if ($.inArray(value.id, localCommands.toKeep) == -1) {
                            //this means that this default command shall not be kept
                            if ($scope.localschema != undefined) {
                                $scope.localschema.commandSchema.toExclude.push(value.id);
                                log.debug('excluding command {0}'.format(value.id));
                            }
                        }
                    });
                }
                $scope.cachedCommands = resultCommands;
                return resultCommands;
            };

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.i18NCommandLabel = function (command) {
                if (command.default != undefined && command.default == true) {
                    var i18NValue;
                    var popUpMode = GetPopUpMode();
                    if ((popUpMode == "browser" || popUpMode == "nomenu") && command.id == "cancel") {
                        i18NValue = $scope.i18N('general.close', command.label);
                    } else {
                        i18NValue = $scope.i18N('general.' + command.id, command.label);
                    }

                    return $scope.commandLabel($scope.schema, command.id, i18NValue);
                }

                return i18NService.getI18nCommandLabel(command, $scope.schema);
            };

            //this is for the defaultCommands.
            //TODO: review
            $scope.commandLabel = function (schema, id, defaultValue) {
                return commandService.commandLabel(schema, id, defaultValue);
            };

            $scope.doCommand = function (command) {
                commandService.doCommand($scope, command);
            };

            //this is used for the default commands and take in consideration remove expressions
            //TODO: review this
            $scope.shouldDisplayCommand = function (commandSchema, id) {
                if (!$scope.cachedCommands) {
                    $scope.getCommands();
                }
                if ($scope.localschema != undefined) {
                    return commandService.shouldDisplayCommand($scope.localschema.commandSchema, id);
                }
                return commandService.shouldDisplayCommand(commandSchema, id);
            };

            $scope.isCommandHidden = function (schema, command) {
                var tabId = $element.parents('.tab-pane').attr('id');
                if (command.showExpressionFn != undefined && typeof (command.showExpressionFn) === 'function') {
                    return !command.showExpressionFn();
                }
                return commandService.isCommandHidden($scope.datamap, schema, command, tabId);
            };

            $scope.isCommandEnabled = function (schema, command) {
                var tabId = $element.parents('.tab-pane').attr('id');

                var enableExpression = command.enableExpression;
                if (enableExpression && enableExpression.startsWith("$scope:")) {
                    var result = $scope.invokeFn(enableExpression);
                    if (result == null) {
                        return true;
                    }
                    return result;
                }
                // if there is no $scope use regular expression evaluation
                return commandService.isCommandEnabled($scope.datamap, schema, command, tabId);
            }

            $scope.isEditing = function (schema, datamap) {
                return datamap[schema.idFieldName] != null;
            };

            $scope.invokeFn = function (expr, throwExceptionIfNotFound) {
                var methodname = expr.substr(7);
                var fn =this[methodname];
                if (fn != null) {
                    return fn();
                } else if (throwExceptionIfNotFound) {
                    throw new Error("parameterless method {0} not found on scope".format(methodname));
                }
                return null;
            }

            $scope.shouldCommandBeEnabled = function () {
                var datamap = $scope.datamap;

                for (var idx in datamap) {
                    if (datamap[idx] != undefined && datamap[idx] != null
                        && datamap[idx].fields != undefined
                        && datamap[idx].fields.checked) {
                        return true;
                    }
                }

                return false;
            }

        }
    };
});

;
app.directive("fileread", function (alertService, $log, contextService, submitService) {
    return {
        scope: {
            fileread: "=",
            path: "="
        },
        link: function (scope, element, attributes) {
            element.bind("change", function (changeEvent) {
                var log = $log.getInstance('fileread#change');
                log.debug('file change detected');
                if (!submitService.isValidAttachment(this.value)) {
                    if (!isIe9()) {
                        changeEvent.currentTarget.value = "";
                        scope.$apply(function () {
                            scope.fileread = undefined;
                            scope.path = undefined;
                        });
                    }

                    alert("Invalid file. Please choose another one.");
                    return;
                }

                if (isIe9()) {
                    
                    //to bypass required validation --> real file data will be set using form submission
                    //ie9 does not have the FileReaderObject
                    scope.fileread = "xxx";
                } else {
                    var fileName;
                    var hasFiles = changeEvent.target.files.length > 0;
                    if (!hasFiles) {
                        return;
                    }
                    var file = changeEvent.target.files[0];
                    fileName = file.name;
                    changeEvent.currentTarget.parentNode.parentNode.children[0].value = fileName;
                    scope.path = fileName;

                    var reader = new FileReader();
                    reader.onload = function (loadEvent) {
                        scope.$apply(function () {
                            scope.fileread = loadEvent.target.result;
                        });
                    };
                    reader.readAsDataURL(file);
                }
                scope.$digest();

            });
        }
    };
});;
app.directive('menuWrapper', function ($compile) {
    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            menu: '=',
            popupmode: '@'
        },
        link: function (scope, element, attrs) {
            if (scope.popupmode == 'none') {
                element.append(
                  "<menu menu='menu'/>"
              );
              $compile(element.contents())(scope);
            }
        }
    }
});

app.directive('menu', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/menu.html'),
        scope: {
            menu: '='
        },
        controller: function ($scope, $rootScope, $timeout) {

            $scope.level = -1;

            $scope.$on('ngRepeatFinished', function (ngRepeatFinishedEvent) {

                if ($scope.menu.displacement == 'vertical') {

                    $('.dropdown-container').on({
                        "shown.bs.dropdown": function (event) {
                            if ($(this)[0] === event.target) {
                                $(this).data('closable', false);
                            }
                        },
                        "click": function (event) {
                            if ($(this).children()[0] === event.target) {
                                $(this).data('closable', true);
                            }

                            //HAP-698 - reinit scrollpane after menu item opens/closes
                            $timeout(function () {
                                var api = $('.menu-primary').jScrollPane({ maintainPosition: true }).data('jsp');
                                api.reinitialise();
                            }, 250);
                        },
                        "hide.bs.dropdown": function (event) {
                            if ($(this)[0] === event.target) {
                                return $(this).data('closable');
                            }
                        }
                    });

                    // workaround to expand all sub-menus for gric/scottsdale/manchester in horizontal menu
                    if ($rootScope.clientName == 'gric' || $rootScope.clientName == 'scottsdale' || $rootScope.clientName == 'manchester') {
                        $('.dropdown-container').find("span").toggleClass("right-caret bottom-caret");
                        $('.dropdown-container').addClass("open");
                        $('.dropdown-container').data('closable', false);
                    }

                    //HAP-698 - replace following with scrollpane.js (changing page height, allowing content to scroll out of window)
                    // To scroll the Sidebar along with the window
                    //if ($rootScope.clientName == 'hapag') {

                    //    $(window).scroll(function (event) {
                    //        var scroll = $(event.target).scrollTop();
                    //        $("#sidebar").scrollTop(scroll);
                    //    });

                    //    $('.dropdown-container').on(
                    //    {
                    //        "shown.bs.dropdown": function (event) {
                    //            var windowHeight = $(window).height();
                    //            var sidebarHeigth = $('.alignment-logo').outerHeight() + $('[role=menu]').outerHeight();

                    //            if (sidebarHeigth > windowHeight) {
                    //                $('.col-main-content').height(sidebarHeigth);
                    //            }
                    //        },
                    //        "hide.bs.dropdown": function (event) {
                    //            var windowHeight = $(window).height();
                    //            var sidebarHeigth = $('.alignment-logo').outerHeight() + $('[role=menu]').outerHeight();

                    //            if (sidebarHeigth < windowHeight) {
                    //                $('.col-main-content').height(sidebarHeigth);
                    //            }
                    //        }

                    //    });

                    //}
                }
            });
        }
    };
});

app.directive('subMenu', function ($compile) {
    return {
        restrict: "E",
        replace: true,
        template: "<ul class='dropdown-menu submenu'></ul>",
        scope: {
            leaf: '=',
            displacement: '=',
            level: '='
        },
        link: function (scope, element, attrs) {
            if (angular.isArray(scope.leaf.leafs)) {
                element.append(
                    "<menu-item leaf='leaf' displacement='displacement' level='level' ng-repeat='leaf in leaf.leafs'></menu-item>"
                );
                $compile(element.contents())(scope);
            }
        }
    }
});

app.directive('menuItem', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('Content/Templates/menuitem.html'),
        scope: {
            leaf: '=',
            displacement: '=',
            level: '='
        },
        controller: function ($scope, $http, $rootScope, menuService, i18NService, mockService) {

            $scope.level = $scope.level + 1;

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.isButtonStyleContainer = function (level, leaf) {
                if (leaf.module != null) {
                    return false;
                }
                return level == 0;
            }

            $scope.i18NMenuLabel = function (menuItem, tooltip) {
                return menuService.getI18nMenuLabel(menuItem, tooltip);
            };

            $scope.isMenuItemTitle = function (menuItem, tooltip) {
                return menuItem.icon;
            };


            $scope.goToApplication = function (leaf, target) {
                menuService.goToApplication(leaf, target);
            };

            $scope.doAction = function (leaf, target) {
                menuService.doAction(leaf, target);
            };

            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.search = function (module, application, searchFields, id, schema) {
                var searchText = $('#' + id).val();
                if (nullOrUndef(schema)) {
                    schema = "list";
                }
                contextService.insertIntoContext('currentmodule', module);
                contextService.deleteFromContext('currentmetadata');
                if (searchText != null && searchText != '') {
                    searchText = '%' + searchText.trim() + '%';
                    var params = $.param({ 'application': application, 'searchFields': searchFields, 'searchText': searchText, 'schema': schema });
                    $http.get(url("/api/generic/Data/Search" + "?" + params)).success(
                        function (data) {
                            $rootScope.$broadcast("sw_redirectactionsuccess", data);
                        }
                    );
                }
            };


            $scope.handleContainerClick = function (container, target) {
                if (container.controller != null && !$(target).find("span").hasClass('bottom-caret') && !mockService.isMockedContainerDashBoard()) {
                    menuService.doAction(container, target);
                }
                if ($scope.displacement == 'vertical') {
                    $(target).find("span").toggleClass("right-caret bottom-caret");
                }


            };
        }
    };
});;
//ie9 fix, since performance object doesn´t exist
if (!performance) {
    var performance = {};
}
if (!performance.now) {
    performance.now = function () {
        return new Date().getTime();
    };
}


app.directive('ngrepeatinspector', function ($timeout, $log) {
    return {
        restrict: 'A',
        scope: {
            name: "@"
        },
        link: function (scope, element, attr) {
            var name = scope.name == null ? "" : scope.name;
            var log = $log.getInstance('inspector#ngrepeat');
            if (scope.$first) {
                log.debug('init ngrepeat for {0}'.format(name));
            } else {
                log.trace('ngrepeat processed for {0}'.format(name));
            }
            if (scope.$last === true || (scope.datamap && scope.datamap.length == 0)) {
                log.debug('finish ngrepeat for {0}').format(name);
            }
        }
    };
});

app.directive('localClick', ['$parse', '$rootScope', '$exceptionHandler', function ($parse, $rootScope, $exceptionHandler) {
    var directiveName = 'localClick';
    var eventName = 'click';
    return {
        restrict: 'A',
        compile: function ($element, attr) {

            $rootScope.$localApply = function $localApply(expr) {
                try {
                    this.$$phase = '$apply';
                    return this.$eval(expr);
                } catch (e) {
                    $exceptionHandler(e);
                } finally {
                    this.$$phase = null;
                    try {
                        // instead of starting dirty checking at the root
                        // $rootScope.$digest();
                        // start at the scope where called
                        this.$digest();
                    } catch (e) {
                        $exceptionHandler(e);
                        throw e;
                    }
                }
            };

            var fn = $parse(attr[directiveName]);
            return function limitedClickHandler(scope, element) {
                element.on(eventName, function (event) {
                    var callback = function () {
                        fn(scope, { $event: event });
                    };
                    // use $localApply instead of $apply
                    $rootScope.$localApply(callback);
                });
            };
        }
    };
}]);
var PRINTMODAL_$_KEY = '[data-class="printModal"]';

app.directive('printModal', function ($log, contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/printModal.html'),
        scope: {
            schema: '=',
            datamap: '=',
        },

        controller: function ($scope, printService, tabsService, i18NService) {
                        
            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.print = function () {
                if (Array.isArray($scope.datamap)) {
                    printService.printDetailedList($scope.printSchema, $scope.datamap,$scope.searchSort, $scope.buildPrintOptions());
                } else {
                    printService.printDetail($scope.printSchema, $scope.datamap, $scope.buildPrintOptions());
                }
            };

            $scope.buildPrintOptions = function () {
                var printOptions = {};
                printOptions.shouldPageBreak = $scope.shouldPageBreak;
                printOptions.shouldPrintMain = $scope.shouldPrintMain;
                printOptions.compositionsToExpand = $scope.compositionstoprint;
                return printOptions;

            };

            $scope.nonInlineCompositions = function(schema) {
                return tabsService.tabsDisplayables(schema);
            };
            
            $scope.$on('sw_hideprintmodal', function (event) {

                var modal = $(PRINTMODAL_$_KEY);
                modal.modal('hide');

            });

            $scope.$on('sw_showprintmodal', function (event, schema,searchSort) {
                $log.getInstance('printmodal').info("starting printing modal");
                $scope.compositionstoprint = {};
                $scope.printSchema = schema;
                $scope.searchSort = searchSort;

                var tabs = tabsService.tabsDisplayables($scope.printSchema);
                for (var i = 0; i < tabs.length ; i++) {
                    var tab = tabs[i];
                    if (tab.type == "ApplicationCompositionDefinition") {
                        var schemaToPrint = tab.schema.schemas.print ? tab.schema.schemas.print : tab.schema.schemas.list;
                        $scope.compositionstoprint[tab.relationship] = { value: false, schema: schemaToPrint };
                    } else {
                        $scope.compositionstoprint[tab.id] = { value: false, schema: tab };
                    }
                }
                
                var modal = $(PRINTMODAL_$_KEY);
                modal.draggable();
                modal.modal('show');

            });

            function init() {

                $scope.printSchema = $scope.schema.printSchema != null ? $scope.schema.printSchema : $scope.schema;

                //make all the ng-models´s objects true by default... angular will just work on binding upon click
                $scope.compositionstoprint = {};
                $scope.shouldPageBreak = false;
                $scope.shouldPrintMain = true;
                
            };

            init();
        }
    };
});;
var app = angular.module('sw_layout');

app.directive('printsectionrendered', function ($timeout, $log) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                var opened = false;
                $log.getInstance("printrendered#event").debug("Print Rendered event call");
                scope.$on("sw_bodyrenderedevent", function(key, value) {
                    $timeout(function () {
                        if (!opened) {
                            scope.$emit('sw_printsectionrendered');
                            opened = true;
                        }

                        //to avoid opening it twice
                        scope.$$listeners['sw_bodyrenderedevent'] = null;
                    }, 2000, false);
                });

            }
        }
    };
});

app.directive('printSection', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/printSection.html'),
        scope: {
            schema: '=',
            datamap: '=',
        },

        controller: function ($scope, $timeout, $log, printService, tabsService, i18NService, compositionService, fieldService) {

            $scope.compositionstoprint = [];
            $scope.shouldPageBreak = true;
            $scope.showPrintSection = false;
            $scope.showPrintSectionCompostions = false;

            $scope.shouldPageBreakComposition=function(first) {
                if (first) {
                    return $scope.shouldPageBreak && $scope.shouldPrintMain;
                }
                return $scope.shouldPageBreak;
            }

            $scope.$on('sw_readytoprintevent', function (event, compositionData, shouldPageBreak, shouldPrintMain) {
                
                var compositionstoprint = [];
                $scope.shouldPageBreak = shouldPageBreak;
                $scope.shouldPrintMain = shouldPrintMain;
                $scope.printSchema = $scope.schema.printSchema != null ? $scope.schema.printSchema : $scope.schema;
                
                $.each(compositionData, function (key, value) {
                    var compositionToPrint = {};
                    if (value.schema != undefined) {
                        //this happens for tabs
                        compositionToPrint.schema = value.schema;
                        $scope.datamap.fields[key] = value.items;
                        compositionToPrint.title = value.title;
                    } else {
                        compositionToPrint.schema = compositionService.locatePrintSchema($scope.printSchema, key);
                        $scope.datamap.fields[key] = value;
                        compositionToPrint.title = compositionService.getTitle($scope.printSchema, key);
                    }
                    compositionToPrint.key = key;
                    compositionstoprint.push(compositionToPrint);
                });
                $scope.compositionstoprint = compositionstoprint;
                $scope.printDatamap = Array.isArray($scope.datamap) ? $scope.datamap : new Array($scope.datamap);
                $scope.showPrintSection = true;
                $scope.showPrintSectionCompostions = compositionstoprint.length > 0;
                $scope.shouldPageBreakMain = $scope.shouldPageBreak && $scope.datamap.length > 1;

            });

            $scope.$on('sw_readytoprintdetailedlistevent', function (event, detailedListData, compositionsToExpand, shouldPageBreak, shouldPrintMain) {
                var compositionstoprint = [];
                $scope.shouldPageBreak = shouldPageBreak;
                $scope.shouldPrintMain = shouldPrintMain;
                $scope.printSchema = $scope.schema.printSchema != null ? $scope.schema.printSchema : $scope.schema;

                $.each(compositionsToExpand, function (key, value) {
                    var compositionToPrint = {};
                    compositionToPrint.schema = value.schema;
                    compositionToPrint.key = key;
                    if (value.schema.type == 'ApplicationTabDefinition') {
                        compositionToPrint.title = value.schema.label;
                    } else {
                        compositionToPrint.title = compositionService.getTitle($scope.printSchema, key);
                    }
                    compositionstoprint.push(compositionToPrint);
                });
                
                $scope.compositionstoprint = compositionstoprint;
                $scope.printDatamap = detailedListData;
                $scope.showPrintSection = true;
                $scope.showPrintSectionCompostions = compositionstoprint.length > 0;
                $scope.shouldPageBreakMain = $scope.shouldPageBreak && $scope.datamap.length > 1;
            });

            $scope.i18nValue = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.$on('sw_printsectionrendered', function () {
                printService.hidePrintModal();
                printService.doPrint();                
                $scope.showPrintSection = false;
                $scope.showPrintSectionCompostions = false;
                $scope.printSchema = null;
            });

        }
    };
});;
var app = angular.module('sw_layout');

app.factory('cmpAutocompleteClient', function ($rootScope, $timeout, fieldService, contextService) {

    return {

        unblock: function (displayable) {
            var element = $("select[data-associationkey='" + displayable.associationKey + "']");
            var combobox = $(element).data('combobox');
            if (combobox != undefined) {
                combobox.enable();
            }
        },

        block: function (displayable) {
            var element = $("select[data-associationkey='" + displayable.associationKey + "']");
            $(element).data('combobox').disable();
        },

        refreshFromAttribute: function (attribute) {
            var combo = $('#' + RemoveSpecialChars(attribute)).data('combobox');
            if (combo != undefined) {
                combo.refresh();
            }
        },

        init: function (bodyElement, datamap, schema, scope) {
            var selects = $('select.combobox-dynamic', bodyElement);
            for (var i = 0; i < selects.length; i++) {
                var select = $(selects[i]);
                var associationKey = select.data('associationkey');
                var parent = $(select.parents("div[rel=input-form-repeat]"));
                if (parent.data('selectenabled') == false || select.data('alreadyconfigured')) {
                    continue;
                }

                var fieldMetadata = fieldService.getDisplayablesByAssociationKey(schema, associationKey);
                var minLength = null;
                var pageSize = contextService.isLocal() ? 30 : 300;
                if (fieldMetadata != null && fieldMetadata.length > 0 && fieldMetadata[0].rendererParameters['minLength'] != null) {
                    minLength = parseInt(fieldMetadata[0].rendererParameters['minLength']);
                }
                if (fieldMetadata != null && fieldMetadata.length > 0 && fieldMetadata[0].rendererParameters['pageSize'] != null) {
                    pageSize = parseInt(fieldMetadata[0].rendererParameters['pageSize']);
                }

                select.data('alreadyconfigured', true);
                select.combobox({
                    minLength: minLength,
                    pageSize: pageSize
                });
            }
        }



    }

});


;
var app = angular.module('sw_layout');

app.factory('cmpAutocompleteServer', function ($log, associationService) {

    function beforeSendPostJsonDatamap(jqXhr, settings, datamap) {
        var jsonString = angular.toJson(datamap);
        settings.type = 'POST';
        settings.data = jsonString;
        settings.hasContent = true;
        jqXhr.setRequestHeader("Content-type", "application/json");
        return true;
    }

    return {

        refreshFromAttribute: function (associationFieldMetadata, scope) {
            var value = associationService.getFullObject(associationFieldMetadata, scope.datamap, scope.associationOptions);
            var associationkey = associationFieldMetadata.associationKey;
            var label = value == null ? null : value.label;
            $log.getInstance('cmpAutocompleteServer#refreshFromAttribute').debug("update autocomplete-server {0} with value {1}".format(associationkey, label));
            $("input[data-association-key=" + associationkey + "]").typeahead('val', label);
        },

        init: function (bodyElement, datamap, schema,scope) {
            $('input.typeahead', bodyElement).each(function (index, element) {
                var jelement = $(element);
                if (true == $(jelement.parent()).data('initialized')) {
                    return;
                }
                var associationKey = element.getAttribute('data-association-key');
                var dataTarget = element.getAttribute('data-target');
                $log.getInstance("cmpAutocompleteServer#init").debug("init autocomplete {0}".format(associationKey));

                var applicationName = schema.applicationName;
                var parameters = {};
                parameters.key = {};
                parameters.key.schemaId = schema.schemaId;
                parameters.key.mode = schema.mode;
                parameters.key.platform = platform();
                parameters.associationFieldName = associationKey;

                var urlToUse = url("/api/data/" + applicationName + "?labelSearchString=%QUERY&" + $.param(parameters));

                var engine = new Bloodhound({

                    datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
                    queryTokenizer: Bloodhound.tokenizers.whitespace,
                    limit: 30,
                    remote: {
                        url: urlToUse,
                        rateLimitFn: 'debounce',
                        rateLimitWait: 500,
                        filter: function (parsedResponse) {
                            var options = parsedResponse.resultObject;
                            if (options[associationKey] != null) {
                                return options[associationKey].associationData;
                            }
                            return null;
                        },
                        ajax: {
                            beforeSend: function (jqXhr, settings) {
                                beforeSendPostJsonDatamap(jqXhr, settings, datamap);
                            }
                        }

                    }
                });
                engine.initialize();

                jelement.typeahead({ minLength: 3 }, {
                    displayKey: 'label',
                    source: engine.ttAdapter()
                });

                jelement.on("typeahead:selected typeahead:autocompleted", function (e, datum) {
                    datamap[dataTarget] = datum.value;
                    scope.associationOptions[associationKey] = [{ value: datum.value, label: datum.label,extrafields:datum.extrafields }];
                    scope.$digest();
                });

                $(jelement.parent()).data('initialized', true);

            });

            // Configure autocompletes layout
            $('span.twitter-typeahead', bodyElement).css('width', '100%');
            $('input.tt-hint', bodyElement).addClass('form-control');
            $('input.tt-hint', bodyElement).css('width', '96%');
            $('input.tt-query', bodyElement).css('width', '97%');
        }



    }

});


;
var app = angular.module('sw_layout');

app.factory('cmpCombobox', function ($rootScope, $timeout, fieldService) {
    /// <summary>
    ///  for ie9 we have to handle crazy angular bug where we need to repaint the component whenever it´s values changes by a non "user-action"
    /// </summary>
    /// <param name="$rootScope"></param>
    /// <param name="$timeout"></param>
    /// <param name="fieldService"></param>
    /// <returns type=""></returns>
    function fixIe9Bug(displayable) {
        var element = $("select[data-comboassociationkey='" + displayable.associationKey + "']");
        element.hide();
        element.show();
        //this workaround, fixes a bug where only the fist charachter would show...
        //taken from http://stackoverflow.com/questions/5908494/select-only-shows-first-char-of-selected-option
        element.css('width', 0);
        element.css('width', '');
    }

    return {

        unblock: function (displayable) {
            if (isIe9()) {
                fixIe9Bug(displayable);
            }
        },

        block: function (displayable) {
            if (isIe9()) {
                fixIe9Bug(displayable);
            }
        },

        refreshFromAttribute: function (displayable) {
            if (isIe9()) {
                fixIe9Bug(displayable);
            }
        },

        init: function (bodyElement, datamap, schema, scope) {

        }



    }

});


;
var app = angular.module('sw_layout');

app.factory('cmpComboDropdown', function ($rootScope, $timeout, i18NService) {

    return {

        block: function (attribute) {
            var element = $("select[data-associationkey='" + attribute + "']");
            $(element).multiselect('destroy');
        },

        unblock: function (attribute) {
            var element = $("select[data-associationkey='" + attribute + "']");
            $(element).multiselect('refresh');
        },

        refresh: function (element) {
            if (element.find('option').size() > 0) {

                element.multiselect({
                    nonSelectedText: 'Select One',
                    includeSelectAllOption: true,
                    enableCaseInsensitiveFiltering: true,
                    disableIfEmpty: true
                });
                element.multiselect('refresh');
            }
        },

        refreshFromAttribute: function (attribute) {
            var combo = $('#' + RemoveSpecialChars(attribute));
            this.refresh(combo);
        },

        getSelectedTexts: function (fieldMetadata) {
            var combo = $('#' + RemoveSpecialChars(fieldMetadata.attribute));
            var custombuttontext = fieldMetadata.rendererParameters['custombuttontext'];
            var selectedTexts = new Array();

            combo.find(':selected').each(function () {
                selectedTexts.push($(this).text());
            });
            if (selectedTexts.length > 0) {
                var selected = i18NService.get18nValue('combodropdown.selected', 'selected') + ': ';
                var text = fieldMetadata.label + ' ' + selected + selectedTexts.length;
                if (!nullOrUndef(custombuttontext)) {
                    text = custombuttontext + ' ' + selected + selectedTexts.length;
                }
                combo[0].setAttribute('custombuttontext', text);
            }
            return selectedTexts;
        },

        init: function (bodyElement) {
            var fn = this;
            $timeout(
                function () {

                    $('.multiselect', bodyElement).each(function (index) {
                        fn.refresh($(this));

                    });

                }, 0, false);
        }



    }

});


;
var app = angular.module('sw_layout');

app.factory('cmpfacade', function ($timeout, $log, cmpComboDropdown, cmpAutocompleteClient, cmpAutocompleteServer, cmpCombobox, screenshotService, fieldService) {

    return {

        unblock: function (displayable, scope) {
            var log = $log.getInstance('cmpfacade#unblock');
            var rendererType = displayable.rendererType;
            var attribute = displayable.attribute;
            log.debug('unblocking association {0} of type {1}'.format(attribute, rendererType));
            if (rendererType == 'autocompleteclient') {
                cmpAutocompleteClient.unblock(displayable);
            }
            else if (rendererType == 'combodropdown') {
                cmpComboDropdown.unblock(attribute);
            }
            //            else if (rendererType == 'autocompleteserver') {
            //                cmpAutocompleteServer.unblock(displayable, scope);

            this.digestAndrefresh(displayable, scope);
        },

        block: function (displayable, scope) {
            var log = $log.getInstance('cmpfacade#block');
            var rendererType = displayable.rendererType;
            var attribute = displayable.attribute;
            log.debug('block association {0} of type {1}'.format(attribute, rendererType));
            if (rendererType == 'autocompleteclient') {
                cmpAutocompleteClient.block(displayable);
            }
                //            else if (rendererType == 'autocompleteserver') {
                //                cmpAutocompleteServer.unblock(displayable, scope);
            else if (rendererType == 'combodropdown') {
                cmpComboDropdown.block(displayable.associationKey);
            }else if (rendererType == "combo") {
                cmpCombobox.refreshFromAttribute(displayable);
            }
            this.digestAndrefresh(displayable, scope);
        },


        digestAndrefresh: function (displayable, scope) {
            var rendererType = displayable.rendererType;
            //            if (rendererType != 'autocompleteclient' && rendererType != 'autocompleteserver' && rendererType != 'combodropdown') {
            //                return;
            //            }
            try {
                scope.$digest();
                this.refresh(displayable, scope, true);
            } catch (e) {
                //nothing to do, just checking if digest was already in place or not, because we need angular to update screen first of all
                //if inside a digest already, exception would be thrown --> force a timeout with false flag
                var fn = this;
                $timeout(
                    function () {
                        fn.refresh(displayable, scope, true);
                    }, 0, false);
            }
        },

        refresh: function (displayable, scope, fromDigestAndRefresh) {
            var attribute = displayable.attribute;

            var log = $log.getInstance('cmpfacade#refresh');
            var rendererType = displayable.rendererType;
            if (fromDigestAndRefresh) {
                log.debug('calling digest and refresh for field {0}, component {1}'.format(displayable.attribute, rendererType));
            } else {
                log.debug('calling refresh for field {0}, component {1}'.format(displayable.attribute, rendererType));
            }

            if (rendererType == 'autocompleteclient') {
                cmpAutocompleteClient.refreshFromAttribute(attribute);
            } else if (rendererType == 'autocompleteserver') {
                cmpAutocompleteServer.refreshFromAttribute(displayable, scope);
            } else if (rendererType == 'combodropdown') {
                cmpComboDropdown.refreshFromAttribute(attribute);
            } else if (rendererType == 'default' || rendererType == 'combo') {
                cmpCombobox.refreshFromAttribute(displayable);
            }
        },

        init: function (bodyElement, scope) {
            var datamap = scope.datamap;
            var schema = scope.schema;
            cmpComboDropdown.init(bodyElement);
            cmpAutocompleteClient.init(bodyElement, datamap, schema, scope);
            cmpAutocompleteServer.init(bodyElement, datamap, schema, scope);
            screenshotService.init(bodyElement, datamap);
        },

        blockOrUnblockAssociations: function (scope, newValue, oldValue, association) {
            if (oldValue == newValue) {
                return;
            }
            var displayables = fieldService.getDisplayablesByAssociationKey(scope.schema, association.associationKey);
            if (displayables == null) {
                return;
            }
            //reversing to preserve focus order. see https://controltechnologysolutions.atlassian.net/browse/HAP-674
            displayables = displayables.reverse();
            var fn = this;
            if (oldValue == true && newValue == false) {
                $.each(displayables, function (idx, value) {
                    fn.unblock(value, scope);
                });
            } else if ((oldValue == false || oldValue == undefined) && newValue == true) {
                $.each(displayables, function (idx, value) {
                    fn.block(value, scope);
                });
            }
        }


    }

});


;
/*
Copyright Vassilis Petroulias [DRDigit]

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
var B64 = {
    alphabet: 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=',
    lookup: null,
    ie: /MSIE /.test(navigator.userAgent),
    ieo: /MSIE [67]/.test(navigator.userAgent),
    encode: function (s) {
        var buffer = B64.toUtf8(s),
            position = -1,
            len = buffer.length,
            nan0, nan1, nan2, enc = [, , , ];
        if (B64.ie) {
            var result = [];
            while (++position < len) {
                nan0 = buffer[position];
                nan1 = buffer[++position];
                enc[0] = nan0 >> 2;
                enc[1] = ((nan0 & 3) << 4) | (nan1 >> 4);
                if (isNaN(nan1))
                    enc[2] = enc[3] = 64;
                else {
                    nan2 = buffer[++position];
                    enc[2] = ((nan1 & 15) << 2) | (nan2 >> 6);
                    enc[3] = (isNaN(nan2)) ? 64 : nan2 & 63;
                }
                result.push(B64.alphabet.charAt(enc[0]), B64.alphabet.charAt(enc[1]), B64.alphabet.charAt(enc[2]), B64.alphabet.charAt(enc[3]));
            }
            return result.join('');
        } else {
            var result = '';
            while (++position < len) {
                nan0 = buffer[position];
                nan1 = buffer[++position];
                enc[0] = nan0 >> 2;
                enc[1] = ((nan0 & 3) << 4) | (nan1 >> 4);
                if (isNaN(nan1))
                    enc[2] = enc[3] = 64;
                else {
                    nan2 = buffer[++position];
                    enc[2] = ((nan1 & 15) << 2) | (nan2 >> 6);
                    enc[3] = (isNaN(nan2)) ? 64 : nan2 & 63;
                }
                result += B64.alphabet[enc[0]] + B64.alphabet[enc[1]] + B64.alphabet[enc[2]] + B64.alphabet[enc[3]];
            }
            return result;
        }
    },
    decode: function (s) {
        if (s.length % 4)
            throw new Error("InvalidCharacterError: 'B64.decode' failed: The string to be decoded is not correctly encoded.");
        var buffer = B64.fromUtf8(s),
            position = 0,
            len = buffer.length;
        if (B64.ieo) {
            var result = [];
            while (position < len) {
                if (buffer[position] < 128) 
                    result.push(String.fromCharCode(buffer[position++]));
                else if (buffer[position] > 191 && buffer[position] < 224) 
                    result.push(String.fromCharCode(((buffer[position++] & 31) << 6) | (buffer[position++] & 63)));
                else 
                    result.push(String.fromCharCode(((buffer[position++] & 15) << 12) | ((buffer[position++] & 63) << 6) | (buffer[position++] & 63)));
            }
            return result.join('');
        } else {
            var result = '';
            while (position < len) {
                if (buffer[position] < 128) 
                    result += String.fromCharCode(buffer[position++]);
                else if (buffer[position] > 191 && buffer[position] < 224) 
                    result += String.fromCharCode(((buffer[position++] & 31) << 6) | (buffer[position++] & 63));
                else 
                    result += String.fromCharCode(((buffer[position++] & 15) << 12) | ((buffer[position++] & 63) << 6) | (buffer[position++] & 63));
            }
            return result;
        }
    },
    toUtf8: function (s) {
        var position = -1,
            len = s.length,
            chr, buffer = [];
        if (/^[\x00-\x7f]*$/.test(s)) while (++position < len)
            buffer.push(s.charCodeAt(position));
        else while (++position < len) {
            chr = s.charCodeAt(position);
            if (chr < 128) 
                buffer.push(chr);
            else if (chr < 2048) 
                buffer.push((chr >> 6) | 192, (chr & 63) | 128);
            else 
                buffer.push((chr >> 12) | 224, ((chr >> 6) & 63) | 128, (chr & 63) | 128);
        }
        return buffer;
    },
    fromUtf8: function (s) {
        var position = -1,
            len, buffer = [],
            enc = [, , , ];
        if (!B64.lookup) {
            len = B64.alphabet.length;
            B64.lookup = {};
            while (++position < len)
                B64.lookup[B64.alphabet.charAt(position)] = position;
            position = -1;
        }
        len = s.length;
        while (++position < len) {
            enc[0] = B64.lookup[s.charAt(position)];
            enc[1] = B64.lookup[s.charAt(++position)];
            buffer.push((enc[0] << 2) | (enc[1] >> 4));
            enc[2] = B64.lookup[s.charAt(++position)];
            if (enc[2] == 64) 
                break;
            buffer.push(((enc[1] & 15) << 4) | (enc[2] >> 2));
            enc[3] = B64.lookup[s.charAt(++position)];
            if (enc[3] == 64) 
                break;
            buffer.push(((enc[2] & 3) << 6) | enc[3]);
        }
        return buffer;
    }
};;
//base idea: http://blog.projectnibble.org/2013/12/23/enhance-logging-in-angularjs-the-simple-way/

var app = angular.module('sw_layout');
app.run(['$log', 'contextService', enhanceAngularLog]);

function ltEnabledLevel(currLevel, enabledLevel) {
    if (enabledLevel == "none") {
        return true;
    }
    if (enabledLevel == "trace") {
        return false;
    }
    if (enabledLevel == "debug") {
        return currLevel.equalsAny('trace');
    }
    if (enabledLevel == "debug") {
        return currLevel.equalsAny('trace');
    }
    if (enabledLevel == "info") {
        return currLevel.equalsAny('trace', 'debug');
    }
    if (enabledLevel == "warn") {
        return currLevel.equalsAny('trace', 'debug', 'info');
    }
    if (enabledLevel == "error") {
        return currLevel.equalsAny('trace', 'debug', 'info', 'error');
    }
    return true;
}

function getContextLevel(context) {

    var methodLogLevel = sessionStorage["log_" + context];
    if (methodLogLevel !== undefined) {
        return methodLogLevel;
    }
    var indexOf = context.indexOf("#");
    if (indexOf != -1) {
        var serviceName = context.substr(0, indexOf);
        var serviceLogLevel = sessionStorage["log_" + serviceName];
        if (serviceLogLevel !== undefined) {
            return serviceLogLevel;
        }
    }
    return null;
};

function getMinLevel(globalLevel, contextLevel) {
    if (contextLevel == null) {
        return globalLevel;
    }

    if (contextLevel == "trace") {
        return contextLevel;
    }

    if (contextLevel == "debug") {
        return globalLevel == "trace" ? globalLevel : contextLevel;
    }

    if (contextLevel == "info") {
        return globalLevel.equalsAny("trace", "debug") ? globalLevel : contextLevel;
    }

    if (contextLevel == "warn") {
        return globalLevel.equalsAny("trace", "debug", "info") ? globalLevel : contextLevel;
    }

    return globalLevel;
}

function enhanceAngularLog($log, contextService) {

    $log.enabledContexts = [];

    $log.get = function (context) {
        return this.getInstance(context);
    };

    $log.getInstance = function (context) {
        return {
            log: enhanceLogging($log.log, 'log', context, contextService),
            info: enhanceLogging($log.info, 'info', context, contextService),
            warn: enhanceLogging($log.warn, 'warn', context, contextService),
            debug: enhanceLogging($log.debug, 'debug', context, contextService),
            trace: enhanceLogging($log.debug, 'trace', context, contextService),
            error: enhanceLogging($log.error, 'error', context, contextService),
            enableLogging: function (enable) {
                $log.enabledContexts[context] = enable;
            },
            isLevelEnabled: function (level) {
                return isLevelEnabled(level, context);
            }


        };
    };

    function isLevelEnabled(level, context) {
        var enabledLevel = sessionStorage.loglevel;
        if (enabledLevel == undefined) {
            enabledLevel = contextService.retrieveFromContext('defaultlevel');
        }
        var contextLevel = getContextLevel(context);
        enabledLevel = getMinLevel(enabledLevel, contextLevel);

        return !ltEnabledLevel(level, enabledLevel);
    }

    function enhanceLogging(loggingFunc, level, context) {
        return function () {
            var isEnabled = this.isLevelEnabled(level, context);
            if (!isEnabled) {
                return;
            }

            var modifiedArguments = [].slice.call(arguments);
            modifiedArguments[0] = [moment().format("dddd hh:mm:ss:SSS a") + '::[' + context + ']> '] + modifiedArguments[0];
            loggingFunc.apply(null, modifiedArguments);
            if (localStorage.logs == undefined) {
                localStorage.logs = "";
            }
            if (localStorage.logs.length > 10000) {
                //clear to avoid exploding... todo --> send to server side
                localStorage.logs = "";
            }
            localStorage.logs += modifiedArguments[0] + "\n";
        };
    }
}