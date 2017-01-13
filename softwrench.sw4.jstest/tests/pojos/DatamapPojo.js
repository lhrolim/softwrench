
class DatamapPojo {


    static BaseSRItem(id, description, ld) {
        return {
            "ticketid": id,
            summary: description,
            "class": "SR",
            "siteid": "BEDFORD",
            "orgid": "EAGLENA",
            "ld_.description": ld
        }
    }

    static BaseInvIssueItem(id, description, ld) {
        return {
            "ticketid": id,
            summary: description,
            "class": "INVISSUE",
            "siteid": "BEDFORD",
            "orgid": "EAGLENA",
            "ld_.description": ld
        }
    }






}