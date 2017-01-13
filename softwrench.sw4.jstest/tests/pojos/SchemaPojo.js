
class SchemaPojo {


    static BaseWithSection() {
        const field1 = FieldMetadataPojo.Required("attr1");
        return SchemaPojo.WithIdAndDisplayables("detail", [field1, FieldMetadataPojo.Section(FieldMetadataPojo.Ordinary("attr2"))]);
    }

    static CompositionDetailSchema() {
        const field1 = FieldMetadataPojo.Required("attr1");
        const displayables = [field1, FieldMetadataPojo.Section(FieldMetadataPojo.Ordinary("attr2"))];
        const schema = SchemaPojo.WithIdAndDisplayables("detail",displayables,"worklog");
        schema.stereotype = "compositiondetail";
        return schema;
    }

    static CompositionListSchema() {
        const field1 = FieldMetadataPojo.Required("attr1");
        const displayables = [field1, FieldMetadataPojo.Section(FieldMetadataPojo.Ordinary("attr2"))];
        const schema = SchemaPojo.WithIdAndDisplayables("list",displayables,"worklog");
        schema.stereotype = "compositionlist";
        return schema;
    }

    static InLineMultiAssetSchema() {
        const field1 = FieldMetadataPojo.Hidden("multiid");
        const field2 = FieldMetadataPojo.Hidden("siteid");
        const assetField=  FieldMetadataPojo.ForAssociation("asset_", "assetnum");
        const locField=  FieldMetadataPojo.ForAssociation("location_", "location");
        const isPrimary = FieldMetadataPojo.Hidden("isprimary");
        const isDirty = FieldMetadataPojo.Hidden("#isDirty");
        isDirty.defaultLaborExpression = true;

        const displayables = [field1,field2, isPrimary,assetField,locField,isDirty];
        const schema = SchemaPojo.WithIdAndDisplayables("newlist",displayables,"multiassetlocci");
        schema.stereotype = "compositionlist";
        return schema;
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

    static WithIdAndDisplayables(id, displayables,applicationName = "sr", idFieldName= "id") {
        return {
            applicationName:applicationName,
            schemaId: id,
            displayables: displayables,
            properties: {},
            commandSchema: {},
            idFieldName: idFieldName,
            mode: "input",
            cachedCompositions: {}
        };
    }

    static WithIdAndEvent(id, eventName, eventService, eventMethod) {
        const result = {
            applicationName: "sr",
            schemaId: id,
            displayables: [],
            events: {},
            cachedCompositions: {},
            mode:"input"
        };
        result.events[eventName] = {
            service: eventService,
            method: eventMethod
        }
        return result;

    }


    }