namespace Homework_4.Exceptions;

public class ModelException : Exception
{
    private ModelException(string message) : base(message) {}

    public static ModelException InvalidModelInput<TValue>(string type, TValue value) =>
        throw new ModelException($"Invalid {type}: {value}");
}