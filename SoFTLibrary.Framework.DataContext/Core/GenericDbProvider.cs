using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SoFTLibrary.Framework.DataContext.Infrastructure;

namespace SoFTLibrary.Framework.DataContext.Core
{
    public abstract class GenericDbProvider : ConnectionStringx , IDisposable
    {

        /// <summary>
        /// Gets IEnumerable of object entities mapped from SqlDataReader based on IDataReader
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="reader">IDataReader</param>
        /// <returns>IEnumerable of object</returns>
        protected Task<IEnumerable<T>> MapDataReader<T>(IDataReader reader) where T : class
        {
            try
            {
                var listReturn = new List<T>();

                var t = typeof(T); //untuk lihat tipenya. jika primitive ga perlu create instance

                if (t.IsPrimitive || t == typeof(object) || t == typeof(string) || t == typeof(Decimal) || t == typeof(TimeSpan) || t == typeof(DateTime) || t == typeof(DateTimeOffset))
                {
                    while (reader.Read()) //baca datareader
                    {
                        var values = new object[reader.FieldCount];
                        var countvalue = reader.GetValues(values);

                        for (var i = 0; i < countvalue; i++)
                        {
                            var val = (T)values[i];
                            listReturn.Add(val);
                        }
                    }
                }
                else
                {
                    while (reader.Read()) //baca datareader
                    {
                        var entity = Activator.CreateInstance<T>();
                        for (var index = 0; index < entity.GetType().GetProperties().Length; index++)
                        {
                            var prop = entity.GetType().GetProperties()[index];
                            for (var i = 0; i < reader.FieldCount; i++) //bandingkan kolomnya, antisipasi jika user select beberapa kolom aja sedangkan model ada banyak kolom
                            {
                                if (reader.GetName(i) != prop.Name) continue;
                                if (!Equals(reader[prop.Name], DBNull.Value))
                                {
                                    prop.SetValue(entity, reader[prop.Name], null);
                                }
                            }
                        }
                        listReturn.Add(entity);
                    }
                }
                IEnumerable<T> dataEnumerable = listReturn;
                return Task.Run(() => dataEnumerable);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }


        /// <summary>
        /// Gets IEnumerable of object entities mapped from SqlDataReader based on stored procedure that returns table
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="spName"></param>
        /// <returns>IEnumerable of object</returns>
        public async Task<IEnumerable<T>> MapStoredProcedure<T>(string spName) where T : class
        {
            try
            {
                DCommand = SqlDatabase.GetStoredProcCommand(spName);
                DReader = SqlDatabase.ExecuteReader(DCommand);
                return await MapDataReader<T>(DReader);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            finally
            {
                SqlDatabase.CreateConnection().Close();
                SqlDatabase.CreateConnection().Dispose();
            }
        }

        bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~GenericDbProvider()
        {
            Dispose(false);
        }
    }
}
