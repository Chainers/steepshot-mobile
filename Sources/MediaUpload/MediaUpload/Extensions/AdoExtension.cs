using System;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;

namespace MediaUpload.Extensions
{
    public static class AdoExtension
    {
        public static readonly DateTime DefaultDateTime = new DateTime(1970, 1, 1);

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

        #region NpgsqlParameterCollection

        public static void AddValue(this NpgsqlParameterCollection collection, string key, string value)
        {
            var param = new NpgsqlParameter<string>(key, NpgsqlDbType.Text) { TypedValue = value };
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, ulong value)
        {
            var param = new NpgsqlParameter<decimal>(key, NpgsqlDbType.Numeric) { TypedValue = value };
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, decimal value)
        {
            var param = new NpgsqlParameter<decimal>(key, NpgsqlDbType.Numeric) { TypedValue = value };
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, long value)
        {
            var param = new NpgsqlParameter<long>(key, NpgsqlDbType.Bigint) { TypedValue = value };
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, byte[] value)
        {
            var param = new NpgsqlParameter<byte[]>(key, NpgsqlDbType.Bytea) { TypedValue = value };
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, int value)
        {
            var param = new NpgsqlParameter<int>(key, NpgsqlDbType.Integer) { TypedValue = value };
            collection.Add(param);
        }

        public static void AddValue(this NpgsqlParameterCollection collection, string key, Guid value)
        {
            var param = new NpgsqlParameter<Guid>(key, NpgsqlDbType.Uuid) { TypedValue = value };
            collection.Add(param);
        }

        #endregion

        #region DbDataReader

        public static long GetLongOrDefault(this DbDataReader reader, int col)
        {
            if (reader.IsDBNull(col))
                return 0;
            return reader.GetFieldValue<long>(col);
        }

        public static long? GetLongOrNull(this DbDataReader reader, int col)
        {
            if (reader.IsDBNull(col))
                return null;
            return reader.GetFieldValue<long>(col);
        }

        public static DateTime GetDateTimeOrDefault(this DbDataReader reader, int col)
        {
            if (reader.IsDBNull(col))
                return DefaultDateTime;
            return reader.GetFieldValue<DateTime>(col);
        }

        public static decimal GetDecimalOrDefault(this DbDataReader reader, int col)
        {
            if (reader.IsDBNull(col))
                return 0;
            return reader.GetFieldValue<decimal>(col);
        }

        public static byte[] GetBytesOrDefault(this DbDataReader reader, int col)
        {
            if (reader.IsDBNull(col))
                return new byte[0];
            return reader.GetFieldValue<byte[]>(col);
        }

        public static int GetIntegerOrDefault(this DbDataReader reader, int col)
        {
            if (reader.IsDBNull(col))
                return 0;
            return reader.GetFieldValue<int>(col);
        }

        public static string GetStringOrDefault(this DbDataReader reader, int col)
        {
            if (reader.IsDBNull(col))
                return string.Empty;
            return reader.GetFieldValue<string>(col);
        }

        public static ulong GetULongOrDefault(this DbDataReader reader, int col)
        {
            if (reader.IsDBNull(col))
                return 0;
            return (ulong)reader.GetFieldValue<decimal>(col);
        }

        #endregion

        #region NpgsqlBinaryImporter

        public static void WriteValue(this NpgsqlBinaryImporter binaryImporter, long? value)
        {
            if (value.HasValue)
                binaryImporter.Write(value.Value, NpgsqlDbType.Bigint);
            else
                binaryImporter.WriteNull();
        }

        public static void WriteValue(this NpgsqlBinaryImporter binaryImporter, DateTime value)
        {
            binaryImporter.Write(value, NpgsqlDbType.Timestamp);
        }

        public static void WriteValue(this NpgsqlBinaryImporter binaryImporter, DateTime value, NpgsqlDbType npgsqlDbType)
        {
            binaryImporter.Write(value, npgsqlDbType);
        }

        public static void WriteValue(this NpgsqlBinaryImporter binaryImporter, byte[] value)
        {
            binaryImporter.Write(value, NpgsqlDbType.Bytea);
        }

        public static void WriteValue(this NpgsqlBinaryImporter binaryImporter, long value)
        {
            binaryImporter.Write(value, NpgsqlDbType.Bigint);
        }

        public static void WriteValue(this NpgsqlBinaryImporter binaryImporter, string value)
        {
            binaryImporter.Write(value, NpgsqlDbType.Text);
        }

        public static void WriteValue(this NpgsqlBinaryImporter binaryImporter, string value, NpgsqlDbType npgsqlDbType)
        {
            binaryImporter.Write(value, npgsqlDbType);
        }

        public static void WriteValue(this NpgsqlBinaryImporter binaryImporter, int value)
        {
            binaryImporter.Write(value, NpgsqlDbType.Integer);
        }

        public static void WriteValue(this NpgsqlBinaryImporter binaryImporter, ulong value)
        {
            binaryImporter.Write((decimal)value, NpgsqlDbType.Numeric);
        }

        public static void WriteValue(this NpgsqlBinaryImporter binaryImporter, decimal value)
        {
            binaryImporter.Write(value, NpgsqlDbType.Numeric);
        }


        #endregion
    }
}
