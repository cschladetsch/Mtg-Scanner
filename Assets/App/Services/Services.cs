using App.MtgService;
using UnityEngine;

namespace App.Services {
    public class Services : MonoBehaviour {
        public CardLibraryService CardLibraryService;
        public PersistentDataService PersistentDataService;
        public CostConverterService CostConverterService;
    }
}