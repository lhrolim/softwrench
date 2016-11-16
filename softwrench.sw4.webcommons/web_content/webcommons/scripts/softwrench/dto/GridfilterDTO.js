class GridFilterDTO {
    /**
     * 
     * @param {String} quickSearchData free text string to search for
     * @param {Array} compositionsToInclude 
     * @param {Array} compositionsToInclude 
     * 
     * @returns {} 
     */





    constructor(alias, id, applicationName, searchDTO) {
        this.id = id;
        this.alias = alias;
        this.applicationName = applicationName;
        this.searchDTO = searchDTO;
    }

    static PreviousDefaultFilter(applicationName,searchDTO) {
        // The previous filter needs to have an ID that will never be used by the regular filter creation methods
        const previousFilterId = -2;
        // The previous filter must be given an alias to make it stand out from the user created filters
        const previousFilterAlias = "*Previous Unsaved Filter*";
        return new GridFilterDTO(previousFilterAlias, previousFilterId, applicationName, searchDTO);
    }
    
    toString() {
        return JSON.stringify(this);
    }


}


