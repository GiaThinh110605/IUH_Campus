# Cốt truyện "Đêm Glitch" — Kế hoạch thực hiện

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Thêm một "đạo diễn cốt truyện" dẫn người chơi qua Mở đầu → Biến cố → 3 chương giành lại khu → Trùm → Kết, chỉ bật đúng quái của chương hiện tại.

**Architecture:** Logic chương là lớp C# thuần `StoryProgress` (asmdef không engine, test EditMode). Kịch bản là ScriptableObject `IUHStoryScript` (asmdef riêng). `IUHStoryDirector` (Assembly-CSharp, vì cần các kiểu của Thịnh) đặt trên 1 object `STORY_DIRECTOR` trong scene: khóa mọi nguồn quái lúc đầu, mở từng encounter theo chương, phát hiện thắng khi encounter tự `SetActive(false)`, khôi phục mọi thứ khi phá đảo. Không sửa file nào của Thịnh.

**Tech Stack:** Unity 6000.5.8f1, URP, uGUI (`UnityEngine.UI.Text`, font `LegacyRuntime.ttf`), Unity Test Framework (EditMode), PlayerPrefs.

**Spec:** `Docs/superpowers/specs/2026-11-16-dem-glitch-story-design.md`

## Global Constraints

- Nhánh `minhnhat/story-dem-glitch` tạo từ `origin/main`. Không commit vào `main`, không push khi chưa được yêu cầu, stage chọn lọc (không `git add -A`/`.`), không stage `*.csproj`, `*.slnx`, `Logs/`, `ProjectSettings/` do Unity tự ghi.
- **Không sửa** bất kỳ file nào sẵn có trong `Scripts/Combat/`, `Scripts/Guardian/*.cs`, `Scripts/Guardian/AR/`, `Scripts/UI/`. Chỉ thêm file mới trong `Assets/IUH_Campus/Scripts/Guardian/Story/`, `Assets/IUH_Campus/Data/Story/`, `Assets/IUH_Campus/Materials/Story/`.
- Scene `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity` thuộc Gia Thịnh: **chỉ** thêm 1 GameObject gốc `STORY_DIRECTOR`, **sau khi** người dùng xác nhận Thịnh đã đồng ý (Task 5). Không sửa object có sẵn.
- Tắt object `STORY_DIRECTOR` ⇒ game chạy y như trước (tiêu chí bắt buộc).
- Gold chỉ đến từ phần thưởng sẵn có của `IUHMonsterEncounter`; director không cộng Gold.
- Không dùng `Shader.Find` (bị strip trên iOS): mọi vật liệu là asset tham chiếu qua `IUHStoryScript`.
- Không `GameObject.Find`/`FindObjectsByType`/`Camera.main` trong `Update` — chỉ trong `Start`/khi đổi chương, rồi cache.
- Chạy cả PC và iPhone (không AR) ngang: khung thoại qua được bằng click/chạm/Enter/Space; nút "Bỏ qua" bấm được bằng chạm; chữ ≥ 30 px ở reference 1920×1080; không che `IUH_MOBILE_CONTROLS_CANVAS` (thoại ở **giữa-trên** màn hình, mục tiêu ở **góc trên-trái**).
- Khóa lưu: tiền tố `IUH_Story_` (PlayerPrefs). Mọi chữ hiển thị bằng tiếng Việt có dấu.
- Kết thúc mỗi task: Unity compile xong, Console 0 error, Missing Script = 0.
- Bằng chứng: `Docs/Evidence/CampusGuardian/STORY/` (ảnh + `README.md` ghi commit, ngày, môi trường).

## Review Focus

1. **Thoát game giữa chương rồi vào lại** → vào đúng chương, quái đã thắng trong chương đó không hồi sinh, quái chương trước không hiện. (Task 1: `Deserialize_RestoresDefeatedIds`; Task 6 bước kiểm tay.)
2. **Thua trận match-3** → encounter vẫn còn, chương không chuyển, không bị tính là thắng (encounter không tắt khi thua). (Task 4: chỉ tính thắng khi encounter đã từng `activeSelf==true` do director bật rồi chuyển sang `false`; Task 6 kiểm tay.)
3. **Tên object trong kịch bản sai/không có trong scene** → không kẹt im lặng. (Task 2: test `StoryScript_AllNamesExistInScene`; Task 4: director `Debug.LogError` liệt kê tên thiếu và coi encounter thiếu là đã thắng để không kẹt.)
4. **Dữ liệu lưu hỏng/khác phiên bản** → bắt đầu lại từ đầu, không exception. (Task 1: `Deserialize_GarbageReturnsFresh`.)
5. **Các hệ của Thịnh tự bật quái lại** (phím K của `IUHCampusZoneDirector`, ambush khi đi vào bán kính) → không xảy ra trong lúc truyện vì component của chúng bị `enabled=false`. (Task 4; Task 6 kiểm tay: đi qua cả 4 vị trí ambush và bấm K trước biến cố, không có quái.)

---

### Task 1: `StoryProgress` — máy trạng thái thuần + test

