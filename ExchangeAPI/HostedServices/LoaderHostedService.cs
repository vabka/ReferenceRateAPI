using System;
using System.Threading;
using System.Threading.Tasks;
using ExchangeRateLoader;
using Microsoft.Extensions.Hosting;

namespace ExchangeAPI.HostedServices
{
    public class LoaderHostedService : IHostedService
    {
        private readonly Loader _loader;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task? _task;

        public LoaderHostedService(Loader loader, CancellationTokenSource cancellationTokenSource)
        {
            _loader = loader;
            _cancellationTokenSource = cancellationTokenSource;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var token = _cancellationTokenSource.Token;
            _task = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    await _loader.LoadFreshData(token);
                    await Task.Delay(TimeSpan.FromMinutes(15), token);
                }
            }, token);
            return Task.CompletedTask;
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            if (_task != null)
                await _task;
        }
    }
}
