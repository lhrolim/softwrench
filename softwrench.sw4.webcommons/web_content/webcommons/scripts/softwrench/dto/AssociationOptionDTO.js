class AssociationOptionDTO {

    constructor({value, label, type, extrafields}) {
        this.value = value;
        this.label = label;
        this.type = type;
        this.extrafields = extrafields;
    }

   
    toString() {
        return JSON.stringify(this);
    }


}


