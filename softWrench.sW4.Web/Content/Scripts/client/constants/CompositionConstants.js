//indicates that this composition entry has modified data for the server side processing
const IsDirty = "#isDirty";
//indicates that this composition entry should be marked for deletion on the server side
const Deleted = "#deleted";

const Edited = "#edited";

const Selected = "#selected";

//flag that indicates that the given entry is being created other than edited
const IsCreation = "_iscreation";


class CompositionConstants {

    static get IsDirty() {
        return IsDirty;
    }

    static get Edited() {
        return Edited;
    }

    static get Deleted() {
        return Deleted;
    }

    static get Selected() {
        return Selected;
    }

    static get IsCreation() {
        return IsCreation;
    }

}