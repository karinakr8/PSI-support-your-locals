using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using SupportYourLocals.Data;
using System.Threading.Tasks;

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
        public async Task<List<T>> Get()
        {
            return await _storage.GetAllData();
        }

        [HttpGet("{id}")]
        public async Task<T> Get(string id)
        {
            return await _storage.GetData(id);
        }

        [HttpGet]
        [Route("/api/[controller]/count")]
        public async Task<int> GetCount()
        {
            return await _storage.GetDataCount();
        }

        [HttpPost]
        public async Task Post([FromBody] T value)
        {
            await _storage.AddData(value);
            await _storage.SaveData();
        }

        [HttpPut]
        public async Task Put([FromBody] T value)
        {
            await _storage.UpdateData(value);
            await _storage.SaveData();
        }

        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            await _storage.RemoveData(id);
            await _storage.SaveData();
        }
    }
}
