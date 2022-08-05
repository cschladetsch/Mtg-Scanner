using System.Collections.Generic;

namespace App.MtgService {
    public class GoogleVisionResponse {
        public class FullTextAnnotation {
            public List<object> pages;              // don't care
            public string text;
        }

        public class Response {
            public List<object> textAnnotations;    // don't care
            public FullTextAnnotation FullTextAnnotation;
        }

        public List<Response> responses;
    }
}

