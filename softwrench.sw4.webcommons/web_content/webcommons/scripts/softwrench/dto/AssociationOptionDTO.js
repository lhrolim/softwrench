class AssociationOptionDTO {

    constructor({value, label, type, extraFields}) {
        this.value = value;
        this.label = label;
        this.type = type;
        this.extraFields = extraFields;
    }

   
    toString() {
        return JSON.stringify(this);
    }


}


