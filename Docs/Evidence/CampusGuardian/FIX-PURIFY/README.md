# FIX-PURIFY — đếm Purify trong nhóm ghép và vùng nổ

Ngày 09/11/2026, Asia/Bangkok. Codex, phần Minh Nhật.
Môi trường: Windows, Unity 6000.5.8f1 Editor + MCP.
Nhánh độc lập `minhnhat/fix-match3-purify`, nền origin/main
`6432dc03c60e482faf39f0ba1691ca135b118cc6`.
Commit FIX-PURIFY: commit chứa thư mục bằng chứng này; tra bằng
`git log -1 --format=%H -- Docs/Evidence/CampusGuardian/FIX-PURIFY/README.md`.
Test chạy trên working tree FIX-PURIFY trước commit. Không push/PR.

Nguyên nhân: switch đếm group.Coords trong ProcessStep thiếu TileType.Purify,
trong khi switch BlastCoords có; lỗi từ e5aecbb theo chẩn đoán được cung cấp.
Đã tái hiện trên nền main: 66 ca hoàn tất, đúng một failure
ProcessStep_CountsPurifyTiles, expected3/actual0 (editmode-before.json).

Thay đổi: một private static CountTile dùng chung cho hai vòng, gồm đủ năm loại
ô. Giữ nguyên uniqueMatched.Add, thứ tự duyệt, clear/drop/spawn, luật ghép 4/5,
mega-bomb/color bomb và cân bằng. Không thêm UnityEngine; Core giữ noEngineReferences=true.
Không sửa test cũ. Thêm đúng một test ProcessStep_CountsPurifyInOverlappingBlastAreasOnce:
5 Sword tạo mega-bomb, một Purify ở (2,1) thuộc các vùng nổ chồng lấp;
assert SwordCount5, PurifyCount1 và tọa độ nổ xuất hiện đúng một lần.

| ID | Ca | Mong đợi | Thực tế | Pass/Fail |
|---|---|---|---|---|
| a | Compile và Console | Compile xong, 0 error | Editor idle, không compile; read_console 0 error | Pass |
| b | Toàn EditMode | Tất cả ca được khám phá Pass | 67/67 passed, failed0, skipped0; 10.2300015s, summary Passed | Pass theo bộ test thực tế của main |
| b1 | Test lỗi Purify có sẵn | Đếm 3 | ProcessStep_CountsPurifyTiles Passed | Pass |
| b2 | Test blast mới | Đếm Purify đúng một lần | ProcessStep_CountsPurifyInOverlappingBlastAreasOnce Passed | Pass |
| c | PlayMode IUHCampus.Guardian.Match3.PlayModeTests | Ghi kết quả thật, không suy đoán | init_timeout120000; 9/9 hoàn tất; một failure FreeCursorStopsPendingMovementOnFirstApply: NullReferenceException; result=null | Fail — có sẵn từ e5aecbb, ngoài phạm vi |
| d | Scene campus | missingScripts0, isDirty=false | missingScripts0, brokenPrefabs0, isDirty=false, 48 roots; git diff scene rỗng | Pass |
| e | Hồ sơ | Nguyên nhân/thay đổi/kết quả/giới hạn rõ | README và phản hồi MCP gốc được lưu | Đã ghi |

Số lượng khác prompt 69/70: origin/main 6432dc0 có 66 EditMode, gồm các test
Progress P1/P1.5 nhưng chưa có 14 test GuardianCombatReport của B8. Thêm test
blast thành 67; không mang code/test B8 sang nhánh độc lập để tăng số lượng.

PlayMode job `865453b5990d4b12bfac79b6993be8b3`: progress.completed=9,
total=9, failures_so_far chứa đúng một ca nêu trên, failures_capped=false.
MCP result=null nên không dựng summary passed/skipped. Nguyên nhân lỗi PlayMode
đã được Claude Code chẩn đoán và người dùng xác nhận: test gọi
`GetMethod("Apply", BindingFlags.Instance | BindingFlags.NonPublic)`, trong khi
commit e5aecbb đổi `IUHConceptPreviewController.Apply` thành public tại
`Assets/IUH_Campus/SharedCampusConcept/Scripts/IUHConceptPreviewController.cs:65`.
GetMethod trả null nên Invoke gây NullReferenceException. Đã đọc lại lời gọi
reflection hiện tại và `public void Apply()` ở e5aecbb để đối chiếu. Không sửa
test PlayMode hoặc controller; người dùng cho phép commit phần FIX-PURIFY với
lỗi cũ được ghi rõ là Fail ngoài phạm vi.

Review độc lập chỉ đọc không phát hiện vấn đề trong diff model/test Purify.
Test Runner đổi ProjectSettings/EditorSettings.asset: m_EnterPlayModeOptions 0→1.
Theo chỉ dẫn người dùng, đã xác nhận Unity không Play Mode rồi chạy
`git checkout -- ProjectSettings/EditorSettings.asset`; diff --ignore-cr-at-eol
rỗng và giá trị trên đĩa trở về 0. Không stage ProjectSettings hoặc scene.
Material, project files, Logs và file ngoài phạm vi giữ nguyên, không stage/revert.
