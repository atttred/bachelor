if (typeof Vy === "undefined") Vy = {};
if (typeof Vy.Crm === "undefined") Vy.Crm = {};

Vy.Crm.Account = new function () {
    var self = this;

    this.Constants = {
        Attributes: {
            Name:             "vy_name",
            PrimaryContactId: "vy_primarycontactid",
            CustomerTypeCode: "vy_customertypecode",
            Telephone:        "vy_telephone1",
            Email:            "vy_emailaddress1",
            WebsiteUrl:       "vy_websiteurl",
            AddressCity:      "vy_address1_city",
            StateCode:        "statecode"
        }
    };

    this.onLoad = function (executionContext) {
        var fc = executionContext.getFormContext();
        self.lockPrimaryContactIfInactive(fc);
        var stateAttr = fc.getAttribute(self.Constants.Attributes.StateCode);
        if (stateAttr) stateAttr.addOnChange(function (ec) {
            self.lockPrimaryContactIfInactive(ec.getFormContext());
        });
    };

    this.lockPrimaryContactIfInactive = function (fc) {
        var stateAttr = fc.getAttribute(self.Constants.Attributes.StateCode);
        if (!stateAttr) return;
        var inactive = stateAttr.getValue() === 1;
        var ctrl = fc.getControl(self.Constants.Attributes.PrimaryContactId);
        if (ctrl) ctrl.setDisabled(inactive);
    };
};
