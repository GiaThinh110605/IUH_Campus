# Guardian Progress P1

Namespace/assembly: `IUHCampus.Guardian.Progress`; tests: `IUHCampus.Guardian.Progress.Tests`. Không phụ thuộc Backend/Match3, không tự thêm vào scene. Rules dự án ưu tiên hơn skill.

## Khởi tạo bằng code

```csharp
var progress = GuardianProgressService.EnsureCreated(database, config);
progress.Wallet.AddCore(n, "combat:" + eventId);
progress.Inventory.Add(randomSeedId, 1, "combat-seed:" + eventId);
```

Caller cung cấp database/config asset (tham chiếu Inspector ở module tích hợp sau). Không dùng Resources tự động và không sửa scene. `EnsureCreated` giữ một instance, DontDestroyOnLoad; không có RuntimeInitialize tự tạo service.

## API

- `GuardianWallet.Core`, `AddCore(amount, rewardId): bool`, `SpendCore(amount, reason): bool`, `Changed`.
- `GuardianInventory.Add(itemId, qty, rewardId=null): bool`, `Remove: bool`, `Count`, `Has(itemId, qty=1)`, `Items` là snapshot dictionary chỉ đọc, `Changed`.
- Add vượt giới hạn bị từ chối toàn bộ, không clamp/cấp một phần. maxStack là tổng số lượng cho từng id (P1 chưa có slot inventory). Tool không stack có sức chứa 1. Số âm/0, overflow Core, reward sai/trùng bị từ chối. ID không có database cảnh báo một lần cho mỗi id trên instance inventory, không log mỗi frame.
- Reward ledger dùng chung cho ví và kho. Cùng id đã cấp Core thì không thể cấp đồ bằng id đó nữa. Vì vậy ví dụ combat dùng hai id khác nhau. Grant thất bại không tiêu thụ rewardId. Inventory không có rewardId là thao tác thường, không có chống trùng; ví bắt buộc rewardId.
- `GuardianProgressFactory.Create(config, database, Random)` chỉ dùng khi tạo state mới. Có một bình tưới + hai hạt ngẫu nhiên, có thể cùng loại; loại hạt và số lượng lấy từ config. Store đọc được save sẽ không gọi factory/cấp lại đồ.
- `GuardianProgressService.State/Wallet/Inventory/Store/Database/Config`. Wallet/kho Changed tự đánh dấu dirty. Nếu module sau sửa upgrades/gardenPlots/purifiedAreas trực tiếp thì phải gọi `MarkDirty()` sau giao dịch; `Flush()` để lưu ngay.

## Assets

Trong `Assets/IUH_Campus/Data/Guardian/Progress/`: seed_sunflower (Hạt hướng dương), seed_rose (Hạt hoa hồng), seed_daisy (Hạt hoa cúc), tool_watering_can (Bình tưới), fertilizer (Phân bón), GuardianItemDatabase, GuardianProgressConfig. Sprite tạm null. Id là khóa cố định, không đổi khi rename asset/displayName; database Validate ném lỗi khi id trùng/rỗng.

Config: stageMinutes1, fertilizerMultiplier2, plotCount6, saveDebounceSeconds0.5, starting items/seeds; coreRewardOnBloom5; bảng giá hạt5/phân3 và nâng cấp10/20 là **giá trị mẫu để P4 cân bằng**, không tuyên bố đã playtest. P1 chỉ định nghĩa dữ liệu garden/P4, chưa chạy tăng trưởng, trồng/tưới, shop hoặc giao dịch upgrade. Logic tạo state đọc plotCount/đồ khởi đầu từ config, không hardcode balance trong ví/kho/factory.

## Lưu và version

`GuardianProgressStore(directory)` dùng guardian_progress.json. Service mặc định Application.persistentDataPath. JSON version1 có Core, inventory, upgrades0..2, garden plots/stage/UTC timestamps/flags, appliedRewardIds, purifiedAreas. On-disk temp được Flush(true) rồi File.Replace (hoặc Move khi chưa có file), backup `.bak` giữ bản trước. Khi đọc chính hỏng, đọc backup và cảnh báo; cả hai hỏng thì factory state mới và cảnh báo. Warning phục hồi tối đa một lần trên instance store. Sau phục hồi, lần Save tiếp theo không ghi đè backup tốt bằng file chính hỏng.

Chưa có schema cũ để migrate: version hiện hỗ trợ1; thiếu/khác version bị coi là không đọc được và đi qua fallback backup/new. Trước khi phát hành schema mới phải bổ sung migration/khả năng bảo vệ save phiên bản tương lai; không dùng P1 để đọc save từ bản game mới hơn. JSON cục bộ không mã hóa, không chống sửa tay/cheat và không phải server authority.

Service debounce theo realtime: mỗi mutation dời hạn ~0.5s, Update chỉ kiểm cờ/hạn, không ghi/log mỗi frame. Pause(true), Quit và destroy instance flush dirty. Lần Initialize ghi state ngay để đồ khởi đầu có trên đĩa. Thao tác file đồng bộ; P1 chưa đo hiệu năng save trên iPhone hoặc lifecycle kill hệ điều hành trước debounce.

## Kiểm thử

Evidence trong `Docs/Evidence/CampusGuardian/P1/`. EditMode kiểm limits/không âm, shared reward ledger, warning unknown id, duplicate database, snapshot read-only, starter theo config và chỉ cấp một lần qua reload, reward còn hiệu lực sau reload, JSON/backup recovery, asset database/config thật. Không gắn service vào scene, chưa PlayMode integration/pause trên thiết bị.
