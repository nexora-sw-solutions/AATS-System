using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace AATS.Desktop.Services;

/// <summary>
/// SignalR client implementation of ITelemetryDataService for live backend streaming (Step 4 specification).
/// Includes automatic retry and seamless fallback to MockTelemetryDataService if the server is offline or unreachable,
/// ensuring the dashboard remains dynamic and functional during frontend development without blocking the UI.
/// </summary>
public class SignalRTelemetryDataService : ITelemetryDataService
{
    private static SignalRTelemetryDataService? _instance;
    public static SignalRTelemetryDataService Instance => _instance ??= new SignalRTelemetryDataService();

    private HubConnection? _hubConnection;
    private readonly ITelemetryDataService _fallbackService = MockTelemetryDataService.Instance;
    private bool _isUsingFallback;

    public event Action<double>? TelemetryTickReceived;

    private SignalRTelemetryDataService() { }

    public async Task StartStreamingAsync()
    {
        try
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7123/hubs/telemetry") // Standard development backend endpoint
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<double>("ReceiveTelemetryTick", (latencyMs) =>
            {
                TelemetryTickReceived?.Invoke(latencyMs);
            });

            await _hubConnection.StartAsync();
            _isUsingFallback = false;
        }
        catch (Exception)
        {
            // Fallback to mock loop if backend is offline or endpoint is not yet available
            _isUsingFallback = true;
            _fallbackService.TelemetryTickReceived += OnFallbackTickReceived;
            await _fallbackService.StartStreamingAsync();
        }
    }

    private void OnFallbackTickReceived(double latencyMs)
    {
        TelemetryTickReceived?.Invoke(latencyMs);
    }

    public async Task StopStreamingAsync()
    {
        if (_isUsingFallback)
        {
            _fallbackService.TelemetryTickReceived -= OnFallbackTickReceived;
            await _fallbackService.StopStreamingAsync();
            _isUsingFallback = false;
        }

        if (_hubConnection is not null)
        {
            try
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
            catch
            {
                // Ignore shutdown exceptions
            }
            finally
            {
                _hubConnection = null;
            }
        }
    }
}
