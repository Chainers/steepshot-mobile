using System;
using MediaUpload.Extensions;
using MediaUpload.Models;
using Npgsql;

namespace MediaUpload.DataAccess
{
    public static class MediaAccessor
    {
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);

        public static void InsertNewMedia(this NpgsqlConnection connection, MediaModel model)
        {
            model.Id = GetNextGuid();

            var cmd = "INSERT INTO public.media(id) VALUES (@p1);";

            var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = cmd
            };
            command.Parameters.AddValue("@p1", model.Id);
            command.ExecuteNonQuery();
        }

        private static Guid GetNextGuid()
        {
            var guidBytesLength = 16;
            var guid = new byte[guidBytesLength];
            var datetime = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
            Buffer.BlockCopy(datetime, 0, guid, 0, datetime.Length);

            var tail = new byte[guidBytesLength - datetime.Length];
            Random.NextBytes(tail);
            Buffer.BlockCopy(tail, 0, guid, datetime.Length, tail.Length);
            return new Guid(guid);
        }
    }
}
