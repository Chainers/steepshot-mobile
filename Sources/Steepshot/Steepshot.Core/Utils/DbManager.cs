using System;
using SQLite;
using SQLiteNetExtensions.Extensions;
using Steepshot.Core.Jobs;
using Steepshot.Core.Models.Database;

namespace Steepshot.Core.Utils
{
    public class DbManager
    {
        private const string DbPath = "SteepshotSqlLiteDb.db3";
        private readonly SQLiteConnection _db;

        public DbManager()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _db = new SQLiteConnection(System.IO.Path.Combine(folder, DbPath));
            _db.CreateTable<UploadMediaContainer>();
            _db.CreateTable<UploadMediaItem>();
            _db.CreateTable<Job>();
        }

        /// <summary>
        /// Returns a queryable interface to the table represented by the given type.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <returns>A queryable object that is able to translate Where, OrderBy, and Take queries into native SQL.</returns>
        public TableQuery<T> SelectTable<T>()
            where T : new()
        {
            return _db.Table<T>();
        }

        /// <summary>
        /// Attempts to retrieve an object with the given primary key from the table associated with the specified type. Use of this method requires that the given type have a designated PrimaryKey (using the PrimaryKeyAttribute).
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="id">The primary key.</param>
        /// <returns>The object with the given primary key. Throws a not found exception if the object is not found.</returns>
        public T Select<T>(int id)
            where T : new()
        {
            return _db.GetWithChildren<T>(id);
        }

        /// <summary>
        /// Updates all of the columns of a table using the specified object except for its primary key. The object is required to have a primary key.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="item">The object to update. It must have a primary key designated using the PrimaryKeyAttribute.</param>
        /// <returns> The number of rows updated.</returns>
        public void Update<T>(T item)
        {
            _db.UpdateWithChildren(item);
        }

        /// <summary>
        /// Inserts the given object (and updates its auto incremented primary key if it has one). The return value is the number of rows added to the table.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="item">The object to insert.</param>
        /// <returns>The number of rows added to the table.</returns>
        public void Insert<T>(T item)
        {
            _db.InsertWithChildren(item);
        }

        /// <summary>
        /// Deletes the object with the specified primary key.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="id">The primary key of the object to delete.</param>
        /// <returns>The number of objects deleted.</returns>
        public int Delete<T>(int id)
        {
            return _db.Delete<T>(id);
        }
    }
}
