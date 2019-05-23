using System;

namespace KL.HttpScheduler
{
    /// <summary>
    /// Conflict exception
    /// </summary>
    [Serializable]
    public class ConflictException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public ConflictException()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ConflictException(string message) : base(message)
        {
        }
    }
}