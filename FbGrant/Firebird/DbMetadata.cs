using System.Collections.Generic;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;

namespace FbGrant.Firebird
{
    internal static class DbMetadata
    {
        public static string GetCharset(DbConnection connection)
        {
            const string sql = "SELECT rdb$character_set_name FROM rdb$database";
            return connection.FetchString(sql).FirstOrDefault();
        }

        public static List<string> GetRelations(DbConnection connection)
        {
            const string sql = "select RDB$RELATION_NAME from RDB$RELATIONS " +
                               "where (RDB$SYSTEM_FLAG = 0 or RDB$SYSTEM_FLAG is null) ";

            return connection.FetchString(sql);
        }


        public static List<string> GetListOfTables(DbConnection connection)
        {
            const string sql = "select RDB$RELATION_NAME from RDB$RELATIONS " +
                               "where (RDB$SYSTEM_FLAG = 0 or RDB$SYSTEM_FLAG is null) " +
                               "and RDB$VIEW_SOURCE is null ORDER BY 1";

            return connection.FetchString(sql);
        }
        
        public static List<string> GetListOfStoreProcedures(DbConnection connection)
        {
            const string sql = "select RDB$PROCEDURE_NAME from RDB$PROCEDURES " +
                               "where (RDB$SYSTEM_FLAG = 0 or RDB$SYSTEM_FLAG is null) " +
                               "ORDER BY 1";

            return connection.FetchString(sql);
        }

        public static List<string> GetListOfGenerators(DbConnection connection)
        {
            const string sql = "select RDB$GENERATOR_NAME from RDB$GENERATORS " +
                               "where (RDB$SYSTEM_FLAG = 0 or RDB$SYSTEM_FLAG is null) order by 1";

            return connection.FetchString(sql);
        }

        public static List<string> GetTableDependencies(DbConnection connection, string tablename)
        {
            const string sqlFk = "select r2.rdb$relation_name from rdb$relation_constraints r1" +
                                 " join rdb$ref_constraints c ON r1.rdb$constraint_name = c.rdb$constraint_name" +
                                 " join rdb$relation_constraints r2 on c.RDB$CONST_NAME_UQ  = r2.rdb$constraint_name" +
                                 " where r1.rdb$relation_name= @tableName " +
                                 " and (r1.rdb$constraint_type='FOREIGN KEY') ";

            const string sqlCheck = "select distinct d.RDB$DEPENDED_ON_NAME from rdb$relation_constraints r " +
                                    " join rdb$check_constraints c on r.rdb$constraint_name=c.rdb$constraint_name " +
                                    "      and r.rdb$constraint_type = 'CHECK' " +
                                    " join rdb$dependencies d on d.RDB$DEPENDENT_NAME = c.rdb$trigger_name and d.RDB$DEPENDED_ON_TYPE = 0 " +
                                    "      and d.rdb$DEPENDENT_TYPE = 2 and d.rdb$field_name is null " +
                                    " where r.rdb$relation_name= @tableName ";

            var listFk = connection.FetchString(sqlFk, tablename);
            var listCheck = connection.FetchString(sqlCheck, tablename);

            List<string> dependencies = new List<string>();
            dependencies.AddRange(listFk);
            dependencies.AddRange(listCheck);
            dependencies.Sort();

            return dependencies;
        }

        public static List<string> GetFields(DbConnection connection, string tablename)
        {
            const string sql = " SELECT r.rdb$field_name FROM rdb$relation_fields r" +
                               " JOIN rdb$fields f ON r.rdb$field_source = f.rdb$field_name" +
                               " WHERE r.rdb$relation_name = @tableName " +
                               " AND f.rdb$computed_blr is null" +
                               " ORDER BY 1";

            return connection.FetchString(sql, tablename);
        }

        public static List<string> GetPrimayKeys(DbConnection connection, string tablename)
        {
            const string sql = " select i.rdb$field_name" +
                               " from rdb$relation_constraints r, rdb$index_segments i " +
                               " where r.rdb$relation_name=@tablename and r.rdb$index_name=i.rdb$index_name" +
                               " and (r.rdb$constraint_type='PRIMARY KEY') ";


            return connection.FetchString(sql, tablename);
        }

        public static List<string> FetchString(this DbConnection connection, string sql)
        {
            List<string> list = new List<string>();
            
                using (FbCommand cmd = connection.CreateCommand(sql))
                {
                    list.AddRange(cmd.Read(x => x.GetTrimmedString(0)));
                }
            
            return list;
        }

        private static List<string> FetchString(this DbConnection connection, string sql, string tablename)
        {
            List<string> list = new List<string>();
            using (FbCommand cmd = connection.CreateCommand(sql))
            {
                cmd.Parameters.Add("@tableName", FbDbType.VarChar).Value = tablename;
                list.AddRange(cmd.Read(x => x.GetTrimmedString(0)));
            }

            return list;
        }
    }
}
