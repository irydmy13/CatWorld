using System;
using CatWorld.Services;

namespace CatWorld.Services;

public sealed class NightModeService : INightModeService
{
    private readonly ISettingsService _settings;

    public NightModeService(ISettingsService settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Возвращает true, если текущее локальное время попадает в интервал [NightStart, NightEnd).
    /// Корректно работает, если ночь "перетекает" через полночь (например, 21:00–08:00).
    /// </summary>
    public bool IsNightNow()
    {
        var now = DateTime.Now.TimeOfDay;
        var start = _settings.NightStart;   // например, 21:00
        var end = _settings.NightEnd;     // например, 08:00

        if (start == end)
            return false; // одинаковые значения считаем "ночь выключена"

        if (start < end)
        {
            // обычный случай в рамках суток: [21:00 .. 23:59] или [00:00 .. 08:00] НЕТ
            return now >= start && now < end;
        }
        else
        {
            // ночь переходит через полночь: 21:00..24:00 ИЛИ 00:00..08:00
            return now >= start || now < end;
        }
    }
}
