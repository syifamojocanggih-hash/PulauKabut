# Panduan Kontribusi & Aturan Kerja — Pulau Kabut

Dokumen ini menjelaskan alur kerja Git dan aturan kolaborasi tim **Pulau Kabut**. Alur kerja kita **TANPA Pull Request**: perubahan dari branch kerja digabungkan langsung ke `dev` menggunakan `git merge`, lalu `dev` digabungkan langsung ke `main` setelah lolos tes build mingguan.

---

## Alur Kerja Harian

1. **Perbarui branch `dev` lokal:**
   ```bash
   git checkout dev
   git pull origin dev
   ```

2. **Bawa perubahan terbaru `dev` ke branch kerja Anda:**
   ```bash
   git checkout nama-branch-kerja
   git merge dev
   ```

3. **Kerjakan tugas dengan commit kecil dan rutin**, lalu push branch kerja ke GitHub setiap selesai sesi:
   ```bash
   git add .
   git commit -m "feat: deskripsi singkat perubahan"
   git push origin nama-branch-kerja
   ```

4. **Gabungkan ke `dev` setelah fitur selesai** dan proyek dapat dibuka di Unity tanpa error:
   ```bash
   git checkout dev
   git pull origin dev
   git merge --no-ff nama-branch-kerja
   git push origin dev
   ```

5. **Beri tahu tim di grup** bahwa `dev` sudah diperbarui, lalu semua anggota segera menjalankan:
   ```bash
   git checkout dev
   git pull origin dev
   ```

6. **Hapus branch kerja yang sudah di-merge** (lokal dan remote):
   ```bash
   git branch -d nama-branch-kerja
   git push origin --delete nama-branch-kerja
   ```

7. **Tes build rutin tiap Jumat dari `dev`:**
   - Jika lolos (**60 FPS rata-rata** di laptop acuan, tanpa crash):
     ```bash
     git checkout main
     git pull origin main
     git merge --no-ff dev
     git push origin main
     git tag v0.1
     git push origin v0.1
     ```
     *(Ganti `v0.1` dengan nomor versi yang sesuai.)*

8. **Jika tes build gagal:**
   - Buat branch `fix/nama-bug` dari `dev`:
     ```bash
     git checkout dev
     git pull origin dev
     git checkout -b fix/nama-bug
     ```
   - Perbaiki bug, uji ulang di Unity, lalu gabungkan kembali ke `dev` dengan cara yang sama (langkah 4–6).
   - **`main` tidak disentuh sampai tes build lolos.**

---

## Aturan Kolaborasi

- **Tidak ada commit langsung ke `main` atau `dev`.** Semua perubahan masuk melalui merge dari branch kerja.
- **Cek kualitas sebelum merge ke `dev`:** pastikan sudah menjalankan `git pull origin dev` dan proyek dapat dibuka di Unity **tanpa error**, **tanpa missing reference**, dan **scene masih bisa dimainkan**.
- **Jangan pernah `git push --force`** ke `main` atau `dev`.
- **Satu branch = satu pekerjaan.** Lakukan merge kecil dan sering (maksimal seminggu sekali).
- **Scene utama (`Zone_Mangrove`) hanya diedit oleh pemilik scene (Game Designer).** Pekerjaan lain dilakukan di prefab atau scene sandbox (`Assets/_Project/Scenes/Sandbox/`).
- **Konflik pada scene atau prefab:** jika terjadi konflik pada file `.unity` atau `.prefab`, **jangan menimpa secara asal**. Hubungi pemilik file dan selesaikan bersama.
- **Format pesan commit wajib menggunakan awalan:**
  - `feat: ...` — fitur atau sistem gameplay baru
  - `art: ...` — aset 3D, tekstur, material, animasi, shader, VFX
  - `audio: ...` — musik, SFX, ambience
  - `fix: ...` — perbaikan bug
  - `docs: ...` — dokumentasi (README, panduan, catatan)
- **Branch yang sudah di-merge harus dihapus** agar daftar branch tetap bersih.
