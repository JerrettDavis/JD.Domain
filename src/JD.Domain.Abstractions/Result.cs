namespace JD.Domain.Abstractions;

/// <summary>
/// Represents the result of an operation that can succeed with a value or fail with errors.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly IReadOnlyList<DomainError> _errors;

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the success value. Throws if the result is a failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing value on a failure result.</exception>
    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException(
                    "Cannot access value of a failed result. Check IsSuccess before accessing Value.");
            }

            return _value!;
        }
    }

    /// <summary>
    /// Gets the collection of errors. Empty for successful results.
    /// </summary>
    public IReadOnlyList<DomainError> Errors => _errors;

    private Result(T value)
    {
        _value = value;
        _errors = [];
        IsSuccess = true;
    }

    private Result(IReadOnlyList<DomainError> errors)
    {
        if (errors == null) throw new ArgumentNullException(nameof(errors));
        
        if (errors.Count == 0)
        {
            throw new ArgumentException("At least one error is required for a failure result.", nameof(errors));
        }

        _value = default;
        _errors = errors;
        IsSuccess = false;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful result.</returns>
    public static Result<T> Success(T value)
    {
        return value == null 
            ? throw new ArgumentNullException(nameof(value)) : new Result<T>(value);
    }

    /// <summary>
    /// Creates a failure result with the specified error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failure result.</returns>
    public static Result<T> Failure(DomainError error)
    {
        if (error == null) throw new ArgumentNullException(nameof(error));
        return new Result<T>([error]);
    }

    /// <summary>
    /// Creates a failure result with the specified errors.
    /// </summary>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A failure result.</returns>
    public static Result<T> Failure(IReadOnlyList<DomainError> errors)
    {
        return new Result<T>(errors);
    }

    /// <summary>
    /// Creates a failure result with the specified errors.
    /// </summary>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A failure result.</returns>
    public static Result<T> Failure(params DomainError[] errors)
    {
        return new Result<T>(errors);
    }

    /// <summary>
    /// Matches the result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<IReadOnlyList<DomainError>, TResult> onFailure)
    {
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onFailure == null) throw new ArgumentNullException(nameof(onFailure));

        return IsSuccess ? onSuccess(_value!) : onFailure(_errors);
    }

    /// <summary>
    /// Maps the success value to a new type using the specified function.
    /// </summary>
    /// <typeparam name="TResult">The type of the mapped value.</typeparam>
    /// <param name="map">The mapping function.</param>
    /// <returns>A result with the mapped value if successful, otherwise the original errors.</returns>
    public Result<TResult> Map<TResult>(Func<T, TResult> map)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));

        return IsSuccess
            ? Result<TResult>.Success(map(_value!))
            : Result<TResult>.Failure(_errors);
    }

    /// <summary>
    /// Binds the result to another result-producing function.
    /// </summary>
    /// <typeparam name="TResult">The type of the bound result.</typeparam>
    /// <param name="bind">The binding function.</param>
    /// <returns>The result of the binding function if successful, otherwise the original errors.</returns>
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> bind)
    {
        if (bind == null) throw new ArgumentNullException(nameof(bind));

        return IsSuccess
            ? bind(_value!)
            : Result<TResult>.Failure(_errors);
    }

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    /// <param name="value">The value.</param>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly converts a domain error to a failure result.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result<T>(DomainError error) => Failure(error);
}
