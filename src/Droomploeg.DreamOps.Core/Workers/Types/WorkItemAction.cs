using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droomploeg.DreamOps.Domain.Workers.Types;

public enum WorkItemAction
{
    Create,
    Start,
    Finished,
    Cancel,
}
