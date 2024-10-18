using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droomploeg.DreamOps.IntegrationsTests.Pages.Queues.GridColumns
{
    internal enum QueueOverviewGridColumn
    {
        State = 0,
        Name = 1,
        ActiveMessageCount = 2,
        ScheduledMessageCount = 3,
        DeadLetterCount = 4,
        Open = 2,
    }
}
