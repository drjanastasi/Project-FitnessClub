namespace FitnessClub.Api.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(StatusCodes.Status404NotFound, message)
    {
    }
}

public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(StatusCodes.Status409Conflict, message)
    {
    }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base(StatusCodes.Status400BadRequest, message)
    {
    }
}
