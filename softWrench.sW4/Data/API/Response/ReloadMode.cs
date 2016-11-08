namespace softWrench.sW4.Data.API.Response {
    public enum ReloadMode {
        /// <summary>
        /// None --> No reload at all, default value
        /// MainDetail --> Only the main detail needs to be refreshed, no extra compositions
        /// FullRefresh --> the entire page would be refreshed, not ideal... but useful under some circumstances to avoid issues
        /// </summary>
        None, MainDetail, FullRefresh
    }
}