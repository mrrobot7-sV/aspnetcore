// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Metrics;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Server.Kestrel.Core;

/// <summary>
/// Kestrel server.
/// </summary>
public class KestrelServer : IServer
{
    private readonly KestrelServerImpl _innerKestrelServer;

    /// <summary>
    /// Initializes a new instance of <see cref="KestrelServer"/>.
    /// </summary>
    /// <param name="options">The Kestrel <see cref="IOptions{TOptions}"/>.</param>
    /// <param name="transportFactory">The <see cref="IConnectionListenerFactory"/>.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
    public KestrelServer(IOptions<KestrelServerOptions> options, IConnectionListenerFactory transportFactory, ILoggerFactory loggerFactory)
    {
        _innerKestrelServer = new KestrelServerImpl(
            options,
            new[] { transportFactory ?? throw new ArgumentNullException(nameof(transportFactory)) },
            Array.Empty<IMultiplexedConnectionListenerFactory>(),
            loggerFactory,
            new KestrelMetrics(new DummyMeterFactory()));
    }

    /// <inheritdoc />
    public IFeatureCollection Features => _innerKestrelServer.Features;

    /// <summary>
    /// Gets the <see cref="KestrelServerOptions"/>.
    /// </summary>
    public KestrelServerOptions Options => _innerKestrelServer.Options;

    /// <inheritdoc />
    public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken) where TContext : notnull
    {
        return _innerKestrelServer.StartAsync(application, cancellationToken);
    }

    // Graceful shutdown if possible
    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _innerKestrelServer.StopAsync(cancellationToken);
    }

    // Ungraceful shutdown
    /// <inheritdoc />
    public void Dispose()
    {
        _innerKestrelServer.Dispose();
    }

    // This factory used when type is created without DI. For example, via KestrelServer.
    private sealed class DummyMeterFactory : IMeterFactory
    {
        public Meter CreateMeter(string name) => new Meter(name);

        public Meter CreateMeter(MeterOptions options) => new Meter(options.Name, options.Version);
    }
}
