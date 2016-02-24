using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Shared;

namespace WebApp
{
    [RoutePrefix("api/deviceupdate")]
    public class DeviceUpdateController : ApiController
    {
        [Route(""), HttpPost]
        public IHttpActionResult NewMessage(DevicePowerUsage message)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
            context.Clients.All.deviceUpdate(message);
            return Ok();
        }
    }
}
