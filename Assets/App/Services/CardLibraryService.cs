using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Flurl;
using Flurl.Http;

using Newtonsoft.Json;
using App.Services;

#pragma warning disable 649

namespace App.MtgService {
    public class CardLibraryService : MonoBehaviour {
        public string Name
        {
            get { return "Test"; } 
            set {}  
        }
        private PersistentDataService persistentDataService;

        public string DataPath;
        public IEnumerable<string> allExisitingCardnames => _allCardNames?.data;
        public string FileName => Path.Combine(DataPath, "Cards.json");
        public string AllCardsFileName => Path.Combine(DataPath, "AllCards.json");

        private PersistentLibrary _library;
        private DateTime _lastQuery;
        private string _visionApiKey;
        private static Logger Log = new Logger(typeof(CardLibraryService));
        private const string GoogleVisionApiKeyFileName = "GoogleVisonAPIKey.txt";
        private const string ScryFallEndpoint = "https://api.scryfall.com";
        private const float CardAspectRatio = 6.35f / 8.89f;  // width/height
        private const int SentImageWidth = 350;
        private const int SentImageHeight = (int)(SentImageWidth / CardAspectRatio + 0.5f);
        private class AllCardNames {
            public string @object;
            public string uri;
            public int total_values;
            public List<string> data;
        }
        private PersistentDataService _persistentDataService;
        private AllCardNames _allCardNames = new AllCardNames();

        public override string ToString() {
            return "A Library";
        }

        public IEnumerable<Card> Cards {
            get {
                foreach (var kv in _library.Counts) {
                    for (var n = 0; n < kv.Value; ++n) {
                        yield return _library.Cards[kv.Key];
                    }
                }
            }
        }

        public void Start() {
            _persistentDataService = FindObjectOfType<PersistentDataService>();
            DataPath = UnityEngine.Application.dataPath;
            Log.Warning("DataPath: " + DataPath);
            _visionApiKey = "AIzaSyC9X6yvf2PVjyi7I47hVk7SB81_fORqvfs";// _persistentDataService.ReadString(GoogleVisionApiKeyFileName);
            Log.Info("key=" + _visionApiKey);

            return;
        }

        public void Clear()
            => _library.Clear();

        public Card FindCard(Guid id)
            => _library.Find(id);

        private async void GetAllCardNamesSync()
            => await FetchAllCardNames();

        public void SaveLibrary(string fileName)
            => File.WriteAllText(fileName, JsonConvert.SerializeObject(_library));

        public Card FindCard(string name) {
            var title = ClosestStringMatchService.Find(name, _allCardNames.data);
            return string.IsNullOrEmpty(title) ? null : _library.Find(title);
        }

        public async Task<Card> ProcessFileImage(string fileName) {
            var bytes = ScaleDown(fileName);
            var result = await AnalyseImageText(Convert.ToBase64String(bytes));
            var response = JsonConvert.DeserializeObject<GoogleVisionResponse>(result);
            return response != null ? ProcessVisualAnalysisResponse(response) : null;
        }

        public async Task<int> LoadLibrary(string libraryFileName) {
            libraryFileName = Path.Combine(DataPath, libraryFileName);
            if (!File.Exists(libraryFileName)) {
                Log.Warning("Starting new library");
                _library = new PersistentLibrary();
                File.WriteAllText(libraryFileName, JsonConvert.SerializeObject(_library));
            }

            if (!File.Exists(AllCardsFileName)) {
                Log.Warning("Do not have list of all cards; fetching");
                await FetchAllCardNames();
            }

            try {
                _library = JsonConvert.DeserializeObject<PersistentLibrary>(File.ReadAllText(libraryFileName));
                return _library.Counts.Values.Aggregate(0, (a, b) => a + b);
            } catch (Exception ex) {
                Log.Error(ex.ToString());
            }
            return 0;
        }

