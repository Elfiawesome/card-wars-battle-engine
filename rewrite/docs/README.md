# Card Wars Rewrite: Design Documents

This folder is the design for the ground-up rewrite of Card Wars. Nothing here is implemented yet. Code is written only after these documents are reviewed and agreed.

The legacy code on `main` (`src/`, `gamedata/`, `CardWars.sln`) is reference material only. The rewrite does not modify it. On this branch it stays in place, untouched, and all new work lives under `rewrite/`.

## Reading order

| Doc | Topic |
|---|---|
| [00-overview.md](00-overview.md) | Goals, principles as enforceable rules, decisions made so far, project graph, glossary |
| [01-data.md](01-data.md) | The data layer: `DataTag`, codecs, formats, errors, schemas, versioning. The foundation for everything else |
| [02-ids-registries.md](02-ids-registries.md) | Identifiers, typed keys, registries, holders, content tags |
| [03-modding.md](03-modding.md) | Mod packages, manifests, load order, assembly loading, entry points, content pipeline, content sync |
| [04-script.md](04-script.md) | The data-driven script language and interpreter |
| [05-battle.md](05-battle.md) | Battle engine: state, components, changes, events, behaviours, actions, prompts, visibility |
| [06-vanilla-battle.md](06-vanilla-battle.md) | How the vanilla card game is built on the engine |
| [07-network.md](07-network.md) | Transports, framing, protocol phases, packets |
| [08-server.md](08-server.md) | Server host, players, instances, persistence, commands |
| [09-client.md](09-client.md) | Godot client, client core, assets and PCKs, views, integrated host |
| [10-infrastructure.md](10-infrastructure.md) | Virtual file system, logging, clock, random |
| [11-conventions.md](11-conventions.md) | Code style, naming, error handling, tooling, enforcement |
| [12-plan.md](12-plan.md) | Milestones, spikes, risks, testing strategy, parity checklist |
| [legacy-audit.md](legacy-audit.md) | What is wrong with the legacy code, with file references |

## How to review

Each document ends with an **Open questions** section where a decision from you is needed. Anything not listed there is a proposal I am confident in, but it is still open to change.
