using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

namespace KL.HttpScheduler.Api.Models
{
    /// <summary>
    /// GetAll method parameters
    /// </summary>
    public class GetAllParameters
    {
        /// <summary>
        /// Start index (sorted by scheduledequeue score)
        /// </summary>
        [Range(0, int.MaxValue)]
        public int Start { get; set; } = 0;

        /// <summary>
        /// Max number of elements to return
        /// </summary>
        [Range(1, 100)]
        public int Count { get; set; } = 10;
    }

    internal class GetAllParametersExample : IExamplesProvider<GetAllParameters>
    {
        public GetAllParameters GetExamples()
        {
            return new GetAllParameters()
            {
                Start = 0,
                Count = 10
            };
        }
    }
}
