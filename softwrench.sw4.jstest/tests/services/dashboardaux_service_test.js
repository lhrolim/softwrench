describe('dashboardService Test', function () {

    //function buildPanel(idx) {

    //    var panel = {
    //        position: idx-1,
    //        panel: {
    //            title: 'panel' + (idx)
    //        }
    //    };

    //    return panel;
    //}

    //var panel1 = buildPanel(1);
    //var panel2 = buildPanel(2);
    //var panel3 = buildPanel(3);
    //var panel4 = buildPanel(4);
    //var panel5 = buildPanel(5);
    //var panel6 = buildPanel(6);

    //var dashboardAuxService;
    //beforeEach(module('sw_layout'));
    //beforeEach(inject(function (_dashboardAuxService_) {
    //    dashboardAuxService = _dashboardAuxService_;
    //}));

    //it('first panel added', function () {

    //    var dashboard = {};
    //    var layout = dashboardAuxService.readjustLayout(dashboard, 1, 1);
    //    expect(layout).toBe("1");
    //    expect(dashboard.layout).toBe("1");
    //    var addingpanel = { a: 'a' };
    //    dashboard = dashboardAuxService.readjustPositions(dashboard, addingpanel, 1, 1);
    //    expect(layout).toBe("1");
    //    expect(dashboard.panels.length).toBe(1);
    //    expect(dashboard.panels[0].panel).toBe(addingpanel);
    //    expect(dashboard.panels[0].position).toBe(0);
    //});

    //it('second panel added right', function () {
    //    var dashboard = {
    //        panels: [
    //            panel1
    //        ],
    //        layout: '1'
    //    };

    //    var layout = dashboardAuxService.readjustLayout(dashboard, 1, 2);
    //    expect(layout).toBe("2");
    //    expect(dashboard.layout).toBe("2");

    //    var addingpanel = { a: 'a' };

    //    dashboard = dashboardAuxService.readjustPositions(dashboard, addingpanel, 1, 2);
    //    expect(layout).toBe("2");
    //    expect(dashboard.panels.length).toBe(2);

    //    expect(dashboard.panels[0]).toBe(panel1);
    //    expect(dashboard.panels[0].position).toBe(0);

    //    expect(dashboard.panels[1].panel).toBe(addingpanel);
    //    expect(dashboard.panels[1].position).toBe(1);
    //});


    //it('second panel added left', function () {
    //    var dashboard = {
    //        panels: [
    //            panel1
    //        ],
    //        layout: '1'
    //    };

    //    var layout = dashboardAuxService.readjustLayout(dashboard, 1, 1);
    //    expect(layout).toBe("2");
    //    expect(dashboard.layout).toBe("2");

    //    var addingpanel = { a: 'a' };

    //    dashboard = dashboardAuxService.readjustPositions(dashboard, addingpanel, 1, 1);
    //    expect(layout).toBe("2");
    //    expect(dashboard.panels.length).toBe(2);

    //    expect(dashboard.panels[0].panel).toBe(addingpanel);
    //    expect(dashboard.panels[0].position).toBe(0);

    //    expect(dashboard.panels[1].panel).toBe(panel1.panel);
    //    expect(dashboard.panels[1].position).toBe(1);
    //});

    //it('second panel added second row', function () {
    //    var dashboard = {
    //        panels: [
    //            panel1
    //        ],
    //        layout: '1'
    //    };

    //    var layout = dashboardAuxService.readjustLayout(dashboard, 2, 1);
    //    expect(layout).toBe("1,1");
    //    expect(dashboard.layout).toBe("1,1");

    //    var addingpanel = { a: 'a' };

    //    dashboard = dashboardAuxService.readjustPositions(dashboard, addingpanel, 2, 1);
    //    expect(layout).toBe("1,1");
    //    expect(dashboard.panels.length).toBe(2);

    //    expect(dashboard.panels[0]).toBe(panel1);
    //    expect(dashboard.panels[0].position).toBe(0);

    //    expect(dashboard.panels[1].panel).toBe(addingpanel);
    //    expect(dashboard.panels[1].position).toBe(1);
    //});

    //it('3 panels add forth', function () {

    //    var dashboard = {
    //        panels: [
    //            panel1,
    //            panel2,
    //            panel3,
    //        ],
    //        layout: '1,1,1'
    //    };

    //    var layout = dashboardAuxService.readjustLayout(dashboard, 2, 2);
    //    expect(layout).toBe("1,2,1");
    //    expect(dashboard.layout).toBe("1,2,1");

    //    var addingpanel = { a: 'a' };


    //    dashboard = dashboardAuxService.readjustPositions(dashboard, addingpanel, 2, 2);
    //    expect(layout).toBe("1,2,1");
    //    expect(dashboard.panels.length).toBe(4);

    //    expect(dashboard.panels[0]).toBe(panel1);
    //    expect(dashboard.panels[1]).toBe(panel2);

        
    //    expect(dashboard.panels[2].panel).toBe(addingpanel);
    //    expect(dashboard.panels[2].position).toBe(2);
    //    expect(dashboard.panels[3].panel).toBe(panel3.panel);
    //    expect(dashboard.panels[3].position).toBe(3);

    //});


    //it('3 panels add forth test2', function () {

    //    var dashboard = {
    //        panels: [
    //            panel1,
    //            panel2,
    //            panel3,
    //            panel4,
    //            panel5,
    //            panel6
    //        ],
    //        layout: '2,2,2'
    //    };

    //    var layout = dashboardAuxService.readjustLayout(dashboard, 2, 2);
    //    expect(layout).toBe("2,3,2");
    //    expect(dashboard.layout).toBe("2,3,2");

    //    var addingpanel = { a: 'a' };


    //    dashboard = dashboardAuxService.readjustPositions(dashboard, addingpanel, 2, 2);
    //    expect(layout).toBe("2,3,2");
    //    expect(dashboard.panels.length).toBe(7);

    //    expect(dashboard.panels[0]).toBe(panel1);
    //    expect(dashboard.panels[1]).toBe(panel2);
    //    expect(dashboard.panels[2]).toBe(panel3);

    //    expect(dashboard.panels[3].panel).toBe(addingpanel);
    //    expect(dashboard.panels[3].position).toBe(3);
        

    //});

    //it('locate panel from matrix', function() {
    //    var dashboard = {
    //        panels: [
    //            panel1,
    //            panel2,
    //            panel3,
    //            panel4,
    //            panel5,
    //            panel6
    //        ],
    //        layout: '1,3,2'
    //    };
    //    var panel = dashboardAuxService.locatePanelFromMatrix(dashboard, 1, 1);
    //    expect(panel).toBe(panel3);

    //    panel = dashboardAuxService.locatePanelFromMatrix(dashboard, 2, 1);
    //    expect(panel).toBe(panel6);

    //    panel = dashboardAuxService.locatePanelFromMatrix(dashboard, 1, 0);
    //    expect(panel).toBe(panel2);
    //});

});