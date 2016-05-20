using System;
using System.Collections.Generic;
using FbGrant.Firebird;

namespace FbGrant
{
    internal class GrantService
    {
        public string ConnectionString { get; set; }

        public Commands Command { get; set; } = Commands.Grant;
        public Privileges Privilege { get; set; } = Privileges.All;
        public bool WithAdminOption { get; set; } = false;
        public bool CreateRole { get; set; } = false;

        readonly List<string> _statements = new List<string>();
        public IEnumerable<string> Statements => _statements;

        public void PrepareStatements(string username, string rolename = "")
        {
            _statements.Clear();
            string usr;
            // If there's a rolename, we'll do stuff with it 
            if (!string.IsNullOrEmpty(rolename))
            {
                /* If a role name is supplied, create the role if requested */
                if (CreateRole)
                    AddStatemement($"CREATE ROLE {rolename};", true);

                usr = rolename;
            }
            else
            {
                /* We are not interested in the role: permissions are just for user */
                usr = username;
            }

            /* Decide whether it's a GRANT or a REVOKE script*/
            string stub = "GRANT";
            string toFrom = "TO";
            string adminOption = WithAdminOption ? "WITH ADMIN OPTION" : "";
            if (Command == Commands.Revoke)
            {
                stub = "REVOKE";
                toFrom = "FROM";
                adminOption = "";
            }

            if (!string.IsNullOrEmpty(rolename))
            {
                AddStatemement($"{stub} {rolename} {toFrom} {username} {adminOption};");
            }

            /* If ANY was oassed in as prvilege, create all perms separately */
            string priv;
            if (Privilege == Privileges.Any)
                priv = "SELECT, DELETE, INSERT, UPDATE, REFERENCES";
            else
                priv = "ALL";


            if (string.IsNullOrEmpty(rolename))
                adminOption = "";

            /* Cycle throught the table an view names and create a statement for each */
            string[] relations = GetRelations();
            foreach (string relationName in relations)
            {
                AddStatemement($"{stub} {priv} ON {relationName} {toFrom} {usr} {adminOption};");
            }

            /* Cycle throught the store procedures names and create a statement for each */
            string[] procedures = GetStoreProcedures();
            foreach (string procedure in procedures)
            {
                AddStatemement($"{stub} EXECUTE ON PROCEDURE {procedure} {toFrom} {usr} {adminOption};");
            }
        }

        private void AddStatemement(string statement, bool addCommit = false)
        {
            _statements.Add(statement);
            if (addCommit)
                _statements.Add("COMMIT;");
        }

        private string[] GetRelations()
        {
            List<string> relations;
            using (var conn = new DbConnection(ConnectionString))
            {
                conn.Open();
                relations = DbMetadata.GetRelations(conn);
                conn.Close();
            }
            return relations.ToArray();
        }

        private string[] GetStoreProcedures()
        {
            List<string> relations;
            using (var conn = new DbConnection(ConnectionString))
            {
                conn.Open();
                relations = DbMetadata.GetListOfStoreProcedures(conn);
                conn.Close();
            }
            return relations.ToArray();
        }

        public void ExecuteStatements()
        {
            if (_statements.Count == 0)
                return;

            using (var conn = new DbConnection(ConnectionString))
            {
                conn.Open();
                foreach (string statement in _statements)
                {
                    Output(statement);
                    conn.Execute(statement);
                }
                conn.Commit();
                conn.Close();
            }

        }

        private void Output(string statement)
        {
            Console.WriteLine($"sql > {statement}");
        }
    }

    enum Commands
    {
        Grant,
        Revoke
    }

    enum Privileges
    {
        All,
        Any
    }
}
