using System;

namespace AVC.DatabaseModels
{
    public class Summary : BaseEntity
    {
        public virtual Machine machine { get; set; }
        public virtual string machineName { get => machine?.name; }
        public virtual long count { get; set; } = 0;
        public virtual long _time { get; set; } = 0;
        public virtual double time { get => Math.Round(TimeSpan.FromSeconds(_time).TotalHours, 2); }
        public virtual string display { get => _time == 0 ? "00:00:00" : TimeSpan.FromSeconds(_time).ToString(); }
        public virtual double percent { get => Math.Round(TimeSpan.FromSeconds(_time).TotalSeconds * 100 / TimeSpan.FromHours(10).TotalSeconds, 2); }
    }
}