namespace hms.Application.Models.Exceptions
{
    public class IdentityOperationException : Exception
    {
        public IEnumerable<string> Errors { get; }

        public IdentityOperationException(IEnumerable<string> errors)
            : base("Identity operation failed.")
        {
            Errors = errors;
        }
    }
}