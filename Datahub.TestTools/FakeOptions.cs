using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceProvisioner.SpecflowTests
{
    public class FakeOptions<T> : IOptions<T> where T : class, new()
    {
        public FakeOptions(T value) => Value = value;
        public T Value { get; }
    }

    public static class FakeOptionsSnapshot
    {
        public static IOptions<T> Create<T>(T value) where T : class, new() => new FakeOptions<T>(value);
        public static IOptions<T> AsOptions<T>(this T value) where T : class, new() => new FakeOptions<T>(value);

    }
}
