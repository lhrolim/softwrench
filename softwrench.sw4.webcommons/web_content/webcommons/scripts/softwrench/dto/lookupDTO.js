class LookupDTO {

    constructor(fieldMetadata, quickSearchDTO) {
        this.fieldMetadata = fieldMetadata;
        this.quickSearchDTO = quickSearchDTO;

        if (!fieldMetadata) {
            throw new Error("fieldMetadata is required");
        }
    }

    set options(options) {
        this.options = options;
    }

    get options() {
        return this.options;
    }


    set code (value) {
        this.code = value;
    }

    get code() {
        return this.code;
    }

    set schemaId(value) {
        this.schemaId = value;
    }

    get schemaId() {
        return this.schemaId;
    }

    set application(value) {
        this.application = value;
    }

    get application() {
        return this.application;
    }

}


