(function () {
    "use strict";


class ContextData {
    
    constructor(panelId, schemaId, entryId) {
        this.panelId = panelId;
        this.schemaId = schemaId || "#global";
        /**
         * entry id is only used for compositions where we need to specify a context for a given composition row
         */
        this.entryId = entryId;
    }
//
//    get panelId() {
//        return this.panelId;
//    }
//
//    get schemaId() {
//        return this.schemaId;
//    }
//
//    get entryId() {
//        return this.entryId;
//    }

}

window.ContextData = ContextData;

})();
