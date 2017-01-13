const required = (paramname) => {
    throw new Error(`${paramname} is required`);
}

class LookupDTO {

    constructor(fieldMetadata = required("fieldMetadata")) {
        this.fieldMetadata = fieldMetadata;
        this._modalPaginationData = null;
        this._options = null;
        this._schema = null;
    }

    set options(options) {
        this._options = options;
    }

    get options() {
        return this._options;
    }

    get schema() {
        return this._schema;
    }

    set schema(schema) {
        this._schema = schema;
    }

    get modalPaginationData() {
        return this._modalPaginationData;
    }

    set modalPaginationData(modalPaginationData) {
        this._modalPaginationData = modalPaginationData;
    }

}


