using System.Threading;
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
        public bool EnableForward { get; set; }

        public MyActionBlock(
            ForwardJob forwardJobProcessor, 
            JobProcessorWrapper jobProcessorWrapper
            )
        {
            ActionBlock = new ActionBlock<ActionBlockInput>((input) =>
            {
                if (input.Forward)
                {
                    return forwardJobProcessor.ForwardAsync(input.HttpJob);
                }
                else
                {
                    return jobProcessorWrapper.ProcessAsync(input.HttpJob);
                }
            });

        }

        public Task CompleteAsync()
        {
            ActionBlock.Complete();
            return ActionBlock.Completion;
        }

        public bool Post(HttpJob httpJob, bool forward)
        {
            return ActionBlock.Post(new ActionBlockInput()
            {
                HttpJob = httpJob,
                Forward = EnableForward && forward
            });
        }

        /// <summary>
        /// Action Block Input
        /// </summary>
        private class ActionBlockInput
        {
            public HttpJob HttpJob { get; set; }
            public bool Forward { get; set; }
        }
    }
}
