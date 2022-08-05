using System.IO;

namespace App.MtgService {
    internal class Persistence {
        private static readonly string _dataPath = UnityEngine.Application.persistentDataPath;

        public static string GetLocalStoragePath(string filename) {
            var output = Path.Combine(_dataPath, filename);
            if (File.Exists(output)) {
                return output;
            }

            File.Copy(Path.Combine(_dataPath, filename), output);
            return output;
        }

        public static void WriteAllText(string path, string text) {
            File.WriteAllText(GetLocalStoragePath(path), text);
        }

        public static void WriteAllBytes(string path, byte[] bytes) {
            File.WriteAllBytes(GetLocalStoragePath(path), bytes);
        }

        public static string ReadAllText(string path, string text) {
            return File.ReadAllText(GetLocalStoragePath(path));
        }

        public static byte[] ReadAllBytes(string path, byte[] bytes) {
            return File.ReadAllBytes(GetLocalStoragePath(path));
        }
    }
}
