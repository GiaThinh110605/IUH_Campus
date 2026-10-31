# Nang vat lieu ambientCG - 31/10/2026: CHUA DAT

Da tiep tuc yeu cau cuoi cua chat "Chinh khuon vien IUH theo anh tham chieu".

Nhanh: codex/ambientcg-graphics-upgrade. Base: f646b726056c6133d970bc168aa3ad03687e3440.
Backup scene: Docs/Backups/IUH_SharedCampus_Concept_before_ambientcg_20261031_184015.unity.
Backup volume: xem Docs/Backups/Campus_volume_before_ambientcg_*.asset.

## Da lam
- Xac nhan scene da luu, dirty=false truoc sua; du 6 prefab props tren dia va Unity.
- Tao editor script moi IUHSharedCampusAmbientCGPass.cs, khong sua builder/polish goc.
- Copy 7 bo ambientCG; tao 8 material URP/Lit (Bricks104 va WoodFloor062 de du phong).
- Import NormalGL dang NormalMap; roughness khong gan vao shader; AO dung OcclusionMap.
- Phat hien ten material thuc te la cac alias VisualPolish; bo sung anh xa 6 alias, thay 25 slot.
- Chi thay sharedMaterials; them marker AMBIENTCG_PASS_APPLIED. Khong doi mesh, transform, collider, player, camera, den, skybox, pipeline.
- Chinh profile Campus volume.asset: exposure .15, contrast 12, saturation 10, bloom threshold 1.15/intensity .35/scatter .72, them vignette .22. Profile da backup.
- Chup 6 goc before va 6 goc after trong Play Mode, camera cung toa do.

## Ly do dung
Anh after-overview.png va after-paving-closeup.png cho thay tiling 18x18 khong phu hop UV mesh hien tai: mat lat nhin gan nhu phang nau, mat co qua sang va mat chi tiet so voi before. Chua dat tieu chi nang chat luong. Theo nguyen tac 7 cua prompt, dung chinh sua khi ket qua xau hon before.

Scene hien tai da luu ban thu de doi chieu; chua tu dong khoi phuc backup hay chinh tiling tiep sau khi phat hien ket qua khong dat. Da thoat Play Mode.

## Kiem chung chua hoan thanh
- Da vao Play Mode va chup anh; chua chay 4 tuyen vi da cham dieu kien dung.
- Snapshot truoc: 847 component gom 822 component campus cu va 25 component cua root/props. Chua hoan tat doi chieu sau.
- Console khong thay loi compile; co warning WebSocket cua MCP sau reload. Khong khang dinh 0 warning moi.
- Chua commit, chua push, khong build APK, khong sua scene khac.

Anh va mapping: Docs/Evidence/AmbientCG_20261031/. File after-edit-* la anh trung gian truoc khi sua anh xa alias, khong phai ket qua cuoi. Anh after-* moi la ket qua Play Mode.

Can luot dieu chinh UV tiling theo kich thuoc/UV thuc te cua mesh va giam saturation/contrast truoc khi verify va commit.

## Lượt Tune Surface Scale
Đã áp thông số theo yêu cầu mới, chưa đạt cổng duyệt ảnh; dừng trước 4 tuyến. Báo cáo: [Tuned](Evidence/AmbientCG_20261031/Tuned/Report.md). Chưa commit/push.


## Hybrid pass
Đã trả paving/lawn về VisualPolish, giữ AmbientCG facade/brick/wood, khôi phục volume gốc. 4/4 tuyến CharacterController đạt (2.582 bước); 822 component campus giữ cấu trúc, chỉ 7 renderer đổi sharedMaterials đúng scope. [Báo cáo và ảnh](Evidence/AmbientCG_20261031/Hybrid/Report.md). Chưa commit/push.

