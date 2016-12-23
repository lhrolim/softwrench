
class SchemaPojo {


    static BaseWithSection() {
        const field1 = FieldMetadataPojo.Required("attr1");
        return SchemaPojo.WithIdAndDisplayables("detail", [field1, FieldMetadataPojo.Section(FieldMetadataPojo.Ordinary("attr2"))]);
    }


    static WithId(id, applicationName = "sr") {
        return {
            applicationName: applicationName,
            schemaId: id,
            displayables: [],
            properties: {},
            commandSchema: {},
            idFieldName: "id",
            mode:"input"
        };
    }

    static WithIdAndDisplayables(id, displayables) {
        return {
            applicationName:"sr",
            schemaId: id,
            displayables: displayables,
            properties: {},
            commandSchema: {},
            idFieldName: "id",
            mode: "input"
        };
    }

    static WithIdAndEvent(id, eventName, eventService, eventMethod) {
        const result = {
            applicationName: "sr",
            schemaId: id,
            displayables: [],
            events: {},
            mode:"input"
        };
        result.events[eventName] = {
            service: eventService,
            method: eventMethod
        }
        return result;

    }


}