**Files:**
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/Core/IUHCampus.Guardian.Story.Core.asmdef` (`"name": "IUHCampus.Guardian.Story.Core"`, `"rootNamespace": "IUHCampus.Guardian.Story"`, `"noEngineReferences": true`)
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/Core/StoryProgress.cs`
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/Tests/Editor/IUHCampus.Guardian.Story.Tests.asmdef` (theo mẫu `Guardian/Match3/Tests/Editor/IUHCampus.Guardian.Match3.Tests.asmdef`, references `IUHCampus.Guardian.Story.Core`)
- Test: `Assets/IUH_Campus/Scripts/Guardian/Story/Tests/Editor/StoryProgressTests.cs`

**Interfaces:**
- Produces (namespace `IUHCampus.Guardian.Story`):
  - `public enum StoryPhase { Prologue, Incident, Chapter, Ending, Completed }`
  - `public sealed class StoryChapterDef { public StoryChapterDef(string id, IReadOnlyList<string> encounterIds); public string Id; public IReadOnlyList<string> EncounterIds; }`
  - `public sealed class StoryProgress`
    - `StoryProgress(int checkInCount, IReadOnlyList<StoryChapterDef> chapters)` — bắt đầu `Phase=Prologue`, `CheckInsDone=0`, `ChapterIndex=0`.
    - `StoryPhase Phase { get; }`, `int CheckInsDone { get; }`, `int ChapterIndex { get; }`, `StoryChapterDef CurrentChapter { get; }` (null nếu không ở `Chapter`)
    - `bool CompleteCheckIn(int index)` — chỉ nhận khi `Phase==Prologue && index==CheckInsDone`; check-in cuối ⇒ `Phase=Incident`.
    - `void FinishIncident()` — `Incident` ⇒ `Chapter`, `ChapterIndex=0`; phase khác: không làm gì.
    - `bool MarkEncounterDefeated(string id)` — chỉ nhận id thuộc chương hiện tại, chưa thắng; trả `true` khi việc này làm **hoàn thành chương**. Chương cuối xong ⇒ `Phase=Ending`, ngược lại `ChapterIndex++`.
    - `bool IsDefeated(string id)`; `IReadOnlyCollection<string> DefeatedIds { get; }`
    - `void FinishEnding()` — `Ending` ⇒ `Completed`.
    - `string Serialize()`; `static StoryProgress Deserialize(string data, int checkInCount, IReadOnlyList<StoryChapterDef> chapters)` — định dạng `v1|<phase>|<chapterIndex>|<checkInsDone>|<id1>,<id2>`; null/rỗng/sai định dạng/chỉ số vượt giới hạn ⇒ bản mới; id không có trong kịch bản bị bỏ.

- [ ] **Step 1: Viết test (thất bại)** — `StoryProgressTests.cs`, dùng kịch bản mẫu 3 check-in, chương `e:[A,B]`, `canteen:[C]`, `boss:[D]`:
```csharp
[Test] public void CheckIns_MustBeInOrder() { var p = New(); Assert.IsFalse(p.CompleteCheckIn(1)); Assert.IsTrue(p.CompleteCheckIn(0)); Assert.AreEqual(1, p.CheckInsDone); }
[Test] public void LastCheckIn_GoesToIncident() { var p = New(); p.CompleteCheckIn(0); p.CompleteCheckIn(1); p.CompleteCheckIn(2); Assert.AreEqual(StoryPhase.Incident, p.Phase); }
[Test] public void Encounters_IgnoredBeforeChapters() { var p = New(); Assert.IsFalse(p.MarkEncounterDefeated("A")); Assert.IsFalse(p.IsDefeated("A")); }
[Test] public void Encounter_FromOtherChapter_Ignored() { var p = AtChapter0(); Assert.IsFalse(p.MarkEncounterDefeated("C")); Assert.AreEqual(0, p.ChapterIndex); }
[Test] public void Chapter_CompletesOnlyWhenAllDefeated() { var p = AtChapter0(); Assert.IsFalse(p.MarkEncounterDefeated("A")); Assert.IsFalse(p.MarkEncounterDefeated("A")); Assert.IsTrue(p.MarkEncounterDefeated("B")); Assert.AreEqual(1, p.ChapterIndex); }
[Test] public void LastChapter_GoesToEnding_ThenCompleted() { var p = AtChapter0(); p.MarkEncounterDefeated("A"); p.MarkEncounterDefeated("B"); p.MarkEncounterDefeated("C"); Assert.IsTrue(p.MarkEncounterDefeated("D")); Assert.AreEqual(StoryPhase.Ending, p.Phase); p.FinishEnding(); Assert.AreEqual(StoryPhase.Completed, p.Phase); }
[Test] public void Deserialize_RestoresDefeatedIds() { var p = AtChapter0(); p.MarkEncounterDefeated("A"); var q = StoryProgress.Deserialize(p.Serialize(), 3, Chapters()); Assert.AreEqual(StoryPhase.Chapter, q.Phase); Assert.AreEqual(0, q.ChapterIndex); Assert.IsTrue(q.IsDefeated("A")); Assert.IsFalse(q.IsDefeated("B")); }
[TestCase(null)][TestCase("")][TestCase("garbage")][TestCase("v1|Chapter|99|3|")][TestCase("v9|Ending|0|0|")]
public void Deserialize_GarbageReturnsFresh(string s) { var q = StoryProgress.Deserialize(s, 3, Chapters()); Assert.AreEqual(StoryPhase.Prologue, q.Phase); Assert.AreEqual(0, q.CheckInsDone); }
[Test] public void Deserialize_DropsUnknownIds() { var q = StoryProgress.Deserialize("v1|Chapter|0|3|A,ZZZ", 3, Chapters()); Assert.IsTrue(q.IsDefeated("A")); Assert.IsFalse(q.IsDefeated("ZZZ")); }
```
(`New()`, `AtChapter0()` = 3 check-in + `FinishIncident()`, `Chapters()` là helper trong file test.)

- [ ] **Step 2: Chạy test, xác nhận thất bại** — Unity Test Runner → EditMode → `IUHCampus.Guardian.Story.Tests` (hoặc MCP `run_tests` mode EditMode, filter `StoryProgressTests`). Mong đợi: lỗi compile / FAIL vì chưa có `StoryProgress`.
- [ ] **Step 3: Cài đặt `StoryProgress.cs`** theo Interfaces ở trên (dùng `HashSet<string>` cho id đã thắng; `Serialize` sắp xếp id để ổn định).
- [ ] **Step 4: Chạy lại, mong đợi 13 kết quả PASS** (8 test đơn + 5 case của `Deserialize_GarbageReturnsFresh`).
- [ ] **Step 5: Commit** — kèm spec + plan:
```bash
git add Docs/superpowers/specs/2026-11-16-dem-glitch-story-design.md Docs/superpowers/plans/2026-11-16-dem-glitch-story.md Assets/IUH_Campus/Scripts/Guardian/Story.meta Assets/IUH_Campus/Scripts/Guardian/Story/Core Assets/IUH_Campus/Scripts/Guardian/Story/Core.meta Assets/IUH_Campus/Scripts/Guardian/Story/Tests Assets/IUH_Campus/Scripts/Guardian/Story/Tests.meta
git commit -m "feat(story): add StoryProgress chapter state machine with tests"
```

### Task 2: Kịch bản `IUHStoryScript` + asset dữ liệu + vật liệu

**Files:**
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/Data/IUHCampus.Guardian.Story.Data.asmdef` (references `IUHCampus.Guardian.Story.Core`, engine bật)
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/Data/IUHStoryScript.cs`
- Create: `Assets/IUH_Campus/Data/Story/StoryScript_DemGlitch.asset`
- Create: `Assets/IUH_Campus/Materials/Story/M_Story_CorruptionFog.mat` (URP/Unlit, Transparent, màu `#8A2BE2` alpha 0.35), `M_Story_Beacon.mat` (URP/Unlit, màu `#19F2FF`). LED Ngày hội không cần vật liệu mới (đổi bằng MaterialPropertyBlock, Task 4).
- Test: `Assets/IUH_Campus/Scripts/Guardian/Story/Tests/Editor/StoryScriptSceneTests.cs` (thêm reference `IUHCampus.Guardian.Story.Data` vào asmdef test)

