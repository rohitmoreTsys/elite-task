using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Common.Utilities.CommonType
{
    public enum TopicCategory
    {
        NA = 0,
        Information = 1,
        Approval = 2,
        Approval_only_Shareholder_Representatives = 3
    }
    public enum TopicCategoryGerman
    {
        NA = 0,
        Kenntnisnahme = 1,
        Beschluss = 2,
        Beschluss_nur_Anteilseignervertreter = 3
    }
    public static class TopicCategoryDisplayName
    {
        public static readonly Dictionary<TopicCategory, string> TopicCategoryList
                = new Dictionary<TopicCategory, string>
                {
                    { TopicCategory.NA, "" },
                    { TopicCategory.Information, "(I)" },
                    { TopicCategory.Approval, "(A)" },
                    { TopicCategory.Approval_only_Shareholder_Representatives, "(A*)" }
                };
        public static readonly Dictionary<TopicCategoryGerman, string> TopicCategoryListGerman
               = new Dictionary<TopicCategoryGerman, string>
               {
                    { TopicCategoryGerman.NA, "" },
                    { TopicCategoryGerman.Kenntnisnahme, "(K)" },
                    { TopicCategoryGerman.Beschluss, "(B)" },
                    { TopicCategoryGerman.Beschluss_nur_Anteilseignervertreter, "(B*)" }
               };
    }
}
