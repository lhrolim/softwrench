using System;
using softWrench.sW4.Metadata;

namespace softWrench.sW4.Data.Persistence.Relational.DataConstraint {
    class DataConstraintValidator {
        readonly MaximoHibernateDAO _dao = new MaximoHibernateDAO();


        public String ValidateContraint(softWrench.sW4.Security.Entities.DataConstraint constraint) {
            try {
                var entityMetadata = MetadataProvider.Entity(constraint.EntityName);
                if (entityMetadata == null) {
                    return String.Format("EntityName {0} not found, unable to apply constraint", constraint.EntityName);
                }
                var query = new EntityQueryBuilder().CountRowsFromConstraint(entityMetadata, constraint);
                _dao.FindByNativeQuery(query.Sql, query.Parameters);
                return null;
            }
            catch (Exception e) {
                return String.Format("error saving constraint {0}.Cause:{1}", constraint.WhereClause, e.Message);
            }
        }


    }
}
