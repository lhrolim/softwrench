using System;
using System.IO;
using System.Security.AccessControl;

namespace cts.commons.Util {

    /// <summary>
    /// Utility methods for directory actions.
    /// </summary>
    static public class DirectoryUtil {
        public static bool HasWriteAccessToFolder(string folderPath) {
            try {
                // Attempt to get a list of security permissions from the folder. 
                // This will raise an exception if the path is read only or do not have access to view the permissions. 
                if (Directory.Exists(folderPath)) {
                    bool isWriteAccess = false;
                    try {
                        AuthorizationRuleCollection collection = Directory.GetAccessControl(folderPath)
                            .GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));

                        foreach (FileSystemAccessRule rule in collection) {
                            if ((((FileSystemAccessRule)rule).FileSystemRights & FileSystemRights.WriteData) > 0) {
                                isWriteAccess = true;
                                break;
                            }
                        }
                    } catch (Exception ex) {
                        isWriteAccess = false;
                    }

                    return isWriteAccess;
                }

                return false;
            } catch (UnauthorizedAccessException) {
                return false;
            }
        }
    }
}
