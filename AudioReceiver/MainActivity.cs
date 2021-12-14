using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.Snackbar;
using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace AudioReceiver
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Thread streamThread;
        MaterialButton startButton, stopButton;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            startButton = FindViewById<MaterialButton>(Resource.Id.button1);
            startButton.Click += StartButton_Click;

            stopButton = FindViewById<MaterialButton>(Resource.Id.button2);
            stopButton.Click += StopButton_Click;

            getDeviceIP();
            isHeadsetConnected();

            EventListener eventListener = new EventListener();
            eventListener.setMain(this);
            RegisterReceiver(eventListener, new IntentFilter(Intent.ActionHeadsetPlug));
            RegisterReceiver(eventListener, new IntentFilter("android.bluetooth.headset.profile.action.CONNECTION_STATE_CHANGED"));
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            AppCompatEditText textBox = FindViewById<AppCompatEditText>(Resource.Id.editText1);
            string ip = textBox.Text;
            IPAddress iPAddress;
            if (IPAddress.TryParse(ip, out iPAddress))
            {
                streamThread = new Thread(() => Client.Instance.Listen(ip));
                streamThread.Start();
                View view = FindViewById<MaterialButton>(Resource.Id.button1);
                Snackbar snackBar = Snackbar.Make(view, "Pressed Start", 1000);
                snackBar.Show();
                startButton.Enabled = false;
            }
            else
            {
                View view = FindViewById<MaterialButton>(Resource.Id.button1);
                Snackbar snackBar = Snackbar.Make(view, "Invalid IP Address", 1000);
                snackBar.Show();
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            Client.Instance.Stop();
            View view = FindViewById<MaterialButton>(Resource.Id.button2);
            Snackbar snackBar = Snackbar.Make(view, "Pressed Stop", 1000);
            snackBar.Show();
            startButton.Enabled = true;
        }

        private void getDeviceIP()
        {
            var ip = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();
            TextView displayIP = FindViewById<TextView>(Resource.Id.textView1);
            if(ip.ToString() == "127.0.0.1")
                displayIP.Text = "Device IP: ";
            else
                displayIP.Text = "Device IP: " + ip.ToString();
        }

        public void isHeadsetConnected()
        {
            TextView headSet = FindViewById<TextView>(Resource.Id.textView2);
            AudioManager audioManager = (AudioManager)Application.Context.GetSystemService(Context.AudioService);
            if (audioManager.WiredHeadsetOn || audioManager.BluetoothA2dpOn)
            {
                headSet.SetTextColor(Android.Graphics.Color.Green);
                headSet.Text = "Headset Connected";
            }
            else
            {
                headSet.SetTextColor(Android.Graphics.Color.Red);
                headSet.Text = "Headset Not Connected";
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        /*private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener)null).Show();
        }*/

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
