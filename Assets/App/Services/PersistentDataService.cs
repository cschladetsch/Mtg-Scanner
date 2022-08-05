using System;
using System.Collections;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

namespace App.Services {
    
    public class PersistentDataService : MonoBehaviour {

        private static readonly Logger Log = new Logger(typeof(PersistentDataService));

        private string MakePersistentFilename(string fileName) {
            return "jar:file://" + UnityEngine.Application.dataPath + "!/assets/" + fileName;
        }

        public class Result {
            public UnityWebRequest request;

            public Result() {
            }

            public Result(UnityWebRequest request) {
                this.request  = request;
            }
        }

        public string ReadString(string path) {
            return BetterStreamingAssets.ReadAllText(path);
            //string dbPath = "";

            //if (UnityEngine.Application.platform == RuntimePlatform.Android)
            //{
            //  // Android
            //  string oriPath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "db.bytes");
              
            //  // Android only use WWW to read file
            //  WWW reader = new WWW(oriPath);
            //  while ( ! reader.isDone) {}
              
            //  var realPath = UnityEngine.Application.persistentDataPath + "/db";
            //  System.IO.File.WriteAllBytes(realPath, reader.bytes);
              
            //  dbPath = realPath;
            //}
            //else
            //{
            //  // iOS
            //  dbPath = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "db.bytes");
            //}

            //var result = new Result();
            //StartCoroutine(ReadStringCoro(path, (received) => { result = received;} ));
            //if (result.request.error != null) {
            //    Log.Error($"Failed to read {path}: " + result.request.error);
            //    return null;
            //}

            //return result.request.downloadHandler.text;
            //var result = MakePersistentFilename(path).GetAsync();
            //return Task.Run(() => Task.FromResult(ReadStringAsync(result))).Result.Result;
            //return null;
        }

        Task<string> ReadStringAsync(string path) {
            return null;
        }

        public IEnumerator ReadStringCoro(string uri, Action<Result> onCompleteReadString) {
            Log.Info($"Read Url: {MakePersistentFilename(uri)}");
            using var webRequest = UnityWebRequest.Get(uri);
            var result = new Result(webRequest);
            yield return webRequest.SendWebRequest();

            if (webRequest.responseCode != 200) {
                Log.Error("Couldn't GET: " + webRequest.error);
                onCompleteReadString(result);
                yield break;
            }

            while (!webRequest.isDone) {
                Log.Info("...");
                switch (webRequest.result) {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Log.Error(webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Log.Error(webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Log.Info("Succeed: " + webRequest.downloadHandler.text);
                    Log.Info("Succeed: " + webRequest.downloadHandler.data);
                    break;
                case UnityWebRequest.Result.InProgress:
                    Log.Info("In Progress");
                    break;
                default:
                    Log.Error("Unknown error");
                    throw new ArgumentOutOfRangeException();
                }
            }
            onCompleteReadString(new Result(webRequest));
        }

        public void WriteBytes(string uri, byte[] body) {
        }

        public IEnumerator WriteBytesCoro(string uri, byte[] body) {
            Log.Info($"Write Url: {uri}");
            using var webRequest = UnityWebRequest.Put(uri, body);
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Log.Error(webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Log.Error(webRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                Log.Info("Succeed: " +webRequest.downloadHandler.text);
                break;
            case UnityWebRequest.Result.InProgress:
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

    }
}