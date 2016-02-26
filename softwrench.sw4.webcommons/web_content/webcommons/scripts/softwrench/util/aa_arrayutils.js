Array.prototype.subarray = function (start, end) {
    if (!end) {
        end = -1;
    }
    return this.slice(start, this.length + 1 - (end * -1));
}

/**
 * Flattens a nested array (works with any level of 'nestedness') e.g.
 * [1, [2, 3, [4, 5]], 6, [7, [8, [9]]]].flatten() === [1, 2, 3, 4, 5, 6, 7, 8, 9]
 * @see http://stackoverflow.com/questions/27266550/how-to-flatten-nested-array-in-javascript 
 * @returns flattened array 
 */
Array.prototype.flatten = function () {
    var toString = Object.prototype.toString;
    var arrayTypeStr = "[object Array]";

    var result = [];
    var nodes = this;
    var node;

    if (!this.length) {
        return result;
    }

    node = nodes.pop();

    do {
        if (toString.call(node) === arrayTypeStr) {
            nodes.push.apply(nodes, node);
        } else {
            result.push(node);
        }
    } while (nodes.length && (node = nodes.pop()) !== undefined);

    result.reverse(); // we reverse result to restore the original order
    return result;
};


Array.prototype.firstOrDefault = function (fn) {
    var arr = this;
    for (var i = 0; i < arr.length; i++) {
        var item = arr[i];
        if (fn(item)) {
            return item;
        }
    }
    return null;
}


Array.prototype.findIndex = Array.prototype.findIndex || function (predicate, thisArg) {
    "use strict";
    //TODO: Check predicate is a function.
    var lastIndex = -1;
    if (!Array.prototype.some.call(this, function (val, index, arr) {
        return predicate.call(thisArg, val, lastIndex = index, arr);
    })) {
        return -1;
    }

    return lastIndex;
}