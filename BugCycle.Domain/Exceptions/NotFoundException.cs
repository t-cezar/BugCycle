namespace BugCycle.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} cu identificatorul '{key}' nu a fost gasit.")
    {
    }
}
