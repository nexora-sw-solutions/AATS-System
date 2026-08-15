using System;
using System.Threading;
using System.Threading.Tasks;

namespace AATS.Desktop.Services;

/// <summary>
/// Separate mock data source for development purposes.
/// Emulates multi-user system telemetry (Global API Response Latency) fluctuating between ~15ms and ~45ms.
/// Can be easily replaced with a SignalR backend implementation when ready.
/// </summary>
public class MockTelemetryDataService : ITelemetryDataService
{
    private static MockTelemetryDataService? _instance;
    public static MockTelemetryDataService Instance => _instance ??= new MockTelemetryDataService();

    private readonly Random _random = new();
    private CancellationTokenSource? _cts;
    private bool _isStreaming;

    public event Action<double>? TelemetryTickReceived;

    private MockTelemetryDataService() { }

    public async Task StartStreamingAsync()
    {
        if (_isStreaming) return;
        _isStreaming = true;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(400));
            while (!token.IsCancellationRequested && await timer.WaitForNextTickAsync(token))
            {
                // API latency fluctuates smoothly around ~15ms to ~45ms per specification
                double simulatedLatency = 25 + (_random.NextDouble() * 18 - 8);
                TelemetryTickReceived?.Invoke(simulatedLatency);
            }
        }
        catch (OperationCanceledException)
        {
            // Clean shutdown when cancelled
        }
        finally
        {
            _isStreaming = false;
        }
    }

    public Task StopStreamingAsync()
    {
        _cts?.Cancel();
        _isStreaming = false;
        return Task.CompletedTask;
    }
}
