#if PLATFORM_ANDROID
#else
#endif

using System;
using System.IO;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using App.MtgService;

namespace App.Camera {
    public class WebCamViewer : MonoBehaviour {
        public string RelativePathToImage;
        public string AbsolutePathToImage;
        public string WebCamName; // USB Video Device
        public byte[] Data;

        private WebCamTexture _webCamTexture;
        private static readonly Logger _log = new Logger(typeof(WebCamViewer));

        private void Start() {
            var device = WebCamTexture.devices[GetCamera()];
            _webCamTexture = new WebCamTexture(device.name);
            GetComponent<Renderer>().material.mainTexture = _webCamTexture;
            _webCamTexture.Play();
        }

        private int GetCamera() {
            if (WebCamTexture.devices.Length == 0) {
                _log.Error("Cannot find any camera");
                return 0;
            }

            _log.Info("Here's a list of all cameras:");
            foreach (var t in WebCamTexture.devices) {
                _log.Info("Device: " + t.name);
            }
            _log.Info("----------------------------");

            for (var i = 0; i < WebCamTexture.devices.Length; i++) {
                var cameraName = WebCamTexture.devices[i].name;
                if (!cameraName.ToLower().Contains(WebCamName.ToLower())) {
                    continue;
                }

                _log.Info("Found device, index=" + i);
                return i;
            }

            _log.Error("Can not find your camera name. Here's a list of all cameras:");
            foreach (var t in WebCamTexture.devices) {
                _log.Info("Device: " + t.name);
            }

            return 0;
        }

        internal void SetImageArt(Card card, Button button) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(SampleCard);
            LoadImage(card, "art-crop");
        }

        internal void SetImage(Card card, Button button) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(SampleCard);
            LoadImage(card, "normal");
        }

        private void SampleCard() {
            TurnOnCamera();
        }

        private void LoadImage(Card card, string type) {
            TurnOffCamera();
            var path = Path.Combine(UnityEngine.Application.persistentDataPath, "images", $"{card.ScryfallId}-{type}.jpg");
            var texture = new Texture2D(_webCamTexture.width, _webCamTexture.height);
            texture.LoadImage(File.ReadAllBytes(path));
            GetComponent<Renderer>().material.mainTexture = texture;
        }

        private void TurnOnCamera() {
            _webCamTexture.Play();
            _webCamTexture.autoFocusPoint = new Vector2(0.5f, 0.3f);
            GetComponent<Renderer>().material.mainTexture = _webCamTexture;
        }

        private void TurnOffCamera() {
            _webCamTexture.Stop();
        }

        public IEnumerator TakePhotoCoro(Action<WebCamViewer> completed) {
            yield return new WaitForEndOfFrame();

            var texture = new Texture2D(_webCamTexture.width, _webCamTexture.height);
            texture.SetPixels(_webCamTexture.GetPixels());
            texture.Apply();

            Data = texture.EncodeToPNG();
            AbsolutePathToImage = Path.Combine(UnityEngine.Application.persistentDataPath, RelativePathToImage, "image.png");
            File.WriteAllBytes(AbsolutePathToImage, Data);
            completed(this);
        }
    }
}

