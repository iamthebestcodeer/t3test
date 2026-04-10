# T3 Code Feature Inventory

This file inventories implemented product features and UX behavior present in the current codebase. It is source-linked and intentionally concrete: if a behavior is listed here, it is backed by code in this repo rather than roadmap text.

## Product Scope

- T3 Code is a minimal GUI for coding agents, currently supporting `codex` and `claudeAgent`, with desktop, web, server, and marketing surfaces. Refs: `README.md`, `packages/contracts/src/orchestration.ts`, `packages/contracts/src/model.ts`
- The repo ships multiple entrypoints: browser/web app, Electron desktop app, server process, and marketing/download pages. Refs: `package.json`, `apps/web/package.json`, `apps/desktop/src/main.ts`, `apps/marketing/src/pages/index.astro`
- Desktop distribution is explicitly supported for Windows, macOS, and Linux; the README also advertises `npx t3`, `winget`, Homebrew cask, and AUR install paths. Refs: `README.md`, `apps/marketing/src/pages/download.astro`

## Web App Shell

- The root route blocks on native API availability and shows a full-screen `Connecting to T3 Code server...` state until the app can talk to the backend. Refs: `apps/web/src/routes/__root.tsx`
- The app bootstraps server state synchronization on load and wraps the UI in both regular and anchored toast providers. Refs: `apps/web/src/routes/__root.tsx`
- The root shell includes coordinated WebSocket connection UX, including connection-state handling and slow-RPC acknowledgement surfacing. Refs: `apps/web/src/routes/__root.tsx`, `apps/web/src/components/WebSocketConnectionSurface.tsx`
- Router-level failures show a full-screen error card with app branding, a friendly message, `Try again`, `Reload app`, and expandable raw error details. Refs: `apps/web/src/routes/__root.tsx`
- The app migrates older local settings into server-backed settings after receiving the server `welcome` event. Refs: `apps/web/src/routes/__root.tsx`, `apps/web/src/hooks/useSettings.ts`
- Server `welcome` can auto-expand a bootstrap project and auto-navigate the user from `/` into a bootstrap thread. Refs: `apps/web/src/routes/__root.tsx`
- Server config reloads produce toasts; valid keybinding reloads show success, and invalid keybindings show a warning toast with an `Open keybindings.json` action. Refs: `apps/web/src/routes/__root.tsx`
- Consecutive streamed `thread.message-sent` events for the same message are coalesced client-side so the UI behaves like a continuously updating assistant message instead of many separate message fragments. Refs: `apps/web/src/routes/__root.tsx`

## Global Navigation And Layout

- The left app sidebar is part of the main app shell, is resizable, and persists its width. Refs: `apps/web/src/components/AppSidebarLayout.tsx`
- The layout supports desktop menu actions coming from Electron, including opening Settings from the native menu. Refs: `apps/web/src/components/AppSidebarLayout.tsx`, `apps/desktop/src/main.ts`
- The chat index route provides a true empty state: desktop Electron shows a drag-region title bar with `No active thread`, while the browser/mobile layout shows a small `Threads` header with a sidebar trigger. Refs: `apps/web/src/routes/_chat.index.tsx`
- Settings uses a dedicated layout with browser/mobile and Electron-specific headers; both show `Restore defaults` when there are modified settings. Refs: `apps/web/src/routes/settings.tsx`
- Pressing `Escape` inside Settings navigates back in browser history. Refs: `apps/web/src/routes/settings.tsx`
- Visiting `/settings` redirects to `/settings/general`. Refs: `apps/web/src/routes/settings.tsx`

## Sidebar, Projects, And Thread List UX

- Projects can be added to the sidebar by path or, in desktop mode, via a native folder picker. Refs: `apps/web/src/components/Sidebar.tsx`, `apps/web/src/wsNativeApi.ts`, `apps/desktop/src/main.ts`
- Projects can be removed from the sidebar. Refs: `apps/web/src/components/Sidebar.tsx`
- Project paths can be copied from the UI. Refs: `apps/web/src/components/Sidebar.tsx`
- Projects can be sorted by `Last user message`, `Created at`, or `Manual`. Refs: `apps/web/src/components/Sidebar.tsx`, `packages/contracts/src/settings.ts`
- Manual project ordering supports drag-and-drop reordering. Refs: `apps/web/src/components/Sidebar.tsx`, `apps/web/src/components/Sidebar.logic.ts`
- Project expanded/collapsed state persists. Refs: `apps/web/src/uiStateStore.ts`, `apps/web/src/components/Sidebar.tsx`
- Threads are grouped under projects. Refs: `apps/web/src/components/Sidebar.tsx`
- Threads can be sorted by `Last user message` or `Created at`. Refs: `apps/web/src/components/Sidebar.tsx`, `packages/contracts/src/settings.ts`
- Thread groups can collapse to preview subsets while preserving visibility of the active thread. Refs: `apps/web/src/components/Sidebar.tsx`, `apps/web/src/components/Sidebar.logic.ts`
- Thread rows show active and selected states separately, including multi-select highlighting. Refs: `apps/web/src/components/Sidebar.tsx`, `apps/web/src/threadSelectionStore.ts`
- Thread rows show status pills for live thread conditions such as running work, pending approval, waiting for user input, plan ready, and unread completion. Refs: `apps/web/src/components/Sidebar.tsx`, `apps/web/src/components/Sidebar.logic.ts`
- Thread rows show a pulsing terminal activity indicator when a terminal process is running for that thread. Refs: `apps/web/src/components/Sidebar.tsx`, `apps/web/src/terminalStateStore.ts`
- Thread rows show PR state badges and tooltips for `open`, `closed`, and `merged` PRs, and clicking the badge opens the PR URL. Refs: `apps/web/src/components/Sidebar.tsx`, `packages/contracts/src/git.ts`
- Thread rows support open on click, keyboard open on `Enter` or `Space`, right-click context menus, inline rename, archive, and delete flows. Refs: `apps/web/src/components/Sidebar.tsx`, `apps/web/src/hooks/useThreadActions.ts`
- Thread context menus include actions such as rename, archive, delete, copy workspace path, and copy thread ID. Refs: `apps/web/src/components/Sidebar.tsx`, `apps/web/src/hooks/useThreadActions.ts`
- The sidebar supports Cmd/Ctrl-click toggle selection, Shift-click range selection, explicit anchor tracking, and `Escape` to clear selection. Refs: `apps/web/src/components/Sidebar.tsx`, `apps/web/src/threadSelectionStore.ts`, `apps/web/src/routes/_chat.tsx`
- Multi-selected threads can be acted on from a shared context menu instead of only per-thread menus. Refs: `apps/web/src/components/Sidebar.tsx`
- Keyboard-driven thread traversal is supported with previous, next, and jump-to-index commands, and the UI can temporarily show jump hints. Refs: `apps/web/src/components/Sidebar.tsx`, `apps/web/src/components/Sidebar.logic.ts`, `apps/web/src/keybindings.ts`, `packages/contracts/src/keybindings.ts`
- Sidebar branding includes app version and stage labeling. Refs: `apps/web/src/components/Sidebar.tsx`, `apps/web/src/branding.ts`
- Desktop update state is surfaced directly inside the sidebar via an update pill and related warning/CTA UI. Refs: `apps/web/src/components/sidebar/SidebarUpdatePill.tsx`, `apps/web/src/components/Sidebar.tsx`

