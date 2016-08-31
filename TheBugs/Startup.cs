using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TheBugs.Startup))]
namespace TheBugs
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
