using LemonLibrary;
using Lierda.WPFHelper;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Lemon_App
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        LierdaCracker cracker = new LierdaCracker();
        protected override void OnStartup(StartupEventArgs e)
        {
            cracker.Cracker();
            base.OnStartup(e);
        }
        public App() {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string i = "\r\n柠萌账号:" + Settings.USettings.UserName + "\r\n柠萌版本:5.0" + "\r\n" + ((Exception)e.ExceptionObject).Message + "\r\n 导致错误的对象名称:" + ((Exception)e.ExceptionObject).Source + "\r\n 引发异常的方法:" + ((Exception)e.ExceptionObject).TargetSite + "\r\n  帮助链接:" + ((Exception)e.ExceptionObject).HelpLink + "\r\n 调用堆:" + ((Exception)e.ExceptionObject).StackTrace;
            FileStream fs = new FileStream(InfoHelper.GetPath() + @"Log.log", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(i);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            string i = "\r\n柠萌账号:" + Settings.USettings.UserName + "\r\n柠萌版本:5.0"+ "\r\n" + ((Exception)e.Exception).Message + "\r\n 导致错误的对象名称:" + ((Exception)e.Exception).Source + "\r\n 引发异常的方法:" + ((Exception)e.Exception).TargetSite + "\r\n  帮助链接:" + ((Exception)e.Exception).HelpLink + "\r\n 调用堆:" + ((Exception)e.Exception).StackTrace;
            FileStream fs = new FileStream(InfoHelper.GetPath() + @"Log.log", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(i);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0)
                Shutdown();
            else {
                var qq = e.Args[0];
                new Task(new Action(async delegate
                {
                    if (File.Exists(InfoHelper.GetPath() + qq + ".st"))
                        Settings.LoadUSettings(Encoding.Default.GetString(Convert.FromBase64String(TextHelper.TextDecrypt(File.ReadAllText(InfoHelper.GetPath() + qq + ".st"), LemonLibrary.TextHelper.MD5.EncryptToMD5string(qq + ".st")))));
                    else Settings.SaveSettings(qq);
                    var sl = TextHelper.XtoYGetTo(await HttpHelper.GetWebAsync("http://r.pengyou.com/fcg-bin/cgi_get_portrait.fcg?uins=" + qq, Encoding.Default), "portraitCallBack(", ")", 0);
                    JObject o = JObject.Parse(sl);
                    await HttpHelper.HttpDownloadFileAsync($"http://q2.qlogo.cn/headimg_dl?bs=qq&dst_uin={qq}&spec=100", InfoHelper.GetPath() + qq + ".jpg");
                    Settings.USettings.UserName = o[qq][6].ToString();
                    Settings.USettings.UserImage = InfoHelper.GetPath() + qq + ".jpg";
                    Settings.USettings.LemonAreeunIts = qq;
                    Settings.SaveSettings();
                    Dispatcher.Invoke(new Action(delegate { new MainWindow().Show(); }));
                })).Start();
            }
        }
    }
}