## New Thread Behavior

- New threads are draft-first rather than forcing immediate server creation. Refs: `apps/web/src/hooks/useHandleNewThread.ts`, `apps/web/src/composerDraftStore.ts`
- Creating a new thread can inherit branch, worktree path, provider, model, runtime mode, and other sticky composer state from the current thread/draft. Refs: `apps/web/src/hooks/useHandleNewThread.ts`, `apps/web/src/composerDraftStore.ts`
- New thread creation can revive an existing draft thread for the same project instead of duplicating drafts. Refs: `apps/web/src/hooks/useHandleNewThread.ts`
- New threads respect the configured default environment mode: `Local` or `New worktree`. Refs: `apps/web/src/hooks/useHandleNewThread.ts`, `apps/web/src/components/Sidebar.logic.ts`, `packages/contracts/src/settings.ts`
- Global shortcuts support two distinct new-thread behaviors: inherited-context thread creation and force-local thread creation. Refs: `apps/web/src/routes/_chat.tsx`, `apps/server/src/keybindings.ts`

## Chat Route And Thread View

- The active thread route validates diff-related search params and retains them across navigation. Refs: `apps/web/src/routes/_chat.$threadId.tsx`
- If a requested thread no longer exists after bootstrap, the route redirects back to `/`. Refs: `apps/web/src/routes/_chat.$threadId.tsx`
- Diff UI is responsive: on wide screens it opens as an inline resizable right sidebar; on narrower screens it opens as a right-hand sheet. Refs: `apps/web/src/routes/_chat.$threadId.tsx`
- The inline diff panel persists its width and refuses resizes that would compress the composer below its minimum usable width. Refs: `apps/web/src/routes/_chat.$threadId.tsx`
- The diff panel is lazy-loaded and kept warm after first open to improve subsequent toggles. Refs: `apps/web/src/routes/_chat.$threadId.tsx`
- The main thread page contains a top header, timeline, composer, optional plan sidebar, optional terminal drawer, and optional diff UI. Refs: `apps/web/src/components/ChatView.tsx`

## Chat Header And Thread Toolbar

- The thread header shows the thread title and project context. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/components/chat/ChatHeader.tsx`
- The header can show a `No Git` warning when the active workspace is not a Git repo. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/components/chat/ChatHeader.tsx`
- The header exposes `Open in` actions for the workspace and file manager/editor integrations. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/components/chat/OpenInPicker.tsx`
- The header exposes project script actions, Git actions, terminal toggle, and diff toggle. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/components/ProjectScriptsControl.tsx`, `apps/web/src/components/GitActionsControl.tsx`
- Provider availability warnings appear in-thread via a banner instead of silently failing. Refs: `apps/web/src/components/chat/ProviderStatusBanner.tsx`
- Thread-level transport or runtime errors appear in a dismissible thread error banner. Refs: `apps/web/src/components/chat/ThreadErrorBanner.tsx`

## Timeline And Message Rendering

- The conversation timeline is virtualized for performance. Refs: `apps/web/src/components/chat/MessagesTimeline.tsx`, `apps/web/src/components/ChatView.tsx`
- Scroll behavior preserves the expected chat experience, including automatic bottom-following when appropriate. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/chat-scroll.ts`
- A `Scroll to bottom` affordance appears when the user is away from the live end of the thread. Refs: `apps/web/src/components/ChatView.tsx`
- The timeline renders user messages, assistant markdown messages, tool/work-log entries, streaming indicators, working indicators, and proposed-plan entries in a unified chronological view. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/session-logic.ts`, `apps/web/src/components/chat/MessagesTimeline.tsx`
- User messages support image attachments. Refs: `apps/web/src/components/chat/MessagesTimeline.tsx`, `packages/contracts/src/orchestration.ts`
- User messages support copy-to-clipboard. Refs: `apps/web/src/components/chat/MessagesTimeline.tsx`
- User messages can render inline terminal-context chips showing captured terminal snippets referenced in the prompt. Refs: `apps/web/src/components/chat/TerminalContextInlineChip.tsx`, `apps/web/src/components/chat/userMessageTerminalContexts.ts`
- User messages can expose a checkpoint rewind action such as `revert to this message` when the thread has checkpoint history. Refs: `apps/web/src/components/chat/MessagesTimeline.tsx`, `apps/web/src/components/ChatView.tsx`
- Assistant messages render markdown. Refs: `apps/web/src/components/ChatMarkdown.tsx`, `apps/web/src/components/chat/MessagesTimeline.tsx`
- Markdown/file-like links can be resolved back into project-relative targets. Refs: `apps/web/src/markdown-links.ts`
- Assistant messages surface changed-file summaries, diff stats, expandable file trees, and `View diff` entry points. Refs: `apps/web/src/components/chat/MessagesTimeline.tsx`, `apps/web/src/components/chat/ChangedFilesTree.tsx`, `apps/web/src/components/chat/DiffStatLabel.tsx`
- Image attachments can be opened into a full-screen preview/lightbox with previous and next navigation. Refs: `apps/web/src/components/chat/ExpandedImagePreview.tsx`, `apps/web/src/components/ChatView.tsx`

## Proposed Plans And Plan UX

