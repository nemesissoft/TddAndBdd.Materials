﻿namespace Behavioral.Tools;

public interface ITradingSystemClient
{
    // set time in future
    Task SetTimeTo(DateTime time);
    Task UpdateTimeBy(TimeSpan time);
}