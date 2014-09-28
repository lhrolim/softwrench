namespace softWrench.sW4.Configuration.Definitions {
    public enum ConditionMatch {
        Exact, GeneralMatchMetadataId,GeneralMatchModule, GeneralMatchProfile, GeneralMatch,GeneralMatchAll, No
    }

    static internal class ConditionMatchExtensions {
        public static int GetPriority(this ConditionMatch matchType) {
            switch (matchType) {
                case ConditionMatch.Exact:
                    return 6;
                case ConditionMatch.GeneralMatchMetadataId:
                    return 5;
                case ConditionMatch.GeneralMatchModule:
                    return 4;
                case ConditionMatch.GeneralMatchAll:
                    return 3;
                case ConditionMatch.GeneralMatchProfile:
                    return 2;
                case ConditionMatch.GeneralMatch:
                    return 1;
                
                case ConditionMatch.No:
                    return 0;
            }
            return 0;
        }

        public static ConditionMatch And(this ConditionMatch condition, ConditionMatch other) {
            //keep lowest priority as we do and ==> once "NO" or "GENERAL", no chance to go up to Exact for instance.
            //However, we could go down, like from instance from General to NO
            return other.GetPriority() < condition.GetPriority() ? other : condition;
        }
    }


}