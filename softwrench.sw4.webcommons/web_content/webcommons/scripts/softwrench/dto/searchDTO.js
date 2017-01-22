/**
 * Clone of server side class
 * 
*/
const defaultDto = {
    searchParams:"",
    searchValues:"",
    quickSearchDTO:{},
    searchSort: {},
    searchTemplate:null,
    SearchAscending:true,
    pageNumber:1,
    pageSize:30,
    totalCount:null,
    addPreSelectedFilters:false,
    multiSearchSort:{},
    schemaFilterId:null,
    needsCountUpdate:true
}

class SearchDTO {
  
    constructor(
    {searchParams,searchValues,quickSearchDTO,searchSort,pageNumber,
        pageSize, multiSearchSort, searchOperator, searchData, totalCount,SearchAscending, needsCountUpdate,searchTemplate, numberOfPages, addPreSelectedFilters, schemaFilterId} = defaultDto ) {

            this.searchParams = searchParams;
            this.searchValues = searchValues;
            this.searchSort = searchSort;
            this.quickSearchDTO = quickSearchDTO;

            //#region pagination
            this.numberOfPages = numberOfPages;
            this.pageNumber = pageNumber;
            this.pageSize = pageSize;
            this.totalCount = totalCount;
            //#endregion

            this.SearchAscending = SearchAscending;
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
            this.needsCountUpdate = needsCountUpdate;
            this.searchTemplate = searchTemplate;

            this.addPreSelectedFilters = addPreSelectedFilters;
            this.schemaFilterId = schemaFilterId;
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


