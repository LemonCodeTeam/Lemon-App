using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LemonLib.Helpers
{
    internal class lmExtension
    {
        public Type ExtensionClass { get; set; }
        //public string Name {  get; set; }
        //public string AssemblyName { get; set; }
        public Version AssemblyVersion { get; set; }
        public string MethodName { get; set; }
        public object Invoke(object[] para)
        {
            return ExtensionClass.GetMethod(MethodName).Invoke(null, para);
        }
    }
}