**Interfaces:**
- Consumes: `StoryChapterDef` (Task 1).
- Produces (namespace `IUHCampus.Guardian.Story`):
```csharp
[CreateAssetMenu(menuName = "IUH/Story Script")]
public sealed class IUHStoryScript : ScriptableObject {
  [Serializable] public struct CameraShot { public Vector3 position; public Vector3 lookAt; public float seconds; }
  [Serializable] public sealed class CheckInStep { public string checkInObjectName; public string objective; public string[] lines; }
  [Serializable] public sealed class Chapter { public string id; public string title; public string objective; public string[] introLines; public string[] encounterObjectNames; public string[] purifiedLines; }
  public string introCaption; public CameraShot[] introShots; public string[] prologueLines;
  public CheckInStep[] checkIns; public float checkInRadius = 4f;
  public string[] incidentLines; public Chapter[] chapters; public string[] endingLines; public string endingCaption;
  public Material fogMaterial; public Material beaconMaterial; public string festivalLedMaterialName = "Festival LED";
  public IReadOnlyList<StoryChapterDef> ToChapterDefs();
}
```

- [ ] **Step 1: Viết test (thất bại)** `StoryScriptSceneTests.cs`:
```csharp
[Test] public void StoryScript_AllNamesExistInScene() {
  var script = AssetDatabase.LoadAssetAtPath<IUHStoryScript>("Assets/IUH_Campus/Data/Story/StoryScript_DemGlitch.asset");
  Assert.NotNull(script);
  EditorSceneManager.OpenScene("Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity", OpenSceneMode.Additive);
  // gom tên mọi GameObject (kể cả inactive) của scene đó; mọi checkInObjectName và encounterObjectNames phải có, và mỗi tên đúng 1 object
  ...; CloseScene(..., true) trong TearDown; không lưu scene.
}
[Test] public void StoryScript_ChapterIdsUniqueAndEncounterNamesUnique() { /* không trùng id, một encounter không thuộc 2 chương, tên không chứa ',' hay '|' */ }
[Test] public void StoryScript_MaterialsAssigned() { Assert.NotNull(script.fogMaterial); Assert.NotNull(script.beaconMaterial); }
```
- [ ] **Step 2: Chạy, xác nhận FAIL** (chưa có kiểu/asset).
- [ ] **Step 3: Viết `IUHStoryScript.cs`, tạo vật liệu, tạo asset** với nội dung chính xác sau:

