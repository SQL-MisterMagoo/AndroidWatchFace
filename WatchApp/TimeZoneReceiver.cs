using System;
using Android.Content;

namespace WatchApp
{
  partial class MyWatchFaceService
  {
    public class TimeZoneReceiver : BroadcastReceiver
    {
      public Action<Intent> Receive { get; set; }

      public override void OnReceive(Context context, Intent intent)
      {
        Receive?.Invoke(intent);
      }
    }
  }
}