- The system has an explicit plan interaction mode alongside the default interaction mode. Refs: `packages/contracts/src/orchestration.ts`
- Proposed plans are stored per thread and surfaced as first-class objects with lifecycle metadata including `implementedAt` and `implementationThreadId`. Refs: `packages/contracts/src/orchestration.ts`
- Proposed plans can be expanded and collapsed in the chat UI. Refs: `apps/web/src/components/chat/ProposedPlanCard.tsx`
- Proposed plans can be copied, downloaded, and saved into the workspace as files. Refs: `apps/web/src/components/chat/ProposedPlanCard.tsx`, `apps/web/src/components/PlanSidebar.tsx`, `packages/contracts/src/project.ts`
- A dedicated plan sidebar can surface plan history/catalog data. Refs: `apps/web/src/components/PlanSidebar.tsx`, `apps/web/src/components/ChatView.tsx`
- When a plan is actionable, the composer switches to follow-up actions such as refine plan, implement, or implement in a new thread. Refs: `apps/web/src/components/chat/ComposerPlanFollowUpBanner.tsx`, `apps/web/src/components/chat/ComposerPrimaryActions.tsx`, `apps/web/src/proposedPlan.ts`

## Composer Core UX

- The composer is a rich prompt editor rather than a plain textarea. Refs: `apps/web/src/components/ComposerPromptEditor.tsx`
- The composer supports inline file and directory mentions triggered with `@`, including chip rendering instead of raw text paths. Refs: `apps/web/src/components/ComposerPromptEditor.tsx`, `apps/web/src/components/chat/ComposerCommandMenu.tsx`
- The composer supports slash-command style menus, including model/path related insertion UX. Refs: `apps/web/src/components/ComposerPromptEditor.tsx`, `apps/web/src/components/chat/ComposerCommandMenu.tsx`
- The editor preserves custom cursor behavior around inline chips/tokens so tokenized content is still editable. Refs: `apps/web/src/components/ComposerPromptEditor.tsx`, `apps/web/src/composer-logic.ts`
- Prompt text is limited by the provider contracts to `120000` input characters. Refs: `packages/contracts/src/orchestration.ts`
- The composer supports pasted and dropped images. Refs: `apps/web/src/components/ChatView.tsx`
- The composer supports image attachment previews and removal before send. Refs: `apps/web/src/components/ChatView.tsx`
- The composer limits sends to at most `8` attachments and `10 MB` per image. Refs: `packages/contracts/src/orchestration.ts`, `apps/web/src/components/ChatView.tsx`
- Sending only images is supported; the client injects a fallback prompt so the model can respond meaningfully without separate typed text. Refs: `apps/web/src/components/ChatView.tsx`
- Drafts with unsaved image attachments show persistence-related warnings because those blobs are not durable until sent. Refs: `apps/web/src/composerDraftStore.ts`, `apps/web/src/components/ChatView.tsx`
- Sent messages use optimistic attachment preview handoff so previews remain visible while the server catches up. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/components/ChatView.logic.ts`
- Send/stop state is derived from thread phase and local dispatch state rather than only button clicks. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/components/ChatView.logic.ts`

## Composer Modes, Runtime, And Provider Controls

- The composer footer includes provider selection. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/components/chat/ProviderModelPicker.tsx`
- The composer footer includes model selection. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/components/chat/ProviderModelPicker.tsx`
- The app has two runtime modes: `approval-required` and `full-access`. Refs: `packages/contracts/src/orchestration.ts`, `apps/web/src/components/ChatView.tsx`
- The app has two interaction modes: `default` and `plan`. Refs: `packages/contracts/src/orchestration.ts`, `apps/web/src/components/ChatView.tsx`
- The composer exposes runtime-mode toggles in the footer, with lock/unlock visual language. Refs: `apps/web/src/components/ChatView.tsx`
- The composer exposes a plan/build mode toggle in the footer. Refs: `apps/web/src/components/ChatView.tsx`
- The composer exposes a context-window meter that reflects model/context-window state. Refs: `apps/web/src/components/chat/ContextWindowMeter.tsx`, `apps/web/src/lib/contextWindow.ts`
- The composer footer supports compact layouts when space is tight instead of simply overflowing. Refs: `apps/web/src/components/composerFooterLayout.ts`, `apps/web/src/components/ChatView.tsx`
- The app tracks provider-specific model capabilities and only shows traits/controls that the selected model can actually support. Refs: `apps/web/src/providerModels.ts`, `apps/web/src/components/chat/composerProviderRegistry.tsx`
- Codex model options include reasoning effort (`xhigh`, `high`, `medium`, `low`) and `Fast mode`. Refs: `packages/contracts/src/model.ts`, `apps/web/src/components/chat/TraitsPicker.tsx`
- Claude model options include `Thinking`, effort (`low`, `medium`, `high`, `max`, `ultrathink`), `Fast mode`, and provider-defined context-window options. Refs: `packages/contracts/src/model.ts`, `apps/web/src/components/chat/TraitsPicker.tsx`
- Prompt-injected effort prefixes are supported for Claude models whose capabilities declare prompt-injected effort levels. Refs: `apps/web/src/components/ChatView.tsx`, `packages/contracts/src/model.ts`, `packages/shared/src/model.ts`
- Provider/model defaults exist and are user-visible through initial selections: Codex defaults to `gpt-5.4`, Claude defaults to `claude-sonnet-4-6`. Refs: `packages/contracts/src/model.ts`

## Approval And User-Input Flows

- Provider approval requests interrupt normal compose UX and replace it with approval-specific UI. Refs: `apps/web/src/components/chat/ComposerPendingApprovalPanel.tsx`, `apps/web/src/components/ChatView.tsx`
- Approval decisions support `accept`, `acceptForSession`, `decline`, and `cancel`. Refs: `packages/contracts/src/orchestration.ts`
- Pending user-input flows are modeled separately from approvals. Refs: `apps/web/src/components/chat/ComposerPendingUserInputPanel.tsx`, `packages/contracts/src/providerRuntime.ts`
- Pending user-input questions are shown one at a time. Refs: `apps/web/src/components/chat/ComposerPendingUserInputPanel.tsx`
- Users can answer pending questions by clicking options or by pressing number keys. Refs: `apps/web/src/components/chat/ComposerPendingUserInputPanel.tsx`, `apps/web/src/components/chat/ComposerPrimaryActions.tsx`
- Pending user-input UX supports previous and next navigation through multi-question flows. Refs: `apps/web/src/components/chat/ComposerPendingUserInputPanel.tsx`
- Optional custom typed answers can be entered into the composer when the question configuration allows it. Refs: `apps/web/src/components/chat/ComposerPendingUserInputPanel.tsx`
- The client preserves draft answers/progress while a multi-step user-input flow is being completed. Refs: `apps/web/src/pendingUserInput.ts`, `apps/web/src/components/ChatView.tsx`

