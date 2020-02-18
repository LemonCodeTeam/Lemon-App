using System;
using System.Collections.Generic;
using Un4seen.Bass;

namespace LemonLib
{
    public class MusicPlayer
    {
        private int stream = -1024;
        private IntPtr wind;
        public MusicPlayer(IntPtr win)
        {
            wind = win;
            BassNet.Registration("lemon.app@qq.com", "2X52325160022");
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_CPSPEAKERS, win);
        }
        public void Load(string file)
        {
            stream = Bass.BASS_StreamCreateFile(file, 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT);
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
                stream = Bass.BASS_StreamCreateURL(url + "\r\n"
                                                   + "Host: musichy.tc.qq.com\r\n"
                                                   + "Connection: keep-alive\r\n"
                                                   + "Accept-Encoding: identity;q=1, *;q=0\r\n"
                                                   + "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.66 Safari/537.36 Edg/80.0.361.40\r\n"
                                                   + "Accept: */*\r\n"
                                                   + "Accept-Language: zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6\r\n"
                                                   + "Cookie:" + Settings.USettings.Cookie
             , 0, BASSFlag.BASS_SAMPLE_FLOAT, Bassdl._myDownloadProc, ip);
                Bassdl.stream = stream;
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
}
