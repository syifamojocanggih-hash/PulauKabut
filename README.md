# Pulau Kabut

**Pulau Kabut** adalah game petualangan 3D *stylized* yang dikembangkan menggunakan **Unity 6 (URP)** oleh tim beranggotakan 3 orang (**Programmer**, **3D Artist**, dan **Game Designer / UI / Audio**).

Dokumen ini menjadi panduan utama untuk menyiapkan lingkungan kerja, memahami struktur proyek, mematuhi konvensi penamaan aset, serta menjalankan alur kerja Git tim.

---

## Panduan Cepat Harian Tim (Anti Tabrakan / Anti Conflict)

> **Bisa kerja bareng di proyek yang sama tanpa tabrakan?** **BISA!** Selama setiap orang bekerja di **branch & folder bagiannya masing-masing** dan tidak mengedit file scene `.unity` yang sama secara bersamaan.

### 1. Zona Aman Masing-Masing (Biar 0% Conflict)

| Anggota / Peran | Branch Kerja Kamu | Folder yang Boleh Kamu Edit | JANGAN Di-Save / Diedit |
| --- | --- | --- | --- |
| **Programmer** | `feat/player-movement`, `feat/lantern-fog`, `feat/ui-dialog`, `feat/shards-story` | `Assets/_Project/Scripts/`, `Prefabs/Player/`, `Sandbox_Player` | Scene orang lain |
| **3D Artist** | `art/characters`, `art/environment` | `Assets/_Project/Art/`, `Animations/`, `Shaders/`, `Sandbox_Characters`, `Sandbox_Environment` | `Zone_Mangrove.unity`, `Scripts/` |
| **Game Designer / UI / Audio** | `level/greybox-mangrove`, `audio/sfx-music` | `Assets/_Project/Scenes/Zone_Mangrove/`, `Dialog/`, `UI/`, `Audio/`, `Sandbox_UI` | `Scripts/` |

### 2. Langkah Harian (Copy-Paste di Terminal / Git Bash)

**A. Sebelum mulai kerja (ambil update terbaru dari teman):**
```bash
git checkout dev
git pull origin dev
git lfs pull
```

**B. Pindah ke branch bagianmu & sinkronkan dengan `dev`** *(ganti `art/characters` sesuai branch kamu)*:
```bash
git checkout art/characters
git merge dev
```

**C. Kerjakan tugasmu di Unity**, lalu simpan (**`Ctrl + S`**).
> **Tips penting:** Kalau kamu cuma mau buka `Zone_Mangrove.unity` untuk *playtest* (padahal itu bukan bagianmu), saat menutup/pindah scene klik **"Don't Save"** supaya tidak ikut mengubah scene temanmu!

**D. Simpan & upload hasil kerjamu ke GitHub:**
```bash
git status
git add .
git commit -m "art: jelaskan singkat perubahanmu"
git push origin art/characters
```

**E. Gabungkan ke `dev` kalau fitur/asetmu sudah siap dipakai teman lain:**
```bash
git checkout dev
git pull origin dev
git merge --no-ff art/characters
git push origin dev
git checkout art/characters
```

### 3. Tiga Aturan Emas Anti-Tabrakan
1. **Jangan pernah commit langsung di `main` atau `dev`** — selalu pakai branch bagianmu sendiri.
2. **Jangan ubah atau hapus file `.meta` lewat File Explorer** — selalu pindahkan/hapus aset lewat jendela **Project** di dalam Unity.
3. **Kalau tidak sengaja mengubah file yang bukan bagianmu**, batalkan sebelum commit:
   ```bash
   git checkout -- path/file_yang_tidak_sengaja_diubah
   ```
   *(Atau klik kanan file tersebut di GitHub Desktop / VS Code -> **Discard Changes**).*

---

## Daftar Isi