| Trường | Giá trị |
|---|---|
| `introCaption` | `Đại học Công nghiệp TP.HCM (IUH) — 3 ngày trước Ngày hội IUH` |
| `introShots` | 4 shot, mỗi shot 4 s, nhìn xuống lần lượt: cổng chính (`CHECKIN_SPOT_GATE` ≈ (0,0,-32)), đài phun nước (≈ (0,0,-4)), khu Ngày hội (≈ (16,0,3)), sân đấu phía bắc (≈ (0,0,22)); người thực hiện chọn `position` cao 15–25 m trong Scene view sao cho thấy rõ tòa nhà |
| `prologueLines` | `Chào Guardian! Mình là Core-Bot, trợ lý của IUH Core — hệ thống AI vừa được lắp để quản lý đèn, cổng và mạng toàn trường.` / `Ba ngày nữa là Ngày hội IUH. Ca trực hôm nay: đi một vòng kiểm tra campus cùng mình nhé.` |
| `checkIns[0]` | `CHECKIN_SPOT_GATE`, objective `Check-in tại Cổng chính`, lines `Đây là cổng chính của IUH. Mỗi ngày hàng nghìn sinh viên đi qua đây.` |
| `checkIns[1]` | `CHECKIN_SPOT_FOUNTAIN`, `Check-in tại Đài phun nước`, `Đài phun nước — chỗ hẹn quen thuộc của sinh viên IUH. Mọi chỉ số đều bình thường.` |
| `checkIns[2]` | `CHECKIN_SPOT_FESTIVAL`, `Check-in tại Khu Ngày hội`, `Sân khấu Ngày hội đã dựng xong. IUH Core đang tải bản cập nhật cuối cùng cho hệ thống đèn...` |
| `incidentLines` | `Khoan... bản cập nhật này không có chữ ký của trường!` / `Có thứ gì đó đang chiếm IUH Core. Nó tự gọi mình là GLITCH.` / `Dữ liệu hỏng đang tràn ra, bám vào đồ điện tử và đồ vật quanh trường — chúng đang biến thành Data Monster!` / `Guardian, chúng ta phải giành lại từng khu. Bắt đầu từ Nhà E!` |
| `chapters[0]` | id `building_e`, title `Chương 1 — Nhà E`, objective `Thanh tẩy Nhà E`, intro `Nhà E bị nhiễm nặng nhất. Hạ hết Data Monster ở đó!`, encounters `CORRUPTED_ENCOUNTER_BUILDING_E`, `CORRUPTED_ENCOUNTER_SPORTS`, purified `Nhà E sạch rồi! Đèn đã sáng lại. GLITCH đang chuyển sang Canteen.` |
| `chapters[1]` | `canteen_garden`, `Chương 2 — Canteen & Vườn`, `Thanh tẩy Canteen và Vườn`, `Cây trong vườn đang héo vì dữ liệu hỏng. Nhanh lên!`, `MONSTER_02_ORC`, `CORRUPTED_ENCOUNTER_GARDEN`, `Hoa nở lại rồi! Tín hiệu GLITCH dồn về sân chính.` |
| `chapters[2]` | `main_plaza`, `Chương 3 — Sân chính`, `Thanh tẩy Sân chính`, `GLITCH chiếm cả cổng và khu lễ hội. Giữ vững nhé, Guardian!`, `MONSTER_01_BLUEDEMON`, `MONSTER_03_PHANTOM`, `Sân chính đã an toàn. Lõi của GLITCH lộ ra ở sân đấu phía bắc!` |
| `chapters[3]` | `boss`, `Chương cuối — GLITCH`, `Đánh bại GLITCH ở sân đấu phía bắc`, `Đó là hình dạng thật của GLITCH — Đại Ma Vương Hắc Ám. Trận cuối!`, `MONSTER_04_DEMON_BOSS`, `GLITCH đã bị xóa. IUH Core đang khởi động lại...` |
| `endingLines` | `IUH Core đã trở lại bình thường. Ngày hội IUH được cứu rồi!` / `Cảm ơn Guardian. Campus giờ là của bạn — chăm sóc khu vườn, nâng cấp trang bị, và cảnh giác: mảnh vụn của GLITCH vẫn có thể lẩn trong đồ vật quanh ta.` |
| `endingCaption` | `HOÀN THÀNH — IUH Guardian: Đêm Glitch` |

Encounter **không** thuộc chương nào (giữ tắt trong truyện, bật lại khi xong): `CORRUPTED_ENCOUNTER_FOUNTAIN`, `CORRUPTED_ENCOUNTER_CANTEEN`, `MONSTER_05_CORRUPTED_BACKPACK`.
- [ ] **Step 4: Chạy test Task 1 + Task 2, mong đợi tất cả PASS.** Kiểm `git status` không có scene bị sửa (test mở scene additive và đóng không lưu).
- [ ] **Step 5: Commit** — `git add` các file + `.meta` của Task 2; `git commit -m "feat(story): add Dem Glitch story script data and materials"`.

### Task 3: UI cốt truyện — thoại, mục tiêu, camera giới thiệu, cột sáng chỉ đường

