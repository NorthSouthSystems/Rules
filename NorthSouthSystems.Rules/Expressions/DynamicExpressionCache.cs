using System.Collections.Concurrent;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;

namespace Nss.Rules;

public class DynamicExpressionCache
{
    public DynamicExpressionCache(ParsingConfig? parsingConfig = null) =>
        _parsingConfig = parsingConfig;

    private readonly ParsingConfig? _parsingConfig;

    private readonly ConcurrentDictionary<CacheKey, Lazy<Delegate>> _cache = new();
    private record struct CacheKey(Type InputType, Type ResultType, string ExpressionRaw);

    public TResult Evaluate<TInput, TResult>(TInput input, string expressionRaw)
    {
        Throw.IfNullOrWhiteSpace(expressionRaw);

        var method = CompileAndCache(typeof(TInput), typeof(TResult), expressionRaw);
        var methodTyped = (Func<TInput, TResult>)method;

        return methodTyped(input);
    }

    public object? Evaluate(object input, Type resultType, string expressionRaw)
    {
        Throw.IfNull(input);
        Throw.IfNull(resultType);
        Throw.IfNullOrWhiteSpace(expressionRaw);

        var method = CompileAndCache(input.GetType(), resultType, expressionRaw);

        return method.DynamicInvoke(input);
    }

    public ParseException? TryCompileNoCache<TInput, TResult>(string expressionRaw) =>
        TryCompileNoCache(typeof(TInput), typeof(TResult), expressionRaw);

    public ParseException? TryCompileNoCache(Type inputType, Type resultType, string expressionRaw)
    {
        Throw.IfNull(inputType);
        Throw.IfNull(resultType);
        Throw.IfNullOrWhiteSpace(expressionRaw);

        try { Compile(inputType, resultType, expressionRaw); }
        catch (ParseException exception) { return exception; }

        return null;
    }

    private Delegate CompileAndCache(Type inputType, Type resultType, string expressionRaw)
    {
        var key = new CacheKey(inputType, resultType, expressionRaw);
        var method = _cache.GetOrAdd(key, ck => new(() => Compile(ck)));

        return method.Value;
    }

    private Delegate Compile(CacheKey key) =>
        Compile(key.InputType, key.ResultType, key.ExpressionRaw);

    private Delegate Compile(Type inputType, Type resultType, string expressionRaw) =>
        DynamicExpressionParser.ParseLambda(_parsingConfig, false, inputType, resultType, expressionRaw)
            .Compile();
}