using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Linq.Dynamic.Core.Exceptions;

public class T_DynamicExpressionCache
{
    [DynamicLinqType]
    public class TheInput
    {
        public string TheString => "foobar";
        public int TheInt => 43;

        public bool TheBoolMethod() => true;
    }

    [Fact]
    public void Evaluate()
    {
        var cache = new DynamicExpressionCache();

        string theString = cache.Evaluate<TheInput, string>(new(), nameof(TheInput.TheString));
        theString.Should().Be(new TheInput().TheString);

        int theInt = cache.Evaluate<TheInput, int>(new(), nameof(TheInput.TheInt));
        theInt.Should().Be(new TheInput().TheInt);

        bool theBoolMethod = (bool)cache.Evaluate(new TheInput(), typeof(bool), string.Create(InvariantCulture, $"{nameof(TheInput.TheBoolMethod)}()"));
        theBoolMethod.Should().Be(new TheInput().TheBoolMethod());
    }

    [Fact]
    public void TryCompileNoCache()
    {
        var cache = new DynamicExpressionCache();

        var exception = cache.TryCompileNoCache<TheInput, string>(nameof(TheInput.TheString));
        exception.Should().BeNull();

        exception = cache.TryCompileNoCache<TheInput, int>(nameof(TheInput.TheInt));
        exception.Should().BeNull();

        exception = cache.TryCompileNoCache(typeof(TheInput), typeof(bool), string.Create(InvariantCulture, $"{nameof(TheInput.TheBoolMethod)}()"));
        exception.Should().BeNull();

        exception = cache.TryCompileNoCache<TheInput, string>("PropertyDoesNotExist");
        exception.Should().BeOfType<ParseException>().Which.Message.Should().StartWith("No property or field");

        exception = cache.TryCompileNoCache(typeof(TheInput), typeof(string), "MethodDoesNotExist()");
        exception.Should().BeOfType<ParseException>().Which.Message.Should().StartWith("No applicable method");
    }
}