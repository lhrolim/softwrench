//indicates that this composition entry has modified data for the server side processing
const IsDirty = "#isDirty";
//indicates that this composition entry should be marked for deletion on the server side
const Deleted = "#deleted";


class CompositionConstants {

    static get IsDirty() {
        return IsDirty;
    }

    static get Deleted() {
        return Deleted;
    }

}