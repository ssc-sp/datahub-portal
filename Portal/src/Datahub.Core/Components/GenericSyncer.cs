namespace Datahub.Core.Components
{
    public class GenericSyncer
    {
        public event Action Notify;
        public event Func<Task> NotifyAsync;

        public void Ping() => Notify?.Invoke();
        public async Task PingAsync() => await NotifyAsync?.Invoke();
    }
}
