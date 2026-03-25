using Microsoft.Extensions.Configuration;
using System;
namespace Elite.Logging
{
    public interface ILogException
    {
		void LogEliteError(string logUserId, Exception e);

        void LogEliteInfo(string logUserId, Exception e, string ActionType);
    }
}
