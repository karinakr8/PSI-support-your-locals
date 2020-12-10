using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using SupportYourLocals.Data;

namespace SupportYourLocals.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class GenericDataController<T> : ControllerBase where T : GenericData
    {
        private IDataStorage<T> _storage;

        public GenericDataController(IDataStorage<T> storage)
        {
            _storage = storage;
        }

        [HttpGet]
        public IEnumerable<T> Get()
        {
            return _storage.GetAllData();
        }

        [HttpGet("{id}")]
        public T Get(string id)
        {
            return _storage.GetData(id);
        }

        [HttpPost]
        public void Post([FromBody] T value)
        {
            _storage.AddData(value);
            _storage.SaveData();
        }

        [HttpPut("{id}")]
        public void Put([FromBody] T value)
        {
            _storage.UpdateData(value);
            _storage.SaveData();
        }

        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            _storage.RemoveData(id);
            _storage.SaveData();
        }
    }
}
