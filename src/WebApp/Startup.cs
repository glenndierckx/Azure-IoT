using System.Diagnostics;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Swashbuckle.Application;
using WebApp;

[assembly: OwinStartup(typeof(Startup))]

namespace WebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();
            httpConfig
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "A title for your API");
                })
                .EnableSwaggerUi();
            httpConfig.MapHttpAttributeRoutes();
            app.UseWebApi(httpConfig);
            app.MapSignalR();
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
    public class MyHub : Hub
    {
        public void Send(bool test)
        {
            Debug.WriteLine("Turn " + test);
            Clients.All.addMessage("Hello");
        }
    }
}
