using Android.App;
using Android.Content;
using Android.Media;
using System.Threading;
using Xamarin.Essentials;

namespace AudioReceiver
{
    [BroadcastReceiver(Enabled = true)]
    public class EventListener : BroadcastReceiver
    {
        private MainActivity main;
        public void setMain(MainActivity main)
        {
            this.main = main;
        }

        AudioManager audioManager = (AudioManager)Application.Context.GetSystemService(Context.AudioService);
        public override void OnReceive(Context context, Intent intent)
        {
            Thread.Sleep(2000);
            if (Client.Instance.audioTrack != null)
            {
                switch (intent.Action)
                {
                    case Intent.ActionHeadsetPlug:
                        if (audioManager.WiredHeadsetOn && Client.Instance.audioTrack.PlayState == PlayState.Stopped) Client.Instance.audioTrack.Play();
                        else if (!audioManager.WiredHeadsetOn && Client.Instance.audioTrack.PlayState == PlayState.Playing) Client.Instance.audioTrack.Stop();
                        break;
                    case "android.bluetooth.headset.profile.action.CONNECTION_STATE_CHANGED":
                        if (audioManager.BluetoothA2dpOn && Client.Instance.audioTrack.PlayState == PlayState.Stopped) Client.Instance.audioTrack.Play();
                        else if (!audioManager.BluetoothA2dpOn && Client.Instance.audioTrack.PlayState == PlayState.Playing) Client.Instance.audioTrack.Stop();
                        break;
                }
            }
            MainThread.BeginInvokeOnMainThread(() =>
            {
                main.isHeadsetConnected();
            });
        }
    }
}