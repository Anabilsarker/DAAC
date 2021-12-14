using LibVLCSharp.Shared;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace DAAC
{
    /// <summary>
    /// Interaction logic for MediaWindow.xaml
    /// </summary>
    public partial class MediaWindow : Window
    {
        private Dispatcher dispatch;
        public MediaWindow(Dispatcher dis)
        {
            InitializeComponent();
            dispatch = dis;
        }
        
        private bool isPaused = false;
        string mediaLocation;
        public void getMediaLocation(string mediaLocation)
        {
            this.mediaLocation = mediaLocation;
        }

        private void Player_Loaded(object sender, RoutedEventArgs e)
        {
            Core.Initialize();
            LibVLC libVLC = new LibVLC();
            Media media = new Media(libVLC, new Uri(mediaLocation));
            Player.MediaPlayer = new MediaPlayer(media);
            Player.MediaPlayer.SetAudioFormatCallback(AudioSetup, AudioCleanup);
            Player.MediaPlayer.SetAudioCallbacks(PlayAudio, PauseAudio, ResumeAudio, FlushAudio, DrainAudio);
            Player.MediaPlayer.EnableHardwareDecoding = true;
            Player.MediaPlayer.Play();
        }

        void PlayAudio(IntPtr data, IntPtr samples, uint count, long pts)
        {
            int bytes = (int)count * 4; // (16 bit, 2 channels)
            var buffer = new byte[bytes];
            Marshal.Copy(samples, buffer, 0, bytes);
            int segment = 0, unit = (bytes / 4);

            for(int j = 0; j < unit; j++)
            {
                for (int i = 0; i < StreamerServer.Instance.endPoints.Count; i++)
                {
                    if (StreamerServer.Instance.endPoints[i].Status)
                    {
                        try
                        {
                            var stream = StreamerServer.Instance.endPoints[i].Stream;
                            //stream.Write(BitConverter.GetBytes((bytes / unit)), 0, sizeof(int));
                            if(stream.DataAvailable) stream.Flush();
                            stream.Write(buffer, segment, (bytes / unit));
                        }
                        catch
                        {
                            dispatch.Invoke(() => StreamerServer.Instance.endPoints.RemoveAt(i));
                        }
                    }
                }
                segment += (bytes / unit);
            }
        }

        void DrainAudio(IntPtr data)
        {
            for (int i = 0; i < StreamerServer.Instance.endPoints.Count; i++)
            {
                if (StreamerServer.Instance.endPoints[i].Status)
                {
                    try
                    {
                        var stream = StreamerServer.Instance.endPoints[i].Stream;
                        if (stream.DataAvailable) stream.Flush();
                    }
                    catch
                    {
                        dispatch.Invoke(() => StreamerServer.Instance.endPoints.RemoveAt(i));
                    }
                }
            }
        }

        void FlushAudio(IntPtr data, long pts)
        {
            for (int i = 0; i < StreamerServer.Instance.endPoints.Count; i++)
            {
                if (StreamerServer.Instance.endPoints[i].Status)
                {
                    try
                    {
                        var stream = StreamerServer.Instance.endPoints[i].Stream;
                        if (stream.DataAvailable) stream.Flush();
                    }
                    catch
                    {
                        dispatch.Invoke(() => StreamerServer.Instance.endPoints.RemoveAt(i));
                    }
                }
            }
        }

        void ResumeAudio(IntPtr data, long pts)
        {

        }

        void PauseAudio(IntPtr data, long pts)
        {
            for (int i = 0; i < StreamerServer.Instance.endPoints.Count; i++)
            {
                if (StreamerServer.Instance.endPoints[i].Status)
                {
                    try
                    {
                        var stream = StreamerServer.Instance.endPoints[i].Stream;
                        if (stream.DataAvailable) stream.Flush();
                    }
                    catch
                    {
                        dispatch.Invoke(() => StreamerServer.Instance.endPoints.RemoveAt(i));
                    }
                }
            }
        }

        void AudioCleanup(IntPtr opaque)
        {
            
        }

        int AudioSetup(ref IntPtr opaque, ref IntPtr format, ref uint rate, ref uint channels)
        {
            rate = 24000;
            channels = 2;
            return 0;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (Player.MediaPlayer != null)
                    Player.MediaPlayer.Stop();
            }
            catch
            {

            }
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused)
                Player.MediaPlayer.Play();
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            if (Player.MediaPlayer.IsPlaying)
            {
                Player.MediaPlayer.Pause();
                isPaused = true;
            }
        }
    }
}
