using System;
using System.Threading.Tasks;

namespace AATS.Desktop.Services;

/// <summary>
/// Abstraction for live telemetry data streaming (e.g., API Latency & Network Activity).
/// Keeps UI and ViewModel loosely coupled from the underlying data source (Mock or SignalR).
/// </summary>
public interface ITelemetryDataService
{
    /// <summary>
    /// Event triggered when a new telemetry tick (latency in milliseconds) is received.
    /// </summary>
    event Action<double>? TelemetryTickReceived;

    /// <summary>
    /// Starts the telemetry data stream.
    /// </summary>
    Task StartStreamingAsync();

    /// <summary>
    /// Stops the telemetry data stream.
    /// </summary>
    Task StopStreamingAsync();
}
