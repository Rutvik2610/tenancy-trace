# L2 Logical Diagram (1 of 2): Request Path and Data

```mermaid
---
title: "TenancyTrace | L2 Logical Diagram (1 of 2): Request Path and Data"
config:
  themeVariables:
    fontFamily: Arial
---
flowchart LR
    IDP["Identity Provider (SSO)<br/>confirms who is signing in"]:::ext

    subgraph PUB["Public Zone (TB1)"]
        WEB["Web Application<br/>[Single-Page Application]<br/>portal for tenants,<br/>landlords and reviewers"]:::ui
    end

    subgraph APPZ["Application Zone (TB1 and TB4)"]
        TL["Timeline and Report Service<br/>[Read Model]<br/>timeline, condition comparison,<br/>reports, AI summaries"]:::app
        WF["Tenancy Workflow Service<br/>[Policy Enforcement Point]<br/>tenancies, roles, events, status.<br/>Owns the access rules"]:::app
    end

    subgraph INTZ["Internal Zone (TB2)"]
        EV["Evidence and Provenance Service<br/>[Append-Only Ledger]<br/>validates, hashes and stores evidence;<br/>records provenance"]:::app
    end

    subgraph DATZ["Data Zone (TB3)"]
        WFDB[("Workflow Database<br/>tenancies, events, status")]:::data
        LEDGER[("Provenance Ledger<br/>append-only history")]:::data
        EFILES[("Evidence File Store<br/>validated, encrypted files")]:::data
        TLDB[("Timeline Database<br/>timeline, report jobs, summaries")]:::data
        RFILES[("Report File Store<br/>generated reports")]:::data
    end

    WEB <-->|"OIDC sign-in<br/>sync"| IDP
    WEB -->|"REST/HTTPS sync<br/>create tenancy, events, evidence"| WF
    WEB -->|"REST/HTTPS sync<br/>timeline, report, summary requests"| TL
    TL -->|"REST/HTTPS sync<br/>access check"| WF
    WF -->|"REST/HTTPS sync<br/>record event, forward evidence"| EV
    TL -->|"REST/HTTPS sync<br/>record summary events"| EV
    WF -->|"SQL sync<br/>current tenancy state"| WFDB
    EV -->|"SQL insert-only sync<br/>ledger entries and outbox"| LEDGER
    EV -->|"Blob API sync<br/>validated, encrypted files"| EFILES
    TL -->|"SQL sync<br/>timeline, jobs, summaries"| TLDB
    TL -->|"Blob API sync<br/>reports"| RFILES

    N1["Evidence service is internal-only.<br/>Users never call it; Workflow<br/>authorises every request (SC2)"]:::note
    N2["Ledger write is synchronous and<br/>fails closed: no event is accepted<br/>without provenance (ADR-005)"]:::note
    N1 -.- EV
    N2 -.- LEDGER

    classDef ui fill:#1E90FF,stroke:#0b4f94,stroke-width:2px,color:#FFFFFF
    classDef app fill:#2E7D32,stroke:#1b4d1f,stroke-width:2px,color:#FFFFFF
    classDef data fill:#282728,stroke:#000000,stroke-width:2px,color:#FFFFFF
    classDef ext fill:#757575,stroke:#3a414a,stroke-width:2px,color:#FFFFFF
    classDef note fill:#fcfcca,stroke:#3a414a,color:#000000
    classDef bnd fill:#FFFFFF,stroke:#C62828,stroke-width:2px,stroke-dasharray:6 4,color:#000000
    classDef smp fill:#FFFFFF,stroke:#3a414a,color:#000000
    style PUB fill:#FFFFFF,stroke:#C62828,stroke-width:2px,stroke-dasharray:6 4
    style APPZ fill:#FFFFFF,stroke:#C62828,stroke-width:2px,stroke-dasharray:6 4
    style INTZ fill:#FFFFFF,stroke:#C62828,stroke-width:2px,stroke-dasharray:6 4
    style DATZ fill:#FFFFFF,stroke:#C62828,stroke-width:2px,stroke-dasharray:6 4
    style LEG fill:#FFFFFF,stroke:#3a414a,stroke-width:1px
```

> Original Mermaid source preserved, including an unresolved `style LEG` reference. Do not silently fix source; resolve via a separate reviewed change.
