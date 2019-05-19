using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace KL.HttpScheduler
{
    /// <summary>
    /// My action block
    /// </summary>
    public class MyActionBlock
    {
        private ActionBlock<ActionBlockInput> ActionBlock { get; }

        /// <summary>
        /// Action Block
        /// </summary>
        /// <param name="jobProcessorWrapper"></param>
        public MyActionBlock(JobProcessorWrapper jobProcessorWrapper)
        {
            ActionBlock = new ActionBlock<ActionBlockInput>((input) =>
            {
                return jobProcessorWrapper.ProcessAsync(input.HttpJob);
            });

        }

        /// <summary>
        /// Complete this block
        /// </summary>
        /// <returns></returns>
        public Task CompleteAsync()
        {
            ActionBlock.Complete();
            return ActionBlock.Completion;
        }

        /// <summary>
        /// Enqueue the action
        /// </summary>
        /// <param name="httpJob"></param>
        /// <returns></returns>
        public bool Post(HttpJob httpJob)
        {
            return ActionBlock.Post(new ActionBlockInput()
            {
                HttpJob = httpJob
            });
        }

        /// <summary>
        /// Action Block Input
        /// </summary>
        private class ActionBlockInput
        {
            public HttpJob HttpJob { get; set; }
        }
    }
}
