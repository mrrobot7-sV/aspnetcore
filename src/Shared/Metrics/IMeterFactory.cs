// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Metrics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
internal sealed class MetricsOptions
{
    public IList<KeyValuePair<string, object?>> DefaultTags { get; } = new List<KeyValuePair<string, object?>>();
}

internal interface IMetricsBuilder
{
    IServiceCollection Services { get; }
}

internal sealed class MetricsBuilder : IMetricsBuilder
{
    public MetricsBuilder(IServiceCollection services) => Services = services;
    public IServiceCollection Services { get; }
}

internal sealed class MeterOptions
{
    public required string Name { get; set; }
    public string? Version { get; set; }
    public IList<KeyValuePair<string, object?>>? DefaultTags { get; set; }
}

internal interface IMeterFactory
{
    Meter CreateMeter(string name);
    Meter CreateMeter(MeterOptions options);
}

internal interface IMeterRegistry
{
    void Add(Meter meter);
    bool Contains(Meter meter);
}

internal sealed class DefaultMeterRegistry : IMeterRegistry, IDisposable
{
    private readonly object _lock = new object();
    private readonly List<Meter> _meters = new List<Meter>();

    public void Add(Meter meter)
    {
        lock (_lock)
        {
            _meters.Add(meter);
        }
    }

    public bool Contains(Meter meter)
    {
        lock (_lock)
        {
            return _meters.Contains(meter);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var meter in _meters)
            {
                meter.Dispose();
            }
            _meters.Clear();
        }
    }
}
#pragma warning restore RS0016 // Add public types and members to the declared API
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