## Diff And Checkpoint UX

- The app supports per-turn diffs and full-thread diffs. Refs: `packages/contracts/src/orchestration.ts`, `apps/web/src/components/DiffPanel.tsx`
- The diff viewer supports whole-conversation vs per-turn modes. Refs: `apps/web/src/components/DiffPanel.tsx`
- The diff viewer supports file selection within a diff session. Refs: `apps/web/src/components/DiffPanel.tsx`
- The diff viewer supports open-in-editor actions from diff headers. Refs: `apps/web/src/components/DiffPanel.tsx`
- The diff viewer supports stacked and split presentation modes. Refs: `apps/web/src/components/DiffPanel.tsx`
- The diff viewer supports word wrap, and default wrap behavior is configurable in settings. Refs: `apps/web/src/components/DiffPanel.tsx`, `apps/web/src/components/settings/SettingsPanels.tsx`
- The diff viewer supports turn-strip navigation. Refs: `apps/web/src/components/DiffPanel.tsx`
- Turns can carry checkpoint summaries with file-level additions and deletions. Refs: `packages/contracts/src/orchestration.ts`

## Branch, Worktree, Git, And PR UX

- Threads can be bound to either the local workspace or a dedicated Git worktree. Refs: `packages/contracts/src/settings.ts`, `apps/web/src/components/BranchToolbar.tsx`
- The branch toolbar lets users switch environment mode between `Local` and `New worktree`. Refs: `apps/web/src/components/BranchToolbar.tsx`
- The branch toolbar lets users select or change branches. Refs: `apps/web/src/components/BranchToolbar.tsx`
- Changing thread cwd/worktree context can reset or stop the current provider session to avoid running in the wrong workspace. Refs: `apps/web/src/components/BranchToolbar.tsx`, `apps/web/src/components/BranchToolbar.logic.ts`
- Git status tracks whether the current directory is a repo, whether it has an origin remote, whether the branch is the default branch, what the current branch is, whether there are working-tree changes, ahead/behind counts, and linked PR state. Refs: `packages/contracts/src/git.ts`
- Git controls include repo initialization for non-repos. Refs: `apps/web/src/components/GitActionsControl.tsx`, `packages/contracts/src/rpc.ts`
- Git controls include status refresh, pull, push, commit, create PR, and combined stacked flows. Refs: `apps/web/src/components/GitActionsControl.tsx`, `packages/contracts/src/git.ts`
- Supported stacked Git actions are `commit`, `push`, `create_pr`, `commit_push`, and `commit_push_pr`. Refs: `packages/contracts/src/git.ts`
- Git actions stream progress events for phases such as branch, commit, push, and PR, including hook output. Refs: `packages/contracts/src/git.ts`, `apps/web/src/components/GitActionsControl.tsx`
- Git actions expose context-sensitive quick actions plus a fuller action menu. Refs: `apps/web/src/components/GitActionsControl.tsx`
- Commit UX supports optional generated commit messages. Refs: `apps/web/src/components/GitActionsControl.tsx`, `apps/server/src/git/Services/TextGeneration.ts`
- Commit UX supports per-file inclusion/exclusion for changed files. Refs: `apps/web/src/components/GitActionsControl.tsx`
- Git actions can open changed files in the editor. Refs: `apps/web/src/components/GitActionsControl.tsx`, `packages/contracts/src/editor.ts`
- Git safety UX includes default-branch confirmation and an option to check out a feature branch before continuing. Refs: `apps/web/src/components/GitActionsControl.tsx`
- PR URLs can be opened from toasts or Git controls. Refs: `apps/web/src/components/GitActionsControl.tsx`, `apps/web/src/components/Sidebar.tsx`
- The pull-request thread dialog accepts a full PR URL, `gh pr checkout` text, or shorthand like `#123`. Refs: `apps/web/src/components/PullRequestThreadDialog.tsx`
- PR references are resolved live before creating a thread context. Refs: `apps/web/src/components/PullRequestThreadDialog.tsx`, `packages/contracts/src/git.ts`
- PR threads can be prepared in either `local` or `worktree` mode. Refs: `apps/web/src/components/PullRequestThreadDialog.tsx`, `packages/contracts/src/git.ts`
- Git hosting metadata is modeled, with explicit provider kinds `github`, `gitlab`, and `unknown`. Refs: `packages/contracts/src/git.ts`

## Editor And Shell Integration

- The app supports an `Open in` picker that remembers a preferred editor and can use a shortcut to open directly in the favorite editor. Refs: `apps/web/src/components/chat/OpenInPicker.tsx`, `apps/web/src/editorPreferences.ts`
- Supported editor/file-manager targets are `Cursor`, `Trae`, `VS Code`, `VS Code Insiders`, `VSCodium`, `Zed`, `Antigravity`, `IntelliJ IDEA`, and `File Manager`. Refs: `packages/contracts/src/editor.ts`
- Editor launch styles are tailored per editor: direct-path, goto, or line-column. Refs: `packages/contracts/src/editor.ts`
- Shell/external-link opening is routed through the native bridge where available and limited to safe web URLs in desktop builds. Refs: `apps/web/src/wsNativeApi.ts`, `apps/desktop/src/main.ts`

## Terminal UX

