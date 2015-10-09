using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.Util;

namespace softWrench.sW4.Configuration.Definitions {
    public class ConditionMatchResult : IComparable<ConditionMatchResult> {
        private readonly string _module;
        private readonly int? _profile;

        internal Boolean MetadataMatched = false;
        private ConditionMatch _matchType;

        public ConditionMatchResult(string module, int? profile) {
            _module = module;
            _profile = profile;
            NumberOfExacts = 0;
        }

        public ConditionMatch MatchType {
            get {
                if (!_matchType.Equals(ConditionMatch.GeneralMatch)) {
                    return _matchType;
                }
                if (MetadataMatched) {
                    return ConditionMatch.GeneralMatchMetadataId;
                }

                if (ModuleAsked) {
                    return ConditionMatch.GeneralMatchModule;
                }
                if (ProfileAsked) {
                    return ConditionMatch.GeneralMatchProfile;
                }
                return _matchType;
            }
            set {
                _matchType = value;
            }
        }

        public int NumberOfExacts {
            get; set;
        }


        public Boolean ProfileAsked {
            get; set;
        }
        public Boolean ModuleAsked {
            get; set;
        }

        public int CompareTo(ConditionMatchResult other) {
            var comparison = other.MatchType.GetPriority().CompareTo(MatchType.GetPriority());
            if (comparison == 0) {
                var numberOfExacts = other.NumberOfExacts.CompareTo(NumberOfExacts);
                return numberOfExacts == 0 ? 1 : numberOfExacts;
            }

            return comparison;
        }


        public ConditionMatchResult AppendModule(String conditionString, String contextString) {
            var value = Calculate(conditionString, contextString);
            if (value == ConditionMatch.GeneralMatch && contextString != null) {
                //if a module was asked it should be matched no option to general match here
                value = ConditionMatch.No;
            }
            _matchType = _matchType.And(value);
            if (ConditionMatch.Exact.Equals(value)) {
                NumberOfExacts++;
                ModuleAsked = _module != null;
            }
            if (ConditionMatch.GeneralMatchAll.Equals(value)) {
                //TODO:review
                ModuleAsked = _module != null;
            }
            return this;
        }

        public ConditionMatchResult AppendAvailableProfile(int? storedProfile, ICollection<int?> userProfiles) {
            if (storedProfile == null) {
                return this;
            }
            if (userProfiles == null) {
                _matchType = _matchType.And(ConditionMatch.No);
            } else {
                var profileMatch = userProfiles.Contains(storedProfile) ? ConditionMatch.Exact : ConditionMatch.No;
                _matchType = _matchType.And(profileMatch);
                if (ConditionMatch.Exact.Equals(profileMatch)) {
                    NumberOfExacts++;
                    ProfileAsked = _profile != null;
                }
            }
            return this;
        }

        public ConditionMatchResult AppendSelectedProfile(int? userProfile, int? selectedProfile) {
            if (selectedProfile == null) {
                return this;
            }
            if (userProfile == null) {
                LoggingUtil.DefaultLog.WarnFormat("Inconsistent behavior detected. A selected profile was informed for an application that doesn´t contain any");
                return this;
            }
            if (selectedProfile != userProfile) {
                _matchType = _matchType.And(ConditionMatch.No);
            } else {
                _matchType = _matchType.And(ConditionMatch.Exact);
                NumberOfExacts++;
            }
            return this;
        }

        public ConditionMatchResult Append(String conditionString, String contextString) {
            var value = Calculate(conditionString, contextString);
            _matchType = _matchType.And(value);
            if (ConditionMatch.Exact.Equals(value)) {
                NumberOfExacts++;
            }
            return this;
        }

        public ConditionMatchResult AppendMetadataMatch(string conditionString, string contextString) {
            var numberOfExactsBefore = NumberOfExacts;
            var result = Append(conditionString, contextString);
            if (NumberOfExacts > numberOfExactsBefore) {
                MetadataMatched = true;
            }
            return result;
        }

        public ConditionMatchResult Append(ConditionMatch value) {
            _matchType = _matchType.And(value);
            if (ConditionMatch.Exact.Equals(value)) {
                NumberOfExacts++;
            }
            return this;
        }

        public static ConditionMatch Calculate(string conditionString, string contextString) {
            if (contextString == null) {
                if (conditionString == null) {
                    return ConditionMatch.Exact;
                }

                return Conditions.AnyCondition.Equals(conditionString)
                    ? ConditionMatch.GeneralMatchAll
                    : ConditionMatch.No;

            }
            if (conditionString == null) {
                //this means that we asked for something, but this is a general defined value
                return ConditionMatch.GeneralMatch;
            }
            if (conditionString == Conditions.AnyCondition) {
                return ConditionMatch.GeneralMatchAll;
            }
            if (conditionString.Contains(",")) {
                var any = conditionString.Split(',').Any(a => a.Equals(contextString, StringComparison.CurrentCultureIgnoreCase));
                return any ? ConditionMatch.Exact : ConditionMatch.No;
            }

            return conditionString.Equals(contextString, StringComparison.CurrentCultureIgnoreCase)
                ? ConditionMatch.Exact
                : ConditionMatch.No;
        }

        public override string ToString() {
            return string.Format("MatchType: {0}, ProfileAsked: {1}, ModuleAsked: {2}", MatchType, ProfileAsked, ModuleAsked);
        }



    }
}
