using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace LemonLib.Helpers
{
    internal class lmExtension
    {
        public lmExtension(string file,string assemblyName, string methodName)
        {
            FilePath = file;
            AssemblyName = assemblyName;
            MethodName = methodName;
        }
        public void Load()
        {
            ExtensionDomain=new AssemblyLoadContext(AssemblyName,true);
            FileStream stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            ExtensionDomain.LoadFromStream(stream);
            ExAssembly = ExtensionDomain.Assemblies.First();
            AssemblyVersion = ExAssembly.GetName().Version;
            stream.Dispose();
        }
        private string FilePath { get; set; }
        private Assembly ExAssembly { get; set; }
        private AssemblyLoadContext ExtensionDomain { get; set; }
        public string AssemblyName { get; private set; }
        public Version AssemblyVersion { get; private set; }
        public string MethodName { get; private set; }
        public void Unload()
        {
            ExtensionDomain.Unload();
        }
        public object Invoke(object[] para)
        {
            return ExAssembly.GetType(AssemblyName).GetMethod(MethodName).Invoke(null, para);
        }
    }
}