**Files:**
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/IUHStoryDialogueUI.cs`
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/IUHStoryIntroCamera.cs`
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/IUHStoryBeacon.cs`

(Các file này nằm ở `Scripts/Guardian/Story/` — không có asmdef ⇒ thuộc Assembly-CSharp, dùng được `IUHStoryScript`.)

**Interfaces:**
- Produces:
  - `IUHStoryDialogueUI` (MonoBehaviour, tự dựng Canvas Screen Space Overlay, `sortingOrder = 500`, `CanvasScaler` ScaleWithScreenSize 1920×1080 match 0.5):
    - `static IUHStoryDialogueUI Create()`
    - `void ShowLines(string speaker, IReadOnlyList<string> lines, Action onFinished)` — hiện từng dòng ở khung giữa-trên; qua dòng bằng click/chạm vào khung, `Enter`, `Space`; tự qua sau `max(4s, 0.06s × số ký tự)`; `onFinished` gọi 1 lần.
    - `void SetObjective(string text)` — dòng góc trên-trái, `null`/rỗng ⇒ ẩn.
    - `void ShowCaption(string text, float seconds)` — chữ lớn giữa màn hình, mờ dần.
    - `bool IsBusy { get; }`
  - `IUHStoryIntroCamera` — `static void Play(IReadOnlyList<IUHStoryScript.CameraShot> shots, string caption, IUHStoryDialogueUI ui, Action onDone)`: tạo Camera tạm (depth cao hơn camera hiện tại), `Time.timeScale = 0` trong lúc chạy, nội suy bằng `Time.unscaledDeltaTime`, nút "Bỏ qua" (Button uGUI ≥ 160×64 px, góc dưới-phải phía trên vùng điều khiển mobile) hoặc phím `Esc`; luôn trả `timeScale = 1`, hủy camera tạm, rồi gọi `onDone` (kể cả khi bỏ qua).
  - `IUHStoryBeacon` — `static IUHStoryBeacon Create(Material mat)`, `void MoveTo(Vector3? worldPos)` (cột Cylinder cao 25 m, bán kính 0.6, không collider; `null` ⇒ ẩn), `float DistanceTo(Vector3 from)`.

- [ ] **Step 1: Tạo cảnh thử tạm** (không commit): scene mới trống có Camera + Directional Light, gắn một script thử gọi `IUHStoryDialogueUI.Create().ShowLines("Core-Bot", 3 dòng bất kỳ, ...)`, `SetObjective`, `IUHStoryIntroCamera.Play` 2 shot, `IUHStoryBeacon`.
- [ ] **Step 2: Cài đặt 3 lớp** theo Interfaces. Font `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`; cỡ chữ thoại 34, mục tiêu 30, caption 56; nền khung thoại đen alpha 0.75; tên người nói màu `#19F2FF`.
- [ ] **Step 3: Kiểm tay trong Play Mode** ở Game view 1920×1080 và Device Simulator iPhone 12 ngang: qua thoại bằng click và bằng chạm (Simulator); "Bỏ qua" hoạt động và `Time.timeScale` về 1; chữ tiếng Việt hiển thị đủ dấu. Chụp 2 ảnh vào `Docs/Evidence/CampusGuardian/STORY/ui-pc.png`, `ui-iphone12.png`. Console 0 error. Xóa cảnh thử.
- [ ] **Step 4: Commit** — 3 file + `.meta` + 2 ảnh; `git commit -m "feat(story): add story dialogue UI, intro camera and objective beacon"`.

### Task 4: `IUHStoryDirector` + khóa thế giới + vùng nhiễm + chuyển đêm

