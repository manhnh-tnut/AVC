using System;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AVC.DatabaseModels
{
    public class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public virtual string id { get; set; }
        public virtual long timeCreate { get; set; }
        public virtual string userCreate { get; set; }
        public virtual long timeUpdate { get; set; }
        public virtual string userUpdate { get; set; }
        public virtual string date { get => DateTimeOffset.FromUnixTimeSeconds(timeCreate).LocalDateTime.Date.ToShortDateString(); }
        public virtual int year { get => Convert.ToDateTime(date).Year; }
        public virtual int month { get => Convert.ToDateTime(date).Month; }
        public virtual int week { get => GetIso8601WeekOfYear(Convert.ToDateTime(date)); }
        public virtual string quarter { get => month < 4 ? "I" : month < 7 ? "II" : month < 10 ? "III" : "IV"; }
        public BaseEntity()
        {
            id = Guid.NewGuid().ToString();
            timeCreate = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
        }

        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}