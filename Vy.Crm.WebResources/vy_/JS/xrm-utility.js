if (typeof Vy === "undefined") { Vy = {}; }
if (typeof Vy.Crm === "undefined") { Vy.Crm = {}; }

Vy.Crm.Log = {
    trace: function (msg) {
        if (typeof console !== "undefined" && console.log) { console.log("[vy] " + msg); }
    },
    warn: function (msg) {
        if (typeof console !== "undefined" && console.warn) { console.warn("[vy] " + msg); }
    },
    error: function (msg, err) {
        if (typeof console !== "undefined" && console.error) {
            console.error("[vy] " + msg, err || "");
        }
    }
};

Vy.Crm.Notify = {
    info: function (formContext, msg) {
        if (formContext && formContext.ui) {
            formContext.ui.setFormNotification(msg, "INFO", "vy_info");
            setTimeout(function () { formContext.ui.clearFormNotification("vy_info"); }, 5000);
        }
    },
    warn: function (formContext, msg) {
        if (formContext && formContext.ui) {
            formContext.ui.setFormNotification(msg, "WARNING", "vy_warn");
        }
    },
    error: function (formContext, msg) {
        if (formContext && formContext.ui) {
            formContext.ui.setFormNotification(msg, "ERROR", "vy_err");
        }
    },
    clear: function (formContext, id) {
        if (formContext && formContext.ui) {
            formContext.ui.clearFormNotification(id);
        }
    },
    alert: function (msg) {
        if (typeof Xrm !== "undefined" && Xrm.Navigation && Xrm.Navigation.openAlertDialog) {
            Xrm.Navigation.openAlertDialog({ text: msg });
        } else {
            alert(msg);
        }
    }
};
