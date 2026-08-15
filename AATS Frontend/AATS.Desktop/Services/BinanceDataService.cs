using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AATS.Desktop.Models.Binance;

namespace AATS.Desktop.Services
{
    public class BinanceDataService : IBinanceDataService
    {
        private static readonly Lazy<BinanceDataService> _instance = new(() => new BinanceDataService());
        public static BinanceDataService Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cts;

        private BinanceDataService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://api.binance.com/") };
        }

        public async Task<List<BinanceKlineData>> GetHistoricalKlinesAsync(string symbol, string interval, int limit = 100)
        {
            var result = new List<BinanceKlineData>();
            try
            {
                var response = await _httpClient.GetAsync($"api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var array = JsonSerializer.Deserialize<JsonElement[][]>(json);
                    if (array != null)
                    {
                        foreach (var item in array)
                        {
                            result.Add(new BinanceKlineData
                            {
                                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(item[0].GetInt64()).DateTime,
                                Open = double.Parse(item[1].GetString()!),
                                High = double.Parse(item[2].GetString()!),
                                Low = double.Parse(item[3].GetString()!),
                                Close = double.Parse(item[4].GetString()!),
                                Volume = double.Parse(item[5].GetString()!)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching binance historical data: {ex.Message}");
            }
            return result;
        }

        public void StartLiveStream(string symbol, string interval, Action<BinanceKlineData> onTick)
        {
            StopLiveStream();
            _cts = new CancellationTokenSource();
            _ = ReceiveStreamAsync(symbol.ToLower(), interval, onTick, _cts.Token);
        }

        public void StopLiveStream()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            if (_webSocket != null)
            {
                _webSocket.Dispose();
                _webSocket = null;
            }
        }

        private async Task ReceiveStreamAsync(string symbol, string interval, Action<BinanceKlineData> onTick, CancellationToken token)
        {
            _webSocket = new ClientWebSocket();
            var uri = new Uri($"wss://stream.binance.com:9443/ws/{symbol}@kline_{interval}");

            try
            {
                await _webSocket.ConnectAsync(uri, token);
                var buffer = new byte[8192];

                while (_webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
                    }
                    else
                    {
                        var jsonStr = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        try
                        {
                            var doc = JsonDocument.Parse(jsonStr);
                            var root = doc.RootElement;
                            if (root.TryGetProperty("k", out var kline))
                            {
                                var data = new BinanceKlineData
                                {
                                    OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(kline.GetProperty("t").GetInt64()).DateTime,
                                    Open = double.Parse(kline.GetProperty("o").GetString()!),
                                    High = double.Parse(kline.GetProperty("h").GetString()!),
                                    Low = double.Parse(kline.GetProperty("l").GetString()!),
                                    Close = double.Parse(kline.GetProperty("c").GetString()!),
                                    Volume = double.Parse(kline.GetProperty("v").GetString()!)
                                };
                                onTick?.Invoke(data);
                            }
                        }
                        catch { /* Ignore parse errors on stream */ }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Binance WebSocket Error: {ex.Message}");
            }
        }
    }
}