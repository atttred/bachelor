if (typeof Vy === "undefined") Vy = {};
if (typeof Vy.Crm === "undefined") Vy.Crm = {};

Vy.Crm.Opportunity = new function () {
    var self = this;

    this.Constants = {
        Attributes: {
            Name:               "vy_name",
            CustomerId:         "vy_customerid",
            EstimatedValue:     "vy_estimatedvalue",
            EstimatedCloseDate: "vy_estimatedclosedate",
            Probability:        "vy_probability",
            SalesStageCode:     "vy_salesstagecode",
            StateCode:          "statecode"
        },
        Tabs: {
            General:  "tab_general",
            Pipeline: "tab_pipeline"
        },
        Sections: {
            Information: "section_information",
            Financials:  "section_financials",
            Sales:       "section_sales"
        }
    };

    this.onLoad = function (executionContext) {
        var fc = executionContext.getFormContext();
        var valueAttr = fc.getAttribute(self.Constants.Attributes.EstimatedValue);
        if (valueAttr) valueAttr.addOnChange(self.validateMinimum);
        var stageAttr = fc.getAttribute(self.Constants.Attributes.SalesStageCode);
        if (stageAttr) stageAttr.addOnChange(self.onStageChange);
        self.lockProbabilityOnClosed(fc);
    };

    this.onSave = function (executionContext) {
        var fc = executionContext.getFormContext();
        var customer = fc.getAttribute(self.Constants.Attributes.CustomerId).getValue();
        if (!customer || customer.length === 0) {
            executionContext.getEventArgs().preventDefault();
            Vy.Crm.Notify.error(fc, "Клієнт обов'язковий.");
        } else {
            Vy.Crm.Notify.clear(fc, "vy_err");
        }
    };

    this.validateMinimum = function (executionContext) {
        var fc = executionContext.getFormContext();
        var valueAttr = fc.getAttribute(self.Constants.Attributes.EstimatedValue);
        var current = valueAttr.getValue();
        if (current === null || current === undefined) return;

        Vy.Crm.WebApi.getEnvironmentVariableValue(Vy.Crm.Constants.EnvVars.MinimumOpportunityValue)
            .then(function (raw) {
                var floor = parseFloat(raw);
                if (!isNaN(floor) && current < floor) {
                    Vy.Crm.Notify.warn(fc, "Очікувана вартість нижча за налаштований мінімум " + floor + ".");
                    valueAttr.controls.forEach(function (c) { c.setNotification("Нижче мінімуму.", "vy_min"); });
                } else {
                    valueAttr.controls.forEach(function (c) { c.clearNotification("vy_min"); });
                    Vy.Crm.Notify.clear(fc, "vy_warn");
                }
            })
            .catch(function (err) {
                Vy.Crm.Log.warn("Could not read min opp value env var: " + (err && err.message));
            });
    };

    this.onStageChange = function (executionContext) {
        var fc = executionContext.getFormContext();
        var stage = fc.getAttribute(self.Constants.Attributes.SalesStageCode).getValue();
        if (stage === Vy.Crm.Constants.SalesStage.Closing) {
            Vy.Crm.Notify.info(fc, "Етап встановлено на «Закриття». Збережіть, щоб перерахувати ймовірність.");
        }
    };

    this.lockProbabilityOnClosed = function (fc) {
        var stateAttr = fc.getAttribute(self.Constants.Attributes.StateCode);
        if (!stateAttr) return;
        var state = stateAttr.getValue();
        if (state === 1 || state === 2) {
            var probCtrl = fc.getControl(self.Constants.Attributes.Probability);
            if (probCtrl) probCtrl.setDisabled(true);
        }
    };
};
