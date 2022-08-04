using System.Reflection;
using System.Runtime.Loader;

namespace SteamLauncher.Tools
{
    public class DisposableAssemblyLoadContext : AssemblyLoadContext
    {
        public DisposableAssemblyLoadContext() : base(isCollectible: true)
        {

        }

        protected override Assembly Load(AssemblyName assemblyName) => null;
    }
}
