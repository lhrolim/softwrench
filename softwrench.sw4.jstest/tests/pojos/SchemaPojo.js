
class SchemaPojo {

    static WithId(id) {
        return {
            schemaId: id,
            displayables: [],
            properties: {},
            commandSchema: {}

        };
    }

    static WithIdAndEvent(id, eventName, eventService,eventMethod) {
        let result= {
            schemaId: id,
            displayables: [],
            events: {}
        }
        result.events[eventName] = {
            service: eventService,
            method: eventMethod
        }
        return result;

    }

   
}