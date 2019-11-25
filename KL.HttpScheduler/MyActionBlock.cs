using KL.TaskQueue;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KL.HttpScheduler
{
    /// <summary>
    /// My action block options
    /// </summary>
    public class MyActionBlockOptions
    {
        /// <summary>
        /// Max queue length per processor
        /// </summary>
        public int MaxQueueLengthPerProcessor { get; set; } 

        /// <summary>
        /// Max concurrent tasks per processor
        /// </summary>
        public int MaxConcurrentTasksPerProcessor { get; set; }
    }

    /// <summary>
    /// My action block
    /// </summary>
    public class MyActionBlock
    {
        private ITaskQueue<ActionBlockInput> Queue { get; }
        public JobProcessorWrapper JobProcessorWrapper { get; }

        public DateTimeOffset LatestDequeued { get; private set; }

        /// <summary>
        /// Action Block
        /// </summary>
        /// <param name="jobProcessorWrapper"></param>
        /// <param name="options"></param>
        public MyActionBlock(JobProcessorWrapper jobProcessorWrapper, MyActionBlockOptions options)
        {
            Queue = new TaskQueueFactory().Create<ActionBlockInput>(ProcessAsync, options.MaxQueueLengthPerProcessor, options.MaxConcurrentTasksPerProcessor);
            JobProcessorWrapper = jobProcessorWrapper;
        }
        /// <inheritDoc />
        public MyActionBlock(JobProcessorWrapper jobProcessorWrapper, IOptions<MyActionBlockOptions> options): this(jobProcessorWrapper, options.Value) { }


        private Task ProcessAsync(ActionBlockInput actionBlockInput, CancellationToken _)
        {
            switch (actionBlockInput.Input)
            {
                case HttpJob httpJob:
                    return JobProcessorWrapper.ProcessAsync(httpJob);
                case DateTimeOffset datetime:
                    LatestDequeued = datetime;
                    return Task.CompletedTask;
                default:
                    return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Enqueue the action
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        public bool Post(HttpJob httpJob)
        {
            return Queue.Add(new ActionBlockInput()
            {
                Input = httpJob
            });
        }

        /// <summary>
        /// Post guid
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool Post(DateTimeOffset dateTime)
        {
            return Queue.Add(new ActionBlockInput()
            {
                Input = dateTime
            });
        }

        /// <summary>
        /// Run the action block in background
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task RunAsync(CancellationToken cancellationToken)
        {
            return Queue.RunAsync(cancellationToken);
        }

        /// <summary>
        /// Action Block Input
        /// </summary>
        private class ActionBlockInput
        {
            public object Input { get; set; }
        }
    }
}
