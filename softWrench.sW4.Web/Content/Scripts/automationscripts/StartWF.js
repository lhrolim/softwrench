println("[WorkOrder Workflow - START ]-- init custom script");
if (mbo.getUserInfo().getUserLoginDetails().setPersonId == null) {
    println("[WorkOrder Workflow - START ]-- custom security class not applied, cannot set personid");
} else {
    var ownerMBO = mbo.getThisMboSet().getControlee();
    var personId = ownerMBO.getString("CHANGEBY");
    println("[WorkOrder Workflow - START ]-- custom security class applied, changing personid to " + personId);
    mbo.getUserInfo().getUserLoginDetails().setPersonId(personId);
}