- Terminals are scoped per thread. Refs: `packages/contracts/src/terminal.ts`, `apps/web/src/terminalStateStore.ts`
- The thread terminal drawer persists whether it is open and its height. Refs: `apps/web/src/components/ThreadTerminalDrawer.tsx`, `apps/web/src/terminalStateStore.ts`
- A thread can have multiple terminals. Refs: `apps/web/src/components/ThreadTerminalDrawer.tsx`, `apps/web/src/terminalStateStore.ts`
- Terminal split groups are supported. Refs: `apps/web/src/components/ThreadTerminalDrawer.tsx`, `apps/web/src/terminalStateStore.ts`
- The UI tracks an active terminal per thread. Refs: `apps/web/src/terminalStateStore.ts`
- The app keeps a limited set of recently used thread drawers warm-mounted in the background so thread switches feel faster. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/components/ChatView.logic.ts`
- Terminal sessions support explicit open, write, resize, clear, restart, and close actions. Refs: `packages/contracts/src/rpc.ts`, `packages/contracts/src/terminal.ts`
- Terminal IDs are stable and default to `default` when one is not supplied. Refs: `packages/contracts/src/terminal.ts`
- Terminal output supports link detection for web URLs and local file/path references. Refs: `apps/web/src/components/ThreadTerminalDrawer.tsx`, `apps/web/src/terminal-links.ts`
- Terminal links can open externally or in the editor depending on link type. Refs: `apps/web/src/components/ThreadTerminalDrawer.tsx`
- Terminal visuals sync with app theme changes. Refs: `apps/web/src/components/ThreadTerminalDrawer.tsx`, `apps/web/src/hooks/useTheme.ts`
- Terminal resizing is synchronized between UI state and backend PTY size. Refs: `apps/web/src/components/ThreadTerminalDrawer.tsx`, `packages/contracts/src/terminal.ts`
- Terminal keyboard helpers include clear, word navigation, and line-start/line-end navigation. Refs: `apps/web/src/keybindings.ts`, `apps/web/src/components/ThreadTerminalDrawer.tsx`
- Selecting terminal output surfaces an `Add to chat` action that injects the selected lines into the composer as terminal context. Refs: `apps/web/src/components/ThreadTerminalDrawer.tsx`, `apps/web/src/lib/terminalContext.ts`, `apps/web/src/composerDraftStore.ts`
- Captured terminal context becomes both inline chips and appended prompt text during send. Refs: `apps/web/src/lib/terminalContext.ts`, `apps/web/src/components/ChatView.tsx`

## Project Scripts UX

- Projects can define custom scripts/actions attached to the workspace. Refs: `packages/contracts/src/orchestration.ts`, `apps/web/src/components/ProjectScriptsControl.tsx`
- Each project script has an ID, name, command, icon, and `runOnWorktreeCreate` toggle. Refs: `packages/contracts/src/orchestration.ts`
- Supported project-script icons are `play`, `test`, `lint`, `configure`, `build`, and `debug`. Refs: `packages/contracts/src/orchestration.ts`
- Scripts can be added, edited, deleted, and run from the UI. Refs: `apps/web/src/components/ProjectScriptsControl.tsx`, `apps/web/src/components/ChatView.tsx`
- Scripts can be assigned keybindings via generated `script.<id>.run` commands. Refs: `packages/contracts/src/keybindings.ts`, `apps/web/src/projectScripts.ts`, `apps/web/src/components/ProjectScriptsControl.tsx`
- One script can be marked to run automatically when a worktree is created. Refs: `apps/web/src/components/ProjectScriptsControl.tsx`
- The app ensures only one script is marked auto-run for worktree creation at a time. Refs: `apps/web/src/components/ChatView.tsx`
- A preferred or last-invoked script is remembered per project and can be promoted to a primary header action. Refs: `apps/web/src/components/ChatView.tsx`, `apps/web/src/components/ProjectScriptsControl.tsx`

## Settings UX

- Settings are split into `General` and `Archived threads` routes in the sidebar navigation. Refs: `apps/web/src/routes/settings.general.tsx`, `apps/web/src/routes/settings.archived.tsx`, `apps/web/src/components/settings/SettingsSidebarNav.tsx`
- Every mutable settings row supports granular `Reset to default` where the setting is dirty. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- Theme can be set to `System`, `Light`, or `Dark`. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- Time format can be set to `System default`, `12-hour`, or `24-hour`. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `packages/contracts/src/settings.ts`
- `Diff line wrapping` is configurable and controls the default wrap state when the diff panel opens. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- `Assistant output` is configurable and controls whether assistant text is streamed token-by-token during a response. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `packages/contracts/src/settings.ts`
- `New threads` defaults can be set to `Local` or `New worktree`. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `packages/contracts/src/settings.ts`
- `Archive confirmation` controls whether the inline archive action requires a second click before archiving. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- `Delete confirmation` controls whether thread deletion prompts for confirmation. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- The Git text-generation model is configurable separately from chat defaults and is used for generated commit/PR text. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `packages/contracts/src/settings.ts`
- Git text-generation defaults are provider-specific: Codex defaults to `gpt-5.4-mini`; Claude defaults to `claude-haiku-4-5`. Refs: `packages/contracts/src/model.ts`
- Provider settings are shown per provider with live status, version, auth state, and last checked time. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `packages/contracts/src/server.ts`
- Provider status presentation distinguishes `disabled`, `error`, `ready`, and `warning`. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `packages/contracts/src/server.ts`
- Provider settings support manual `Refresh provider status`. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- Providers can be individually enabled or disabled for new sessions. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `packages/contracts/src/settings.ts`
- Provider settings include editable binary paths. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `packages/contracts/src/settings.ts`
- Codex settings additionally expose a `CODEX_HOME` path override. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `packages/contracts/src/settings.ts`
- Provider details list available models and expose capability/tooltips such as `Fast mode`, `Thinking`, and `Reasoning`. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `packages/contracts/src/model.ts`
- Providers support custom model entries in settings, including validation, add/remove flows, and `custom` labeling in the list. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `packages/contracts/src/settings.ts`
- Provider settings can be reset to provider defaults independently. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- Advanced settings expose the persisted `keybindings.json` path and open it in the preferred editor. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- About settings expose the app version and diagnostics/logs folder. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- Desktop builds expose update actions from Settings via the same update-state model used in the sidebar. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`, `apps/web/src/components/desktopUpdate.logic.ts`

## Archived Threads UX

