class ApplicationCompositionSchemaDTO {


    constructor(schemas, inline=false, renderer ={}, collectionProperties ={
        autocommit : false,
        allowInsertion: "false",
        allowRemoval: "false",
        allowUpdate: "false"}) {
            this.schemas = schemas;
            this.inline = inline;
            this.renderer = renderer;
            this.rendererParameters = renderer.parameters || {};
            this.collectionProperties = collectionProperties;
        }

    }


