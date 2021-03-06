﻿using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Text.Format;
using Android.Graphics;
using Android.Support.Wearable.Watchface;
using Android.Icu.Util;
using Android.Text;
using Android.Support.Wearable.Complications;
using Android.Support.Wearable.Complications.Rendering;

namespace WatchApp
{
  partial class MyWatchFaceService
  {
    public class MyWatchFaceEngine : CanvasWatchFaceService.Engine
    {
      CanvasWatchFaceService owner;
      TextPaint brightPaint;
      TextPaint dullPaint;
      Paint backPaint;

      TimeZoneReceiver timeZoneReceiver;
      bool registeredTimezoneReceiver = false;

      GregorianCalendar time;
      private int notificationCount;
      private int unreadCount;
      private static bool ambientMode;
      private TextRenderer complicationRenderer;
      private ComplicationData dateComplicationData;
      private ComplicationDrawable complicationDrawable;

      public MyWatchFaceEngine(CanvasWatchFaceService owner) : base(owner)
      {
        this.owner = owner;
      }

      public override void OnCreate(ISurfaceHolder holder)
      {
        base.OnCreate(holder);

        SetWatchFaceStyle(new WatchFaceStyle.Builder(owner)
            .Build());

        brightPaint = new TextPaint { Color = Color.White, TextSize = 72f };
        dullPaint = new TextPaint() { AntiAlias = true, Color = Color.White, TextSize = 48f };
        backPaint = new TextPaint() { Color = Color.Black };

        UpdateCalendarWithCurrentTime();

        SetDefaultComplicationProvider(101, new ComponentName(Application.Context, Java.Lang.Class.FromType(typeof(MMLuckFactorComplication))), ComplicationData.TypeShortText);
        SetActiveComplications(101);

        //complicationRenderer = new TextRenderer();
        //complicationRenderer.SetPaint(dullPaint);
        //complicationRenderer.SetText("wait");
        //complicationRenderer.SetMinimumCharactersShown(12);

        complicationDrawable = new ComplicationDrawable(Application.Context);
        complicationDrawable.SetTextColorActive(dullPaint.Color.ToArgb());
      }

      // Drawing the watch face

      public override void OnDraw(Canvas canvas, Rect frame)
      {
        UpdateCalendarWithCurrentTime();

        StaticLayout timeLayout = GetTimeLayoutAndText(frame, brightPaint);
        StaticLayout dateLayout = GetDateLayoutAndText(frame, dullPaint);
        StaticLayout notifLayout = GetNotificationsLayoutAndText(frame, dullPaint);

        canvas.Save();

        DrawBackground(canvas, frame);
        DrawNotifications(canvas, notifLayout);

        // Calculate positioning for the time and date
        float textHeight = timeLayout.Height + dateLayout.Height;
        float textYCoordinate = frame.ExactCenterY() - textHeight / 2;
        float textXCoordinate = frame.Left;

        canvas.Translate(textXCoordinate, textYCoordinate);
        DrawTime(canvas, timeLayout);

        canvas.Translate(textXCoordinate, timeLayout.Height);
        DrawDate(canvas, dateLayout);

        canvas.Restore();

        int complicationRendererTop = (int)textYCoordinate + timeLayout.Height + dateLayout.Height + 10;
        complicationDrawable.SetBounds(frame.Left, complicationRendererTop, frame.Right, frame.Bottom);
        complicationDrawable.Draw(canvas);
      }

      private static void DrawNotifications(Canvas canvas, StaticLayout notifLayout)
      {
        // Draw the notifications
        if (! ambientMode)
          notifLayout.Draw(canvas);
      }

      private void DrawBackground(Canvas canvas, Rect frame)
      {
        // Background
        canvas.DrawColor(Color.Black);
        canvas.DrawRect(frame, backPaint);
      }

      private static void DrawTime(Canvas canvas, StaticLayout timeLayout)
      {
        timeLayout.Draw(canvas);
      }

      private void DrawDate(Canvas canvas, StaticLayout dateLayout)
      {
        if (!ambientMode)
        {
          dateLayout.Draw(canvas);
        }
      }

