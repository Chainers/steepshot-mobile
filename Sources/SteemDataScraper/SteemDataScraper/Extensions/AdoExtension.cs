using System;
using System.Data.Common;
using Ditch.Steem.Operations;
using Npgsql;
using NpgsqlTypes;

namespace SteemDataScraper.Extensions
{
    public static class AdoExtension
    {
        public static void RollbackAndDispose(this NpgsqlTransaction transaction)
        {
            if (transaction == null || transaction.IsCompleted)
                return;

            transaction.Rollback();
            transaction.Dispose();
        }

        public static void CommitAndDispose(this NpgsqlTransaction transaction)
        {
            if (transaction == null || transaction.IsCompleted)
                return;

            transaction.Commit();
            transaction.Dispose();
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, string value)
        {
            var param = new NpgsqlParameter(key, NpgsqlDbType.Text);
            param.Value = string.IsNullOrEmpty(value) ? DBNull.Value : (object)value;
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, string value, int size)
        {
            var param = new NpgsqlParameter(key, NpgsqlDbType.Varchar, size);
            param.Value = string.IsNullOrEmpty(value) ? DBNull.Value : (object)value;
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, float value)
        {
            var param = new NpgsqlParameter(key, NpgsqlDbType.Real);
            param.Value = value;
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, ulong value, int size = 20)
        {
            var param = new NpgsqlParameter(key, NpgsqlDbType.Numeric, size);
            param.Value = (decimal)value;
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, decimal value, int size = 20)
        {
            var param = new NpgsqlParameter(key, NpgsqlDbType.Numeric, size);
            param.Value = value;
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, long value)
        {
            var param = new NpgsqlParameter(key, NpgsqlDbType.Bigint);
            param.Value = value;
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, long? value)
        {
            var param = new NpgsqlParameter(key, NpgsqlDbType.Bigint);
            param.Value = value.HasValue ? (object)value : DBNull.Value;
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, byte[] value)
        {
            var param = new NpgsqlParameter(key, NpgsqlDbType.Bytea);
            param.Value = value;
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, DateTime value, NpgsqlDbType parameterType = NpgsqlDbType.Timestamp)
        {
            var param = new NpgsqlParameter(key, parameterType);
            param.Value = value;
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, int value)
        {
            var param = new NpgsqlParameter(key, NpgsqlDbType.Integer);
            param.Value = value;
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, OperationType value)
        {
            var param = new NpgsqlParameter(key, NpgsqlDbType.Smallint);
            param.Value = (byte)value;
            collection.Add(param);
        }


        public static void WriteValue(this NpgsqlBinaryImporter importer, long value)
        {
            importer.Write(value, NpgsqlDbType.Bigint);
        }

        public static void WriteValue(this NpgsqlBinaryImporter importer, int value)
        {
            importer.Write(value, NpgsqlDbType.Integer);
        }

        public static void WriteValue(this NpgsqlBinaryImporter importer, short value)
        {
            importer.Write(value, NpgsqlDbType.Smallint);
        }

        public static void WriteValue(this NpgsqlBinaryImporter importer, DateTime value)
        {
            importer.Write(value, NpgsqlDbType.Timestamp);
        }

        public static void WriteValue(this NpgsqlBinaryImporter importer, string value)
        {
            if (string.IsNullOrEmpty(value))
                importer.Write(string.Empty, NpgsqlDbType.Text);
            else
                importer.Write(value, NpgsqlDbType.Text);
        }

        public static void WriteValue(this NpgsqlBinaryImporter importer, string value, int len)
        {
            if (string.IsNullOrEmpty(value))
                importer.Write(DBNull.Value, NpgsqlDbType.Varchar);
            else
                importer.Write(value, NpgsqlDbType.Varchar);
        }

        public static void WriteValue(this NpgsqlBinaryImporter importer, OperationType value)
        {
            importer.Write((byte)value, NpgsqlDbType.Smallint);
        }


        public static T GetValueOrDefult<T>(this DbDataReader reader, int col)
        {
            var value = reader.GetValue(col);
            if (value == DBNull.Value)
                return default(T);

            return (T)value;
        }
    }
}
