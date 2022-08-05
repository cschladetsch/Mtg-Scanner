using System.Collections.Generic;

namespace App.MtgService {
    internal class Deck {
        public string Name { get; set; }
        public List<Card> Cards { get; } = new List<Card>();

        public int Size() {
            return Cards.Count;
        }

        public bool IsCommander => Size() == 100;
        public bool IsPioneer => Size() == 60;
        public bool IsStandard => Size() == 60;
    }
}

