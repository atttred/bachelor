if (typeof Vy === "undefined") { Vy = {}; }
if (typeof Vy.Crm === "undefined") { Vy.Crm = {}; }

Vy.Crm.QuickActivity = new function () {
    var self = this;

    this.open = function (primaryControl) {
        var fc = primaryControl;
        if (!fc || !fc.data || !fc.data.entity) {
            Vy.Crm.Notify.alert("Open the opportunity first.");
            return;
        }

        var id   = fc.data.entity.getId().replace(/[{}]/g, "");
        var name = fc.getAttribute("vy_name") ? fc.getAttribute("vy_name").getValue() : "";
        var entityType = fc.data.entity.getEntityName();

        var params = {};
        params["vy_regardingobjectid"] = id;
        params["vy_regardingobjectidname"] = name || "";
        params["vy_regardingobjectidtype"] = entityType;

        Xrm.Navigation.openForm({
            entityName: "vy_activity",
            useQuickCreateForm: true
        }, params).then(
            function (result) {
                if (result && result.savedEntityReference && result.savedEntityReference.length > 0) {
                    Vy.Crm.Log.trace("Quick activity created " + result.savedEntityReference[0].id);
                    if (fc.data && fc.data.refresh) fc.data.refresh(false);
                }
            },
            function (err) {
                Vy.Crm.Log.error("Quick activity failed", err);
                Vy.Crm.Notify.alert("Could not open Quick Activity form. Make sure the Activity table is created and has a Quick Create form.");
            }
        );
    };
};
