# 12. Plan

## 1. Order of work

The order follows the project graph bottom-up, so every layer is tested before anything depends on it. Each milestone ends with a pushed commit on `rewrite` where `dotnet build` and `dotnet test` pass, and a short report of what was done and what changed from these documents.

Spikes come first because their results change the client design.

## 2. Spikes

| # | Question | How | Affects |
|---|---|---|---|
| S1 | Can Godot 4.7 bind C# scripts from a mod assembly loaded at runtime when a scene from that mod's PCK references them? | A minimal Godot project, a class library with one `Node` script, a PCK with a scene using it; try loading the DLL into the Godot load context and registering it before mounting the PCK | 09-client.md, section 5.3: Plan A or Plan B |
| S2 | Does Godot 4.7 build and run a project targeting `net10.0`, including an Android export? | Same project retargeted; desktop run; Android export (there is a reported Android template issue with LTS .NET: [godot#118989](https://github.com/godotengine/godot/issues/118989)) | Target framework decision (00-overview.md, section 5) |
| S3 | Can the Godot client load a non-Godot server library into its own load context and run it on a thread, sharing framework assemblies? | Spike project with a stub `IEmbeddedServer` | 09-client.md, section 7 |
| S4 | Does a collectible load context with a mod assembly unload once the environment is disposed? | Console test with `WeakReference` on the context | 03-modding.md, section 9 (keeps unloading possible) |

S1 to S3 need a Godot 4.7 .NET editor. I will try a headless Godot build in the sandbox. If that is not possible, I will prepare the spike projects and ask you to run them and send me the output.

## 3. Milestones

### M0. Skeleton and guardrails

- `rewrite/CardWars.slnx`, `Directory.Build.props`, `.editorconfig`, empty projects per 00-overview.md section 6.
- `tests/CardWars.Architecture.Tests` with the reference-graph and mutable-static checks (11-conventions.md, section 8).
- CI workflow for the `rewrite` folder: build, test, format check.
- Steering file with the conventions.

Done when: empty solution builds in CI, and the architecture tests fail on a deliberately added violation and pass without it.

### M1. Core

- `Id`, `ModId`, `DataPath`, `EquatableArray<T>`, `SemanticVersion`, `VersionRange`.
- `DataTag` kinds, builders, structural equality.
- JSON and binary formats with limits and source positions.
- `Codec<T>`, `DataResult<T>`, contexts, built-in codecs, record codecs, dispatch, holders, schema model.
- `Registry<T>`, `RegistrySet`, `HandlerTable`, freeze.
- Components: `ComponentType<T>`, `ComponentMap`, preserved unknown entries.
- Migrations framework and document header.
- `VirtualPath`, `IFileSystem`, `MountedFileSystem`, `LocalFileSystem`, `MemoryFileSystem`.
- Logging, clock, `DeterministicRandom`.

Done when: round-trip, error-path, limit, and fuzz tests pass; a test runs two codec environments with conflicting dispatch registrations in one process.

### M2. Modding

- Manifest codec, discovery, resolution, ordering.
- `ModLoadContext`, entry instantiation, registrars.
- Content pipeline: read, decode, register and replace, tags, link, validate, freeze; the error report format.
- `ModEnvironment`.

Done when: fixture mods (valid, missing dependency, cycle, bad content, override) produce the expected environments and the expected error reports.

### M3. Script

- Values, signatures, host object types, functions, effects.
- Expression and statement node kinds with codecs.
- Compiler with checks; instruction machine with suspend and resume and a step budget.

Done when: compile-error tests and execution tests pass; legacy scripting scenarios that still apply are ported.

### M4. Battle engine

- State, entities, components, changes and change sets, `BattleView` with indexes.
- Event pipeline, behaviour providers and listener index, prompts.
- Action types, `Submit`, legal actions, system actions.
- `BattleReplicator` with projection by diff; snapshots; state codec.

Done when: engine tests with the tiny test game pass, including the determinism replay test and the projection tests.

### M5. Vanilla battle

- `CardWars.Vanilla.Common`: content types, components, entity types, actions, legal action kinds, cues.
- `CardWars.Vanilla.Server` battle parts: setup, handlers, events, behaviour providers, effects, functions, visibility rule, win and lose conditions.
- Legacy content ported by hand to the new schemas in `rewrite/mods/vanilla/` (about 35 files), with `test_ability` removed.

Done when: parity tests (section 6) pass, and the legal-action agreement helper passes on randomized battles.

### M6. Network, server, headless client

- `CardWars.Network`: transports, framing, phases, packet registry and numbering, keepalive.
- `CardWars.Server`: start, tick loop, authentication (offline), profiles, instance manager, hooks, commands, save layout with atomic writes.
- `CardWars.Vanilla.Server`: world and battle instance types, packets, commands.
- `CardWars.Server.Host` console executable.
- `CardWars.Client.Core`: session, phases, environment build from synced data, battle and world replicas.

Done when: end-to-end tests run two headless clients through login, content sync, world, battle, and back, and check that neither client receives the other's hidden cards.

### M7. Godot client

- App shell, menus, settings, Godot file system and log sink.
- Client mod loading and PCK mounting, asset resolver.
- View registry and the S1 outcome (Plan A or B).
- World screen.
- Battle screen: replica binding, presenter, entity views, layouts, animation racks, hand, attack, and prompt controllers, inspection panels.
- Integrated host through the embedded server loader.

Done when: you can host a game from the client, a second client can join over TCP, and both can play a full battle in the world.

### M8. Parity review

- Walk the parity checklist (section 6) against the running game.
- JSON Schema export for content types, and generated content reference docs for modders.
- A written list of everything that behaves differently from legacy, with the reason.

## 4. After v1

Roughly in order of value: content reload, text script syntax, client mod and PCK download from servers, real authentication, interest management for worlds, quests and world events, game modes and roguelike runs, saving battles that are waiting on a prompt, an optional source generator for record codecs, server code unloading.

## 5. Risks

| Risk | Effect | Mitigation |
|---|---|---|
| Godot cannot bind mod C# scripts in PCK scenes (S1) | Mod UI is built in code around plain scenes | Plan B is already designed; vanilla uses the same path as third-party mods, so it is tested daily |
| `net10.0` problems in Godot 4.7, especially Android (S2) | Framework target | Fall back to `net9.0` (out of support) or `net8.0` (also out of support as of 2026-11-10) for the client only, keeping libraries multi-targeted if needed |
| Explicit record codecs are tedious to write for many types | Slower development | Accept for v1; add a source generator later that emits the same codecs |
| Projection by diff costs too much for large battles | Server CPU | Battles have hundreds of entities at most; only touched entities are diffed. Measure in M5 |
| The rewrite takes long before anything is playable | Motivation, feedback | The headless end-to-end tests in M6 give a playable battle in tests before the Godot work starts |
| Legacy rules that were accidents get copied as features | Wrong game behaviour | Every changed rule is listed in 06-vanilla-battle.md and its open questions |

## 6. Parity checklist

From reading the legacy code and tests. Each item gets a test in the rewrite.

Battle:
- [ ] Seats with teams; any number of teams and seats per team
- [ ] Battlefield with 3 front slots and 1 back slot from content
- [ ] Heroes on the battlefield with hero health
- [ ] Deck from content, drawing into hand
- [ ] Points: round schedule, cap, top-up at round end
- [ ] Playing a unit into an empty own slot, paying its cost
- [ ] Turn order, phases (setup, attacking), rounds
- [ ] Basic attack, custom basic attack, special attacks combined
- [ ] Attack patterns: cell, row, column, all, with offsets and percent or flat damage
- [ ] Charge cost and charge refill at round end
- [ ] Back-row protection and bypass
- [ ] Hero attacks only when no living units remain, damage to hero health at 1 per 10
- [ ] Deaths at round end, graveyard
- [ ] Hero lose condition (default and scripted), seat defeat, board removal
- [ ] Win condition script per team; win and draw outcomes
- [ ] Data-driven abilities on cards, triggered by events, able to prompt
- [ ] Ability types and attack definitions shown in the client
- [ ] Animations: player intro, play card, attack, take damage, death

World and server:
- [ ] Login by username with a persistent id
- [ ] Profiles saved and restored, including location
- [ ] Worlds from content with exits to other worlds
- [ ] Server-authoritative movement, other players visible
- [ ] Entering a battle from the world and returning
- [ ] Integrated host and remote join over TCP

Client:
- [ ] Card display (unit, spell, hero layouts) with textures, stats, abilities, special attacks
- [ ] Hand with hover, selection, and target arrow
- [ ] Attack selection with multiple attackers and target highlights
- [ ] Inspection panels with ability, ability type, and attack pattern grid
- [ ] Battlefield layouts (default polygon layout and line layout)
- [ ] Camera focus on the acting seat, zoom, pan
- [ ] Game over overlay

## 7. How we work

- I implement one milestone at a time on `rewrite`, push, and report: what was built, test results, and any place I departed from these documents and why.
- When a milestone reveals that a document is wrong, the document is updated in the same push.
- Open questions in each document are collected below. Answers can come in any order; anything unanswered when its milestone starts gets the proposed default, and I will say so in the report.

## 8. All open questions

| Doc | Question | Proposed default |
|---|---|---|
| 01 | Dispatch key `"type"` or `"_type"` | `"type"` |
| 01 | Comments in content JSON | allowed |
| 02 | `Replace` on content registries by default | yes |
| 03 | Data-only client mods allowed | yes |
| 04 | Strict booleans in conditions | yes |
| 05 | Event order FIFO or a stack | FIFO |
| 06 | Draw rule | keep legacy (unlimited during own turn) until you decide |
| 06 | Multi-attacker attacks | one attacker per action |
| 06 | Shuffle at setup | yes |
| 06 | Spells and hero plays out of parity scope | yes |
| 09 | One shared mod install for hosting | yes |
| 09 | Origin and license of `MoreRichTextLabel` | keep as is |
| 11 | `Points`/`HeroHealth`/`Attack` in code, or `Pt`/`Hrt`/`Atk` | full words |
