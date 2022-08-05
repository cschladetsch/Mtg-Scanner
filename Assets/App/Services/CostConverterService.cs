using System.IO;
using System.Threading.Tasks;

using UnityEngine;

using Flurl.Http;

namespace App.Services {
    public class CostConverterService : MonoBehaviour {
        private const string _apiKeyName = "cost-converter.api";
        private const string _apiHost = "https://free.currconv.com";
        private string _apiKey;

        public class CurrencyConversion {
            public string Currency;
            public float Conversion;

            public CurrencyConversion() {
            }

            public CurrencyConversion(string currency, float conversion) {
                Currency = currency;
                Conversion = conversion;
            }
        }

        public void Start() {
            _apiKey = File.ReadAllText(Path.Combine(UnityEngine.Application.persistentDataPath, _apiKeyName));
        }

        public async Task<float> UsdToAud(float usd) {
            var request = $"{_apiHost}/api/v7/convert?q=USD_AUD&compact=ultra&_apiKey={_apiKey}";
            var response = await request.GetJsonAsync<CurrencyConversion>();
            return response.Conversion;
        }
    }
}
