using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Primitives
{
    public abstract class Executable<T> : IDisposable
    {
        #region IDisposable Support
        public bool IsDisposed { get; private set; } = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                }

                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        private void VerifyNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        public T Execute()
        {
            VerifyNotDisposed();
            var result = PerformExecute();
            Dispose();
            return result;
        }

        protected abstract T PerformExecute();

        public Task<T> ExecuteAsync()
        {
            return ExecuteAsync(CancellationToken.None);
        }

        public async Task<T> ExecuteAsync(CancellationToken ct)
        {
            VerifyNotDisposed();
            var result = await PerformExecuteAsync(ct);
            Dispose();
            return result;
        }

        protected abstract Task<T> PerformExecuteAsync(CancellationToken ct);
    }
}
