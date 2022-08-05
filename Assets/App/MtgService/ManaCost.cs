using System.Linq;
using System.Collections.Generic;

namespace App.MtgService {
    public class ManaCost {

        private Dictionary<EManaType, int> _costs = new Dictionary<EManaType, int>();

        public int GetCost(EManaType type)
            => !_costs.TryGetValue(type, out var mana) ? 0 : mana;

        public int CommonManaCost()
            => _costs.Sum(kv => kv.Value);

        public int Cmc()
            => CommonManaCost();

        public static ManaCost FromString(string text) {
            var cost = new ManaCost();
            return cost;
        }
    }
}

