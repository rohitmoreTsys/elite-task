using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.CommonLib
{
    public class UserInfo
    {

        public long UserId { get; set; }
        public string Uid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public bool IsActive { get; set; }
        public string DisplayName { get; set; }
        public bool IsInternal { get; set; }
        public bool IsTransient { get; set; }
        [DataMember]
        // public LookUpFields CreatedBy { get; set; }
        public string CreatedBy { get; set; }
        [DataMember]
        public DateTime? CreatedDate { get; set; }
        [DataMember]
        // public LookUpFields ModifiedBy { get; set; }
        public string ModifiedBy { get; set; }
        [DataMember]
        public DateTime? ModifiedDate { get; set; }
        public bool isValid { get; set; }
        public string language { get; set; }


    }
}
