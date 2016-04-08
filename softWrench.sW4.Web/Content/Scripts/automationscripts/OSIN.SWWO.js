//considerations:
// strings are java.lang.strings, not javascript strings --> do not use ===, and check for java documentation rather than javascript´s

importClass(Packages.psdi.server.MXServer);
importPackage(Packages.psdi.workflow);


function beforeMboData(ctx) {

    ctx.log("[WorkOrder Workflow]: init script");
    var mbo = ctx.getMbo();
    var wonum = ctx.getData().getCurrentData("WONUM");

    var workflowInfo = ctx.getData().getCurrentData("WORKFLOWINFO");

    //protocol: personid;actiontype;wfid;wfactionid;wfassignmentid;memo;
    if (workflowInfo == null) {
        ctx.log("[WorkOrder Workflow]: custom field WORKFLOWINFO not present, returning");
        return;
    }

    var array = workflowInfo.split(";");

    var personId = array[0];
    var action = array[1];
    var wfId = array[2];
    var wfActionId = array[3];
    var wfAssignmentId = array[4];

    var memo;
    if (array.length == 5) {
        memo = array[5];
    } else if (array.length > 5) {
        for (var i = 5; i < array.length; i++) {
            memo = memo + array[i];
            if (i != array.length - 1) {
                memo = memo + ";";
            }
        }
    }




    if (action == null || wfId == null) {
        ctx.log("[WorkOrder Workflow]-- no custom logic to execute (null wojp1 or null wojp2), skippinng");
        //no custom action to execute
        return;
    }



    ctx.log("[WorkOrder Workflow]-- Initing action " + action + " on workflow instance " + wfId + " action: " + wfActionId);

    //fetch active workflows of the workorder
    var wfInstanceSet = mbo.getMboSet("ACTIVEWORKFLOW");


    if (!ctx.getUserInfo().getUserLoginDetails().setPersonId) {
        ctx.log("[WorkOrder Workflow]-- custom security class not applied, cannot set personid");
    } else {
        ctx.getUserInfo().getUserLoginDetails().setPersonId(personId);
    }

    if (action == "start") {
        ctx.log("[WorkOrder Workflow]-- Starting workflow " + wfId);
        var wfs = MXServer.getMXServer().lookup("WORKFLOW");
        wfs.initiateWorkflow(wfId, mbo);
        return;
    }



    if (!wfInstanceSet || wfInstanceSet.isEmpty()) {

        ctx.log("[WorkOrder Workflow]-- no active workflows found for workorder " + wonum + " skipping execution");
        return;
    }





    //let´s search for an active workflow with the same id as informed
    for (var i = 0; i < wfInstanceSet.count() ; i++) {
        var wfInst = wfInstanceSet.getMbo(i);
        var id = wfInst.getUniqueIDValue();
        if (id == wfId) {
            ctx.log("active workflow found performing action");
            if (action == "stop") {
                ctx.log("stopping workflow");
                wfInst.stopWorkflow("Auto Stop");
                mbo.getMXTransaction().commit();
                ctx.skipTxn();
            } else if (action == "route") {
                if (wfActionId == null) {
                    ctx.log("[WorkOrder Workflow]-- wojp3 should inform the action to route to");
                    return;
                }
                mbo.setModified(false);
                ctx.log("[WorkOrder Workflow]-- Routing workflow " + wfId + " to action " + wfActionId);
                ctx.log("wfinst: " + wfInst);

                wfInst.completeWorkflowAssignment(parseInt(wfAssignmentId), wfActionId, memo);
                

                ctx.log("[WorkOrder Workflow]--  Workflow Routed");
                ctx.skipTxn();
            } else if (action == "start") {
                ctx.log("[WorkOrder Workflow]-- Starting workflow " + wfId);
                var wfs = MXServer.getMXServer().lookup("WORKFLOW");
                wfs.initiateWorkflow(wfId, mbo);
            }

            return;
        }



    }

    ctx.log("[WorkOrder Workflow]-- Active workflow " + wfId + " not found, no action performed ");

}
