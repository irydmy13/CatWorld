using System;

namespace CatWorld.Models
{
    public class AppSettings
    {
        public bool NightScheduleEnabled { get; set; } = true;
        public TimeSpan NightStart { get; set; } = new(22, 0, 0); // 22:00
        public TimeSpan NightEnd { get; set; } = new(7, 0, 0); // 07:00
    }
}
