using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace FbGrant.Firebird
{
    internal class DbConnection: IDisposable
    {
        private readonly string _connectionString;
        public FbConnection Connection { get; private set; }
        public FbTransaction Transaction { get; private set; }

        public DbConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool Open()
        {
            try
            {
                Connection = new FbConnection(_connectionString);
                Connection.Open();
                Transaction = Connection.BeginTransaction();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Close();
                return false;
            }
            return true;
        }

        public void Close()
        {
            if (Transaction != null)
                Rollback();

            if (Connection != null)
            {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();
                Connection.Dispose();
            }
        }

        public FbCommand CreateCommand(string sql)
        {
            return new FbCommand(sql, Connection, Transaction);
        }


        public void Commit()
        {
            if (Transaction != null)
            {
                Transaction.Commit();
                Transaction = null;
            }
        }

        public void CommitAndStartNewTransaction()
        {
            Transaction?.Commit();
            Transaction = Connection.BeginTransaction();
        }

        public void Rollback()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
                Transaction = null;
            }
        }
        
        public void Dispose()
        {
            Close();
        }

        public int Execute(string sql)
        {
            sql = sql.Trim().TrimEnd(' ', ';');
            if (sql.Equals("COMMIT", StringComparison.OrdinalIgnoreCase))
            {
                CommitAndStartNewTransaction();
                return 0;
            }

            using (var cmd = CreateCommand(sql))
            {
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
