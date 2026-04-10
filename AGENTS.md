## Project Snapshot

T3 Code is a minimal GUI for coding agents (Codex, Claude). The **.NET/WinUI desktop app** (`src/T3Code.App`) is the primary product. Ships multiple surfaces: server process (`t3` CLI) and web app. **Very early WIP** — proposing sweeping changes that improve long-term maintainability is encouraged.

**Priorities**: Performance and reliability first. Behavior must stay predictable under load and during failures (session restarts, reconnects, partial streams). When trading off, choose correctness and robustness over convenience.

**Maintainability**: If adding functionality, first extract shared logic into a separate module. Duplicate logic is a code smell — avoid it. Don't take shortcuts by adding local logic; refactor existing code when needed.

## Developer Commands

```bash
# Bun is the package manager (bun@1.3.9)
bun install

# Full monorepo build
bun run build

# TypeScript typecheck (all packages)
bun run typecheck

# Lint (oxlint)
bun run lint

# Format
bun run fmt

# Run all tests (turbo)
bun run test

# .NET tests (from repo root)
dotnet test

# Run single .NET test project
dotnet test tests/T3Code.Core.Tests
```

## Desktop App

```bash
# Run the desktop app (auto-builds TypeScript server + copies to output)
dotnet run --project src/T3Code.App

# Run without rebuilding the TypeScript server (faster iteration)
dotnet run --project src/T3Code.App -p:SkipServerBuild=true
```

## Development

```bash
# Full stack dev (server + web + contracts)
bun run dev

# Server only (port 3773)
bun run dev:server

# Start built server
bun run start
```

Dev runner port base: server `3773`, web `5733`. Offset via `T3CODE_DEV_INSTANCE=<name>` (hash-based) or `T3CODE_PORT_OFFSET=<n>`.

## Architecture

**Monorepo** with two stacks:
- `src/` — .NET 10/WinUI desktop app (`T3WinUI.sln`). Entry: `T3Code.App`.
- `apps/server` — TypeScript/Effect server (`t3` bin). Node 22+ required.
- `packages/` — Shared TypeScript: `@t3tools/contracts` (RPC schema), `@t3tools/shared` (utilities).
- `scripts/` — Build/release scripts.

**Dependency order for build**: `contracts` → `shared` → `server`.

## Build Artifacts

- Server: `apps/server/dist/bin.mjs` (the `t3` CLI)
- .NET: `src/*/bin/Debug/net10.0/`

## Testing

- **TypeScript**: `vitest` in `apps/server`, `packages/*`. Server tests disable file parallelism (`fileParallelism: false`) and set `testTimeout: 60_000` due to sqlite/git/orchestration load.
- **.NET**: xUnit in `tests/*.Tests`. No test parallelization issues observed.

## Important Conventions

- `.NET`: `Directory.Build.props` enforces `TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended`. All C# projects use `RootNamespace` instead of default namespace conventions.
- **Effect framework**: Server uses Effect for async orchestration. Never use raw Promises in server code paths that go through Effect.
- **Contracts package**: `packages/contracts/src/index.ts` is the canonical export. Don't import from internal `dist/`.
- **No `@t3tools/web`**: The web app (`apps/web`) is not in this workspace — it's deployed separately.

## Key Files

- Server entry: `apps/server/src/bin.ts` → `apps/server/src/cli.ts`
- .NET app entry: `src/T3Code.App/App.xaml.cs`
- Dev runner: `scripts/dev-runner.ts`
- Codex App Server protocol: https://developers.openai.com/codex/sdk/#app-server
