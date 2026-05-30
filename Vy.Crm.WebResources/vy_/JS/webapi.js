if (typeof Vy === "undefined") { Vy = {}; }
if (typeof Vy.Crm === "undefined") { Vy.Crm = {}; }

Vy.Crm.WebApi = new function () {
    var self = this;

    this.getEnvironmentVariableValue = function (schemaName) {
        var fetch =
            "<fetch top='1'>" +
            "  <entity name='environmentvariablevalue'>" +
            "    <attribute name='value' />" +
            "    <link-entity name='environmentvariabledefinition' from='environmentvariabledefinitionid' to='environmentvariabledefinitionid'>" +
            "      <attribute name='defaultvalue' />" +
            "      <filter><condition attribute='schemaname' operator='eq' value='" + schemaName + "' /></filter>" +
            "    </link-entity>" +
            "  </entity>" +
            "</fetch>";
        return Xrm.WebApi.retrieveMultipleRecords("environmentvariablevalue", "?fetchXml=" + encodeURIComponent(fetch))
            .then(function (result) {
                if (result.entities.length > 0) {
                    var v = result.entities[0].value;
                    if (v) return v;
                    return result.entities[0]["environmentvariabledefinition1.defaultvalue"];
                }
                return null;
            });
    };

    this.executeAction = function (actionName, parameters, boundEntity) {
        var request = {
            getMetadata: function () {
                var meta = {
                    boundParameter: boundEntity ? "entity" : null,
                    operationType: 0,
                    operationName: actionName,
                    parameterTypes: {}
                };
                if (boundEntity) {
                    meta.parameterTypes.entity = {
                        typeName: "mscrm." + boundEntity.entityType,
                        structuralProperty: 5
                    };
                }
                if (parameters) {
                    Object.keys(parameters).forEach(function (k) {
                        var v = parameters[k];
                        var t = typeof v === "string" ? "Edm.String"
                              : typeof v === "number" ? "Edm.Int32"
                              : typeof v === "boolean" ? "Edm.Boolean"
                              : "Edm.String";
                        meta.parameterTypes[k] = { typeName: t, structuralProperty: 1 };
                    });
                }
                return meta;
            }
        };
        if (boundEntity) request.entity = boundEntity;
        if (parameters) Object.keys(parameters).forEach(function (k) { request[k] = parameters[k]; });

        return Xrm.WebApi.online.execute(request).then(function (resp) {
            if (resp.ok) return resp.json();
            return Promise.reject(resp);
        });
    };

    this.sendRequest = function (url, body) {
        return fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body || {})
        }).then(function (r) { return r.ok ? r.json() : Promise.reject(r); });
    };
};
