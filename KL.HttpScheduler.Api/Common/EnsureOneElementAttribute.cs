using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace KL.HttpScheduler.Api.Common
{
    internal class EnsureOneElementAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is IList list)
            {
                return list.Count > 0;
            }
            return false;
        }
    }
}
