using System;
using Windows.Media;
using Windows.Storage.Streams;

namespace LemonLib
{
    public enum SMTCMediaStatus
    {
        Playing,
        Paused,
        Stopped
    }
    public class SMTCUpdater
    {
        private readonly SystemMediaTransportControlsDisplayUpdater _updater;
        public SMTCUpdater(SystemMediaTransportControlsDisplayUpdater Updater,string AppMediaId)
        {
            _updater= Updater;
            _updater.AppMediaId = AppMediaId;
            _updater.Type= MediaPlaybackType.Music;
        }
        public SMTCUpdater SetTitle(string title)
        {
            _updater.MusicProperties.Title = title;
            return this;
        }
        public SMTCUpdater SetAlbumTitle(string albumTitle)
        {
            _updater.MusicProperties.AlbumTitle = albumTitle;
            return this;
        }
        public SMTCUpdater SetArtist(string artist)
        {
            _updater.MusicProperties.Artist = artist;
            return this;
        }
        public SMTCUpdater SetThumbnail(string ImgUrl)
        {
            _updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(ImgUrl));
            return this;
        }
        public void Update()=>_updater.Update();
    }
    public class SMTCCreator:IDisposable
    {
        private readonly Windows.Media.Playback.MediaPlayer _player = new();
        private readonly SystemMediaTransportControls _smtc;
        private readonly SMTCUpdater _updater;
        public SMTCCreator(string MediaId)
        {
            //先禁用系统播放器的命令
            _player.CommandManager.IsEnabled = false;
            //直接创建SystemMediaTransportControls对象被平台限制，神奇的是MediaPlayer对象可以创建该NativeObject
            _smtc = _player.SystemMediaTransportControls;
            _updater=new SMTCUpdater(_smtc.DisplayUpdater, MediaId);

            //启用状态
            _smtc.IsEnabled = true;
            _smtc.IsPlayEnabled = true;
            _smtc.IsPauseEnabled = true;
            _smtc.IsNextEnabled = true;
            _smtc.IsPreviousEnabled = true;
            //响应系统播放器的命令
            _smtc.ButtonPressed += _smtc_ButtonPressed;
        }
        public void Dispose()
        {
            _smtc.IsEnabled = false;
            _player.Dispose();
        }
        public SMTCUpdater Info { get => _updater; }
        public event EventHandler PlayOrPause, Previous, Next;
        public void SetMediaStatus(SMTCMediaStatus status)
        {
            _smtc.PlaybackStatus = status switch
            {
                SMTCMediaStatus.Playing => MediaPlaybackStatus.Playing,
                SMTCMediaStatus.Paused => MediaPlaybackStatus.Paused,
                SMTCMediaStatus.Stopped => MediaPlaybackStatus.Stopped,
                _ => throw new NotImplementedException(),
            };
        }


        private void _smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch(args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    PlayOrPause?.Invoke(this, EventArgs.Empty);
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    PlayOrPause?.Invoke(this, EventArgs.Empty);
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Next?.Invoke(this, EventArgs.Empty);
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Previous?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }
    }
}
