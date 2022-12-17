using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EdB.PrepareCarefully; 

public class OptionsHeadType {
    protected int count = 0;
    protected List<CustomHeadType> femaleHeadTypes = new();
    protected List<string> headPaths = new();
    protected List<Graphic> heads = new();
    public List<CustomHeadType> headTypes = new();
    protected List<CustomHeadType> maleHeadTypes = new();
    protected List<CustomHeadType> noGenderHeaderTypes = new();
    public Dictionary<string, CustomHeadType> pathDictionary = new();

    public void AddHeadType(CustomHeadType headType) {
        headTypes.Add(headType);
        //Logger.Debug(headType.ToString());
        if (!headType.GraphicPath.NullOrEmpty() && !pathDictionary.ContainsKey(headType.GraphicPath)) {
            pathDictionary.Add(headType.GraphicPath, headType);
        }

        if (!headType.AlternateGraphicPath.NullOrEmpty() &&
            !pathDictionary.ContainsKey(headType.AlternateGraphicPath)) {
            pathDictionary.Add(headType.AlternateGraphicPath, headType);
        }
    }

    public IEnumerable<CustomHeadType> GetHeadTypesForGender(Gender gender) {
        return headTypes.Where(headType => {
            return headType.Gender == gender || headType.Gender == null;
        });
    }

    public CustomHeadType FindHeadTypeForPawn(Pawn pawn) {
        var alienComp = ProviderAlienRaces.FindAlienCompForPawn(pawn);
        if (alienComp == null) {
            var result = FindHeadTypeByGraphicsPath(pawn.story.HeadGraphicPath);
            if (result == null) {
                Logger.Warning("Did not find head type for path: " + pawn.story.HeadGraphicPath);
            }

            return result;
        }
        else {
            var crownType = ProviderAlienRaces.GetCrownTypeFromComp(alienComp);
            var result = FindHeadTypeByCrownTypeAndGender(crownType, pawn.gender);
            if (result == null) {
                Logger.Warning("Did not find head type for alien crown type: " + crownType);
            }

            return result;
        }
    }

    public CustomHeadType FindHeadTypeByGraphicsPath(string graphicsPath) {
        if (pathDictionary.TryGetValue(graphicsPath, out var result)) {
            return result;
        }

        return null;
    }

    public CustomHeadType FindHeadTypeByCrownType(string crownType) {
        return headTypes.Where(t => {
            return t.AlienCrownType == crownType;
        }).FirstOrDefault();
    }

    public CustomHeadType FindHeadTypeByCrownTypeAndGender(string crownType, Gender gender) {
        return headTypes.Where(t => {
            return t.AlienCrownType == crownType && (t.Gender == null || t.Gender == gender);
        }).FirstOrDefault();
    }

    public CustomHeadType FindHeadTypeForGender(CustomHeadType headType, Gender gender) {
        if (headType.Gender == null || headType.Gender == gender) {
            return headType;
        }

        if (headType.AlienCrownType.NullOrEmpty()) {
            var graphicsPath = headType.GraphicPath;
            var prefixReplacementString = "/" + gender + "_";
            var directoryReplacementString = "/" + gender + "/";
            if (headType.Gender == Gender.Male) {
                graphicsPath = graphicsPath.Replace("/Male/", directoryReplacementString);
                graphicsPath = graphicsPath.Replace("/Male_", prefixReplacementString);
            }

            if (headType.Gender == Gender.Female) {
                graphicsPath = graphicsPath.Replace("/Female/", directoryReplacementString);
                graphicsPath = graphicsPath.Replace("/Female_", prefixReplacementString);
            }
            else {
                graphicsPath = graphicsPath.Replace("/None/", directoryReplacementString);
                graphicsPath = graphicsPath.Replace("/None_", prefixReplacementString);
            }

            var result = FindHeadTypeByGraphicsPath(graphicsPath);
            if (result == null) {
                Logger.Warning("Could not find head type for gender: " + graphicsPath);
            }

            return result != null ? result : headType;
        }
        else {
            var targetGender = gender;
            if (headType.Gender == Gender.Male) {
                targetGender = Gender.Female;
            }

            if (headType.Gender == Gender.Female) {
                targetGender = Gender.Male;
            }
            else {
                return headType;
            }

            var result = headTypes.Where(h => {
                return h.AlienCrownType == headType.AlienCrownType && h.Gender == targetGender;
            }).FirstOrDefault();
            return result != null ? result : headType;
        }
    }
}
