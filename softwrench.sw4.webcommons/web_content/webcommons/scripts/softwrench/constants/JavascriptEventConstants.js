const modalShown = "sw.modal.show";
const HideModal = "sw.modal.hide";
const crudSaved = "sw.crud.detail.savecompleted";
const CrudSubmitData = "sw.crud.detail.submit";

const BodyRendered = "sw.crud.body.rendered";

//internal event to indicate that all tabs of the crud_body have rendered
const TabsLoaded = "sw.crud.body.alltabsloaded";

const MoveFocus = "sw.crud.body.movefocus";

const ResetFocusToCurrent = "sw.crud.input.resetfocustocurrent";

//used to navigate back of forward towards next/previous entries on the form
const NavigateRequestCrawl = "sw.crud.body.crawl";

const AjaxError = "sw.ajax.error";

const AjaxFinished = "sw.ajax.finished";

const AjaxInit = "sw.ajax.init";

//#region print

const ReadyToPrint = "sw.application.print.ready";

const PrintReadyForList = "sw.application.print.readyforlist";
const PrintReadyForDetailedList = "sw.application.print.readyfordetailedlist";

const PrintShowModal = "sw.application.print.showmodal";
const PrintHideModal = "sw.application.print.hidemodal";

const PrintSectionRendered = "sw.application.print.sectionrendered";

//#endregion


//#region navigation

/**
 * Event that happens whenever an application (and not only a schema) changes
 */
const AppChanged = "sw.crud.applicationchanged";
//TODO:
const AppRedirected = "sw.crud.navigation.applicationredirected";

const TitleChanged = "sw.crud.navigation.titlechanged";

const RenderView = "sw.crud.navigation.renderview";

const REDIRECT_BEFORE = "sw.crud.navigation.beforeredirection";

//Happens after the server side has returned data and the app is ready to redirect
const REDIRECT_AFTER = "sw.crud.navigation.redirected";

const REDIRECT_AFTERACTION = "sw.crud.navigation.actionredirected";



//#endregion

//#region grid
//Event to dispatch grid refresh on the current list
const REFRESH_GRID = "sw.crud.list.refreshgrid";

//Event to indicate that the grid has been refreshed
const GRID_REFRESHED = "sw.crud.list.gridrefreshed";

//event to clear the underlying quick search
const ClearQuickSearch = "sw.crud.list.clearQuickSearch";

//event to toggle the selection mode on grids with checkboxes
const ToggleSelectionMode = "sw.crud.list.toggleselectionmode";

//event to switch the mode showing only the selecte or all entries of a grid
const ToggleSelected = "sw.crud.list.toggleselected";

//event to indicate that the crud_tbody has finished processing for real
const ListTableRendered = "sw.crud.list.tbodyrendered";

//event to indicate that the filter row has finished rendering
const FilterRowRendered = "sw.crud.list.filterrowrendered";

//event to indicate that a new grid rendering is needed. This is after the result is already fetched from the server
const GridDataChanged = "sw.crud.list.griddatachanged";

//setting a new filter
//TODO: viewmodel approach?
const GRID_SETFILTER = "sw.crud.list.setfilter";

const GridClearFilter = "sw.crud.list.clearfilter";




//TODO: refactor
const GRID_CHANGED = "sw.crud.list.gridchanged";

//TODO:refactor
const GRID_REFRESH = "sw.grid.refresh";

//#endregion


//#region association

//indicates that the associations of the form have been resolved
const AssociationResolved = "sw.crud.association.resolved";

// ??
const AssociationUpdated = "sw.crud.association.updated";

//indicates that the a eager association has been updated
const Association_EagerOptionUpdated = "sw.crud.associations.updateeageroptions";

//#endregion

//#region composition

const COMPOSITION_RESOLVED = "sw.crud.composition.dataresolved";
const COMPOSITION_EDIT = "sw.crud.composition.edit";

//#endregion

class JavascriptEventConstants {



    static get ModalShown() {
        return modalShown;
    }

    //#region print

    static get ReadyToPrint() {
        return ReadyToPrint;
    }

    static get PrintReadyForList() {
        return PrintReadyForList;
    }

    static get PrintReadyForDetailedList() {
        return PrintReadyForDetailedList;
    }

    static get PrintShowModal() {
        return PrintShowModal;
    }

    static get PrintHideModal() {
        return PrintHideModal;
    }

    static get PrintSectionRendered() {
        return PrintSectionRendered;
    }

    //#endregion



    static get HideModal() {
        return HideModal;
    }

    static get ErrorAjax() {
        return AjaxError;
    }

    static get AjaxFinished() {
        return AjaxFinished;
    }

    static get AjaxInit() {
        return AjaxInit;
    }


    static get BodyRendered() {
        return BodyRendered;
    }

    static get TabsLoaded() {
        return TabsLoaded;
    }

    static get NavigateRequestCrawl() {
        return NavigateRequestCrawl;
    }

    static get MoveFocus() {
        return MoveFocus;
    }

    static get ResetFocusToCurrent() {
        return ResetFocusToCurrent;
    }


    //#region navigation
    
    static get AppChanged() {
        return AppChanged;
    }



    static get AppBeforeRedirection() {
        return REDIRECT_BEFORE;
    }

    static get REDIRECT_AFTER() {
        return REDIRECT_AFTER;
    }

    static get ActionAfterRedirection() {
        return REDIRECT_AFTERACTION;
    }

    static get ApplicationRedirected() {
        return AppRedirected;
    }

    static get TitleChanged() {
        return TitleChanged;
    }

    static get RenderView() {
        return RenderView;
    }



    //#endregion

    static get CrudSaved() {
        return crudSaved;
    }

    static get CrudSubmitData() {
        return CrudSubmitData;
    }


    //#region association

    static get AssociationResolved() {
        return AssociationResolved;
    }
    static get Association_EagerOptionUpdated() {
        return Association_EagerOptionUpdated;
    }

    //#endregion


    //#region grid

    static get RefreshGrid() {
        return REFRESH_GRID;
    }

    static get GRID_REFRESHED() {
        return GRID_REFRESHED;
    }

    //TODO:
    static get GRID_REFRESH2() {
        return GRID_REFRESH;
    }

    static get GRID_SETFILTER() {
        return GRID_SETFILTER;
    }

    static get GRID_CLEARFILTER() {
        return GridClearFilter;
    }

    static get GRID_CHANGED() {
        return GRID_CHANGED;
    }

    static get ListTableRendered() {
        return ListTableRendered;
    }

    static get GridDataChanged() {
        return GridDataChanged;
    }

    static get FilterRowRendered() {
        return FilterRowRendered;
    }


    static get ClearQuickSearch() {
        return ClearQuickSearch;
    }

    static get ToggleSelectionMode() {
        return ToggleSelectionMode;
    }

    static get ToggleSelected() {
        return ToggleSelected;
    }

    //#endregion


    //#region composition

    static get COMPOSITION_RESOLVED() {
        return COMPOSITION_RESOLVED;
    }

    static get CompositionEdit() {
        return COMPOSITION_EDIT;
    }

    //#endregion

}




