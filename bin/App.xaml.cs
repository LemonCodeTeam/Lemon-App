using LemonLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace bin
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App() {
            if (Directory.Exists(InfoHelper.GetPath()) == false)
                Directory.CreateDirectory(InfoHelper.GetPath());
        }
    }
}
