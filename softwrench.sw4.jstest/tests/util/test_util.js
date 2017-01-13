/**
 * Sets a value on an object based upon a string expression with either . or []
 * 
 * Took from http://stackoverflow.com/questions/11032572/implement-dot-notation-getter-setter
 * @param {} exp 
 * @param {} value 
 * @param {} scope 
 * @returns {} 
 */
var set = function (exp, value, scope) {
    exp = replaceAll(exp, "\\[", ".");
    exp = replaceAll(exp, "\\]", "");
    const levels = exp.split('.');
    var max_level = levels.length - 1;
    var target = scope;
    levels.some(function (level, i) {
        if (typeof level === 'undefined') {
            return true;
        }
        if (i === max_level) {
            target[level] = value;
        } else {
            const obj = target[level] || {};
            target[level] = obj;
            target = obj;
        }
    });
};

const getParams = qstr => {
    var query = {};
    var a = (qstr[0] === '?' ? qstr.substr(1) : qstr).split('&');
    for (let i = 0; i < a.length; i++) {
        const b = a[i].split('=');
        let value = decodeURIComponent(b[1] || '');
        const key = decodeURIComponent(b[0]);
        if (value.startsWith("{")) {
            value = JSON.parse(value);
        }
        set(key, value,query);
    }
    return query;
};

var objectComparison = (baseObject, assertionObject) => {
    return Object.keys(assertionObject).some(k => {
        if (!baseObject.hasOwnProperty(k)) {
            console.log(k);
            //property not even present
            return true;
        }
        if (typeof(assertionObject[k]) !== 'object') {
            // ReSharper disable once CoercedEqualsUsing
            //always string..
            if (assertionObject[k] == null) {
                return baseObject[k] == null;
            }

            const comparison = String(baseObject[k]) === String(assertionObject[k]);
            if (!comparison) {
                debugger;
                console.log(" found " +baseObject[k] + " expected " + assertionObject[k]);    
            }
            return !comparison;
        }

        return objectComparison(baseObject[k],assertionObject[k]);
    });
}

var assertHttp = (url, assertUrl, parametersMap, exactMatch=false) => {
    if (!url.startsWith(assertUrl)) {
        return false;
    }
    const idx = url.indexOf("?");

    if (idx === -1 || idx + 1 === url.length) {
        //no params at all
        return parametersMap === undefined || Object.keys(parametersMap).length === 0;
    }
    var params = getParams(url.substring(idx + 1));
    if (!parametersMap) {
        //no parameter expectation
        return true;
    }
    const missingParameters = objectComparison(params, parametersMap);
    if (missingParameters) {
        return false;
    }
    if (exactMatch) {
        return parametersMap === params;
    }
    return true;
}


