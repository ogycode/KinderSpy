using System;
using System.Collections.Generic;

namespace Core
{
    interface IBrowser
    {
        string Name { get; set; }
        string MinVersion { get; set; }
        string HistoryPath { get; set; }
        List<HistoryElement> History { get; set; }

        void UpdateHistory();
    }
}
