using Microsoft.AspNetCore.Mvc;

namespace LabeledByAI;

public class ProblemDetailsException : Exception
{
    public ProblemDetailsException(ProblemDetails problemDetails, Exception? innerException = null)
        : base(problemDetails.Detail, innerException)
    {
        ProblemDetails = problemDetails;
    }

    public ProblemDetails ProblemDetails { get; }
}
