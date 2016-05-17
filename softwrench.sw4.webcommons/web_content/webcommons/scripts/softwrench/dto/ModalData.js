class ModalData {

    

    constructor(schema, datamap, savefn, cancelfn, previousschema, previousdata, title, cssclass, closeAfterSave, onloadfn, appResponseData, fromLink) {
        this.schema = schema;
        this.datamap = datamap;
        this.savefn = savefn;
        this.cancelfn = cancelfn;
        this.previousschema = previousschema;
        this.previousdata = previousdata;
        this.title = title;
        this.cssclass = cssclass;
        this.closeAfterSave = closeAfterSave || true;
        this.onloadfn = onloadfn;
        this.appResponseData = appResponseData;
        this.fromLink = fromLink || false;
    }


}

