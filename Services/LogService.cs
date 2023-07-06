using AVC.Collections;
using AVC.DatabaseModels;
using AVC.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AVC.Services
{
    public class LogService : ILogService
    {
        private readonly ILogCollection _logCollection;

        public LogService(ILogCollection logCollection)
        {
            _logCollection = logCollection;
        }
        public Task CreateAsync(Log obj)
        {
            return _logCollection.CreateAsync(obj);
        }

        public Task CreateManyAsync(IEnumerable<Log> obj)
        {
            return _logCollection.CreateManyAsync(obj);
        }

        public Task<Log> DeleteByIdAsync(string id)
        {
            return _logCollection.DeleteByIdAsync(id);
        }

        public Task<Log> UpdateByIdAsync(string id, Log obj)
        {
            return _logCollection.UpdateByIdAsync(id, obj);
        }

        public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<Log> filter)
        {
            return await _logCollection.DeleteManyAsync(filter);
        }

        public async Task<Log> FindByIdAsync(string id)
        {
            return await _logCollection.FindByIdAsync(id);
        }

        public async Task<IEnumerable<Log>> GetsAsync(FilterDefinition<Log> filter, FindOptions<Log, Log> options)
        {
            return await _logCollection.GetsAsync(filter, options);
        }

        public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<Log> filter, UpdateDefinition<Log> update)
        {
            return await _logCollection.UpdateManyAsync(filter, update);
        }

        public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<Log> filter, UpdateDefinition<Log> update, UpdateOptions options = null)
        {
            return await _logCollection.UpdateManyAsync(filter, update, options);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~LogService()
        {
            Dispose();
        }
    }
}