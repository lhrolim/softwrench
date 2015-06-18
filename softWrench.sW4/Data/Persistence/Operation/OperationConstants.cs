namespace softWrench.sW4.Data.Persistence.Operation {
    public class OperationConstants {
        public const string CRUD_FIND_BY_ID = "crud_find_by_id";
        public const string CRUD_FIND_ALL = "crud_find_all";
        public const string CRUD_CREATE = "crud_create";
        public const string CRUD_UPDATE = "crud_update";
        public const string CRUD_DELETE = "crud_delete";

        public const string CRUD_BATCH = "crud_batch";


        public static bool IsCrud(string operation) {
            return operation.StartsWith("crud_");
        }
    }
}
