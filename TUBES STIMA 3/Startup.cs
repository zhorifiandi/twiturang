using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TUBES_STIMA_3.Startup))]
namespace TUBES_STIMA_3
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
