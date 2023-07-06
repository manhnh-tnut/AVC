using MongoDB.Bson;
using MongoDB.Driver;
using AVC.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AVC.Collections
{
    public abstract class BaseCollection<TEntity> : Interfaces.ICollection<TEntity> where TEntity : class
    {
        protected readonly IMongoDatabase _database;
        protected readonly IMongoCollection<TEntity> _collection;

        protected BaseCollection(IMongoContext context, string collection = null)
        {
            _database = context.Database;
            _collection = collection == null ? _database.GetCollection<TEntity>(typeof(TEntity).Name) : _database.GetCollection<TEntity>(collection);
        }

        public virtual async Task CreateAsync(TEntity obj)
        {
            await _collection.InsertOneAsync(obj);
        }

        public virtual async Task CreateManyAsync(IEnumerable<TEntity> obj)
        {
            await _collection.InsertManyAsync(obj);
        }

        public virtual async Task<TEntity> FindByIdAsync(string id)
        {
            return await _collection.Find(Builders<TEntity>.Filter.Eq("_id", id)).SingleOrDefaultAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetsAsync(FilterDefinition<TEntity> filter, FindOptions<TEntity, TEntity> options = null)
        {
            return await (await _collection.FindAsync(filter ?? Builders<TEntity>.Filter.Empty, options ?? new FindOptions<TEntity, TEntity>())).ToListAsync();
        }

        public virtual Task<TEntity> UpdateByIdAsync(string id, TEntity obj)
        {
            return _collection.FindOneAndReplaceAsync(Builders<TEntity>.Filter.Eq("_id", id), obj);
        }

        public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, UpdateOptions options = null)
        {
            return await _collection.UpdateManyAsync(filter, update, options);
        }

        public virtual Task<TEntity> DeleteByIdAsync(string id)
        {
            return _collection.FindOneAndDeleteAsync(Builders<TEntity>.Filter.Eq("_id", id));
        }

        public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<TEntity> filter = null)
        {
            return await _collection.DeleteManyAsync(filter);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}