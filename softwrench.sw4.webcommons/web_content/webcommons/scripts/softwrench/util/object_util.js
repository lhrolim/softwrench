/// <summary>
/// Copy all of the matching properties of the src object into the destination object
/// </summary>
/// <param name="src"></param>
/// <param name="destination"></param>
function mergeObjects(src, destination) {

    for (var prop in src) {
        if (!src.hasOwnProperty(prop)) {
            continue;
        }
        if (destination.hasOwnProperty(prop)) {
            destination[prop] = src[prop];
        }
    }
    return destination;

}

var isObjectEmpty = function (ob) {
    for (var prop in ob) {
        if (ob.hasOwnProperty(prop)) {
            return false;
        }
    }
    return true;
}


function setDeep(el, key, value) {
    key = key.split('.');
    var i = 0, n = key.length;
    for (; i < n - 1; ++i) {
        el = el[key[i]];
    }
    return el[key[i]] = value;
}

function getDeep(el, key) {
    key = key.split('.');
    var i = 0, n = key.length;
    for (; i < n; ++i) {
        el = el[key[i]];
    }
    return el;
}

