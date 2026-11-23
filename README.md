# Pulau Kabut

**Pulau Kabut** adalah game petualangan 3D *stylized* yang dikembangkan oleh tim beranggotakan 3 orang (**Programmer**, **3D Artist**, dan **Game Designer / UI / Audio**).

---

## Informasi Proyek

- **Engine**: Unity 6 (`6000.6.10f1`) — Universal Render Pipeline (URP)
- **Repository**: https://github.com/syifamojocanggih-hash/PulauKabut.git
- **Tim**:
  - **Programmer**: Mekanik pemain, lentera & kabut, sistem dialog/UI, arsitektur inti
  - **3D Artist**: Karakter, lingkungan, props, material, tekstur, animasi
  - **Game Designer / UI / Audio**: Desain level (`Zone_Mangrove`), UI/UX, narasi/dialog, musik & SFX

---

## Prasyarat & Instalasi Git LFS

Proyek ini menggunakan **Git LFS (Large File Storage)** untuk menyimpan aset biner besar (`*.fbx`, `*.blend`, `*.png`, `*.jpg`, `*.psd`, `*.tga`, `*.exr`, `*.wav`, `*.mp3`, `*.ogg`, `*.mp4`, `*.ttf`, `*.otf`).

1. Install Git dan Git LFS dari https://git-lfs.com/ (atau lewat package manager OS masing-masing).
2. Aktifkan Git LFS sekali di komputer Anda:
   ```bash
   git lfs install
   ```

---

## Cara Clone dan Membuka Proyek

1. Clone repository:
   ```bash
   git clone https://github.com/syifamojocanggih-hash/PulauKabut.git
   cd PulauKabut
   ```
2. Pastikan semua file Git LFS terunduh:
   ```bash
   git lfs pull
   ```
3. Pindah ke branch `dev` sebelum mulai bekerja:
   ```bash
   git checkout dev
   ```
4. Buka proyek melalui **Unity Hub** menggunakan **Unity `6000.6.10f1` (Unity 6 URP)**.

### Pengaturan Wajib di Unity Editor

Setiap anggota tim **wajib** memastikan pengaturan berikut aktif di Unity agar file proyek ramah terhadap Git dan tidak kehilangan referensi aset:

- `Edit > Project Settings > Editor > Asset Serialization` = **Force Text**
- `Edit > Project Settings > Editor > Version Control Mode` = **Visible Meta Files**

> Jangan pernah menghapus atau mengabaikan file `.meta`. Selalu pindahkan atau ubah nama aset dari dalam jendela Project di Unity Editor.

---

## Struktur Folder Utama

```text
Assets/_Project/
├── Art/
│   ├── Characters/
│   ├── Environment/
│   ├── Props/
│   ├── Materials/
│   └── Textures/
├── Animations/
├── Audio/
│   ├── Music/
│   ├── SFX/
│   └── Ambience/
├── Prefabs/
│   ├── Player/
│   ├── NPC/
│   ├── Props/
│   └── VFX/
├── Scenes/
│   ├── Zone_Mangrove/   # Scene utama (hanya diedit Game Designer)
│   └── Sandbox/         # Scene uji coba per pemilik (lihat Sandbox/README.md)
├── Scripts/
│   ├── Player/
│   ├── Lantern/
│   ├── Dialog/
│   ├── UI/
│   └── Core/
├── Shaders/
├── UI/
├── VFX/
└── Dialog/
```

---

## Daftar Branch

### Branch Utama
- `main` — Branch rilis stabil. Diperbarui hanya dari `dev` setelah tes build mingguan lolos.
- `dev` — Branch integrasi utama tempat semua branch kerja digabungkan.

### Branch Kerja Awal (dibuat dari `dev`)
- `feat/player-movement` — Pergerakan dan kontrol pemain
- `feat/lantern-fog` — Mekanik lentera dan kabut
- `feat/ui-dialog` — Sistem UI dan dialog
- `art/characters` — Aset 3D dan material karakter
- `art/environment` — Aset 3D dan material lingkungan
- `audio/sfx-music` — Musik latar, SFX, dan ambience
- `level/greybox-mangrove` — Greybox dan tata letak level `Zone_Mangrove`

### Branch yang Dibuat Belakangan
- `feat/shards-story` — Dibuat belakangan setelah fitur inti dan struktur level dasar siap.
- `release/demo-v1` — Dibuat belakangan saat memasuki tahap persiapan rilis demo v1.

---

## Alur Kerja & Aturan Kolaborasi

Alur kerja tim ini **TANPA Pull Request**:
- Semua pekerjaan dilakukan di branch kerja masing-masing, lalu digabungkan langsung ke `dev` menggunakan `git merge --no-ff`.
- Setiap Jumat dilakukan tes build dari `dev`. Jika lolos (rata-rata 60 FPS di laptop acuan dan tanpa crash), `dev` digabungkan langsung ke `main` dan diberi tag versi (misalnya `v0.1`).
- **Dilarang commit langsung** ke `main` atau `dev`, dan **dilarang keras** melakukan `git push --force` ke `main` atau `dev`.
- **Scene utama (`Zone_Mangrove`)** hanya boleh diedit oleh Game Designer. Anggota lain bekerja di prefab atau scene sandbox.

Untuk panduan langkah demi langkah harian, format commit, dan aturan lengkap, baca [CONTRIBUTING.md](CONTRIBUTING.md).
