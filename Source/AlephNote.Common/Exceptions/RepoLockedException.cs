using System;

namespace AlephNote.Common.Exceptions
{
    public class RepoLockedException: Exception
    {
        public RepoLockedException()
        {
        }

        public RepoLockedException(string message) : base(message)
        {
        }

        public RepoLockedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
