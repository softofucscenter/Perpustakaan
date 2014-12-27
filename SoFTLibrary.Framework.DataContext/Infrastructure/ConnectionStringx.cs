using System.Configuration;
using SoftFocusCenter.EncryptionWrapper;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Oracle.ManagedDataAccess.Client;

namespace SoFTLibrary.Framework.DataContext.Infrastructure
{
    public abstract class ConnectionStringx
    {
        private static readonly string SqlConnectionString = ConfigurationManager.ConnectionStrings["ModelSample"].ToString();
        private static readonly string OracleConnection = ConfigurationManager.ConnectionStrings["ModelSample"].ToString();

        protected DbCommand DCommand;
        protected IDataReader DReader;
        private Database _sqlDatabase;
        private OracleConnection _oracleDatabase;

        protected Database SqlDatabase
        {
            get
            {
                var connString = Encryption.Decrypt(SqlConnectionString);
                SqlDatabase = new SqlDatabase(connString);
                return _sqlDatabase;
            }
            set
            {
                _sqlDatabase = value;
            }

        }

        protected OracleConnection OracleDb
        {
            get
            {
                var connString = Encryption.Decrypt(OracleConnection);
                _oracleDatabase = new OracleConnection(connString);
                return _oracleDatabase;

            }
            set
            {
                _oracleDatabase = value;
            }

        }

    }
}
