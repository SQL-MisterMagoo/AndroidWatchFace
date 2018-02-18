using System.Threading;
using Android.Util;
using Android.OS;
using Android.Graphics.Drawables;
using Android.Support.Wearable.Watchface;
using Android.Service.Wallpaper;
using Java.Util.Concurrent;

namespace WatchApp
{
  partial class MyWatchFaceService : CanvasWatchFaceService
  {
    public override WallpaperService.Engine OnCreateEngine()
    {
      return new MyWatchFaceEngine(this);
    }
  }
}