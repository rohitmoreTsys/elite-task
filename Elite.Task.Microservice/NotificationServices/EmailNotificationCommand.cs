using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.NotificationServices
{
   public class EmailNotificationCommand : IRequest<long>
    {
        public long Id { get; set; }
    }
}
