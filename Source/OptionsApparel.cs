using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EdB.PrepareCarefully {
    public class OptionsApparel {
        private List<ThingDef> emptyList = new List<ThingDef>();

        private Dictionary<PawnLayer, List<ThingDef>> pawnLayerApparelLookup =
            new Dictionary<PawnLayer, List<ThingDef>>();

        public IEnumerable<ThingDef> AllApparel {
            get {
                IEnumerable<ThingDef> result = null;
                foreach (var list in pawnLayerApparelLookup.Values) {
                    if (result == null) {
                        result = list;
                    }
                    else {
                        result = result.Concat(list);
                    }
                }

                return result;
            }
        }

        public void Add(PawnLayer layer, ThingDef def) {
            List<ThingDef> list;
            if (!pawnLayerApparelLookup.TryGetValue(layer, out list)) {
                list = new List<ThingDef>();
                pawnLayerApparelLookup.Add(layer, list);
            }

            list.Add(def);
        }

        public List<ThingDef> GetApparel(PawnLayer layer) {
            List<ThingDef> list;
            if (!pawnLayerApparelLookup.TryGetValue(layer, out list)) {
                return emptyList;
            }
            else {
                return list;
            }
        }

        public void Sort() {
            foreach (var list in pawnLayerApparelLookup.Values) {
                if (list != null) {
                    list.Sort((ThingDef x, ThingDef y) => {
                        if (x.label == null) {
                            return -1;
                        }
                        else if (y.label == null) {
                            return 1;
                        }
                        else {
                            return x.label.CompareTo(y.label);
                        }
                    });
                }
            }
        }
    }
}
