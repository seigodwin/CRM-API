
namespace CRMApi.Exceptions.Types
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base (message)
        {
            
        }
    }

    
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base (message)
        {
            
        }
    }

    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base (message)
        {
            
        }
    }

    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base (message)
        {
            
        }
    }

    public class ValidationsException : Exception
    {
        IReadOnlyList<string> Messages { get; }
        public ValidationsException(IEnumerable<string> messages) : base ("One or more validation errors occurred.")
        {
            Messages = messages.ToList();
        }
    }
}