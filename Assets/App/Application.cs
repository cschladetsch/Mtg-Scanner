#define TEST

using System.IO;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using App.MtgService;
using App.Camera;
using App.Services;

namespace App {
    public class Application : MonoBehaviour {

        public static Logger Log = new Logger(typeof(Application));

        public Services.Services Services;

        public Button CardArt;
        public Button CardInfo;
        public Button CardRulings;
        public Button TakeImageButton;

        public TextMeshProUGUI CardName;
        public TextMeshProUGUI CardDescription;
        public TextMeshProUGUI CardFlavor;
        public TextMeshProUGUI CardPrice;
        public TextMeshProUGUI CardType;
        public TextMeshProUGUI CardManaCost;
        public TextMeshProUGUI CardPowerToughness;
        public TextMeshProUGUI TakeImageButtonText;

        public WebCamViewer webCamViewer;

        public CardLibraryService CardLibrary => Services.CardLibraryService;
        public PersistentDataService PersistentData => Services.PersistentDataService;
        public CostConverterService CostConverter => Services.CostConverterService;

        private void Start() {
            BetterStreamingAssets.Initialize();
            LoadCard();
        }

        private async void LoadCard() {
            await CardLibrary.LoadLibrary("MainLibrary.json");
#if TEST
            await CardLibrary.ProcessFileImage("b3656310-093d-4724-a399-7f7010843b1f-normal.jpg");
            var card = CardLibrary.FindCard("Ajanis Pridemate");
            if (card == null) {
                return;
            }

            PopulateText(card.ScryfallCard);
#endif
        }

        // Called from UI
        public void TakeSnap() {
            DisableTakeImageButtonText();
            StartCoroutine(webCamViewer.TakePhotoCoro(TakenImage));
        }

        private void EnableTakeImageButtonText() { 
            TakeImageButtonText.text = "Snap";
            TakeImageButton.enabled = true;
        }

        private void DisableTakeImageButtonText() {
            TakeImageButtonText.text = "...";
            TakeImageButton.enabled = false;
        }

        private async void TakenImage(WebCamViewer webCamViewer) {
            Log.Info($"WebCamViewer: {webCamViewer}");
            Log.Info($"Path: {Path.GetFileName(webCamViewer.AbsolutePathToImage)}");
            Log.Info($"WebCamViewer.name: {webCamViewer.name}");
            Log.Info($"Library: {CardLibrary}");
            Log.Info($"WebCamViewer.libraryFileName: {CardLibrary.FileName}");
            Log.Info($"Taken image at {Path.GetFileName(webCamViewer.AbsolutePathToImage)}. Library={CardLibrary.FileName}. webCamViewer={webCamViewer.name}");

            EnableTakeImageButtonText();

            var card = await CardLibrary.ProcessFileImage(webCamViewer.AbsolutePathToImage);
            if (card == null) {
                Log.Error("Could not recognise card. Try again.");
            } else {
                Populate(card);
            }
        }

        private void Populate(Card card) {
            Debug.Assert(card != null);
            Debug.Assert(card.ScryfallCard != null);
            CardName.text = card.Title;
            CardDescription.text = card.Description;
            CardPrice.text = card.priceAud.ToString("c") + " AUD";
            PopulateText(card.ScryfallCard);
        }

        private void PopulateText(ScryfallCard scryfallCard) {
            if (scryfallCard == null) {
                Log.Error("No ScryFallCard");
                EnableTakeImageButtonText();
                return;
            }

            if (CardType == null) {
                Log.Error("No CardType");
                EnableTakeImageButtonText();
                return;
            }
           
            CardType.text = scryfallCard.type_line;
            if (scryfallCard.mana_cost != null) {
                CardManaCost.text = scryfallCard.mana_cost;
                Log.Info(CardManaCost.text = scryfallCard.mana_cost);
            }

            if (scryfallCard.flavor_text != null) {
                CardFlavor.text = scryfallCard.flavor_text;
            } else {
                CardFlavor.enabled = false;
            }
        }
    }
}

