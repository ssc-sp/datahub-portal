using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceProvisioner.SpecflowTests
{
    public class FakeOptionsSnapshot<T> : IOptionsSnapshot<T> where T : class, new()
    {
        public FakeOptionsSnapshot(T value) => Value = value;
        public T Value { get; }
        public T Get(string name) => Value;
    }

    public static class FakeOptionsSnapshot
    {
        public static IOptionsSnapshot<T> Create<T>(T value) where T : class, new() => new FakeOptionsSnapshot<T>(value);
        public static IOptionsSnapshot<T> AsOptionsSnapshot<T>(this T value) where T : class, new() => new FakeOptionsSnapshot<T>(value);

    }
}
