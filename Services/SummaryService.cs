using AVC.Collections;
using AVC.DatabaseModels;
using AVC.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AVC.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly ISummaryCollection _totalCollection;

        public SummaryService(ISummaryCollection totalCollection)
        {
            _totalCollection = totalCollection;
        }
        public Task CreateAsync(Summary obj)
        {
            return _totalCollection.CreateAsync(obj);
        }

        public Task CreateManyAsync(IEnumerable<Summary> obj)
        {
            return _totalCollection.CreateManyAsync(obj);
        }

        public Task<Summary> DeleteByIdAsync(string id)
        {
            return _totalCollection.DeleteByIdAsync(id);
        }

        public Task<Summary> UpdateByIdAsync(string id, Summary obj)
        {
            return _totalCollection.UpdateByIdAsync(id, obj);
        }

        public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<Summary> filter)
        {
            return await _totalCollection.DeleteManyAsync(filter);
        }

        public async Task<Summary> FindByIdAsync(string id)
        {
            return await _totalCollection.FindByIdAsync(id);
        }

        public async Task<IEnumerable<Summary>> GetsAsync(FilterDefinition<Summary> filter, FindOptions<Summary, Summary> options)
        {
            return await _totalCollection.GetsAsync(filter, options);
        }

        public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<Summary> filter, UpdateDefinition<Summary> update, UpdateOptions options = null)
        {
            return await _totalCollection.UpdateManyAsync(filter, update, options);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~SummaryService()
        {
            Dispose();
        }
    }
}