**Files:**
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/IUHStoryWorldLock.cs`
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/IUHStoryCorruptionZone.cs`
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/IUHStoryNightShift.cs`
- Create: `Assets/IUH_Campus/Scripts/Guardian/Story/IUHStoryDirector.cs`

**Interfaces:**
- Consumes: `StoryProgress`/`StoryPhase` (Task 1), `IUHStoryScript` (Task 2), `IUHStoryDialogueUI`/`IUHStoryIntroCamera`/`IUHStoryBeacon` (Task 3).
- Produces:
  - `IUHStoryWorldLock` (lớp thường):
    - `void CaptureAndLock()` — gom (kể cả inactive) và **ghi lại trạng thái gốc** rồi khóa:
      - `GameObject.SetActive(false)`: GameObject của mọi `IUHMonsterEncounter`, `IUHMonsterAI`, `IUHFlyingMonsterAI`, `IUHBuildingMonsterSpawner`, và GameObject `IUH_HOT_AIR_BALLOON`. Spawner phải dùng `Capture` + `SetActive(false)` để collider không còn phát `OnTriggerEnter`; chỉ `enabled=false` vẫn nhận trigger. `RestoreAll` khôi phục trạng thái GameObject đã capture.
      - `Behaviour.enabled = false`: mọi `IUHCampusZoneDirector`, `IUHEmergencyDroneSupplyManager`, `IUHCampusCombatManager`, `IUHHotAirBalloonController`, `IUHAerialFlightController` (giữ GameObject bật để `Instance` của chúng vẫn hợp lệ).
    - `GameObject FindEncounter(string name)`
    - `void Release(GameObject encounter)` — `SetActive(true)` một encounter của chương.
    - `void RestoreAll(Func<string,bool> isDefeated)` — trả mọi thứ về trạng thái gốc, **trừ** encounter có `isDefeated(name)==true` (giữ tắt).
  - `IUHStoryCorruptionZone` — `static IUHStoryCorruptionZone Create(IReadOnlyList<Vector3> encounterPositions, Material fog)`: vòm (Sphere dẹt, không collider) đặt ở trọng tâm các vị trí, bán kính = khoảng cách xa nhất tới trọng tâm + 6 m; `void Purify()` thu nhỏ & mờ trong 2 s rồi tự hủy, phát `IUHCombatFloatingText.Spawn(..., "KHU VỰC ĐÃ ĐƯỢC THANH TẨY", cyan, 1.4f)`.
  - `IUHStoryNightShift` — `void ToNight(float seconds)`, `void ToDay(float seconds)`, `void SetNightImmediate()`: đổi `intensity`/`color` của Directional Light đầu tiên tìm thấy (đêm: ×0.25, màu `#6A5ACD`), `RenderSettings.ambientLight` tương ứng, và LED Ngày hội: mọi `Renderer` có `sharedMaterial.name` bắt đầu bằng `festivalLedMaterialName` ⇒ `MaterialPropertyBlock` `_EmissionColor = #B000FF × 4` (đêm) / xóa block (ngày). **Không** sửa asset vật liệu. Lúc biến cố: chớp tắt đèn 3 lần (0.15 s) trước khi chuyển.
  - `IUHStoryDirector : MonoBehaviour`, `[DefaultExecutionOrder(-500)]`, field `[SerializeField] IUHStoryScript m_StoryScript;` (không dùng `m_Script` — trùng field nội bộ của MonoBehaviour), khóa lưu `IUH_Story_Progress`:
    - `Start()`: đọc tiến độ (`StoryProgress.Deserialize`). Nếu `Completed` ⇒ không làm gì (game như cũ). Ngược lại `CaptureAndLock()` rồi vào đúng phase: `Prologue` (CheckInsDone==0 ⇒ chạy camera giới thiệu + `prologueLines`; >0 ⇒ tiếp check-in đang dở), `Incident` ⇒ chạy biến cố, `Chapter` ⇒ `SetNightImmediate()` và mở chương hiện tại, `Ending` ⇒ chạy kết.
    - Check-in: mỗi 0.25 s (coroutine, không phải `Update`) đo khoảng cách ngang người chơi (cache `GameObject.Find("Campus Student") ?? FindGameObjectWithTag("Player")` một lần) tới object check-in; ≤ `checkInRadius` ⇒ `CompleteCheckIn`, thoại, lưu.
    - Mở chương: `Release` các encounter của chương chưa thắng; ghi nhận "đã bật"; tạo vùng nhiễm; beacon tới trọng tâm; `SetObjective("<objective> (còn N quái)")`; thoại `introLines`.
    - Phát hiện thắng: mỗi 0.5 s, encounter đã được bật mà nay `activeSelf == false` **và** `Match3CombatManager.Instance == null || !IsCombatActive` ⇒ `MarkEncounterDefeated`, lưu ngay. Encounter tên thiếu trong scene ⇒ `Debug.LogError("[Story] Missing encounter: <tên>")` và coi như đã thắng.
    - Hoàn thành chương ⇒ `Purify()`, thoại `purifiedLines`, rồi mở chương sau (hoặc kết).
    - Kết: `ToDay(3)`, `IUHCampusFireworksManager.Instance?.TriggerVictoryCelebration()`, `ShowCaption(endingCaption, 5)`, thoại `endingLines`, `FinishEnding()`, lưu, `RestoreAll(progress.IsDefeated)`, ẩn beacon/mục tiêu.
    - `public void ResetStory()` — xóa `IUH_Story_Progress` rồi tải lại scene hiện tại. Phím `F9` gọi `ResetStory()` chỉ khi `Application.isEditor || Debug.isDebugBuild`.
    - `OnDisable`/`OnDestroy` khi đang khóa: **không** tự RestoreAll (tránh bật quái giữa truyện khi đổi scene); tắt `STORY_DIRECTOR` trước Play ⇒ `Start` không chạy ⇒ không khóa gì.

- [ ] **Step 1: Cài đặt 4 lớp** theo Interfaces.
- [ ] **Step 2: Compile, Console 0 error.** Chạy lại toàn bộ EditMode test Story (Task 1–2) — PASS.
- [ ] **Step 3: Commit** — 4 file + `.meta`; `git commit -m "feat(story): add story director, world lock, corruption zones and night shift"`.

### Task 5: Đặt `STORY_DIRECTOR` vào scene (CẦN THỊNH ĐỒNG Ý)

**Files:**
- Modify: `Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity` (chỉ thêm 1 GameObject gốc)

- [ ] **Step 1:** Thịnh đã đồng ý (người dùng xác nhận 16/11/2026). Trước khi mở scene: `git fetch` và kiểm `origin/main` không có commit mới sửa scene campus kể từ lúc tạo nhánh; nếu có ⇒ DỪNG, báo người dùng (có thể Thịnh đang sửa scene).
- [ ] **Step 2:** Mở scene, kiểm `isDirty=false`. Tạo GameObject gốc `STORY_DIRECTOR`, thêm `IUHStoryDirector`, gán `m_StoryScript = StoryScript_DemGlitch.asset`. Lưu scene.
- [ ] **Step 3:** Kiểm diff scene chỉ chứa các khối của `STORY_DIRECTOR` (GameObject, Transform, MonoBehaviour) và thay đổi danh sách root; nếu có khác (do Unity serialize lại) thì báo người dùng thay vì commit. Missing Script = 0, Console 0 error.
- [ ] **Step 4: Commit** — `git add Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity`; `git commit -m "feat(story): place STORY_DIRECTOR in campus scene"`.

### Task 6: Chơi thử toàn truyện + bằng chứng

**Files:**
- Create: `Docs/Evidence/CampusGuardian/STORY/README.md` + ảnh

