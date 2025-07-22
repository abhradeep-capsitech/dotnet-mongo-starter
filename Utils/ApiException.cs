namespace DotnetMongoStarter.Utils
{
    public class ApiException: Exception
    {
        public int StatusCode { get; }
        public List<string> Errors { get; }

        public ApiException(string message, int statusCode = 400, List<string>? errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors ?? new List<string>();
        }
        public class NotFoundException : ApiException
        {
            public NotFoundException(string message) : base(message, 404) { }
        }

        public class ValidationException : ApiException
        {
            public ValidationException(string message, List<string> errors)
                : base(message, 400, errors) { }
        }

        public class UnauthorizedException : ApiException
        {
            public UnauthorizedException(string message, List<string>? errors = null)
                : base(message, 401, errors) { }
        }

        public class ForbiddenException : ApiException
        {
            public ForbiddenException(string message, List<string>? errors = null)
                : base(message, 403, errors) { }
        }
    }
}
