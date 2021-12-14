using Android.App;
using Android.Content;
using Android.Media;
using System;
using System.Net.Sockets;

namespace AudioReceiver
{
    public class Client
    {
        private static readonly Client _instance = new Client();
        public static Client Instance { get { return _instance; } }

        TcpClient client;
        NetworkStream stream;
        public AudioTrack audioTrack;
        public void Listen(string ip)
        {
            try
            {
                client = new TcpClient(ip, 5769);

                if (client.Connected)
                {
                    stream = client.GetStream();

                    int sampleRate = AudioTrack.GetNativeOutputSampleRate(Android.Media.Stream.Music);
                    int bufferSize = AudioTrack.GetMinBufferSize(sampleRate, ChannelOut.Stereo, Encoding.Pcm16bit);

                    var audioFormatBuilder = new AudioFormat.Builder();
                    audioFormatBuilder.SetEncoding(Encoding.Pcm16bit);
                    audioFormatBuilder.SetSampleRate(24000);
                    var audioFormat = audioFormatBuilder.Build();

                    var audioTrackBuilder = new AudioTrack.Builder();
                    audioTrackBuilder.SetAudioFormat(audioFormat);
                    audioTrackBuilder.SetTransferMode(AudioTrackMode.Stream);
                    audioTrackBuilder.SetBufferSizeInBytes(bufferSize);
                    audioTrack = audioTrackBuilder.Build();
                    AudioManager audioManager = (AudioManager)Application.Context.GetSystemService(Context.AudioService);

                    if ((audioManager.BluetoothA2dpOn || audioManager.WiredHeadsetOn) && audioTrack.PlayState == PlayState.Stopped)
                    {
                        audioTrack.Play();
                    }
                    int segmentLength = 4;
                    byte[] data = new byte[segmentLength];
                    while (true)
                    {
                        stream?.Read(data, 0, segmentLength);
                        audioTrack.Write(data, 0, segmentLength);
                    }
                    //stream?.BeginRead(data, 0, segmentLength, OnReceive, data);

                    /*byte[] lengthbyte = new byte[sizeof(int)];
                    stream?.BeginRead(lengthbyte, 0, lengthbyte.Length, OnReceiveLength, lengthbyte);*/
                }
            }
            catch (Exception e)
            {
                Stop();
            }
            
        }
        public void Stop()
        {
            try
            {
                audioTrack.Stop();
                audioTrack.Dispose();
                client.Close();
                client.Dispose();
            }
            catch
            {
                return;
            }
        }
        /*private void OnReceiveLength(IAsyncResult ar)
        {
            try
            {
                stream?.EndRead(ar);
                var lengthByte = (byte[])ar.AsyncState;
                int length = BitConverter.ToInt32(lengthByte, 0);
                byte[] data = new byte[length];
                stream?.BeginRead(data, 0, length, OnReceive, data);
            }
            catch
            {
                return;
            }
        }
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int length = stream.EndRead(ar);
                var data = (byte[])ar.AsyncState;
                audioTrack.Write(data, 0, length);
                data = new byte[1920];
                stream?.BeginRead(data, 0, 1920, OnReceive, data);

                *//*byte[] lengthbyte = new byte[sizeof(int)];
                stream?.BeginRead(lengthbyte, 0, lengthbyte.Length, OnReceiveLength, lengthbyte);*//*
            }
            catch
            {
                return;
            }
        }*/
    }
}