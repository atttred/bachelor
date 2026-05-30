if (typeof Vy === "undefined") Vy = {};
if (typeof Vy.Crm === "undefined") Vy.Crm = {};

Vy.Crm.Lead = new function () {
    var self = this;

    this.Constants = {
        Attributes: {
            Topic:       "vy_topic",
            FullName:    "vy_fullname",
            CompanyName: "vy_companyname",
            Email:       "vy_emailaddress1",
            Telephone:   "vy_telephone1",
            LeadSource:  "vy_leadsourcecode",
            StateCode:   "statecode"
        }
    };

    this.onLoad = function (executionContext) {
        var fc = executionContext.getFormContext();
        var phoneAttr = fc.getAttribute(self.Constants.Attributes.Telephone);
        if (phoneAttr) phoneAttr.addOnChange(self.formatPhone);
        var emailAttr = fc.getAttribute(self.Constants.Attributes.Email);
        if (emailAttr) emailAttr.addOnChange(self.validateEmail);
    };

    this.formatPhone = function (executionContext) {
        var fc = executionContext.getFormContext();
        var attr = fc.getAttribute(self.Constants.Attributes.Telephone);
        var raw = attr.getValue();
        if (!raw) return;
        var digits = raw.replace(/\D/g, "");
        if (digits.length >= 10) {
            var formatted = "+" + digits.slice(0, digits.length - 10) + " (" +
                digits.slice(-10, -7) + ") " + digits.slice(-7, -4) + "-" + digits.slice(-4);
            attr.setValue(formatted.trim());
        }
    };

    this.validateEmail = function (executionContext) {
        var fc = executionContext.getFormContext();
        var attr = fc.getAttribute(self.Constants.Attributes.Email);
        var v = attr.getValue();
        if (!v) return;
        var ok = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);
        var ctrl = fc.getControl(self.Constants.Attributes.Email);
        if (ctrl) {
            if (ok) ctrl.clearNotification("vy_email"); else ctrl.setNotification("Invalid email format.", "vy_email");
        }
    };

    this.qualifyAdvanced = function (primaryControl) {
        var fc = primaryControl;
        if (!fc || !fc.data || !fc.data.entity) {
            Vy.Crm.Notify.alert("Open the lead first.");
            return;
        }
        var id = fc.data.entity.getId();
        var leadRef = { entityType: "vy_lead", id: id };

        Vy.Crm.WebApi.executeAction("vy_QualifyLeadAdvanced", null, leadRef)
            .then(function (result) {
                Vy.Crm.Notify.info(fc, "Qualified. Opportunity created.");
                if (fc.data && fc.data.refresh) fc.data.refresh(false);
                if (result && result.OpportunityId && result.OpportunityId !== "00000000-0000-0000-0000-000000000000") {
                    Xrm.Navigation.openForm({ entityName: "vy_opportunity", entityId: result.OpportunityId });
                }
            })
            .catch(function (err) {
                Vy.Crm.Log.error("Qualify advanced failed", err);
                var msg = err && err.message ? err.message : "Qualification failed.";
                Vy.Crm.Notify.error(fc, msg);
            });
    };
};