        private Card AddCard(GoogleVisionResponse visionResponse) {
            var text = visionResponse.responses[0].FullTextAnnotation.text;
            var split = text.Split('\n');
            var rawTitle = TrimMana(split[0]);
            var title = ClosestStringMatchService.Find(rawTitle, _allCardNames.data);

            Log.Info($"Found {title} as best match for {rawTitle}");

            var card = new Card() {
                Title = title,
            };

            var existing = _library.Find(title);
            card.FetchInfoSync(DataPath);
            if (existing != null) {
                Log.Info($"Duplicate card {card.Title}");
                card.TypeId = existing.TypeId;
            } else {
                Log.Warning($"New card {card.Title}");
                card.TypeId = Guid.NewGuid();
                _library.AddType(card);
            }

            _library.AddInstance(card.TypeId);

            return card;
        }

        public void Export(string fileName) {
            var ext = Path.GetExtension(fileName);
            switch (ext) {
                case ".tappedout":
                    ExportTappedOut(fileName);
                    break;
            }
        }

        private static byte[] ScaleDown(string fileName) {
            var texture = new Texture2D(SentImageWidth, SentImageHeight, TextureFormat.ARGB32, false);
            texture.LoadImage(File.ReadAllBytes(fileName));
            var bytes = texture.EncodeToJPG();
            File.WriteAllBytes(fileName + "-short.jpg", bytes);
            return bytes;
        }

        private Card ProcessVisualAnalysisResponse(GoogleVisionResponse visionResponse) {
            if (visionResponse == null) {
                Log.Error("Failed to get any vision response");
                return null;
            }

            if (visionResponse.responses.Count == 0) {
                Log.Error("Got multiple visual responses. Can't disambiguate");
                return null;
            }

            var fullText = visionResponse.responses[0].FullTextAnnotation;
            if (fullText == null) {
                Log.Warning("Empty vision response. Try again.");
                return null;
            }

            return AddCard(visionResponse);
        }

        private async Task<string> AnalyseImageText(string base64Content) {
            var text = @"
            {
             'requests': [
              {
                'image': {
                  'content': '$CONTENT'
                },
                'features': [
                {
                  'type': 'TEXT_DETECTION'
                }
               ]
              }
             ]
            }
            ".Replace("$CONTENT", base64Content);
            var baseUrl = "https://vision.googleapis.com/v1/images:annotate?fields=responses&key=" + _visionApiKey;
            var response = await baseUrl.WithTimeout(TimeSpan.FromMinutes(1))
                .PostJsonAsync(JsonConvert.DeserializeObject(text));
            return await response.Content.ReadAsStringAsync();
        }

        private static string TrimMana(string title) {
            var trim1 = title.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ');
            return trim1;
        }

        private void ExportTappedOut(string fileName) {
            var sb = new StringBuilder();
            foreach (var c in _library.Counts) {
                var entry = $"{c.Value}x {_library.Cards[c.Key].Title}";
                sb.AppendLine(entry);
            }

            File.WriteAllText(fileName, sb.ToString());
        }

        private async Task FetchAllCardNames() {
            try {
                _allCardNames = await ScryFallEndpoint.AppendPathSegment("catalog/card-names")
                    .SetQueryParam("format", "json")
                    .GetJsonAsync<AllCardNames>();
                File.WriteAllText(AllCardsFileName, JsonConvert.SerializeObject(_allCardNames));
                Log.Info($"Fetched {_allCardNames.data.Count} card names");
                return;
            } catch (Exception e) {
                Log.Error($"Error: {e.Message}");
                return;
            }
        }

        private async Task<bool> FetchInfoAllCards() {
            const int minIntervalMillis = 250;
            foreach (var card in _library.Cards.Values) {
                var now = DateTime.Now;
                var delta = now - _lastQuery;
                if (delta.TotalMilliseconds < minIntervalMillis)
                    await Task.Delay(TimeSpan.FromMilliseconds(minIntervalMillis));

                _lastQuery = now;
                if (!await card.FetchInfo(DataPath))
                    Log.Warning($"Couldn't find info on {card.Title}");
            }

            return true;
        }
    }
}

