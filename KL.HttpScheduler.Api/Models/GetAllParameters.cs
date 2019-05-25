﻿using Swashbuckle.AspNetCore.Examples;
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
        public int Start { get; set; }

        /// <summary>
        /// Max number of elements to return
        /// </summary>
        [Range(1, 100)]
        public int Count { get; set; }
    }

    internal class GetAllParametersExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new GetAllParameters()
            {
                Start = 0,
                Count = 10
            };
        }
    }
}
