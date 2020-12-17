using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using SupportYourLocals.Data;

namespace SupportYourLocals.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class GenericDataController<T> : ControllerBase where T : GenericData
    {
        private readonly IDataStorage<T> _storage;

        // Temporary solution, until we'll have the Entity Framework setup
        private static readonly object dataLock = new object(); 

        public GenericDataController(IDataStorage<T> storage)
        {
            _storage = storage;
        }

        [HttpGet]
        public List<T> Get()
        {
            lock (dataLock)
            {
                return _storage.GetAllData();
            }
        }

        [HttpGet("{id}")]
        public T Get(string id)
        {
            lock (dataLock)
            {
                return _storage.GetData(id);
            }
        }

        [HttpGet("id:int")]
        public int Get(int id)
        {
            lock (dataLock)
            {
                return _storage.GetDataCount();
            }
        }

        [HttpPost]
        public void Post([FromBody] T value)
        {
            lock (dataLock)
            {
                _storage.AddData(value);
                _storage.SaveData();
            }
        }

        [HttpPut]
        public void Put([FromBody] T value)
        {
            lock (dataLock)
            {
                _storage.UpdateData(value);
                _storage.SaveData();
            }
        }

        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            lock (dataLock)
            {
                _storage.RemoveData(id);
                _storage.SaveData();
            }
        }
    }
}
