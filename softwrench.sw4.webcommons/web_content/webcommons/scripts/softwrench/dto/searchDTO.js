/**
 * Clone of server side class
 * 
*/
const defaultDto = {
    searchParams:"",
    searchValues:"",
    quickSearchDTO:{},
    searchSort: {},
    pageNumber:1,
    pageSize:30,
    multiSearchSort:{}
}

class SearchDTO {
    /**
     * 
     * @param {String} quickSearchData free text string to search for
     * @param {Array} compositionsToInclude 
     * @param {Array} compositionsToInclude 
     * 
     * @returns {} 
     */


    constructor({searchParams,searchValues,quickSearchDTO,searchSort,pageNumber,pageSize, multiSearchSort, searchOperator, searchData} = defaultDto ) {
        this.searchParams = searchParams;
        this.searchValues = searchValues;
        this.searchSort = searchSort;
        this.quickSearchDTO = quickSearchDTO;
        this.pageNumber = pageNumber;
        this.pageSize = pageSize;
        this.multiSearchSort = multiSearchSort;
        /**
         * an object whose keys are the columns and the values the search values. Basically, a combination of the SearchParams and the searchValues.
         * Used when these not provided
         */
        this.searchData = searchData;
        /**
         * an object whose keys are the columns and the values a SearchOperator object, which contains information about which operator has been used on the search
         */
        this.searchOperator = searchOperator;
    }

        isDefault() {
            return !Object.keys(this).some(p => {
                return defaultDto.hasOwnProperty(p) && this[p] !== defaultDto[p];
            });
        }

        toString() {
            return JSON.stringify(this);
        }


    }


