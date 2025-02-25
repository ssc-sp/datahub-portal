using Azure;
using Azure.Core;

namespace Datahub.Functions.UnitTests
{
    public static class AsyncPageableHelper
    {
        public static AsyncPageable<T> CreateAsyncPageable<T>(IEnumerable<T> items)
        {
            return new TestAsyncPageable<T>(items);
        }

        private class TestAsyncPageable<T> : AsyncPageable<T>
        {
            private readonly IEnumerable<T> _items;

            public TestAsyncPageable(IEnumerable<T> items)
            {
                _items = items;
            }

            public override async IAsyncEnumerable<Page<T>> AsPages(string? continuationToken = null, int? pageSizeHint = null)
            {
                yield return Page<T>.FromValues(_items.ToList(), null, new MockResponse());
            }

            private class MockResponse : Response
            {
                public override int Status => 200;
                public override string ReasonPhrase => "OK";
                public override Stream? ContentStream { get; set; }
                public override string ClientRequestId { get; set; } = string.Empty;
                public override void Dispose() { }
                protected override bool TryGetHeader(string name, out string? value) { value = null; return false; }
                protected override bool TryGetHeaderValues(string name, out IEnumerable<string>? values) { values = null; return false; }
                protected override bool ContainsHeader(string name) => false;
                protected override IEnumerable<HttpHeader> EnumerateHeaders() => Enumerable.Empty<HttpHeader>();
            }
        }
    }
}