1. [Gambaran Proyek & Pembagian Peran](#1-gambaran-proyek--pembagian-peran)
2. [Spesifikasi Teknis & Paket Unity](#2-spesifikasi-teknis--paket-unity)
3. [Prasyarat & Instalasi Git LFS](#3-prasyarat--instalasi-git-lfs)
4. [Cara Clone & Membuka Proyek](#4-cara-clone--membuka-proyek)
5. [Pengaturan Wajib di Unity Editor](#5-pengaturan-wajib-di-unity-editor)
6. [Struktur Folder Proyek](#6-struktur-folder-proyek)
7. [Aturan Scene Utama & Scene Sandbox](#7-aturan-scene-utama--scene-sandbox)
8. [Konvensi Penamaan Aset & Kode](#8-konvensi-penamaan-aset--kode)
9. [Daftar Branch & Strategi Branching](#9-daftar-branch--strategi-branching)
10. [Ringkasan Alur Kerja Git (Tanpa Pull Request)](#10-ringkasan-alur-kerja-git-tanpa-pull-request)
11. [Standar Kualitas & Tes Build Mingguan](#11-standar-kualitas--tes-build-mingguan)
12. [Troubleshooting Umum](#12-troubleshooting-umum)

---

## 1. Gambaran Proyek & Pembagian Peran

- **Judul**: Pulau Kabut
- **Genre / Gaya Visual**: Petualangan 3D *Stylized*
- **Area Awal**: `Zone_Mangrove`
- **Repository GitHub**: https://github.com/syifamojocanggih-hash/PulauKabut.git

### Pembagian Peran Tim

| Peran | Fokus Utama | Folder Utama | Scene Sandbox |
| --- | --- | --- | --- |
| **Programmer** | Pergerakan pemain, mekanik lentera & kabut, sistem dialog, arsitektur inti | `Assets/_Project/Scripts/`, `Prefabs/Player/` | `Sandbox_Player`, `Sandbox_Lantern` |
| **3D Artist** | Karakter, lingkungan, props, material, tekstur, animasi, shader, VFX | `Assets/_Project/Art/`, `Animations/`, `Shaders/`, `VFX/` | `Sandbox_Characters`, `Sandbox_Environment` |
| **Game Designer / UI / Audio** | Desain level `Zone_Mangrove`, UI/HUD, narasi/dialog, musik, SFX, ambience | `Assets/_Project/Scenes/Zone_Mangrove/`, `UI/`, `Dialog/`, `Audio/` | `Sandbox_UI` |

---

## 2. Spesifikasi Teknis & Paket Unity

- **Versi Unity**: `6000.6.10f1` (**Unity 6** — pastikan seluruh anggota memakai versi persis yang sama agar tidak terjadi perubahan file scene/prefab yang tidak perlu)
- **Render Pipeline**: Universal Render Pipeline (`com.unity.render-pipelines.universal` `17.6.0`)
- **Paket Penting yang Terpasang** (lihat `Packages/manifest.json`):
  - **Input System** (`com.unity.inputsystem` `1.20.1`)
  - **Cinemachine** (`com.unity.cinemachine` `3.1.7`)
  - **uGUI** (`com.unity.ugui` `2.6.0`)
  - **Unity Services**: Authentication (`3.9.0`), Remote Config (`4.3.0`), Cloud Save (`3.4.2`)
  - **IDE Integration**: Visual Studio (`2.0.28`), Rider (`3.1.0`)

---

## 3. Prasyarat & Instalasi Git LFS

Pastikan perangkat lunak berikut sudah terpasang di komputer Anda:

1. **Unity Hub** dan **Unity Editor `6000.6.10f1`**
2. **Git** (https://git-scm.com/)
3. **Git LFS** (https://git-lfs.com/) — wajib untuk menangani file biner berukuran besar agar repository tetap ringan.
4. **IDE / Code Editor** (Visual Studio, JetBrains Rider, atau VS Code — khusus Programmer atau saat mengedit skrip/dokumentasi)

### Aktivasi Git LFS (Dilakukan Sekali per Komputer)

Jalankan perintah berikut di terminal (PowerShell / Git Bash / Terminal):

```bash
git lfs install
```

Verifikasi bahwa Git LFS sudah terpasang:

```bash
git lfs version
```

### Pola File yang Dilacak oleh Git LFS

Daftar ekstensi berikut otomatis dikelola Git LFS melalui `.gitattributes`:

- **3D & Model**: `*.fbx`, `*.blend`
- **Gambar & Tekstur**: `*.png`, `*.jpg`, `*.psd`, `*.tga`, `*.exr`
- **Audio**: `*.wav`, `*.mp3`, `*.ogg`
- **Video**: `*.mp4`
- **Font**: `*.ttf`, `*.otf`

---

## 4. Cara Clone & Membuka Proyek

1. **Clone repository** ke folder kerja lokal Anda:
   ```bash
   git clone https://github.com/syifamojocanggih-hash/PulauKabut.git
   cd PulauKabut
   ```

2. **Unduh seluruh aset Git LFS** (jika belum otomatis terunduh saat clone):
   ```bash
   git lfs pull
   ```

3. **Pindah ke branch `dev`** (jangan bekerja di `main`):
   ```bash
   git checkout dev
   git pull origin dev
   ```

4. **Buka proyek di Unity Hub**:
   - Buka **Unity Hub** -> klik **Add** -> **Add project from disk**.
   - Pilih folder `PulauKabut`.
   - Pastikan versi Editor terpilih adalah **`6000.6.10f1`**, lalu buka proyek.
   - Tunggu proses import awal selesai (`Library/` akan dibuat secara lokal dan otomatis diabaikan oleh `.gitignore`).

---

## 5. Pengaturan Wajib di Unity Editor

Setiap anggota tim **wajib** memeriksa dan memastikan pengaturan berikut aktif di Unity Editor sebelum mulai membuat atau mengubah aset:

1. Buka menu `Edit > Project Settings > Editor`.
2. Atur:
   - **Asset Serialization** -> **Force Text**
   - **Version Control Mode** -> **Visible Meta Files**

### Aturan File `.meta`
- **Jangan pernah menghapus, mengabaikan, atau mengedit manual** file `.meta`.
- Selalu **pindahkan, ubah nama, atau hapus aset dari dalam jendela Project di Unity Editor**, bukan dari File Explorer/Finder, agar file `.meta` ikut berpindah dan referensi prefab/scene tidak rusak (*missing reference*).

---

## 6. Struktur Folder Proyek

Semua aset buatan tim disimpan di dalam `Assets/_Project/` agar terpisah rapi dari paket pihak ketiga atau aset bawaan Unity.

```text
Assets/_Project/
├── Art/
│   ├── Characters/      # Model 3D karakter utama & NPC
│   ├── Environment/     # Aset lingkungan (pohon mangrove, tanah, batu, air, dsb.)
│   ├── Props/           # Objek interaktif & dekoratif (lentera, dermaga, dsb.)
│   ├── Materials/       # Material URP (.mat)
│   └── Textures/        # Tekstur albedo, normal, roughness, mask, dsb.
├── Animations/          # Animation Clip (.anim) & Animator Controller (.controller)
├── Audio/
│   ├── Music/           # BGM / musik latar
│   ├── SFX/             # Efek suara interaksi, langkah kaki, lentera, UI
│   └── Ambience/        # Suara lingkungan (angin, air, hutan mangrove)
├── Prefabs/
│   ├── Player/          # Prefab karakter pemain & komponennya
│   ├── NPC/             # Prefab NPC
│   ├── Props/           # Prefab props & objek interaktif siap pasang
│   └── VFX/             # Prefab partikel / efek visual
├── Scenes/
│   ├── Zone_Mangrove/   # Scene utama permainan (HANYA diedit Game Designer)
│   └── Sandbox/         # Scene uji coba masing-masing pemilik fitur/aset
├── Scripts/
│   ├── Player/          # Skrip kontrol, pergerakan, interaksi pemain
│   ├── Lantern/         # Skrip mekanik lentera & interaksi dengan kabut
│   ├── Dialog/          # Skrip sistem dialog & alur percakapan
│   ├── UI/              # Skrip HUD, menu, panel dialog
│   └── Core/            # Manager inti, event, utilitas umum
├── Shaders/             # Shader Graph & custom HLSL shader
├── UI/                  # Sprite UI, layout, font, elemen Canvas/UI Toolkit
├── VFX/                 # VFX Graph, Particle System asset, tekstur efek
└── Dialog/              # Data narasi, file dialog (JSON / ScriptableObject)
```

> Folder `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, `UserSettings/`, `.vs/`, `.idea/`, `*.csproj`, dan `*.sln` diabaikan oleh `.gitignore` dan tidak pernah di-commit ke Git.

---

## 7. Aturan Scene Utama & Scene Sandbox

Untuk menghindari konflik merge pada file `.unity` yang sulit diselesaikan:

- **Scene utama (`Assets/_Project/Scenes/Zone_Mangrove/`) hanya boleh diedit oleh Game Designer.**
- Programmer dan 3D Artist bekerja di **Prefab** atau **Scene Sandbox** masing-masing di dalam `Assets/_Project/Scenes/Sandbox/`:
  - `Sandbox_Player` — Pemilik: **Programmer**
  - `Sandbox_Lantern` — Pemilik: **Programmer**
  - `Sandbox_UI` — Pemilik: **Game Designer / UI / Audio**
  - `Sandbox_Characters` — Pemilik: **3D Artist**
  - `Sandbox_Environment` — Pemilik: **3D Artist**
- Detail lebih lanjut tersedia di [Assets/_Project/Scenes/Sandbox/README.md](Assets/_Project/Scenes/Sandbox/README.md).

---

## 8. Konvensi Penamaan Aset & Kode

Gunakan konvensi berikut agar proyek tetap konsisten dan mudah dicari:

| Jenis | Pola / Gaya | Contoh |
| --- | --- | --- |
| **Script C#** | `PascalCase` | `PlayerMovement.cs`, `LanternController.cs` |
| **Prefab** | `PF_NamaObjek` | `PF_Player`, `PF_Lantern`, `PF_MangroveTree_01` |
| **Material** | `MAT_NamaMaterial` | `MAT_MangroveBark`, `MAT_WaterStylized` |
| **Tekstur** | `TEX_Nama_Tipe` | `TEX_Lantern_Albedo`, `TEX_Lantern_Normal` |
| **Model 3D** | `SM_` (Static) / `SK_` (Skeletal) | `SM_WoodenDock_01.fbx`, `SK_Player.fbx` |
| **Audio** | `SFX_` / `MUS_` / `AMB_` | `SFX_LanternIgnite.wav`, `AMB_MangroveWind.ogg` |
| **Scene Sandbox** | `Sandbox_NamaArea` | `Sandbox_Player.unity`, `Sandbox_Lantern.unity` |
| **Shader** | `SHD_NamaShader` | `SHD_StylizedFog`, `SHD_WaterRipple` |

---

## 9. Daftar Branch & Strategi Branching

### Branch Utama
| Branch | Fungsi |
| --- | --- |
| `main` | Branch rilis stabil. Hanya menerima merge dari `dev` setelah tes build mingguan lolos. |
| `dev` | Branch integrasi harian tempat seluruh branch kerja digabungkan. |

### Branch Kerja Awal (Dibuat dari `dev`)
| Branch | Fokus Pekerjaan |
| --- | --- |
| `feat/player-movement` | Pergerakan, kontrol, dan kamera pemain |
| `feat/lantern-fog` | Mekanik lentera dan sistem kabut |
| `feat/ui-dialog` | Sistem UI, HUD, dan dialog |
| `art/characters` | Model, rig, material, dan animasi karakter |
| `art/environment` | Aset lingkungan, props, dan material dunia |
| `audio/sfx-music` | Musik latar, SFX, dan ambience |
| `level/greybox-mangrove` | Greybox dan tata letak level `Zone_Mangrove` |

### Branch yang Dibuat Belakangan
Branch berikut **belum dibuat** pada tahap awal dan akan dibuat dari `dev` ketika tahap pengembangannya dimulai:
- `feat/shards-story` — Fitur pengumpulan *shards* dan progres cerita (dibuat setelah mekanik inti dan greybox level siap).
- `release/demo-v1` — Branch stabilisasi khusus menjelang rilis demo v1.

---

## 10. Ringkasan Alur Kerja Git (Tanpa Pull Request)

Tim menggunakan alur kerja **langsung merge ke `dev` tanpa Pull Request**. Panduan lengkap beserta perintah detail ada di [CONTRIBUTING.md](CONTRIBUTING.md).

### Ringkasan Alur Harian
1. Perbarui `dev`: `git checkout dev && git pull origin dev`
2. Sinkronkan `dev` ke branch kerja: `git checkout nama-branch && git merge dev`
3. Kerjakan tugas dengan commit kecil dan push branch kerja di akhir sesi.
4. Setelah fitur selesai dan teruji di Unity tanpa error, merge ke `dev`:
   ```bash
   git checkout dev
   git pull origin dev
   git merge --no-ff nama-branch
   git push origin dev
   ```
5. Kabari tim di grup agar semua anggota menjalankan `git pull origin dev`.
6. Hapus branch kerja yang sudah di-merge (lokal & remote).

### Format Pesan Commit
Gunakan awalan berikut pada setiap commit:
- `feat: ...` — fitur atau sistem gameplay baru
- `art: ...` — aset 3D, tekstur, material, animasi, shader, VFX
- `audio: ...` — musik, SFX, ambience
- `fix: ...` — perbaikan bug
- `docs: ...` — dokumentasi

### Aturan Keras
- **Dilarang** commit langsung ke `main` atau `dev`.
- **Dilarang** `git push --force` ke `main` atau `dev`.
- **Satu branch = satu pekerjaan**, merge kecil dan sering (maksimal seminggu sekali).
- Jika terjadi konflik pada file `.unity` atau `.prefab`, **jangan menimpa asal** — koordinasikan dengan pemilik file.

---

## 11. Standar Kualitas & Tes Build Mingguan

### Checklist Sebelum Merge ke `dev`
- [ ] Sudah menjalankan `git pull origin dev` dan `git merge dev` di branch kerja.
- [ ] Proyek dibuka di Unity **tanpa error di Console**.
- [ ] Tidak ada *missing script* atau *missing reference* pada prefab/scene yang disentuh.
- [ ] Scene uji coba dan/atau scene utama masih bisa dimainkan (*Play Mode*).
- [ ] Tidak ada aset sementara atau file sampah yang ikut ter-commit.

### Tes Build Rutin (Setiap Jumat)
1. Buat build dari branch `dev`.
2. Uji performa di laptop acuan:
   - Target minimal: **rata-rata 60 FPS** dan **tanpa crash**.
3. **Jika lolos**: gabungkan `dev` ke `main` (`git merge --no-ff dev`), push `main`, lalu buat tag versi (misalnya `v0.1`).
4. **Jika gagal**: buat branch `fix/nama-bug` dari `dev`, perbaiki, lalu merge kembali ke `dev`. Branch `main` **tidak disentuh** sampai tes build lolos.

---

## 12. Troubleshooting Umum

- **File aset (`.fbx`, `.png`, `.wav`) terlihat rusak atau berukuran sangat kecil (~130 byte) di Unity:**
  File tersebut masih berupa pointer Git LFS. Jalankan:
  ```bash
  git lfs install
  git lfs pull
  ```
- **Error kompilasi atau aset hilang setelah `git pull`:**
  Tutup Unity, pastikan tidak ada konflik merge tersisa (`git status`), lalu buka ulang Unity agar `Library/` melakukan re-import.
- **Konflik pada file `.unity` atau `.prefab`:**
  Jangan langsung memilih *Accept Yours/Theirs*. Hubungi pemilik file/scene di grup tim dan selesaikan perubahan bersama.

---

## 13. Integrasi Tools, MCP, & Agent Skills (Unity 6 + Blender)

Proyek ini terhubung dengan alur kerja AI Agent (Antigravity) untuk mempercepat pembuatan aset 3D, penataan scene, audio, UI, dan optimasi build:

### A. Koneksi MCP Aktif
- **`blender` (`mcp-for-blender`)** — terhubung langsung ke **Blender 5.3** (ekspor model *Cozy Stylized Low-Poly* `.fbx` & `.blend` ke `Assets/_Project/Art/Models/`, plus pratinjau viewport `look`).
- **`unityMCP` + `unity-cli` (`com.unity.pipeline`)** — kontrol live Unity 6 Editor untuk kompilasi C#, verifikasi Console (`0` error), *Play Mode testing*, dan otomatisasi Build Windows.

### B. Agent Skills yang Digunakan dalam Pembangunan
| Kategori | Skill | Kegunaan Utama |
| --- | --- | --- |
| **3D & Look Polish** | `craft-game-floors`, `urp-postprocessing`, `shader-graph-create-custom-node` | Menyetel pencahayaan *Golden Hour + Trilight + SSAO*, Bloom, shader minimap oktagon, dan standar kelayakan game (*core loop, audio, game feel*). |
| **Build & Pipeline** | `unity-cli`, `unity-pipeline`, `unity-package-management`, `fix-3d-physics-collisions` | Recompile otomatis, manajemen paket UPM, tes fisika/collider, dan build `.exe` 60+ FPS. |
| **UI & Font** | `ui`, `ui-ugui`, `ui-uitk`, `optimize-text-mesh-pro` | Penataan HUD *chamfered teal-emas* dengan font **Baloo 2** & **Nunito** (`Assets/_Project/Resources/Fonts/`). |
| **Audio & Audit** | `audio-setup-mixers`, `optimize-audio`, `project-auditor-fixes` | Routing *AudioMixer*, kompresi audio hemat RAM, dan audit performa sebelum merge ke `main`. |

> **Catatan tentang Meshy (Opsional):** Plugin Marketplace **Meshy** (*Text/Image-to-3D*) bersifat opsional dan memakai sistem **kredit** (gratis terbatas 100 kredit/bulan dengan lisensi CC BY 4.0, paket berbayar mulai ~\$20/bulan). Untuk proyek ini, kita **tidak wajib memakainya** karena seluruh aset 3D *Cozy Low-Poly* dapat dibuat gratis 100% lewat **Blender 5.3 + `mcp-for-blender`**.

---

Untuk panduan kontribusi dan daftar perintah Git harian secara lengkap, lihat [CONTRIBUTING.md](CONTRIBUTING.md).
