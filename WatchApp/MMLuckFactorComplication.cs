using Android.Support.Wearable.Complications;
using System;

namespace WatchApp
{
  class MMLuckFactorComplication : ComplicationProviderService
  {
    public override void OnComplicationUpdate(int complicationId, int dataType, ComplicationManager complicationManager)
    {
      Log($"OnComplicationUpdate dataType={dataType}");
      var num = new Random((int)Java.Lang.JavaSystem.CurrentTimeMillis()).Next(10, 75);
      ComplicationData complicationData = null;
      switch (dataType)
      {
        case ComplicationData.TypeShortText:
          Log($"OnComplicationUpdate dataType=ShortText");
          complicationData = new ComplicationData.Builder(ComplicationData.TypeShortText)
            .SetShortText(ComplicationText.PlainText($"Luck {num}"))
            .Build();
          break;
        default:
          Log($"Unexpected complication type {dataType}");
          break;
      }
      if (complicationData != null)
        complicationManager.UpdateComplicationData(complicationId, complicationData);
      else
        complicationManager.NoUpdateRequired(complicationId);

    }

    private void Log(string message)
    {
      Console.WriteLine(message);
    }

  }
}