using AVC.Collections;
using AVC.DatabaseModels;
using AVC.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AVC.Services
{
    public class MachineService : IMachineService
    {
        private readonly IMachineCollection _machineCollection;

        public MachineService(IMachineCollection machineCollection)
        {
            _machineCollection = machineCollection;
        }
        public Task CreateAsync(Machine obj)
        {
            return _machineCollection.CreateAsync(obj);
        }

        public Task CreateManyAsync(IEnumerable<Machine> obj)
        {
            return _machineCollection.CreateManyAsync(obj);
        }

        public Task<Machine> DeleteByIdAsync(string id)
        {
            return _machineCollection.DeleteByIdAsync(id);
        }

        public Task<Machine> UpdateByIdAsync(string id, Machine obj)
        {
            return _machineCollection.UpdateByIdAsync(id, obj);
        }

        public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<Machine> filter)
        {
            return await _machineCollection.DeleteManyAsync(filter);
        }

        public async Task<Machine> FindByIdAsync(string id)
        {
            return await _machineCollection.FindByIdAsync(id);
        }

        public async Task<IEnumerable<Machine>> GetsAsync(FilterDefinition<Machine> filter, FindOptions<Machine, Machine> options)
        {
            return await _machineCollection.GetsAsync(filter, options);
        }

        public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<Machine> filter, UpdateDefinition<Machine> update, UpdateOptions options = null)
        {
            return await _machineCollection.UpdateManyAsync(filter, update, options);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~MachineService()
        {
            Dispose();
        }
    }
}