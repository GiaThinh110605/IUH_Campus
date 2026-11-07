# Selected gamedev skills — S1

- Source: [gamedev-skills/awesome-gamedev-agent-skills](https://github.com/gamedev-skills/awesome-gamedev-agent-skills).
- Pinned source commit: `19d93f97c9c1ea0f8b306e77aa81e0d3e352d3e9`.
- Imported: 2026-11-07 (Asia/Bangkok). License: Apache-2.0; upstream LICENSE and NOTICE are included unchanged.
- **rules dự án ưu tiên hơn skill**: follow [coding standards](../../rules/coding-standards.md), [match-3 combat](../../rules/match3-combat.md), [Unity standards](../../rules/unity-standards.md), and [backend/cloud rules](../../rules/w9-backend-cloud.md).
- Manual copy only: shallow-cloned outside the project; read all four SKILL.md files and their four reference files. No upstream script, installer, npx, or external-code command was executed. Reviewed files contain no download/execution command or instruction to override local rules/access secrets. Code fences are examples, not commands to run automatically.

## Included skills and project constraints

| Skill (upstream location) | Summary | Constraints when applying to IUH Campus |
|---|---|---|
| game-feel (`skills/disciplines/game-feel`) | Layered feedback: shake/trauma, hit-stop, eased pop, particles/audio, accessibility. | Never let feedback bypass combat input locks or alter deterministic board logic. A global Time.timeScale change can affect other mini-games; scope effects to Guardian. Sample constants/comments are illustrative: use configured presets, pooling, and project comment rules. The simple Unity hit-stop sample resets to 1 and has no overlap/previous-scale ownership guard; adapt before use. |
| puzzle (`skills/genres/puzzle`) | Pure board state, legal moves, cascades, seeded RNG, deadlocks, solvability, undo. | Board/view separation and input lock agree with project rules. Undo, new scoring formulas, time/move limits, and special pieces are suggestions, not permission to add features. Preserve the existing player/monster turn cycle and configured HP/ATK/DEF; do not apply pseudocode scores/magic numbers directly. |
| game-ui-ux (`skills/disciplines/game-ui-ux`) | Responsive anchors/layouts, scaling, safe area, focus, screen stack, event-driven HUD. | Do not replace common menus/navigation/Canvas without authorization. Keep Landscape, EventSystem pointer/drag handlers and decorative Raycast Target policy. Screen-stack/localization/accessibility proposals stay within requested Guardian scope; do not automatically migrate UI frameworks or input modules. |
| unity-scriptableobjects (`skills/unity/unity-scriptableobjects`) | Config assets, runtime variables, event channels, runtime sets; cleanup and domain-reload reset. | Config assets fit combat rules. Examples lack project namespace and use sample hardcoded values/Fire logging: adapt into Guardian and avoid frame/cascade log spam. Keep board logic independent of Unity objects; SOs hold configuration, not player saves or shared mutable per-enemy HP. |

All four directories, including `references/`, are copied unchanged; this README supplies local constraints without rewriting upstream instructions. Related skills named by upstream are not installed by this import.

## Unity 6000.5 compatibility review

Project: `6000.5.8f1` in ProjectSettings/ProjectVersion.txt; Packages/manifest.json pins Cinemachine `3.1.7`, Input System `1.20.0`, uGUI `2.5.0`. Upstream examples target Unity 6.3 LTS (`6000.3`) and often Godot 4.7; they are not proof of compilation on this project.

- **game-feel:** Godot Camera2D/create_tween/Engine.time_scale are not Unity APIs. Cinemachine is package-version sensitive: in 3.1 use namespace `Unity.Cinemachine`, `CinemachineCamera`, and `CinemachineBasicMultiChannelPerlin.AmplitudeGain/FrequencyGain`, not old 2.x `m_` fields/namespace. [Cinemachine 3.1 reference](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/api/Unity.Cinemachine.CinemachineBasicMultiChannelPerlin.html). `WaitForSecondsRealtime` exists in [6000.5](https://docs.unity3d.com/6000.5/Documentation/ScriptReference/WaitForSecondsRealtime.html); pause ownership/overlap is a sample limitation, not an API removal. Never assume a third-party tween package is installed.
- **puzzle:** Python snippets are incomplete engine-neutral pseudocode, not runnable Unity C#. Translate board indexing/RNG/state snapshots to the project's model; no concrete Unity API mismatch identified.
- **game-ui-ux:** CanvasScaler/RectTransform/EventSystem are uGUI; PanelSettings/VisualElement.Focus are UI Toolkit. Do not mix their APIs. Upstream links Input System 1.14 while this project uses 1.20.0. “UI Toolkit needs no module” is only applicable without a scene uGUI EventSystem: mixed uGUI/Toolkit requires the matching input module. See [Unity 6000.5 runtime UI event system](https://docs.unity3d.com/6000.5/Documentation/Manual/UIE-Runtime-Event-System.html). Verify Active Input Handling and scene composition before changing modules; safe-area anchors must use the actual canvas coordinate system.
- **unity-scriptableobjects:** ScriptableObject/CreateAssetMenu/CreateInstance and editor playModeStateChanged patterns require project compilation/Play Mode checks before adoption; no confirmed removed API identified. Keep UnityEditor references behind UNITY_EDITOR and obey asmdef boundaries. Domain Reload disabled needs explicit reset; OnEnable alone is insufficient. Generic event channels and runtime sets need lifecycle/null/unsubscription review; imported snippets are not a validated library.

Review is documentation/package based; no Unity scripts were compiled, no scene changed, and no claim of runtime compatibility testing is made.

Project rule links: [coding](../../rules/coding-standards.md), [combat](../../rules/match3-combat.md), [Unity](../../rules/unity-standards.md), [backend](../../rules/w9-backend-cloud.md).
