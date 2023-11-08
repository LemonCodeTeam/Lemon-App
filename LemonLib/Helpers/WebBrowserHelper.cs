using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LemonLib
{
    public class WebBrowserHelper
    {
        /// <summary>  
        /// 修改注册表信息来兼容当前程序  
        /// </summary>  
        public static void SetWebBrowserFeatures(int ieVersion)
        {
            // don't change the registry if running in-proc inside Visual Studio  
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;
            var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            UInt32 ieMode = GeoEmulationModee(ieVersion);
            var featureControlRegKey = @"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\";
            //设置浏览器对应用程序（appName）以什么模式（ieMode）运行  
            Registry.SetValue(featureControlRegKey + "FEATURE_BROWSER_EMULATION",
                appName, ieMode, RegistryValueKind.DWord);
            // enable the features which are "On" for the full Internet Explorer browser
            Registry.SetValue(featureControlRegKey + "FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION",
                appName, 1, RegistryValueKind.DWord);
        }
        /// <summary>  
        /// 获取IE内核版本
        /// </summary>  
        /// <returns></returns>  
        public static int GetBrowserVersion()
        {
            int browserVersion = 0;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree,
                System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }
            return browserVersion;
        }
        /// <summary>  
        /// 通过版本得到浏览器模式的值  
        /// </summary>  
        /// <param name="browserVersion"></param>  
        /// <returns></returns>  
        static uint GeoEmulationModee(int browserVersion) => browserVersion switch
        {
            7 => 7000,
            8 => 8000,
            9 => 9000,
            10 => 10000,
            11 => 11000,
            _ => 11000
        };
    }
}
