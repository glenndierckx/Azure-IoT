using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Shared;

namespace WebApp
{
    [RoutePrefix("api/devices")]
    public class DevicesController : ApiController
    {
        private readonly RegistryManager _manager;
        private readonly string _connectionString;

        public DevicesController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["IoTHub"].ConnectionString;
            _manager = RegistryManager.CreateFromConnectionString(_connectionString);
        }

        [Route(""), HttpGet, ResponseType(typeof(IEnumerable<MyDevice>))]
        public async Task<IEnumerable<MyDevice>> GetAll()
        {
            var devices = await _manager.GetDevicesAsync(1000);
            return devices.Select(x => new MyDevice { Id = new Guid(x.Id), Key = x.Authentication.SymmetricKey.PrimaryKey });
        }
        [Route(""), HttpPost, ResponseType(typeof(MyDevice))]

        public async Task<MyDevice> Post()
        {
            var device = await _manager.AddDeviceAsync(new Microsoft.Azure.Devices.Device(Guid.NewGuid().ToString()));
            return new MyDevice { Id = new Guid(device.Id), Key = device.Authentication.SymmetricKey.PrimaryKey };
        }

        [Route("{id}"), HttpGet, ResponseType(typeof(MyDevice))]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            try
            {
                var device = await _manager.GetDeviceAsync(id.ToString());
                return Ok(new MyDevice { Id = new Guid(device.Id), Key = device.Authentication.SymmetricKey.PrimaryKey });
            }
            catch (DeviceNotFoundException)
            {
                return NotFound();
            }
        }

        [Route("{id}"), HttpDelete]
        public async Task Delete(Guid id)
        {
            await _manager.RemoveDeviceAsync(id.ToString());
        }
    }
}