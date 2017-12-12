using System;
using System.Collections.Generic;

namespace ReportViewer
{
    public struct HistoryElement
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime Date { get; set; }
        public int VisitCount { get; set; }

        public override bool Equals(object obj)
        {
            return (Title + Url) == (((HistoryElement)obj).Title + ((HistoryElement)obj).Url);
        }
        public override int GetHashCode()
        {
            var hashCode = -876643517;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Url);
            hashCode = hashCode * -1521134295 + Date.GetHashCode();
            hashCode = hashCode * -1521134295 + VisitCount.GetHashCode();
            return hashCode;
        }
    }
}
