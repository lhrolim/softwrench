using FluentMigrator;

namespace softwrench.sw4.user.Migration {

    [Migration(201605052033)]
    public class MigrationSwweb2284 : FluentMigrator.Migration {
        public override void Up() {
            IfDatabase("MySql")
                .Execute
                .Sql(@"insert into sw_userprofile (name,description,deletable) 
                        select 'approver','Activates newly created users',false 
                            from dual
                            where not exists ( select * from sw_userprofile where lower(name) = lower('approver') )");

            IfDatabase("Oracle").Execute.Sql(@"insert into sw_userprofile (name,description,deletable,id) 
                        select 'approver','Activates newly created users',0,1
                            from dual
                            where not exists ( select * from sw_userprofile where lower(name) = lower('approver') )");


            IfDatabase("SqlServer")
                .Execute
                .Sql(@"if not exists(select * from sw_userprofile where lower(name) = lower('approver'))
                        begin
	                        insert into sw_userprofile (name,description,deletable) values ('approver','Activates newly created users',0)
                        end");
        }

        public override void Down() {}
    }
}
