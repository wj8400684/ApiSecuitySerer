using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ApiSecuity.Client;

public sealed class CancelableCompletionSource<T> : IDisposable
{
    private readonly TaskCompletionSource<T> _source = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private CancellationTokenRegistration? _registration;
    private int _isDisposed;

    public void SetupCancellation(Action callback, CancellationToken token)
    {
        _registration = token.Register(callback);
        if (_isDisposed == 1)
            _registration?.Dispose();
    }

    public void SetResult(T result)
        => _ = _source.TrySetResult(result);

    public void SetException(Exception exception)
        => _ = _source.TrySetException(exception);

    public Task<T> Task => _source.Task;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            return;

        _ = _source.TrySetCanceled();
        _registration?.Dispose();
    }
}

public sealed class AsyncQueue<T> : IDisposable
{
    private readonly object _lock = new();
    private readonly Queue<T> _queue = new();
    private readonly LinkedList<CancelableCompletionSource<T>> _pendingDequeues = new();
    private int _isDisposed;

    public void Enqueue(T item)
    {
        LinkedListNode<CancelableCompletionSource<T>>? node;

        lock (_lock)
        {
            ThrowIfDisposed();

            node = _pendingDequeues.First;
            if (node is not null)
            {
                node.Value.SetResult(item);
                _pendingDequeues.RemoveFirst();
            }
            else
                _queue.Enqueue(item);
        }

        node?.Value.Dispose();
    }

    public async IAsyncEnumerable<T> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
       yield return await DequeueAsync(cancellationToken);
    }
    
    public ValueTask<T> DequeueAsync(CancellationToken cancellationToken = default)
    {
        LinkedListNode<CancelableCompletionSource<T>>? node;

        lock (_lock)
        {
            ThrowIfDisposed();

            if (_queue.Count > 0)
                return new ValueTask<T>(_queue.Dequeue());

            node = _pendingDequeues.AddLast(new CancelableCompletionSource<T>());
        }

        node.Value.SetupCancellation(() => Cancel(node), cancellationToken);
        return new ValueTask<T>(node.Value.Task);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
            return;

        IEnumerable<CancelableCompletionSource<T>> pendingDequeues;

        lock (_lock)
        {
            pendingDequeues = _pendingDequeues.ToArray();
            _pendingDequeues.Clear();
            _queue.Clear();
        }

        foreach (var ccs in pendingDequeues)
        {
            ccs.Dispose();
        }
    }

    private void Cancel(LinkedListNode<CancelableCompletionSource<T>> node)
    {
        lock (_lock)
        {
            try
            {
                _pendingDequeues.Remove(node);
            }
            catch
            {
                // ignored
            }
        }

        try
        {
            node.Value.Dispose();
        }
        catch
        {
            // ignored
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed != 0)
            throw new ArgumentException(nameof(_isDisposed));
    }
}
