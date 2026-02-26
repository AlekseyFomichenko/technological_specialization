namespace Client.Services
{
    public sealed class PendingResponse
    {
        private TaskCompletionSource<PendingResponseResult>? _tcs;
        private readonly object _lock = new();

        public void SetPending()
        {
            lock (_lock)
            {
                _tcs = new TaskCompletionSource<PendingResponseResult>();
            }
        }

        public void CompleteSuccess()
        {
            lock (_lock)
            {
                _tcs?.TrySetResult(PendingResponseResult.Ok());
            }
        }

        public void CompleteError(string code, string message)
        {
            lock (_lock)
            {
                _tcs?.TrySetResult(PendingResponseResult.Fail(code, message));
            }
        }

        public Task<PendingResponseResult> WaitAsync(CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<PendingResponseResult>? tcs;
            lock (_lock)
            {
                tcs = _tcs;
            }
            if (tcs is null)
                return Task.FromResult(PendingResponseResult.Fail("NoPending", "No pending response was set."));
            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            return tcs.Task;
        }
    }
}
