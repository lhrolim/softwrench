/**
 * Clone of server side class
 * 
*/
class QuickSearchDTO {
    /**
     * 
     * @param {String} quickSearchData free text string to search for
     * @param {Array} compositionsToInclude 
     * @param {Array} compositionsToInclude 
     * 
     * @returns {} 
     */
    constructor(quickSearchData, compositionsToInclude, hiddenFieldsToInclude) {
        this.quickSearchData = quickSearchData;
        this.compositionsToInclude = compositionsToInclude;
        this.hiddenFieldsToInclude = hiddenFieldsToInclude;
    }


}


