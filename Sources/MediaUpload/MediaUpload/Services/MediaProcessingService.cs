using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ipfs.CoreApi;
using Ipfs.Http;
using MediaUpload.DataAccess;
using MediaUpload.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MediaUpload.Services
{
    public class MediaProcessingService : BaseDbService
    {
        private static readonly IpfsClient Ipfs = new IpfsClient();
        public static Queue<MediaModel> IpfsQueue = new Queue<MediaModel>();
        public static Queue<MediaModel> AwsQueue = new Queue<MediaModel>();


        public MediaProcessingService(ILogger<MediaProcessingService> logger, IConfiguration configuration)
            : base(logger, configuration)
        {
        }


        public MediaModel Enqueue(MediaModel model)
        {
            NpgsqlConnection connection = null;
            try
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                connection = new NpgsqlConnection(connectionString);
                connection.Open();
                connection.InsertNewMedia(model);
                connection.Close();

                lock (IpfsQueue)
                    IpfsQueue.Enqueue(model);

                if (model.Aws)
                {
                    lock (AwsQueue)
                        AwsQueue.Enqueue(model);
                }

                return model;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{ex.Message}: {model.Id} - {model.FilePath}");
                TryDeleteFile(model.FilePath);
            }
            finally
            {
                if (connection != null)
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }

            return null;
        }

        private void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{ex.Message}: {path}");
            }
        }

        protected override Task DoSomethingAsync(NpgsqlConnection connection, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        private async Task<bool> SaveToIpfsAsync(MediaModel model, CancellationToken token)
        {
            FileStream fs = null;
            try
            {
                var options = new AddFileOptions();
                if (!model.Ipfs)
                    options.OnlyHash = true;

                fs = new FileStream(model.FilePath, FileMode.Open, FileAccess.Read);

                var result = await Ipfs.FileSystem.AddAsync(fs, model.FileName, options, token);
                model.IpfsHash = result.Id;
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
            }
            finally
            {
                fs?.Close();
            }
            return false;
        }
    }
}