- Archived threads live in a dedicated Settings page rather than mixed into the main thread list. Refs: `apps/web/src/routes/settings.archived.tsx`, `apps/web/src/components/settings/SettingsPanels.tsx`
- Archived threads are grouped by project. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- Each archived thread row shows both archived-relative time and created-relative time. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- Archived threads can be unarchived from a button. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- Archived threads support a context menu with `Unarchive` and destructive `Delete`. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`
- An explicit empty state is shown when there are no archived threads. Refs: `apps/web/src/components/settings/SettingsPanels.tsx`

## Persistence And Local Client State

- The main client store keeps a live read model of projects, threads, summaries, and orchestration state and applies incoming events incrementally. Refs: `apps/web/src/store.ts`
- UI state persists project expansion, project ordering, and last-visited timestamps used for unread detection. Refs: `apps/web/src/uiStateStore.ts`
- Composer drafts persist prompt text, attachments, terminal contexts, provider/model/runtime settings, and per-project draft-thread state. Refs: `apps/web/src/composerDraftStore.ts`
- Sticky provider/model defaults can be carried forward into future drafts. Refs: `apps/web/src/composerDraftStore.ts`
- Terminal UI state persists drawer geometry, terminal groups/tabs, active terminal, recent buffers, and running-terminal indicators. Refs: `apps/web/src/terminalStateStore.ts`
- Thread multi-selection state persists its anchor/current set within the app session. Refs: `apps/web/src/threadSelectionStore.ts`

## Keybindings

- The keybinding system supports both built-in commands and per-project-script commands. Refs: `packages/contracts/src/keybindings.ts`
- Built-in commands are `terminal.toggle`, `terminal.split`, `terminal.new`, `terminal.close`, `diff.toggle`, `chat.new`, `chat.newLocal`, `editor.openFavorite`, `thread.previous`, `thread.next`, and `thread.jump.1` through `thread.jump.9`. Refs: `packages/contracts/src/keybindings.ts`
- Keybindings support `when` expressions with identifiers, `!`, `&&`, `||`, and parentheses. Refs: `packages/contracts/src/keybindings.ts`, `apps/server/src/keybindings.ts`
- Default keybindings are `mod+j` terminal toggle; `mod+d`, `mod+n`, `mod+w` for split/new/close when terminal-focused; `mod+d` diff toggle when not terminal-focused; `mod+n` and `mod+shift+o` for inherited new thread; `mod+shift+n` for local new thread; `mod+o` open favorite editor; `mod+shift+[` and `mod+shift+]` previous/next thread; `mod+1` through `mod+9` jump to threads. Refs: `apps/server/src/keybindings.ts`
- Shortcut labels are formatted per platform, using Mac symbols on macOS and `Ctrl+Alt+Shift+...` style elsewhere. Refs: `apps/web/src/keybindings.ts`

## RPC, Streaming, And Backend Surface

- WebSocket RPC methods cover project registry/search/write, editor open, Git actions/status/branches/worktrees/PR preparation, terminal control, server config/settings/keybindings, orchestration snapshot/dispatch/replay/diff, and streaming subscriptions. Refs: `packages/contracts/src/rpc.ts`
- Streaming subscriptions exist for Git status, orchestration domain events, terminal events, server config changes, and server lifecycle events. Refs: `packages/contracts/src/rpc.ts`
- HTTP serving includes static web hosting, SPA fallback, immutable attachment serving, project favicon lookup, and browser OTLP trace ingest/proxy endpoints. Refs: `apps/server/src/http.ts`, `apps/server/src/attachmentStore.ts`, `apps/server/src/project/Layers/ProjectFaviconResolver.ts`
- The server can require a WebSocket token for auth and instruments RPC calls with tracing/metrics. Refs: `apps/server/src/ws.ts`, `apps/server/src/observability/RpcInstrumentation.ts`

## Provider Runtime And Session Features

- Two providers are implemented: Codex and Claude. Refs: `apps/server/src/provider/Layers/CodexProvider.ts`, `apps/server/src/provider/Layers/ClaudeProvider.ts`
- Provider snapshots include enablement, install state, auth state, version, message text, checked time, and model lists. Refs: `packages/contracts/src/server.ts`, `apps/server/src/provider/Layers/ProviderRegistry.ts`
- Provider snapshots refresh on settings changes and via explicit refresh actions. Refs: `apps/server/src/provider/Layers/ProviderRegistry.ts`, `apps/web/src/components/settings/SettingsPanels.tsx`
- The server persists thread-to-provider session bindings and supports resume/recovery of sessions from stored resume state. Refs: `apps/server/src/provider/Layers/ProviderService.ts`, `apps/server/src/provider/Layers/ProviderSessionDirectory.ts`
- The provider command reactor auto-starts or restarts sessions when model/runtime changes require it, starts turns, interrupts turns, handles approval responses, handles user-input answers, and stops sessions. Refs: `apps/server/src/orchestration/Layers/ProviderCommandReactor.ts`
- If backend provider actions fail, the reactor emits visible thread activities so the failure appears in the thread instead of disappearing silently. Refs: `apps/server/src/orchestration/Layers/ProviderCommandReactor.ts`
- First-turn polish includes automatic thread-title generation and worktree branch-name generation from the first user message. Refs: `apps/server/src/orchestration/Layers/ProviderCommandReactor.ts`
- Stale approval or user-input callbacks after restart are converted into explicit restart-the-turn activities. Refs: `apps/server/src/orchestration/Layers/ProviderCommandReactor.ts`
- Runtime ingestion maps provider-native events into canonical UI-facing events including session lifecycle, turn lifecycle, assistant deltas, approvals, user-input prompts, warnings/errors, token usage, tool activity, reasoning/task progress, and proposed-plan tracking. Refs: `apps/server/src/orchestration/Layers/ProviderRuntimeIngestion.ts`, `packages/contracts/src/providerRuntime.ts`
- The server guards against stale or out-of-order provider lifecycle events so the UI state remains coherent. Refs: `apps/server/src/orchestration/Layers/ProviderRuntimeIngestion.ts`
- Assistant delivery mode supports both buffered and streaming UX. Refs: `packages/contracts/src/orchestration.ts`, `apps/server/src/orchestration/Layers/ProviderRuntimeIngestion.ts`, `packages/contracts/src/settings.ts`

## Codex-Specific Features

- Codex provider checks CLI availability/version and reports installation failures as provider status. Refs: `apps/server/src/provider/Layers/CodexProvider.ts`, `apps/server/src/provider/codexCliVersion.ts`
- Codex can skip OpenAI login checks when the selected configuration implies a custom model provider. Refs: `apps/server/src/provider/Layers/CodexProvider.ts`
- Codex probes account/subscription state via `codex app-server` and adjusts model availability accordingly. Refs: `apps/server/src/provider/codexAppServer.ts`, `apps/server/src/provider/codexAccount.ts`, `apps/server/src/provider/Layers/CodexProvider.ts`
- Codex sessions support start, resume, model switching, reasoning effort, fast mode, image attachments as data URLs, interrupt, approvals, structured user input, thread read, and rollback. Refs: `apps/server/src/provider/Layers/CodexAdapter.ts`, `apps/server/src/codexAppServerManager.ts`


## Git, Worktree, And PR Backend Features

- The server exposes repo status, ff-only pull, branch listing, worktree creation/removal, branch creation, checkout, repo init, PR resolution, and PR-thread preparation. Refs: `packages/contracts/src/rpc.ts`, `packages/contracts/src/git.ts`
- Branch listing includes remote/default/current/worktree metadata. Refs: `packages/contracts/src/git.ts`, `apps/server/src/git/Layers/GitCore.ts`
- Git status subscriptions push an initial snapshot plus later local/remote updates, with remote polling while subscribers exist and deduped push-on-change semantics. Refs: `packages/contracts/src/git.ts`, `apps/server/src/git/Layers/GitStatusBroadcaster.ts`
- PR-thread preparation can reuse an existing PR worktree when possible. Refs: `apps/server/src/git/Layers/GitManager.ts`
- PR-thread preparation can optionally run project setup scripts after preparing the worktree/local context. Refs: `apps/server/src/git/Layers/GitManager.ts`, `apps/server/src/project/Layers/ProjectSetupScriptRunner.ts`
- Stacked Git actions include dirty-tree and detached-HEAD guards, optional auto feature-branch creation, text-generation-assisted branch/commit/PR text, and completion toasts/CTAs. Refs: `apps/server/src/git/Layers/GitManager.ts`, `packages/contracts/src/git.ts`

## Terminal Backend Features

- The terminal backend supports reusable terminal IDs, history restoration, multiple terminals per thread, per-thread close-all, delete-history-on-close, and emitted `started`, `output`, `exited`, `error`, `cleared`, `restarted`, and `activity` events. Refs: `apps/server/src/terminal/Layers/Manager.ts`, `packages/contracts/src/terminal.ts`
- Terminal runtime validates cwd, normalizes environment, injects runtime env, blocks problematic env vars, retries alternate shells, caps retained history, evicts old inactive sessions, and escalates process termination when needed. Refs: `apps/server/src/terminal/Layers/Manager.ts`
- Background subprocess activity is detected and surfaced as terminal activity events. Refs: `apps/server/src/terminal/Layers/Manager.ts`, `packages/contracts/src/terminal.ts`
- Terminal input constraints are explicit: cols `20-400`, rows `5-200`, max write payload `65536` characters, bounded env variable names/values, and max 128 env vars. Refs: `packages/contracts/src/terminal.ts`

## Workspace And Project Backend Features

- Workspace search powers the composer/path picker with fuzzy file and directory search. Refs: `apps/server/src/workspace/Layers/WorkspaceEntries.ts`, `packages/contracts/src/project.ts`
- Workspace search uses caching, respects large-directory ignores, prefers Git-aware indexing via `git ls-files`, respects `.gitignore`, and can report truncation. Refs: `apps/server/src/workspace/Layers/WorkspaceEntries.ts`
- Workspace file writing is supported through RPC and is restricted to the workspace root, creating missing parent directories automatically. Refs: `apps/server/src/workspace/Layers/WorkspaceFileSystem.ts`, `packages/contracts/src/project.ts`
- Workspace writes invalidate cached search indexes/checkpointing expectations so follow-up UI search stays current. Refs: `apps/server/src/workspace/Layers/WorkspaceEntries.ts`, `apps/server/src/workspace/Layers/WorkspaceFileSystem.ts`
- Project setup scripts can be run automatically in terminals for bootstrap turns, worktree creation, and PR-thread preparation, and the terminal ID is fed back into orchestration activity. Refs: `apps/server/src/project/Layers/ProjectSetupScriptRunner.ts`, `apps/server/src/ws.ts`, `apps/server/src/git/Layers/GitManager.ts`
- Project favicon resolution is implemented server-side with fallback behavior. Refs: `apps/server/src/project/Layers/ProjectFaviconResolver.ts`, `apps/server/src/http.ts`

## Server Lifecycle, Config, And Observability

- Server startup exposes lifecycle stream events including `welcome` and `ready`. Refs: `packages/contracts/src/server.ts`, `apps/server/src/serverLifecycleEvents.ts`, `packages/contracts/src/rpc.ts`
- Commands are queued until startup readiness so the app can connect before the server is fully ready. Refs: `apps/server/src/serverRuntimeStartup.ts`
- The server can auto-bootstrap a project/thread from cwd on startup. Refs: `apps/server/src/serverRuntimeStartup.ts`, `apps/web/src/routes/__root.tsx`
- Archiving a thread auto-closes its terminals. Refs: `apps/server/src/ws.ts`
- Thread bootstrap from dispatch can create a thread, create a worktree, update thread metadata, run a setup script terminal, append setup-script activities, and clean up the thread if bootstrap fails. Refs: `apps/server/src/ws.ts`
- Server settings are file-backed, streamable, patchable over RPC, and hot-reload on external edits. Refs: `apps/server/src/serverSettings.ts`, `packages/contracts/src/rpc.ts`
- Keybindings are file-backed, streamable, upsertable, validated, merged with defaults, and resilient to external edits. Refs: `apps/server/src/keybindings.ts`, `packages/contracts/src/rpc.ts`
- If the selected Git text-generation provider becomes disabled, settings fall back to a valid default provider/model. Refs: `apps/server/src/serverSettings.ts`
- Observability is first-class: local trace files, optional OTLP traces/metrics, browser trace collection, RPC metrics, git/provider/terminal metrics, startup heartbeat analytics, and per-thread provider logs are all present. Refs: `apps/server/src/observability/Layers/Observability.ts`, `apps/server/src/telemetry/Layers/AnalyticsService.ts`, `apps/server/src/provider/Layers/EventNdjsonLogger.ts`

## Desktop Shell Features

- The desktop app exposes a native directory picker to the renderer and allows both selecting and creating directories. Refs: `apps/desktop/src/main.ts`, `apps/desktop/src/preload.ts`
- The desktop app exposes a native confirmation dialog bridge to the renderer. Refs: `apps/desktop/src/main.ts`, `apps/desktop/src/preload.ts`, `apps/desktop/src/confirmDialog.ts`
- Native confirmation uses a Yes/No dialog with `No` as default and cancel, and whitespace-only messages resolve to `false`. Refs: `apps/desktop/src/confirmDialog.ts`
- The renderer can set Electron theme source to `light`, `dark`, or `system`. Refs: `apps/desktop/src/main.ts`, `apps/desktop/src/preload.ts`
- The renderer can request native context menus with normal, disabled, and destructive items; destructive items are visually separated and receive a trash icon on macOS where available. Refs: `apps/desktop/src/main.ts`, `apps/desktop/src/preload.ts`
- External links from the app are opened in the OS browser rather than a new in-app window, and only `http`/`https` protocols are allowed. Refs: `apps/desktop/src/main.ts`
- Native app menus exist for File, Edit, View, Window, and Help; macOS also gets the standard app menu with About, Services, Hide, and Quit. Refs: `apps/desktop/src/main.ts`
- Native menu items can dispatch renderer actions such as opening Settings, creating/focusing a window first if needed. Refs: `apps/desktop/src/main.ts`
- The desktop window uses a hidden-inset title bar, fixed app title, minimum size, hidden menu bar by default, and show-on-ready behavior. Refs: `apps/desktop/src/main.ts`
- The app includes a native text-edit context menu with Cut, Copy, Paste, Select All, spelling suggestions, and a `No suggestions` fallback. Refs: `apps/desktop/src/main.ts`
- On macOS, activating the app recreates a window if all windows are closed; on non-macOS, closing all windows quits the app. Refs: `apps/desktop/src/main.ts`
- Fatal desktop startup errors are shown in a blocking native error box titled `T3 Code failed to start`. Refs: `apps/desktop/src/main.ts`
- The Electron userData directory is normalized to a shell-friendly lowercase directory name while preserving legacy data locations if they already exist. Refs: `apps/desktop/src/main.ts`
- Platform identity integration includes Windows App User Model ID, Linux desktop entry name, and macOS dock icon. Refs: `apps/desktop/src/main.ts`
- On macOS and Linux the app syncs login-shell environment variables such as `PATH` and `SSH_AUTH_SOCK` so spawned tooling behaves more like a terminal session. Refs: `apps/desktop/src/syncShellEnvironment.ts`, `apps/desktop/src/main.ts`

## Desktop Update UX

- The desktop preload bridge exposes `getUpdateState`, `onUpdateState`, `checkForUpdate`, `downloadUpdate`, and `installUpdate` to the renderer. Refs: `apps/desktop/src/preload.ts`, `apps/desktop/src/main.ts`
- Update states are `disabled`, `checking`, `available`, `up-to-date`, `downloading`, `downloaded`, and `error`. Refs: `apps/desktop/src/updateMachine.ts`
- Update state includes version fields, progress, message/error context, and retry affordances. Refs: `apps/desktop/src/updateMachine.ts`
- Automatic updates are disabled in dev/non-packaged builds, when `T3CODE_DISABLE_AUTO_UPDATE=1`, and on Linux unless the build is an AppImage. Refs: `apps/desktop/src/updateState.ts`, `apps/desktop/src/main.ts`
- The menu includes `Check for Updates...`; when updates are unavailable, it explains why; when current, it shows `You're up to date!`; when check fails, it shows `Update check failed`. Refs: `apps/desktop/src/main.ts`
- Automatic update checks are scheduled once 15 seconds after startup and then every 4 hours. Refs: `apps/desktop/src/main.ts`
- Downloads are manual rather than automatic, and install-on-quit is disabled. Refs: `apps/desktop/src/main.ts`
- Download progress notifications to the renderer are throttled to 10% increments, plus 100%, to avoid noisy UI updates. Refs: `apps/desktop/src/updateState.ts`, `apps/desktop/src/main.ts`
- Installing an update shuts down the backend, destroys windows, and then hands off to the updater. Refs: `apps/desktop/src/main.ts`
- If install handoff fails, the app stays in `downloaded` state with retry enabled. Refs: `apps/desktop/src/main.ts`
- Apple Silicon migration is explicitly handled: when running an Intel build under Rosetta on arm64 macOS, update behavior prefers arm64 packages and disables differential download. Refs: `apps/desktop/src/runtimeArch.ts`, `apps/desktop/src/main.ts`
- The web UI surfaces desktop update state via sidebar/settings pills, install/download/restart copy, dismissal-until-relaunch behavior, and a special `Intel build under Rosetta` warning. Refs: `apps/web/src/components/sidebar/SidebarUpdatePill.tsx`, `apps/web/src/components/desktopUpdate.logic.ts`, `apps/web/src/components/Sidebar.tsx`

