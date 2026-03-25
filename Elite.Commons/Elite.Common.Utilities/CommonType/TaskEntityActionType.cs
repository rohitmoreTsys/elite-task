using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public enum TaskEntityActionType
    {
        CreateTask = 0,
        EditTask = 1,
        CreateSubTask = 2,
        EditSubTask = 3,
        DeleteSubTask = 4,
        ViewTask = 5,
        ViewSubTask = 6,
        EditMainTask = 7,
        Comment = 8,
        Status = 9,
        DeleteTask = 10,
        ChangeCompletedStatus = 11,
        DisableFields = 12
    }
}
