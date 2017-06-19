using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MediaGraph.Startup))]
namespace MediaGraph
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
