# Vy.Crm

*[Українською](README.md)*

A Dynamics 365 / Dataverse solution for a sales CRM. It contains server-side
plugins, custom APIs and client-side web resources, plus the exported managed
solution. The user-facing strings are localized in Ukrainian.

## Structure

```text
Vy.Crm.sln                  Visual Studio solution
Vy.Crm.Plugins/             Plugin and custom API assembly (.NET Framework 4.7.1)
Vy.Crm.Shared/              Shared project: early-bound entities, helpers, services
Vy.Crm.WebResources/        JavaScript, HTML and CSS web resources
solution_managed/           Exported managed solution (customizations, workflows)
packages/                   NuGet dependencies
```

## Plugins

| Plugin | Entity | Stage | Purpose |
| --- | --- | --- | --- |
| `AccountContactCascade` | Account | Post-operation | Copies the account email down to child contacts that have none |
| `ActivityAutoNumber` | Activity | Pre-operation | Assigns a sequential subject (`ACT-NNNNN`) when none is set |
| `EmailDuplicateGuard` | Contact | Pre-validation | Blocks contacts with a duplicate email address |
| `CheckOpportunityUniqueness` | Opportunity | Pre-validation | Prevents duplicate active opportunities for the same customer and value |
| `OpportunityStageGuard` | Opportunity | Pre-operation | Stops the sales stage from moving backwards |
| `OpportunityWinProbability` | Opportunity | Post-operation | Sets win probability based on the sales stage |
| `AuditTrailPlugin` | Opportunity | Post-operation | Writes attribute changes to the plugin trace log |

## Custom APIs

| API | Handler | Purpose |
| --- | --- | --- |
| `vy_QualifyLeadAdvanced` | `QualifyLeadAdvancedHandler` | Qualifies a lead into account, contact and opportunity records |
| `vy_RecalculateForecast` | `RecalculateForecastHandler` | Sums weighted estimated value across open opportunities |
| `vy_SendQuoteEmail` | `SendQuoteEmailHandler` | Creates a quote email for an opportunity |

## Web resources

Form scripts live in `Vy.Crm.WebResources/vy_/JS/` (one per entity, plus shared
`constants.js`, `webapi.js` and `xrm-utility.js`). The `HTML/` folder holds
embedded widgets (KPI cards, pipeline funnel, sales-stage progress) and `CSS/`
holds shared form styling.

## Build

The plugin assembly targets .NET Framework 4.7.1 and is signed with
`Vy.Crm.Plugins/key.snk`.

```sh
dotnet build Vy.Crm.Plugins/Vy.Crm.Plugins.csproj
```

Build in Visual Studio or with MSBuild for the full solution. Web resources are
plain files and require no build step.

## Deployment

Register the plugin steps and import the managed solution from
`solution_managed/` into the target Dataverse environment. The minimum
opportunity value check reads the `vy_plugins_min_opportunity_value` environment
variable.

## Application

Deployed Power Apps application (model-driven):
[Open the Vy.Crm app](https://enterprisebachelor.crm4.dynamics.com/main.aspx?appid=522eccd1-7dcf-4e2d-bae6-553d2e980efe&pagetype=entitylist&etn=vy_lead&viewid=9acb7d0d-d030-43d1-a80f-c1c02c31f96f&viewType=1039)
