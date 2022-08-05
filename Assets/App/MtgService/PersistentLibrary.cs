using System;
using System.Linq;
using System.Collections.Generic;

namespace App.MtgService {
    internal class PersistentLibrary {
        public readonly Dictionary<Guid, Card> Cards = new Dictionary<Guid, Card>();
        public readonly Dictionary<Guid, int> Counts = new Dictionary<Guid, int>();

        public void Clear() {
            Cards.Clear();
            Counts.Clear();
        }

        public void AddInstance(Guid cardTypeId) {
            if (!Counts.ContainsKey(cardTypeId)) {
                Counts.Add(cardTypeId, 0);
            }

            Counts[cardTypeId]++;
        }

        public void AddType(Card card)
            => Cards.Add(card.TypeId, card);

        public Card Find(Guid id)
            => Cards[id];

        public Card Find(string title)
            => Cards.Values.FirstOrDefault(c => c.Title.ToLower() == title.ToLower());
    }
}
