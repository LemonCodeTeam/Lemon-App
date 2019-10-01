using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace LemonLibrary
{
    public class MusicPlayer
    {
        private int stream = 0;
        public MusicPlayer(IntPtr win) {
            BassNet.Registration("lemon.app@qq.com", "2X52325160022");
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_CPSPEAKERS, win);
        }
        public void Load(string file) {
            stream = Bass.BASS_StreamCreateFile(file, 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT);
        }
        public void Play() {
            Bass.BASS_ChannelPlay(stream,false);
        }
        public void Pause() {
            Bass.BASS_ChannelPause(stream);
        }
        public TimeSpan GetLength {
            get {
                double seconds =Bass.BASS_ChannelBytes2Seconds(stream, Bass.BASS_ChannelGetLength(stream));
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
            float[] fft = new float[512];
            Bass.BASS_ChannelGetData(stream, fft, (int)BASSData.BASS_DATA_FFT1024);
            return fft;
        }
        public void Free() {
            Bass.BASS_ChannelStop(stream);
            Bass.BASS_StreamFree(stream);
            Bass.BASS_Stop();
            Bass.BASS_Free();
        }
    }
}
