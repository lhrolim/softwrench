
class CompositionDefinitionPojo {


    static MultiAssetLocciBase(startWithData = "false") {

        const renderer = {
            parameters: {
                mode: "batch",
                "composition.inline.startwithentry": startWithData
            },
            rendererType: "TABLE"
        };

        const compschemas = new CompositionSchemas(null, SchemaPojo.InLineMultiAssetSchema());

        return new ApplicationCompositionSchemaDTO(compschemas, true, renderer);
    }

  


}