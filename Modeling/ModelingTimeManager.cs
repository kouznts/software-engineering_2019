﻿namespace GasStationMs.App.Modeling
{
    internal static class ModelingTimeManager
    {
        internal const int MillisecondsForTimerTick = 20;
        internal static bool IsPaused { get; set; }


        internal static void SetUpModelingTimeManager(System.Windows.Forms.Timer timerModeling)
        {
            timerModeling.Interval = MillisecondsForTimerTick;
            IsPaused = false;
        }
    }
}