- [ ] **Step 1:** Xóa khóa `IUH_Story_Progress` (F9 hoặc PlayerPrefs). Play trên PC, chạy hết truyện. Ghi Pass/Fail từng mục:
  1. Camera giới thiệu + caption IUH hiện; "Bỏ qua" chạy.
  2. Trước biến cố không có quái nào (đi qua `CANTEEN_AMBUSH`, `BUILDING_E_AMBUSH`, `CENTRAL_PLAZA_AMBUSH`, `GARDEN_AMBUSH`, bấm `K`, `F`, click chuột — không có quái/bắn súng; khinh khí cầu không có).
  3. Check-in đúng thứ tự Cổng → Đài phun nước → Ngày hội; check-in sai thứ tự không tính.
  4. Biến cố: đèn chớp, chuyển đêm, LED tím, thoại.
  5. Mỗi chương chỉ quái của chương đó hiện; beacon chỉ đúng khu; mục tiêu đếm "còn N quái".
  6. Thua 1 trận ⇒ quái còn, chương không đổi.
  7. Thoát Play giữa chương 2 sau khi thắng 1 quái ⇒ Play lại vào thẳng chương 2, trời đêm, quái đã thắng không hiện.
  8. Trùm ⇒ kết: trời sáng, pháo hoa, caption; sau đó bắn súng/khinh khí cầu/quái ngoài truyện (`CORRUPTED_ENCOUNTER_FOUNTAIN`, `_CANTEEN`, `MONSTER_05_CORRUPTED_BACKPACK`) hoạt động lại như cũ.
  9. Tắt `STORY_DIRECTOR` rồi Play ⇒ game như trước (vào là có quái).
  10. Device Simulator iPhone 12 ngang: lặp mục 1, 3, 5 bằng chạm; thoại không che điều khiển mobile.
  11. Console 0 error suốt quá trình; Missing Script = 0.
- [ ] **Step 2:** Ghi `README.md` (commit, ngày, Unity 6000.5.8f1, Windows, kết quả 11 mục, ảnh: `prologue.png`, `incident-night.png`, `chapter1-zone.png`, `purified.png`, `ending-fireworks.png`, `iphone12-dialogue.png`). Mục Fail ⇒ sửa (tối đa 3 vòng cho lỗi do code mới), lỗi ngoài phạm vi ⇒ dừng và báo.
- [ ] **Step 3: Commit** — `git add Docs/Evidence/CampusGuardian/STORY`; `git commit -m "docs(story): add Dem Glitch playthrough evidence"`. **Không push** — báo người dùng để duyệt.

---

## Bổ sung 17/11/2026 — Dư chấn, z-fighting, AABB

Spec: mục 10 của `Docs/superpowers/specs/2026-11-16-dem-glitch-story-design.md`. Ràng buộc chung giữ nguyên, trừ: được sửa `IUHMonsterEncounter.cs`, `IUHARCorruptedObjectEncounter.cs` (chỉ thêm hàm reset) và scene campus (chỉ phần z-fighting), vì Thịnh đã đồng ý.

### Task 7: Lịch dư chấn thuần + test

**Files:** Create `Scripts/Guardian/Story/Core/AftershockSchedule.cs`; Test `Scripts/Guardian/Story/Tests/Editor/AftershockScheduleTests.cs`.

**Interfaces (namespace `IUHCampus.Guardian.Story`):**
- `public sealed class AftershockSchedule`
  - `AftershockSchedule(Func<double> random01, float firstDelay = 120f, float minDelay = 240f, float maxDelay = 360f)`
  - `bool IsActive { get; }`, `float SecondsUntilNext { get; }`
  - `bool Tick(float deltaSeconds, bool canCount)` — chỉ trừ thời gian khi `canCount && !IsActive`; trả `true` đúng một lần khi tới hạn và đặt `IsActive = true`.
  - `void Complete()` — `IsActive = false`, hẹn lần sau = `minDelay + random01() * (maxDelay - minDelay)`.
  - `static int PickZone(Func<double> random01, int zoneCount, int lastZone)` — khác `lastZone` khi `zoneCount > 1`.
  - `static IReadOnlyList<string> PickEncounters(Func<double> random01, IReadOnlyList<string> names)` — 1 hoặc 2 tên khác nhau (1 nếu `names.Count == 1`).

- [ ] Test (thất bại trước): `FirstAftershockAfter120Seconds` (Tick 119 ⇒ false, Tick 1 ⇒ true); `DoesNotCountWhenBlocked` (Tick 500 với canCount=false ⇒ false, `SecondsUntilNext` vẫn 120); `NoNewAftershockWhileActive`; `CompleteSchedulesWithinRange` (random 0 ⇒ 240, random 0.999 ⇒ < 360); `PickZoneAvoidsLast` (zoneCount 3, lastZone 1, random trả lần lượt 0, 0.34, 0.67, 0.99 ⇒ không bao giờ 1); `PickEncountersReturnsOneOrTwoDistinct`.
- [ ] Cài đặt, test Pass, commit `feat(story): add aftershock schedule with tests` (kèm spec, plan, prompt `Docs/Prompts/Prompt_Story_Aftershock.md`).

### Task 8: Hồi sinh encounter (code của Thịnh, đã được phép)

**Files:** Modify `Scripts/Guardian/IUHMonsterEncounter.cs`, `Scripts/Guardian/AR/IUHARCorruptedObjectEncounter.cs`.