## Marketing Site And Download UX

- The marketing homepage uses a dark, minimalist shell with logo, GitHub nav link, footer GitHub link, footer Discord link, subtle fade-in animation, and responsive spacing. Refs: `apps/marketing/src/layouts/Layout.astro`
- The homepage hero line is `T3 Code is the best way to code with AI.` Refs: `apps/marketing/src/pages/index.astro`
- The primary CTA defaults to GitHub Releases and is labeled `Download now`. Refs: `apps/marketing/src/pages/index.astro`
- The homepage CTA performs client-side platform detection from `navigator.userAgent` and rewrites its label to `Download for Windows`, `Download for macOS`, or `Download for Linux`. Refs: `apps/marketing/src/pages/index.astro`
- The homepage CTA swaps the platform icon to Apple, Windows, or Linux based on the detected OS. Refs: `apps/marketing/src/pages/index.astro`
- Platform-specific asset selection prefers Windows `-x64.exe`, macOS `-arm64.dmg` with fallback to any `.dmg`, and Linux `.AppImage`. Refs: `apps/marketing/src/pages/index.astro`
- If release lookup fails on the homepage, the CTA falls back to the GitHub Releases page. Refs: `apps/marketing/src/pages/index.astro`, `apps/marketing/src/lib/releases.ts`
- The homepage includes an `Other platforms` link to the dedicated downloads page. Refs: `apps/marketing/src/pages/index.astro`
- The homepage centers product imagery around a screenshot-led presentation with animated entrance. Refs: `apps/marketing/src/pages/index.astro`
- The downloads page exposes explicit cards for macOS Apple Silicon `.dmg`, macOS Intel `.dmg`, Windows x64 `.exe`, and Linux x86_64 `AppImage`. Refs: `apps/marketing/src/pages/download.astro`
- The downloads page shows `Loading latest release...` until release data loads, then updates to `Latest (<tag>)`, or `Could not load release info.` on failure. Refs: `apps/marketing/src/pages/download.astro`
- The downloads page shows a `View changelog` link only when the release has an `html_url`. Refs: `apps/marketing/src/pages/download.astro`
- Each download card resolves release assets by suffix and falls back to GitHub Releases if a matching asset is missing. Refs: `apps/marketing/src/pages/download.astro`
- The downloads page includes an explicit path to older versions via the GitHub Releases page. Refs: `apps/marketing/src/pages/download.astro`
- The downloads page uses a two-column card layout on larger screens and a one-column layout on mobile. Refs: `apps/marketing/src/pages/download.astro`
- Release metadata is fetched from `https://api.github.com/repos/pingdotgg/t3code/releases/latest` and cached in `sessionStorage` under `t3code-latest-release`. Refs: `apps/marketing/src/lib/releases.ts`
