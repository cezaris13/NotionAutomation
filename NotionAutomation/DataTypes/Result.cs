using System;

namespace NotionAutomation.DataTypes;

public readonly struct Result<T, E> {
    private readonly bool m_success;
    public readonly T Value;
    public readonly E Error;

    private Result(T v, E e, bool success) {
        Value = v;
        Error = e;
        m_success = success;
    }

    public bool IsOk => m_success;

    public static Result<T, E> Ok(T v) {
        return new Result<T, E>(v, default(E), true);
    }

    public static Result<T, E> Err(E e) {
        return new Result<T, E>(default(T), e, false);
    }

    public static implicit operator Result<T, E>(T v) => new(v, default(E), true);
    public static implicit operator Result<T, E>(E e) => new(default(T), e, false);

    public R Match<R>(
        Func<T, R> success,
        Func<E, R> failure) =>
        m_success ? success(Value) : failure(Error);
}