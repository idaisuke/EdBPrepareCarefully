using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EdB.PrepareCarefully.Reflection;

namespace EdB.PrepareCarefully;

public static class PresetFiles {
    public static string SavedPresetsFolderPath {
        get {
            try {
                return GenFilePaths.FolderUnderSaveData("PrepareCarefully");
            }
            catch (Exception e) {
                Logger.Error("Failed to get preset save directory");
                throw e;
            }
        }
    }

    //
    // Static Properties
    //
    public static IEnumerable<FileInfo> AllFiles {
        get {
            var directoryInfo = new DirectoryInfo(SavedPresetsFolderPath);
            if (!directoryInfo.Exists) {
                directoryInfo.Create();
            }

            return from f in directoryInfo.GetFiles()
                where f.Extension == ".pcp"
                orderby f.LastWriteTime descending
                select f;
        }
    }

    public static string FilePathForSavedPreset(string presetName) {
        return Path.Combine(SavedPresetsFolderPath, presetName + ".pcp");
    }

    //
    // Static Methods
    //
    public static bool HavePresetNamed(string presetName) {
        foreach (var current in from f in AllFiles
                 select Path.GetFileNameWithoutExtension(f.Name)) {
            if (current == presetName) {
                return true;
            }
        }

        return false;
    }

    public static string UnusedDefaultName() {
        var text = string.Empty;
        var num = 1;
        do {
            text = "Preset" + num;
            num++;
        } while (HavePresetNamed(text));

        return text;
    }
}
