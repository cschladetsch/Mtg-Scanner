using System;
using System.Collections.Generic;

#pragma warning disable 649

// All data formats here accurate as at 5 May 20222
namespace App.MtgService {
    public class ScryfallCard {
        public string @object;
        public Guid id;
        public Guid oracle_id;
        public IList<int> multiverse_ids;
        public int mtgo_id;
        public int mtgo_foil_id;
        public int tcgplayer_id;
        public int cardmarket_id;
        public string name;
        public string lang;
        public string released_at;
        public string uri;
        public string scryfall_uri;
        public string layout;
        public bool highres_image;
        public string image_status;
        public Dictionary<string, string> image_uris;
        public string mana_cost;
        public float cmc;
        public string type_line;
        public string oracle_text;
        public int power;
        public int toughness;
        public IList<string> colors;
        public IList<string> color_identity;
        public IList<string> keywords;
        public Dictionary<string, string> legalities;
        public IList<string> games;
        public bool reserved;
        public bool foil;
        public bool nonfoil;
        public IList<string> finishes;
        public bool oversized;
        public bool promo;
        public bool reprint;
        public bool variation;
        public Guid set_id;
        public string set;
        public string set_name;
        public string set_type;
        public string set_uri;
        public string set_search_uri;
        public string scryfall_set_uri;
        public string rulings_uri;
        public string prints_search_uri;
        public int collector_number;
        public bool digital;
        public string rarity;
        public string flavor_text;
        public Guid card_back_id;
        public string artist;
        public IList<Guid> artist_ids;
        public Guid illustration_id;
        public string border_color;
        public int frame;
        public bool full_art;
        public bool textless;
        public bool booster;
        public bool story_spotlight;
        public int edhrec_rank;
        public Dictionary<string, string> prices;
        public Dictionary<string, string> related_uris;
        public Dictionary<string, string> purchase_uris;

        public override string ToString() {
            return $"{name}";
        }
    }
}