      private StaticLayout GetNotificationsLayoutAndText(Rect frame, TextPaint dullTextPaint)
      {
        // Notifications Layout
        var notifString = notificationCount == 0 ? "" : $"{unreadCount}/{notificationCount}";

        var notifLayout = StaticLayout.Builder.Obtain(notifString, 0, notifString.Length, dullTextPaint, frame.Width())
          .SetAlignment(Layout.Alignment.AlignCenter)
          .Build();
        return notifLayout;
      }

      private StaticLayout GetTimeLayoutAndText(Rect frame, TextPaint brightTextPaint)
      {
        // Time Layout
        var timeString = GetFormattedDateTimeString(FormatStyleFlags.ShowTime);
        Log(timeString);

        var timeLayout = StaticLayout.Builder.Obtain(timeString, 0, timeString.Length, brightTextPaint, frame.Width())
          .SetAlignment(Layout.Alignment.AlignCenter)
          .Build();
        return timeLayout;
      }

      private StaticLayout GetDateLayoutAndText(Rect frame, TextPaint dullTextPaint)
      {
        string dateString = GetFormattedDateTimeString(FormatStyleFlags.ShowDate);
        Log(dateString);

        var dateLayout = StaticLayout.Builder.Obtain(dateString, 0, dateString.Length, dullTextPaint, frame.Width())
          .SetAlignment(Layout.Alignment.AlignCenter)
          .Build();
        return dateLayout;
      }

      // Register for Events

      void RegisterTimezoneReceiver()
      {
        if (registeredTimezoneReceiver)
          return;
        else
        {
          if (timeZoneReceiver == null)
          {
            timeZoneReceiver = new TimeZoneReceiver
            {
              Receive = (intent) =>
              {
                UpdateCalendarWithCurrentTime(true);
              }
            };
          }
          registeredTimezoneReceiver = true;
          IntentFilter filter = new IntentFilter(Intent.ActionTimezoneChanged);
          Application.Context.RegisterReceiver(timeZoneReceiver, filter);
        }
      }

      void UnregisterTimezoneReceiver()
      {
        if (!registeredTimezoneReceiver)
          return;
        registeredTimezoneReceiver = false;
        Application.Context.UnregisterReceiver(timeZoneReceiver);
      }

      // Event Handlers

      public override void OnTimeTick()
      {
        Invalidate();
      }

      public override void OnNotificationCountChanged(int count)
      {
        base.OnNotificationCountChanged(count);
        notificationCount = count;
        Log($"The new notification count is {notificationCount}");
        Invalidate();
      }

      public override void OnUnreadCountChanged(int count)
      {
        base.OnUnreadCountChanged(count);
        unreadCount = count;
        Log($"The new unread count is {unreadCount}");
        Invalidate();
      }

      public override void OnVisibilityChanged(bool visible)
      {
        base.OnVisibilityChanged(visible);

        // If the watch face became visible, register the timezone receiver
        // and get the current time. Else, unregister the timezone receiver:

        if (visible)
        {
          RegisterTimezoneReceiver();
          UpdateCalendarWithCurrentTime();
        }
        else
          UnregisterTimezoneReceiver();
      }

      public override void OnAmbientModeChanged(bool inAmbientMode)
      {
        base.OnAmbientModeChanged(inAmbientMode);
        ambientMode = inAmbientMode;
        Invalidate();
      }

      public override void OnComplicationDataUpdate(int watchFaceComplicationId, ComplicationData data)
      {
        base.OnComplicationDataUpdate(watchFaceComplicationId, data);
        switch (watchFaceComplicationId)
        {
          case 101:
            complicationDrawable.SetComplicationData(data);
            break;
        }
      }
      // Helpers

      private string GetFormattedDateTimeString(FormatStyleFlags formatStyle)
      {
        // Date Layout
        return DateUtils.FormatDateTime(Application.Context, time.TimeInMillis, formatStyle);
      }

      private void Log(string message)
      {
        Console.WriteLine(message);
      }

      private void UpdateCalendarWithCurrentTime(bool resetTimezone = false)
      {
        if (time == null || resetTimezone)
        {
          time = new GregorianCalendar(Android.Icu.Util.TimeZone.Default);
        }
        time.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();
      }

    }
  }
}