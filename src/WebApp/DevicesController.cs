using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Newtonsoft.Json;
using Shared;
using Device = DigitalHouse.Device;

namespace WebApp
{
    [RoutePrefix("api/devices")]
    public class DevicesController : ApiController
    {
        private readonly RegistryManager _manager;

        public DevicesController()
        {
            _manager = RegistryManager.CreateFromConnectionString(ConfigurationManager.ConnectionStrings["IoTHub"].ConnectionString);
        }

        [Route(""), HttpGet, ResponseType(typeof(IEnumerable<Device>))]
        public async Task<IEnumerable<Device>> GetAll()
        {
            var devices = await _manager.GetDevicesAsync(1000);
            return devices.Select(x => new Device { Id = new Guid(x.Id), Key = x.Authentication.SymmetricKey.PrimaryKey });
        }
        [Route(""), HttpPost, ResponseType(typeof(Device))]

        public async Task<Device> Post()
        {
            var device = await _manager.AddDeviceAsync(new Microsoft.Azure.Devices.Device(Guid.NewGuid().ToString()));
            return new Device { Id = new Guid(device.Id), Key = device.Authentication.SymmetricKey.PrimaryKey };
        }

        [Route("{id}"), HttpGet, ResponseType(typeof(Device))]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            try
            {
                var device = await _manager.GetDeviceAsync(id.ToString());
                return Ok(new Device { Id = new Guid(device.Id), Key = device.Authentication.SymmetricKey.PrimaryKey });
            }
            catch (DeviceNotFoundException)
            {
                return NotFound();
            }
        }

        [Route("{id}"), HttpPost]
        public async Task ExecuteCommand(Guid id, DeviceCommand command)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["IoTHub"].ConnectionString;
            var client = ServiceClient.CreateFromConnectionString(connectionString);
            var json = JsonConvert.SerializeObject(command);
            var jsonBInary = Encoding.ASCII.GetBytes(json);
            var message = new Message(jsonBInary);
            await client.SendAsync(id.ToString(), message);
        }

        [Route("{id}"), HttpDelete]
        public async Task Delete(Guid id)
        {
            await _manager.RemoveDeviceAsync(id.ToString());
        }
    }
}