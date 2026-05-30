using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.Text;
using Vy.Crm.Shared.Helpers;

namespace Vy.Crm.Plugins
{
    public class AuditTrailPlugin : IPlugin
    {
        private const string _preImageEntity = "AuditTrailPreImage";

        public void Execute(IServiceProvider serviceProvider)
        {
            var tracer = serviceProvider.Get<ITracingService>();
            var context = serviceProvider.Get<IPluginExecutionContext>();

            if (context.InputParameters.Contains(Constants.Target) && context.InputParameters[Constants.Target] is Entity)
            {
                try
                {
                    var target = (Entity)context.InputParameters[Constants.Target];
                    var preImage = context.PreEntityImages.Contains(_preImageEntity)
                        ? context.PreEntityImages[_preImageEntity]
                        : null;

                    var sb = new StringBuilder();
                    sb.AppendLine($"[audit] {target.LogicalName} id={context.PrimaryEntityId} user={context.InitiatingUserId} at {DateTime.UtcNow:O}");
                    foreach (var kv in target.Attributes)
                    {
                        var oldValue = preImage?.Contains(kv.Key) == true ? preImage[kv.Key] : null;
                        sb.AppendLine($"  {kv.Key}: {Render(oldValue)} -> {Render(kv.Value)}");
                    }
                    tracer.Trace(sb.ToString());
                }
                catch (Exception ex)
                {
                    tracer.Trace(ex.ToString());
                }
            }
        }

        private static string Render(object v)
        {
            if (v == null)
            {
                return "(null)";
            }
            if (v is EntityReference er)
            {
                return $"{er.LogicalName}:{er.Id}";
            }
            if (v is OptionSetValue osv)
            {
                return osv.Value.ToString();
            }
            if (v is Money m)
            {
                return m.Value.ToString();
            }
            return v.ToString();
        }
    }
}
