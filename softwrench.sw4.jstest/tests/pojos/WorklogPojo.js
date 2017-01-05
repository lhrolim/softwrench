
class WorklogPojo {

    static ListSchema() {
        const field1 = FieldMetadataPojo.Hidden("worklogid");
        const description = FieldMetadataPojo.Ordinary("description");
        const recordkey = FieldMetadataPojo.Hidden("recordkey");
        const parentclass = FieldMetadataPojo.Hidden("parentclass");

        const displayables = [field1, description, recordkey, parentclass];
        const schema = SchemaPojo.WithIdAndDisplayables("list",displayables,"worklog", "worklogid");
        schema.stereotype = "compositionlist";
        return schema;
    }

    static DetailSchema() {
        const baseSchema = WorklogPojo.ListSchema();
        const lDescription = FieldMetadataPojo.Ordinary("wld_.ldtext","Description");
        baseSchema.displayables = baseSchema.displayables.concat([lDescription]);
        baseSchema.id = "detail";
        baseSchema.stereotype = "compositiondetail";
        return baseSchema;
    }

    static ListItem(id, description, parentid, parentclass = "sr") {
        return {
            "worklogid": id,
            description,
            recordkey: parentid,
            "class": parentclass
        }
    }

        static DetailItem(id, description,ld, parentid= "100", parentclass = "sr") {
            return {
                "worklogid": id,
                description,
                recordkey: parentid,
                "class": parentclass
            }
        }




    }