using Microsoft.Extensions.Hosting;

namespace AD.Api.Core.Services
{
    public abstract class StartupServiceBase : IHostedLifecycleService
    {
        readonly IServiceScopeFactory _scopeFactory;

        protected StartupServiceBase(IServiceProvider provider)
        {
            _scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        }

        public async Task StartingAsync(CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await this.StartingAsync(scope.ServiceProvider, cancellationToken).ConfigureAwait(false);
        }
        protected abstract Task StartingAsync(IServiceProvider provider, CancellationToken cancellationToken);

        public async Task StartedAsync(CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await this.StartedAsync(scope.ServiceProvider, cancellationToken).ConfigureAwait(false);
        }
        protected virtual Task StartedAsync(IServiceProvider provider, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public virtual async Task StoppingAsync(CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await this.StoppingAsync(scope.ServiceProvider, cancellationToken).ConfigureAwait(false);
        }
        protected virtual Task StoppingAsync(IServiceProvider provider, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public virtual async Task StoppedAsync(CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await this.StoppedAsync(scope.ServiceProvider, cancellationToken).ConfigureAwait(false);
        }
        protected virtual Task StoppedAsync(IServiceProvider provider, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

