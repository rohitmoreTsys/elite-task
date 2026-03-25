using Elite.Task.Microservice.CommonLib;
using MediatR;
using System.Collections.Generic;

namespace Elite.Task.Microservice.NotificationServices
{


    public class EmailTemplatesQuery : IRequest<TaskEmailTemplates>
    {
        int taskId;
    }
}
