using UnityEngine;

namespace App.Camera {
    public class CameraFlashController : MonoBehaviour {
        public bool DefaultOn = true;
        private bool Active;
        private AndroidJavaObject camera1;

        public static Logger Log = new Logger(typeof(CameraFlashController));

        private void Start() {
            if (DefaultOn) {
                TorchStart();
            }
        }

         private void OnDestroy() {
            TorchStop();
        }

        public void TorchStart() {
            AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");
            WebCamDevice[] devices = WebCamTexture.devices;

            int camID = 0;
            camera1 = cameraClass.CallStatic<AndroidJavaObject>("open", camID);

            if (camera1 != null) {
                AndroidJavaObject cameraParameters = camera1.Call<AndroidJavaObject>("getParameters");
                cameraParameters.Call("setFlashMode", "torch");
                camera1.Call("setParameters", cameraParameters);
                camera1.Call("startPreview");
                Active = true;
            } else {
                Log.Error("[CameraParametersAndroid] Camera not available");
            }
        }

        public void TorchStop() {
            if (!Active) {
                return;
            }

            if (camera1 != null) {
                camera1.Call("stopPreview");
                camera1.Call("release");
                Active = false;
            } else {
                Log.Error("[CameraParametersAndroid] Camera not available");
            }
        }
    }
}
