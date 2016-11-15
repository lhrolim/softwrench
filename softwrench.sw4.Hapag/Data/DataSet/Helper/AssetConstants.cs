
using softWrench.sW4.Data.Search;
using softWrench.sW4.Util;
using System;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {
    class AssetConstants {
        internal const string Operating = "OPERATING";
        internal const string Active = "120 Active";
        internal const string Accepted = "100 Accepted";
        internal const string Idle = "150 Idle";
        internal const string Ordered = "010 Ordered";
       
        internal const string Lost = "320 LOST";
        internal const string Stolen = "360 STOLEN";
        internal const string Released = "400 RELEASED";

        internal const string Decommissioned = "DECOMMISSIONED";
        
        

        internal const string Unexistent = "LANCHILDSTATUS";

        internal const string ClassStructureIdColumn = "classstructureid";
        internal const string StatusColumn = "status";
        internal const string CustodianColumn = "usercustodianuser_.personid";
        public static string AssetNumColumn = "assetnum";

        internal const string PhoneClassStructure = "43191511";


        private static readonly string StdClassificationPathChild = StdClassificationPath.Fmt("43211900");

        internal static readonly string StdClassificationPathParent = StdClassificationPath.Fmt("43211500");



        internal static String ClassStructurePathPattern = "asset.ClassStructureID in ({0})";

        internal const string StdClassificationPath = @"
                    select c5.CLASSSTRUCTUREID  from maximo.CLASSSTRUCTURE c1
                        inner join maximo.CLASSSTRUCTURE c2 on c2.PARENT = c1.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c3 on c3.PARENT = c2.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c4 on c4.PARENT = c3.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c5 on c5.PARENT = c4.CLASSSTRUCTUREID
                        where   
                        c1.CLASSSTRUCTUREID = '10000001' and 
                        c2.CLASSSTRUCTUREID = '43000000' and 
                        c3.CLASSSTRUCTUREID = '43210000' and 
                        c4.CLASSSTRUCTUREID = '{0}'
                   ";


        internal const string Test = @"
                    select c2.CLASSSTRUCTUREID  from maximo.CLASSSTRUCTURE c1
                        inner join maximo.CLASSSTRUCTURE c2 on c2.PARENT = c1.CLASSSTRUCTUREID
                        where   
                        c1.CLASSSTRUCTUREID = '1247' 
                   ";

        internal const string PrinterClassificationPath = @"
                    select c5.CLASSSTRUCTUREID  from maximo.CLASSSTRUCTURE c1
                        inner join maximo.CLASSSTRUCTURE c2 on c2.PARENT = c1.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c3 on c3.PARENT = c2.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c4 on c4.PARENT = c3.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c5 on c5.PARENT = c4.CLASSSTRUCTUREID
                        where   
                        c1.CLASSSTRUCTUREID = '10000001' and 
                        c2.CLASSSTRUCTUREID = '43000000' and 
                        c3.CLASSSTRUCTUREID = '43210000' and 
                        c4.CLASSSTRUCTUREID = '43212100'
                   ";

        internal const string OtherWhereClause = @"
asset.ClassStructureID
not in(
select c5.CLASSSTRUCTUREID  from maximo.CLASSSTRUCTURE c1
                        inner join maximo.CLASSSTRUCTURE c2 on c2.PARENT = c1.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c3 on c3.PARENT = c2.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c4 on c4.PARENT = c3.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c5 on c5.PARENT = c4.CLASSSTRUCTUREID
                        where   
                        c1.CLASSSTRUCTUREID = '10000001' and 
                        c2.CLASSSTRUCTUREID = '43000000' and 
                        c3.CLASSSTRUCTUREID = '43210000' and 
                        c4.CLASSSTRUCTUREID = '{0}' 
union 
    select c5.CLASSSTRUCTUREID  from maximo.CLASSSTRUCTURE c1
                        inner join maximo.CLASSSTRUCTURE c2 on c2.PARENT = c1.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c3 on c3.PARENT = c2.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c4 on c4.PARENT = c3.CLASSSTRUCTUREID
                        inner join maximo.CLASSSTRUCTURE c5 on c5.PARENT = c4.CLASSSTRUCTUREID
                        where   
                        c1.CLASSSTRUCTUREID = '10000001' and 
                        c2.CLASSSTRUCTUREID = '43000000' and 
                        c3.CLASSSTRUCTUREID = '43210000' and 
                        c4.CLASSSTRUCTUREID = '43212100'
)                       
";


        internal const string Addclassifications = @"'41113711','43200507','43201537','43201545','43201601','43201817','43201818','43201903',
                                          '43211503','43211507','43211512','43211711',
                                          '43212102','43212104','43212105','43222628','43232901','44101501','44101503','43232901'";

        public static string BuildPrinterWhereClause() {
            return ClassStructurePathPattern.Fmt(PrinterClassificationPath);
        }

        public static string BuildStdWhereClause(bool child) {
            var path = child ? StdClassificationPathChild : StdClassificationPathParent;
            return ClassStructurePathPattern.Fmt(path);
        }

        public static string BuildOtherWhereClause(bool child) {
            var path = child ? "43211900" : "43211500";
            return OtherWhereClause.Fmt(path);
        }


        public static SearchRequestDto BuildPrinterDTO() {
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(ProjectionField.Default(ClassStructureIdColumn));
            dto.AppendWhereClause(PrinterClassificationPath);
            return dto;
        }

        public static SearchRequestDto BuildStdParentDTO() {
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(ProjectionField.Default(ClassStructureIdColumn));
            dto.AppendWhereClause(StdClassificationPathParent);
            return dto;
        }

    }
}
