using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.Misc;

namespace LemonLib
{
    /// <summary>
    /// 封装bass.dll 的音乐播放器
    /// </summary>
    public class MusicPlayer
    {
        /// <summary>
        /// 播放流
        /// </summary>
        private int stream = -1024;
        /// <summary>
        /// 解码流
        /// </summary>
        private int decode = -1024;
        private IntPtr wind;
        public MusicPlayer(IntPtr win)
        {
            wind = win;
            BassNet.Registration("lemon.app@qq.com", "2X52325160022");
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_CPSPEAKERS, win);
        }
        public static async Task PrepareDll() 
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bass.dll") || !File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\bass_fx.dll"))
            {
                if (Environment.Is64BitProcess)
                    await ReleaseDLLFiles(Properties.Resources.bass, Properties.Resources.bass_fx);
                else await ReleaseDLLFiles(Properties.Resources.bass_x86, Properties.Resources.bass_fx_x86);
            }
        }
        private static async Task ReleaseDLLFiles(byte[] maindll,byte[] fxdll) {
            FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\bass.dll", FileMode.Create);
            byte[] datas = maindll;
            await fs.WriteAsync(datas, 0, datas.Length);
            await fs.FlushAsync();
            fs.Close();

            FileStream fss = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\bass_fx.dll", FileMode.Create);
            byte[] datas2 = fxdll;
            await fss.WriteAsync(datas2, 0, datas2.Length);
            await fss.FlushAsync();
            fss.Close();
        }
        /// <summary>
        /// 音量 value:0~1
        /// </summary>
        public float VOL
        {
            get {
                float value = 0;
                Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, ref value);
                return value;
            }
            set {
                if (stream != -1024)
                {
                    Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, value);
                    _Vol = value;
                }
            }
        }

        /// <summary>
        /// VOL 0~1
        /// </summary>
        private float _Vol = 1;
        /// <summary>
        /// Speed
        /// </summary>
        private float _Speed = -1024;
        private float _Pitch = -1024;

        /// <summary>
        /// 播放速度 
        /// </summary>
        public float Speed {
            get {
                float value = 0;
                Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO, ref value);
                return value;
            }
            set
            {
                if (stream != -1024)
                {
                    Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO, value);
                    _Speed = value;
                }
            }
        }

        /// <summary>
        /// 播放频度 (音调高低) 
        /// </summary>
        public float Pitch {
            get {
                float value = 0;
                Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, ref value);
                _Pitch = value;
                return value;
            }
            set {
                if (stream != -1024)
                {
                    Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, value);
                    _Pitch = value;
                }
            }
        }

        /// <summary>
        /// 保存当前的音频文件 包括Fx效果器
        /// </summary>
        /// <param name="file"></param>
        /// <param name="finished"></param>
        public async void SaveToFile(string file,Action finished)
        {
            await Task.Factory.StartNew(() =>
            {
                //从文件中读取解码流
                int strm = Bass.BASS_StreamCreateFile(_file, 0, 0, BASSFlag.BASS_STREAM_DECODE);
                //从strm解码流中创建FX效果器
                strm = BassFx.BASS_FX_TempoCreate(strm, BASSFlag.BASS_STREAM_DECODE);

                //为效果器设置参数  Pitch&Speed
                if (_Pitch != -1024) Bass.BASS_ChannelSetAttribute(strm, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, _Pitch);
                if (_Speed != -1024) Bass.BASS_ChannelSetAttribute(strm, BASSAttribute.BASS_ATTRIB_TEMPO, _Speed);

                //初始化编码器
                EncoderLAME l = new EncoderLAME(strm);
                l.InputFile = null; //STDIN
                l.OutputFile = file;//输出文件路径
                l.LAME_Bitrate = (int)EncoderLAME.BITRATE.kbps_128;//比特率
                l.LAME_Mode = EncoderLAME.LAMEMode.Default;//默认模式
                l.LAME_Quality = EncoderLAME.LAMEQuality.Quality;//高品质
                l.LAME_TargetSampleRate = (int)EncoderLAME.SAMPLERATE.Hz_44100;//44100码率

                //解码流开始(并不是播放，也不会有声音输出)
                Bass.BASS_ChannelPlay(strm, false);
                //开始编码
                l.Start(null, IntPtr.Zero, false);

                byte[] encBuffer = new byte[65536]; // our dummy encoder buffer
                while (Bass.BASS_ChannelIsActive(strm) == BASSActive.BASS_ACTIVE_PLAYING)
                {
                    // getting sample data will automatically feed the encoder
                    int len = Bass.BASS_ChannelGetData(strm, encBuffer, encBuffer.Length);
                }
                l.Stop();  // finish
                Bass.BASS_StreamFree(strm);
                finished();
            });
        }

        private string _file;
        /// <summary>
        /// 从文件中加载
        /// </summary>
        /// <param name="file"></param>
        public void Load(string file)
        {
            _file = file;
            decode = Bass.BASS_StreamCreateFile(file, 0L, 0L, BASSFlag.BASS_STREAM_DECODE);
            stream = BassFx.BASS_FX_TempoCreate(decode, BASSFlag.BASS_SAMPLE_FLOAT);
            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, _Vol);
            if (_Pitch != -1024) Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, _Pitch);
            if (_Speed!=-1024) Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO, _Speed);
        }
        public List<BASSDL> BassdlList = new List<BASSDL>();
        IntPtr ip = IntPtr.Zero;
        /// <summary>
        /// 从URL中加载
        /// </summary>
        /// <param name="path">缓存文件保存目录</param>
        /// <param name="url"></param>
        /// <param name="proc">下载进度回调 all/now</param>
        /// <param name="finish">下载结束回调</param>
        public void LoadUrl(string path, string url, Action<long, long> proc, Action finish)
        {
            try
            {
                ip = new IntPtr(BassdlList.Count);
                var Bassdl = new BASSDL(path);
                BassdlList.Add(Bassdl);
                Bassdl.procChanged = proc;
                Bassdl.finished = finish;
                Bassdl.downloadfailed += (e) => {

                };
                decode = Bass.BASS_StreamCreateURL(url + "\r\n"
                                               + "Accept-Encoding: identity;q=1, *;q=0\r\n"
                                               + "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.66 Safari/537.36 Edg/80.0.361.40\r\n"
                                               + "Accept: */*\r\n"
                                               + "Accept-Language: zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6"
         , 0, BASSFlag.BASS_STREAM_DECODE, Bassdl._myDownloadProc, ip);
                Bassdl.stream = decode;
                stream = BassFx.BASS_FX_TempoCreate(decode, BASSFlag.BASS_SAMPLE_FLOAT);

                Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, _Vol);
                if (_Pitch != -1024) Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, _Pitch); 
                if (_Speed != -1024) Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO, _Speed);
            }
            catch { }
        }

        public void Play()
        {
            Bass.BASS_ChannelPlay(stream, false);
        }
        public void Pause()
        {
            if (stream != -1024)
                Bass.BASS_ChannelPause(stream);
        }
        public TimeSpan GetLength
        {
            get
            {
                double seconds = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
                return TimeSpan.FromSeconds(seconds);
            }
        }
        public TimeSpan Position
        {
            get
            {
                double seconds = Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetPosition(stream));
                return TimeSpan.FromSeconds(seconds);
            }
            set => Bass.BASS_ChannelSetPosition(stream, value.TotalSeconds);
        }
        /// <summary>
        /// 获取FFT数据   可以用来做频谱
        /// </summary>
        /// <returns></returns>
        public float[] GetFFTData()
        {
            float[] fft = new float[256];
            Bass.BASS_ChannelGetData(stream, fft, (int)BASSData.BASS_DATA_FFT256);
            return fft;
        }
        /// <summary>
        /// 更新设备
        /// </summary>
        public void UpdateDevice()
        {
            var data = Bass.BASS_GetDeviceInfos();
            int index = -1;
            for (int i = 0; i < data.Length; i++)
                if (data[i].IsDefault)
                {
                    index = i;
                    break;
                }
            if (!data[index].IsInitialized)
                Bass.BASS_Init(index, 44100, BASSInit.BASS_DEVICE_CPSPEAKERS, wind);
            var a = Bass.BASS_ChannelGetDevice(stream);
            if (a != index)
            {
                Bass.BASS_ChannelSetDevice(stream, index);
                Bass.BASS_SetDevice(index);
            }
        }
        /// <summary>
        /// 释放Bass解码器
        /// </summary>
        public void Free()
        {
            Bass.BASS_ChannelStop(stream);
            Bass.BASS_StreamFree(stream);
            Bass.BASS_Stop();
            Bass.BASS_Free();
        }
    }

    /// <summary>
    /// Bass 从URL中加载并缓存的辅助类
    /// </summary>
    public class BASSDL
    {
        public DOWNLOADPROC _myDownloadProc;
        private FileStream _fs = null;
        private byte[] _data; // local data buffer
        private string DLPath;
        public Action<long, long> procChanged = null;
        public Action finished = null;
        public Action<long> downloadfailed = null;
        public int stream;
        private bool HasStoped = false;
        public BASSDL(string path)
        {
            _myDownloadProc = new DOWNLOADPROC(DownloadCallBack);
            DLPath = path;
        }
        /// <summary>
        /// 指示关闭此下载任务
        /// bass官网申明  不可停止下载   故切断下载回调 (若强制关闭则抛异常)
        /// </summary>
        public void SetClose()
        {
            HasStoped = true;
            procChanged = null;
            finished = null;

        }
        /// <summary>
        /// 由Bass调用   传来下载数据时 将数据保存到缓存文件中
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <param name="user"></param>
        private void DownloadCallBack(IntPtr buffer, int length, IntPtr user)
        {
            // file length
            long len = Bass.BASS_StreamGetFilePosition(stream, BASSStreamFilePosition.BASS_FILEPOS_END);
            // download progress
            long down = Bass.BASS_StreamGetFilePosition(stream, BASSStreamFilePosition.BASS_FILEPOS_DOWNLOAD);
            procChanged?.Invoke(len, down);
            //finished downloading
            if (_fs == null)
            {
                // create the file
                _fs = File.OpenWrite(DLPath + ".cache");
            }
            if (buffer == IntPtr.Zero)
            {
                // finished downloading
                _fs.Flush();
                _fs.Close();
                _fs = null;
                FileInfo fi = new FileInfo(DLPath + ".cache");
                if (fi.Length != len)
                {
                    MainClass.DebugCallBack(fi.Length.ToString(), len.ToString());
                    fi.Delete();
                    if (!HasStoped)
                    {
                        //没有被停止而是链接下载失败
                        downloadfailed(down);
                    }
                }
                else
                {
                    fi.MoveTo(DLPath, true);
                    finished?.Invoke();
                }
            }
            else
            {
                // increase the data buffer as needed
                if (_data == null || _data.Length < length)
                    _data = new byte[length];
                // copy from managed to unmanaged memory
                Marshal.Copy(buffer, _data, 0, length);
                // write to file
                _fs.Write(_data, 0, length);
            }
        }
    }
}
