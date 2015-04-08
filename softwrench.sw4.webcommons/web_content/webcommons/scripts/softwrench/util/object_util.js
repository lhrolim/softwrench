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