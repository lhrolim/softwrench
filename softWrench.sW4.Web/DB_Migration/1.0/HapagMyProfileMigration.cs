using System;
using System.Data;
using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration
{
    [Migration(201311211340)]
    public class HapagMyProfileMigration : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Column("department").OnTable("SW_USER2").AsString(200).Nullable();
            Create.Column("phone").OnTable("SW_USER2").AsString(MigrationUtil.StringMedium).Nullable();
            Create.Column("language").OnTable("SW_USER2").AsString(20).Nullable();
            //Execute.WithConnection(InsertDefaultUserProfiles); // this method is only necessary because i couldn't retrieve identities insert ids
        }

        public override void Down()
        {
            Delete.Column("department").FromTable("SW_USER2");
            Delete.Column("phone").FromTable("SW_USER2");
            Delete.Column("language").FromTable("SW_USER2");
        }

        private void InsertDefaultUserProfiles(IDbConnection conn, IDbTransaction tran)
        {
            // End User Profile
            String enduserProfile = "End User";
            String enduserProfileConstraint = "(SR.affectedperson = ''@username'' OR SR.reportedby = ''@username'')";
            String enduserRole = "enduser";
            
            // Insert Profile, Constraint and Role
            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = String.Format(@"INSERT INTO SW_USERPROFILE (name) VALUES ('{0}')", enduserProfile);
                cmd.ExecuteNonQuery();

                cmd.CommandText = String.Format(@"INSERT INTO SW_DATACONSTRAINT (whereclause,entityname,isactive) VALUES ('{0}','{1}',{2})", enduserProfileConstraint, "SR", 1);
                cmd.ExecuteNonQuery();

                cmd.CommandText = String.Format(@"INSERT INTO SW_ROLE (name,isactive) VALUES ('{0}','{1}')", enduserRole, 1);
                cmd.ExecuteNonQuery();
            }

            // Retrieve ProfileId
            int userProfileId = ExecuteSelectId(tran, String.Format(@"SELECT id FROM SW_USERPROFILE where name = '{0}'", enduserProfile));

            // Retrieve ConstraintId
            int userProfileConstraintId = ExecuteSelectId(tran, String.Format(@"SELECT id FROM SW_DATACONSTRAINT where whereclause = '{0}'", enduserProfileConstraint));

            // Retrieve RoleId
            int roleId = ExecuteSelectId(tran, String.Format(@"SELECT id FROM SW_ROLE where name = '{0}'", enduserRole));            
                        
            using (IDbCommand cmd = tran.Connection.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = String.Format(@"INSERT INTO SW_USERPROFILE_DATACONSTRAINT (profile_id,constraint_id) VALUES ({0},{1})", userProfileId, userProfileConstraintId);
                cmd.ExecuteNonQuery();

                cmd.CommandText = String.Format(@"INSERT INTO SW_USERPROFILE_ROLE (profile_id,role_id) VALUES ({0},{1})", userProfileId, roleId);
                cmd.ExecuteNonQuery();
            }
        }

        private static int ExecuteSelectId(IDbTransaction tran, String select)
        {            
            int id;
            using (IDbCommand cmd = tran.Connection.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = select;
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    id = reader.GetInt32(0);
                }
            }
            return id;
        }




    }
}
