using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.Misc;

namespace LemonLib
{
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
        public void SetVOL(float value)
        {
            if (stream != -1024)
            {
                Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, value);
                _Vol = value;
            }
        }
        public float GetVOL() {
            float value = 0;
            Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, ref value);
            return value;
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

        public void SetSpeed(float value)
        {
            if (stream != -1024)
            {
                Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO, value);
                _Speed = value;
            }
        }
        public float GetSpeed()
        {
            float value = 0;
            Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO, ref value);
            return value;
        }

        public float Pitch {
            get {
                float value = 0;
                Bass.BASS_ChannelGetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, ref value);
                _Speed = value;
                return value;
            }
            set {
                if (stream != -1024)
                {
                    Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, value);
                    _Speed = value;
                }
            }
        }

        public void SaveToFile(string file) {
            int decode = Bass.BASS_StreamCreateFile(_file, 0L, 0L, BASSFlag.BASS_STREAM_DECODE);
            int stream = BassFx.BASS_FX_TempoCreate(decode, BASSFlag.BASS_SAMPLE_FLOAT);
            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, 0);
            if (_Pitch != -1024) Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, _Pitch);
            if (_Speed != -1024) Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_TEMPO, _Speed);
            EncoderLAME l = new EncoderLAME(stream);
            l.InputFile = null; //STDIN
            l.OutputFile = file;
            l.LAME_Bitrate = (int)EncoderLAME.BITRATE.kbps_64;
            l.LAME_Mode = EncoderLAME.LAMEMode.Default;
            l.LAME_Quality = EncoderLAME.LAMEQuality.Q5;
            Bass.BASS_ChannelPlay(stream, false);
            if (l.Start(null, IntPtr.Zero,false))
            {
                // encode the data
                byte[] encBuffer = new byte[65536]; // our dummy encoder buffer (32KB x 16-bit - size it as you like)
                while (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
                {
                    // getting sample data will automatically feed the encoder
                    int len = Bass.BASS_ChannelGetData(stream, encBuffer, encBuffer.Length);
                }
                // finish
                l.Stop();
            }
        }

        private string _file;
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
                                               + "Host: musichy.tc.qq.com\r\n"
                                               + "Accept-Encoding: identity;q=1, *;q=0\r\n"
                                               + "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.66 Safari/537.36 Edg/80.0.361.40\r\n"
                                               + "Accept: */*\r\n"
                                               + "Accept-Language: zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6\r\n"
                                               + "Cookie:" + Settings.USettings.Cookie
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

        public float[] GetFFTData()
        {
            float[] fft = new float[256];
            Bass.BASS_ChannelGetData(stream, fft, (int)BASSData.BASS_DATA_FFT256);
            return fft;
        }
        public void UpdataDevice()
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
        public void Free()
        {
            Bass.BASS_ChannelStop(stream);
            Bass.BASS_StreamFree(stream);
            Bass.BASS_Stop();
            Bass.BASS_Free();
        }
    }

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
        public void SetClose()
        {
            HasStoped = true;
            procChanged = null;
            finished = null;
        }
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
