using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AATS.Desktop.Models.Binance;

namespace AATS.Desktop.Services
{
    public interface IBinanceDataService
    {
        Task<List<BinanceKlineData>> GetHistoricalKlinesAsync(string symbol, string interval, int limit = 100);
        void StartLiveStream(string symbol, string interval, Action<BinanceKlineData> onTick);
        void StopLiveStream();
    }
}