- `IUHMonsterEncounter.ResetForAftershock()`: `m_IsDefeated = false; m_IsEncounterActive = false; m_TriggerCooldown = 0; m_RequireLeaveRange = true;` ẩn overhead/prompt; **không** tự `SetActive`.
- `IUHARCorruptedObjectEncounter.ResetForAftershock()`: `StopAllCoroutines()`; `m_InCombat = m_IsPurified = m_IsMonsterActive = m_IsPlaced = false`; ẩn `m_MonsterObject`; trả màu/cường độ `m_PurpleAuraLight` về giá trị ghi lại ở `Awake`; nếu `m_AutoPlaceOnStart` thì gọi lại `PlaceCorruptedObject(transform.position, transform.rotation)` (chạy lại chuỗi khói → thức tỉnh khi người chơi lại gần). Chỉ gọi khi GameObject đã active.
- Không đổi hành vi nào khác. Kiểm trong Play (execute_code): thắng hoặc giả thắng một encounter ⇒ `SetActive(true)` + `ResetForAftershock()` ⇒ lại gần/bấm E vào trận được; thắng thì encounter tắt như bình thường. Commit `feat(guardian): allow encounters to be reset for aftershocks`.

### Task 9: Chế độ dư chấn trong director

**Files:** Modify `IUHStoryScript.cs` + `StoryScript_DemGlitch.asset`, `IUHStoryWorldLock.cs`, `IUHStoryDirector.cs`; Test PlayMode trong `Story/Tests/PlayMode`, EditMode `StoryScriptSceneTests`.

- `IUHStoryScript`: thêm `[Serializable] class AftershockZone { string id; string title; string[] encounterObjectNames; }`, `AftershockZone[] aftershockZones`, `string aftershockWarningFormat` (`{0}` = title), `string aftershockObjectiveFormat` (`Dư chấn: thanh tẩy {0} (còn {1} quái)`), `string aftershockClearedLine`, `int aftershockGold = 30`. Điền đúng bảng và lời thoại ở spec mục 10. Mở rộng `StoryScript_AllNamesExistInScene` cho tên trong `aftershockZones`.
- `IUHStoryWorldLock.RestoreToys()`: trả trạng thái gốc chỉ cho `IUHCampusCombatManager`, `IUHHotAirBalloonController`, `IUHAerialFlightController`, `IUHEmergencyDroneSupplyManager`, GameObject `IUH_HOT_AIR_BALLOON`.
- Director: kết truyện gọi `RestoreToys()` thay `RestoreAll(...)`, rồi vào chế độ dư chấn. `Start()` với save `Completed`: `CaptureAndLock()` → `RestoreToys()` → chế độ dư chấn (thay cho "không làm gì").
- Chế độ dư chấn: coroutine 0.5 s gọi `schedule.Tick(0.5f, canCount)` (`canCount` theo spec mục 10). Tới hạn ⇒ `PickZone` → `PickEncounters` (bỏ tên thiếu trong scene) → mỗi encounter: `SetActive(true)` rồi `ResetForAftershock()` của cả hai component (nếu có) → vùng nhiễm, beacon, mục tiêu, thoại báo. Phát hiện thắng như chương (encounter đã bật mà `activeSelf == false` và không còn trận) ⇒ còn 0 ⇒ `Purify()`, `AddGold(aftershockGold)` **đúng một lần**, thoại xong, ẩn beacon/mục tiêu, `schedule.Complete()`.
- Tắt `STORY_DIRECTOR` ⇒ game như `main` (giữ tiêu chí).
- PlayMode test: `CompletedSaveKeepsMonstersLockedAndToysOn` (dựng như các test WorldLock hiện có).
- Kiểm tay qua MCP (được tạm rút ngắn `firstDelay` bằng execute_code, không commit giá trị rút ngắn): sau kết truyện không có `IUHMonsterAI`/`IUHFlyingMonsterAI` active, bắn súng + khinh khí cầu dùng được; dư chấn xuất hiện đúng 1 khu, 1–2 quái; thắng ⇒ +30 Gold đúng một lần; dư chấn kế tiếp ở khu khác.
- Commit `feat(story): add controlled GLITCH aftershocks after the ending`.

### Task 10: Mặt đất nhấp nháy + lỗi AABB

- **Z-fighting:** lệnh dò (execute_code, không commit) quét `MeshRenderer` của scene campus có bounds dẹt (`size.y < 0.05`), tìm các cặp chồng nhau trên XZ với chênh lệch mặt trên `< 0.003 m`. Báo danh sách cặp (đường dẫn, vị trí). Ưu tiên khu kết truyện (ảnh người dùng: sân lát gạch cạnh bồn hoa trắng và ghế, gần sân đấu phía bắc). Sửa: bản trùng hoàn toàn (cùng mesh, cùng transform) ⇒ tắt GameObject trùng; khác mesh ⇒ nâng mặt nằm trên +0.01 m. Lưu scene; stage **chỉ** các hunk này bằng patch như Task 5 (6 dòng font giữ nguyên). Quá 10 cặp ⇒ sửa cặp ở khu kết truyện, ghi phần còn lại vào README và báo. Commit `fix(scene): remove ground z-fighting near the north plaza`.
- **AABB:** tạm gắn `Application.logMessageReceived` (không commit): khi gặp `Invalid worldAABB`/`Invalid localAABB`, quét mọi `Renderer` có `transform.position`/`lossyScale` hoặc `bounds` không hữu hạn, log đường dẫn + component một lần. Tái hiện bằng cách chơi tới kết truyện (được dùng execute_code để thắng nhanh các chương). Tìm nơi gán NaN, sửa tại gốc (commit riêng, tối thiểu). Nguồn nằm ngoài code đã được phép sửa ⇒ dừng và báo.

### Kiểm chứng cuối
EditMode + PlayMode toàn dự án Pass, Backend tests Pass, Console 0 error (ghi riêng 4 cảnh báo "referenced script (Unknown)" có sẵn từ trước), Missing Script = 0. Cập nhật README bằng chứng. Không push.
