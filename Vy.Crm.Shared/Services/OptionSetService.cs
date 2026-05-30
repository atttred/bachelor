using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace Vy.Crm.Shared.Services
{
    public class OptionSetService
    {
        private readonly IOrganizationService _service;

        public OptionSetService(IOrganizationService service) { _service = service; }

        public Dictionary<int, string> GetLocalOptions(string entityLogicalName, string attributeLogicalName)
        {
            var req = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName       = attributeLogicalName,
                RetrieveAsIfPublished = true
            };
            var resp = (RetrieveAttributeResponse)_service.Execute(req);
            var meta = resp.AttributeMetadata as EnumAttributeMetadata;
            if (meta == null) return new Dictionary<int, string>();

            return meta.OptionSet.Options
                .Where(o => o.Value.HasValue && o.Label?.UserLocalizedLabel != null)
                .ToDictionary(o => o.Value.Value, o => o.Label.UserLocalizedLabel.Label);
        }

        public string GetLabel(string entityLogicalName, string attributeLogicalName, int optionValue)
        {
            var opts = GetLocalOptions(entityLogicalName, attributeLogicalName);
            return opts.TryGetValue(optionValue, out var label) ? label : optionValue.ToString();
        }
    }
}
