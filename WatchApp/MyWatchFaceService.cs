using System;
using System.Threading;
using Android.App;
using Android.Util;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Text.Format;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Wearable.Watchface;
using Android.Service.Wallpaper;
using Java.Util.Concurrent;
using Android.Icu.Util;
using Android.Text;

namespace WatchApp
{
  class MyWatchFaceService : CanvasWatchFaceService
  {
    public override WallpaperService.Engine OnCreateEngine()
    {
      return new MyWatchFaceEngine(this);
    }

    public class MyWatchFaceEngine : CanvasWatchFaceService.Engine
    {
      CanvasWatchFaceService owner;
      Paint timePaint;
      Paint datePaint;
      Paint backPaint;
      // Broadcast receiver for handling time zone changes:
      TimeZoneReceiver timeZoneReceiver;
      GregorianCalendar time;

      public MyWatchFaceEngine(CanvasWatchFaceService owner) : base(owner)
      {
        this.owner = owner;
      }

      public override void OnCreate(ISurfaceHolder holder)
      {
        base.OnCreate(holder);

        SetWatchFaceStyle(new WatchFaceStyle.Builder(owner)
            .Build());

        timePaint = new Paint();
        timePaint.Color = Color.White;
        timePaint.TextSize = 72f;

        datePaint = new Paint() { AntiAlias = true, Color = Color.White, TextSize = 48f };
        backPaint = new Paint() { Color = Color.Black };

        time = new GregorianCalendar(Android.Icu.Util.TimeZone.Default);

        
      }

      private void logit(string message)
      {
        Console.WriteLine(message);
      }
      public override void OnDraw(Canvas canvas, Rect frame)
      {
        time = new GregorianCalendar(Android.Icu.Util.TimeZone.Default);

        TextPaint timeTextPaint = new TextPaint(timePaint);
        TextPaint dateTextPaint = new TextPaint(datePaint);

        var timeString = DateUtils.FormatDateTime(Application.Context, time.TimeInMillis, FormatStyleFlags.ShowTime);
        logit(timeString);

        var timeLayout = StaticLayout.Builder.Obtain(timeString, 0, timeString.Length, timeTextPaint, frame.Width())
          .SetAlignment(Layout.Alignment.AlignCenter)
          .Build();

        var dateString = DateUtils.FormatDateTime(Application.Context, time.TimeInMillis, FormatStyleFlags.ShowDate);
        logit(dateString);

        var dateLayout = StaticLayout.Builder.Obtain(dateString, 0, dateString.Length, dateTextPaint, frame.Width())
          .SetAlignment(Layout.Alignment.AlignCenter)
          .Build();

        canvas.DrawColor(Color.Black);
        canvas.DrawRect(frame,backPaint);
        canvas.Save();
        float textHeight = timeLayout.Height + dateLayout.Height;

        float textYCoordinate = frame.ExactCenterY() - textHeight / 2;
        float textXCoordinate = frame.Left;

        canvas.Translate(textXCoordinate, textYCoordinate);
        timeLayout.Draw(canvas);

        canvas.Translate(textXCoordinate, timeLayout.Height);
        dateLayout.Draw(canvas);

        canvas.Restore();
      }

      public override void OnTimeTick()
      {
        Invalidate();
      }

      // Called whenever the watch face is becoming visible or hidden. Note that
      // you must call base.OnVisibilityChanged first:

      public override void OnVisibilityChanged(bool visible)
      {
        base.OnVisibilityChanged(visible);

        // If the watch face became visible, register the timezone receiver
        // and get the current time. Else, unregister the timezone receiver:

        if (visible)
        {
          RegisterTimezoneReceiver();
          time = new GregorianCalendar(Android.Icu.Util.TimeZone.Default);
        }
        else
          UnregisterTimezoneReceiver();
      }

      bool registeredTimezoneReceiver = false;

      // Registers the time zone broadcast receiver (defined at the end of 
      // this file) to handle time zone change events:

      void RegisterTimezoneReceiver()
      {
        if (registeredTimezoneReceiver)
          return;
        else
        {
          if (timeZoneReceiver == null)
          {
            timeZoneReceiver = new TimeZoneReceiver();
            timeZoneReceiver.Receive = (intent) =>
            {
              time = new GregorianCalendar(Android.Icu.Util.TimeZone.Default);
            };
          }
          registeredTimezoneReceiver = true;
          IntentFilter filter = new IntentFilter(Intent.ActionTimezoneChanged);
          Application.Context.RegisterReceiver(timeZoneReceiver, filter);
        }
      }

      // Unregisters the timezone Broadcast receiver:

      void UnregisterTimezoneReceiver()
      {
        if (!registeredTimezoneReceiver)
          return;
        registeredTimezoneReceiver = false;
        Application.Context.UnregisterReceiver(timeZoneReceiver);
      }
    }
    // Time zone broadcast receiver. OnReceive is called when the
    // time zone changes:

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