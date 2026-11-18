# Task 4665 DEV monitoring foundation

Status: implementation prepared for the Step 1 PR. Database application remains gated on the approved deployment process. The user authorized Step 2 development against an isolated local database while review proceeds; this does not authorize a manual DEV migration or deployment.

This increment follows [Story 4664](https://dev.azure.com/SSC-FSDH/FSDH/_workitems/edit/4664) and [Task 4665](https://dev.azure.com/SSC-FSDH/FSDH/_workitems/edit/4665): Protected B monitoring in DEV, using existing service access. The older PRE architecture draft in this directory is background; its request ledger, missing-run guarantees and nine-stage breakdown do not govern this increment.

## Verified DEV sources

Read-only checks on 2026-11-17 established:

| Setting | Observed value |
| --- | --- |
| Organization and project | `SSC-FSDH / FSDH` |
| Project ID | `e4102ffd-0961-4cc1-ae2b-d3d09284155c` |
| Infrastructure repository | `fsdh-project-infra-dev`, ID `db7eb06c-c2a8-4f7d-8d26-65411d6d6940` |
| Dispatcher | Definition 93, `1. dev.fsdh-project-infra` |
| Execution | Definition 94, `2. dev.fsdh-project-infra` |
| Default ref | `refs/heads/main` for both definitions |
| Dispatcher YAML | `pipeline/resource-provisioner/1-build-affected-projects.yml` |
| Execution YAML | `pipeline/resource-provisioner/2-action-affected-projects.yaml` |
| Dispatcher fan-out configuration | `pipeline/resource-provisioner/vars/vars-dev.yaml`: `fanoutPipelineId: 94` |
| Existing Service Bus namespace | `g2dcctofsdh-sbus-dev`; YAML output queue resource group `g2dc-cto_fsdh-dev-rg` |

The existing **locally configured RP service identity**, using its existing client-credential setup, successfully read definitions 93/94, build 17191, its timeline and its Pipeline Runs API details. No new credentials or permissions were introduced. This verifies those read operations from this workstation, not deployed Function networking or all pagination endpoints. Credentials and tokens were kept out of output and source files.

RP already binds `InfrastructureRepository:AzureDevOpsConfiguration`, uses `InfraTokenCredentialService`, obtains the DevOps scope through `AzAccessTokenManager`, and registers an authenticated HTTP client. Existing Functions messaging uses `DatahubServiceBus:ConnectionString`. Source defaults still refer to an older organization/project, so monitoring must use explicit verified source configuration rather than those defaults. Runtime bindings remain for Tasks 2 and 3.

The existing Service Bus inventory was read through Azure Resource Manager. `resource-run-request`, `terraform-output-handler` and the two main infrastructure-health queues are active, have maximum delivery count 3 and have broker duplicate detection disabled. **`pipeline-status` does not exist** and needs reviewed provisioning before the handler is activated. No messages were sent or consumed, and no database connection to DEV was opened. Service Bus send/listen access and deployed-host networking remain integration checks before activation.

## Workspace and classification evidence

Current DEV main dispatches one execution run per workspace with `project` and `environment` parameters. [Run 17191](https://dev.azure.com/SSC-FSDH/FSDH/_build/results?buildId=17191) reports `project=NUN1`, `environment=dev`. Its job identifier is `Project_Branchout.Project.Project`, display name `Process NUN1`, attempt 1. The identifier alone does not contain the workspace acronym; correlate it using the supported parameter/YAML version. The source commit was `cdcfbe210396aa99b0a7a5981c68324a8f76f2ce`.

The contract also supports multi-workspace versions using `projects`, with separate stage/job results and attempts. Their support is covered by fixtures; the inspected DEV execution was single-workspace. Missing timelines, partial scans and unmapped records remain explicit rather than implying successful or failed workspace deployments.

Definitions 93/94 and the `project` parameter do **not** establish Protected B classification. Portal `Datahub_Project.Data_Sensitivity` is an existing authoritative classification field; `TerraformWorkspace.IsProtectedB` is derived from it. The poller must not gain Portal DB access to read it. Before activation, review how the infrastructure side establishes scope from existing generated Terraform at the run's source commit or an explicitly approved workspace scope configuration, and how the consumer checks Portal classification where available. Do not infer classification from workspace names, filenames shared by base/enhanced templates, or the definition ID. Deleted, unrecognized or reclassified workspaces require an explicit history/scope policy. The storage contract does not itself certify classification.

Dispatcher failures before workspace discovery may not be attributable to a specific Protected B workspace. Their treatment as shared infrastructure health must be reviewed; do not invent workspace associations or silently broaden the Story to unclassified execution monitoring.

## Contract and persistence design

The shared `PipelineRunObservationMessage` and `PipelineMonitorHealthMessage` are schema version 1. Both carry an immutable observation ID, explicit organization/project/environment/definition identity and UTC observation time. Source identities must match an explicitly supplied allowlist; the store is not registered in a running host yet.

Run messages preserve source change time when available, run identity/names/ref/commit, queue/start/finish times, normalized execution state/result, timeline completeness, evidenced workspace associations and stage/job attempt records. Timeline identity includes timeline ID, record ID, parent ID, attempt and change ID. Unknown values remain unknown; names and times are not original Portal request identity. Source status normalization and supported-ref configuration belong to the poller.

Health messages represent complete, partial, failed, disabled or unknown monitoring, including idle periods. Healthy requires a complete scan at the observation time without an error. Error codes are bounded; source failure summaries must be sanitized before publication. No full logs, tokens, request ledger or Terraform outputs belong in these messages.

| Table | Purpose |
| --- | --- |
| `PipelineObservations` | Immutable run or health snapshot, source identity, run ID where applicable, observation/receipt/source-change times, versioned JSON payload and SHA-256 fingerprint. Primary key is the publisher's observation ID. Source/run/time and source/kind/time indexes support history reads. |
| `PipelineObservationWorkspaces` | Indexed workspace acronym links to snapshots, with no foreign key to mutable Portal workspace rows. Removing a workspace cannot erase its execution history. Links are removed only with their owning observation. |

Stage/job details remain inside each versioned snapshot rather than duplicating every stage and retry into relational projections. This keeps the initial schema small and retains complete evidence. Review this choice against expected retention, volume and query patterns before rollout; frequent job-level filtering may justify a later relational projection. History reads are bounded to 500 observations, default 100. The payload limit is 192 KiB of UTF-8 before messaging-envelope overhead; large timelines need an explicit split protocol before that limit can be increased.

Exact message redeliveries are no-ops, including concurrent insert races. Reusing an observation ID with changed content is an error. Independent polling observations receive new IDs; a transport retry must retain its ID and unchanged payload. Workspace links and the snapshot commit atomically. Database errors propagate so the future queue handler can retry rather than acknowledge failed persistence.

Delayed snapshots are retained and never overwrite earlier snapshots. History is ordered by observation time, not receipt time. **The newest received or observed snapshot is not automatically authoritative current state.** Task 2 must review current-state reconciliation using source change times, timeline revisions and attempt identities; a newer retry may legitimately reopen execution, while a stale delivery must not regress it. No mutable current-state projection is implemented here.

The `pipeline-status` queue name is reserved in `QueueConstants`; queue provisioning, host registration and the live consumer are not part of this foundation. Run links are built from validated source identity and run ID on `dev.azure.com`, rather than accepting arbitrary message URLs. Read/query authorization must be enforced by the administrator-facing service in Task 4; this unregistered storage helper is not an authorized UI query API.

## Migration review and rollback

SQL Server migration: `20261117162949_PipelineMonitoringFoundation`.

SQLite migration: `20261117163026_PipelineMonitoringFoundation`.

Both add only the two monitoring tables, the three history/workspace indexes, and the child-to-observation cascading foreign key. Neither alters existing resource/workspace tables. Review generated migrations and provider snapshots before application. No application host was started and no migration was applied to an existing local, DEV, PRE or PRD database.

Rollback first disables the future publisher/consumer, retains any required monitoring evidence, then reverts only this increment to the preceding `Subnet_unicity` migration using the appropriate context and reviewed deployment procedure. `Down` drops workspace links before observations, losing monitoring history but leaving provisioning/resource data intact. Snapshot history is not recoverable after rollback without a retained backup. Do not run rollback blindly if later migrations have been applied.

## Validation and review gate

Targeted integration tests use disposable SQLite databases, not EF InMemory. Coverage includes run/job result independence, queued runs before source/timeline metadata exists, cancelled/failed/unknown results, JSON round trips, concurrent identical redelivery, conflicting ID reuse, delayed observations, retry attempts, unknown or absent Portal workspace rows, idle/failed health, invalid contracts, safe links, transaction failure/recovery, SQLite migration application/rollback/reapplication and SQL Server migration SQL generation.

SQL Server scripts are checked without connecting to SQL Server. Actual SQL Server migration execution, concurrency and performance remain required DEV/provider checks; SQLite results do not establish SQL Server runtime behavior.

Validation on 2026-11-17: **15 targeted tests passed, zero failed or skipped**. The ResourceProvisioner Functions build succeeded; it reported warnings in existing source files. EF reported no pending model changes for either SQL Server or SQLite. The build did not start a Function host.

Reproduce the targeted tests from the repository root:

```powershell
dotnet test --project Portal/test/Datahub.Infrastructure.UnitTests/Datahub.Infrastructure.UnitTests.csproj --no-restore -- --filter 'FullyQualifiedName~PipelineObservationStoreTests'
```

Before applying migrations or activating the handler, review contracts, snapshot storage and query budget, Protected B scope evidence, retention and sanitized failure content, source configuration, migration/rollback procedure and passing test evidence. Acceptable visibility delay, heartbeat/staleness thresholds and retention periods still need owner agreement before activation. Opening the PR is not permission to merge or deploy it. Portal startup automatically applies pending migrations to its configured database; do not run it against DEV from this branch before the schema change is approved.
