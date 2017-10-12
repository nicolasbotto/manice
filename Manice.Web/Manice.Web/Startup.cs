using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Manice.Web.Startup))]
namespace Manice.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
