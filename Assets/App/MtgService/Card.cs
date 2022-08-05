using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Flurl;
using Flurl.Http;

namespace App.MtgService {
    public class Card {
        public string Title;
        public Guid TypeId;
        public Guid ScryfallId;
        public ManaCost ManaCost;
        public ECardType Type;
        public ERelease Release;
        public string Name;
        public string Text => ScryfallCard?.oracle_text;
        public string Description => Text;
        public ScryfallCard ScryfallCard;
        public string ImageFilename;
        public float priceUsd;
        public float priceAud;

        // TODO
        public bool IsStandard => false;
        public bool IsModern => true;
        public bool IsPioneer => true;

        private static Logger Log = new Logger(typeof(Card));
        private const string ScryfallEndpoint = "https://api.scryfall.com";

        public override string ToString() {
            return ($"{Title}, {Text}");
        }

        public bool FetchInfoSync(string dataPath) {
            Task<bool> task = Task.Run(async () => await FetchInfo(dataPath));
            return task.Result;
        }

        public async Task<bool> FetchInfo(string dataPath) {
            if (ScryfallCard != null) {
                Log.Warning($"Do you really want to FetchInfo on same card {Title} twice?");
            }

            try {
                if (!await CreateScryfallCard(dataPath)) {
                    Log.Error($"Couln't get card details for {Title}");
                }
 
                if (!await GetCardPrice()) {
                    Log.Warning($"Couldn't get price for {Title}");
                }

                return await LoadImage(dataPath, ScryfallCard, "art_crop") && await LoadImage(dataPath, ScryfallCard, "normal");
            } catch (Exception e) {
                Log.Error($"Error getting info on {Title}: {e.Message}");
                return false;
            }
        }

        private async Task<bool> CreateScryfallCard(string dataPath) {
           var text  = await ScryfallEndpoint.AppendPathSegment("cards/named")
                .SetQueryParam("fuzzy", Title)
                .GetStringAsync();

            ScryfallCard = Newtonsoft.Json.JsonConvert.DeserializeObject<ScryfallCard>(text);

            if (ScryfallCard == null) {
                Log.Error($"Couldn't parse info for {Title}");
                return false;
            }

            Log.Info($"Info for {Title}: {ScryfallCard.oracle_text}");
            ScryfallId = ScryfallCard.id;
            Title = ScryfallCard.name;
            ScryfallCard.oracle_text = ExpandText(ScryfallCard.oracle_text);

            return true;
        }

        private async Task<bool> GetCardPrice() {
            if (ScryfallCard.prices == null) {
                if (ScryfallCard.purchase_uris != null) {
                    foreach (var kv in ScryfallCard.purchase_uris) {
                        try {
                            string costingSite = kv.Value;
                            Log.Warning("Trying " + costingSite);
                            var result = await costingSite.GetStringAsync();
                        } catch (Exception ex) {
                            Log.Error($"...failed: {ex.Message}");
                            continue;
                        }
                        break;
                    }
                }

                return false;
            } else {
                priceUsd = float.Parse(ScryfallCard.prices["usd"]);
                priceAud = priceUsd*1.4f;   // TODO: use price CostConverter
            }

            return true;
        }

        private async Task<bool> LoadImage(string dataPath, ScryfallCard result, string imageType) {
            // normal is   488 x 680px = 0.72 aspect
            // art_crop is 626 x 457px = 1.37 aspect
            // TODO: Use local databases
            var imageName = $"{result.id}-{imageType}.jpg";
            ImageFilename = Path.Combine(dataPath, "images", imageName);
            if (File.Exists(ImageFilename)) {
                return true;
            }

            // TODO: Use local databases
            var imageUri = result.image_uris[imageType];
            var bytes = await imageUri.GetBytesAsync();
            File.WriteAllBytes(ImageFilename, bytes);
            Log.Info($"Wrote image for {result.name} to {ImageFilename}");

            return true;
        }

        string ExpandText(string text) {
            var replacements = new Dictionary<string, string>() {
                ["{T}"] = "Tap",
                ["{W}"] = "Plains",
                ["{B}"] = "Swamp",
                ["{R}"] = "Mountain",
                ["{U}"] = "Island",
                ["{G}"] = "Forest",
            };
            return replacements.Aggregate(text, (current, kv) => current.Replace(kv.Key, kv.Value));
        }
    }
}

