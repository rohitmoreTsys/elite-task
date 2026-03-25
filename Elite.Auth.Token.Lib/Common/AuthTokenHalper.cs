using Elite.Auth.Token.Lib.Command;
using Elite.Common.Utilities.CommonType;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Auth.Token.Lib.Common
{
    public class AuthTokenHalper
    {
        public static AuthTokenCommand AuthTokenMapper(OIDCModel oIDCModel, int serviceId) =>
          new AuthTokenCommand()
          {
              AccessToken = oIDCModel.access_token,
              IDToken = oIDCModel.id_token,
              RefreshToken = oIDCModel.refresh_token,
              TokenExpireDateTime = GetTokenExpireDateTime(oIDCModel.TokenExpires),
              Uid = oIDCModel.userId,
              SourceId = serviceId
          };


        public static DateTime GetTokenExpireDateTime(int expiresSec)
        {
            var nowUtc = DateTime.Now;
            return nowUtc.AddSeconds(expiresSec);
        }

        public static string GetServiceName(int serviceId)
        {
            string serviceName = string.Empty;
            switch (serviceId) {
                case 0: serviceName = AuthRequestSource.Oidc.ToString(); break;
                case 1: serviceName = AuthRequestSource.Meeting.ToString(); break;
                case 2: serviceName = AuthRequestSource.Task.ToString(); break;
                case 3: serviceName = AuthRequestSource.Topic.ToString(); break;
                case 4: serviceName = AuthRequestSource.Person.ToString(); break;
                case 5: serviceName = AuthRequestSource.Admin.ToString(); break;
                case 6: serviceName = AuthRequestSource.Attachment.ToString(); break;
                case 7: serviceName = AuthRequestSource.PdfViewer.ToString(); break;
            }

            if (string.IsNullOrWhiteSpace(serviceName))
                throw new Exception("ServerId not found");

            return serviceName; 
        }
    }
}
