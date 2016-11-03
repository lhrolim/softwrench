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


    constructor({searchParams,searchValues,quickSearchDTO,searchSort,pageNumber,pageSize, multiSearchSort} = defaultDto ) {
        this.searchParams = searchParams;
        this.searchValues = searchValues;
        this.searchSort = searchSort;
        this.quickSearchDTO = quickSearchDTO;
        this.pageNumber = pageNumber;
        this.pageSize = pageSize;
        this.multiSearchSort = multiSearchSort;
    }

        isDefault() {
            return !Object.keys(this).some(p => {
                return defaultDto.hasOwnProperty(p) && this[p] !== defaultDto[p];
            });
        }


    }


