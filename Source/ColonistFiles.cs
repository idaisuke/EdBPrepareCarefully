using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EdB.PrepareCarefully.Reflection;

namespace EdB.PrepareCarefully;

public static class ColonistFiles {
    public static string SavedColonistsFolderPath {
        get {
            try {
                return GenFilePaths.FolderUnderSaveData("PrepareCarefully");
            }
            catch (Exception e) {
                Logger.Error("Failed to get colonist save directory");
                throw e;
            }
        }
    }

    //
    // Static Properties
    //
    public static IEnumerable<FileInfo> AllFiles {
        get {
            var directoryInfo = new DirectoryInfo(SavedColonistsFolderPath);
            if (!directoryInfo.Exists) {
                directoryInfo.Create();
            }

            return from f in directoryInfo.GetFiles()
                where f.Extension == ".pcc"
                orderby f.LastWriteTime descending
                select f;
        }
    }

    public static string FilePathForSavedColonist(string colonistName) {
        return Path.Combine(SavedColonistsFolderPath, colonistName + ".pcc");
    }

    //
    // Static Methods
    //
    public static bool HaveColonistNamed(string colonistName) {
        foreach (var current in from f in AllFiles
                 select Path.GetFileNameWithoutExtension(f.Name)) {
            if (current == colonistName) {
                return true;
            }
        }

        return false;
    }

    public static string UnusedDefaultName() {
        var text = string.Empty;
        var num = 1;
        do {
            text = "Colonist" + num;
            num++;
        } while (HaveColonistNamed(text));

        return text;
    }
}
