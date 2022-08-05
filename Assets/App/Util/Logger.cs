using System;

using UnityEngine;

namespace App {
    public class Logger {
        public int Verbosity;

        private Type type;

        public Logger(Type type) {
            this.type = type;
        }

        public Logger(Type type, int verbosity) : this(type) {
            Verbosity = verbosity;
        }

        public void Info(string message) {
            Debug.Log(Format("INFO", message));
        }

        public void Warning(string message) {
            Debug.LogWarning(Format("WARN", message));
        }

        public void Error(string message) {
            Debug.LogError(Format("ERROR", message));
        }

        public void Verbose(int num, string message) {
            if (Verbosity >= num) {
                Info("VERBOSE: " + message);
            }
        }

        private string Format(string scope, string message) {
            //var text = $"[{Time.frameCount}]: {Time.time}: {type.Name}: {message}";
            var text = $"{type.Name}: {message}";
            Console.WriteLine(scope + ": " + text);
            return text;
        }
    }
}

