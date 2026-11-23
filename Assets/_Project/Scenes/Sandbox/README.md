# Folder Sandbox (`Assets/_Project/Scenes/Sandbox/`)

Folder ini digunakan untuk scene uji coba dan pengembangan masing-masing fitur/aset agar tidak menimbulkan konflik pada scene utama.

## Daftar Scene Sandbox & Pemiliknya

Setiap scene sandbox dibuat dan dikelola oleh pemilik masing-masing:

| Nama Scene | Pemilik | Kegunaan |
| --- | --- | --- |
| `Sandbox_Player` | Programmer | Uji coba kontrol, pergerakan, dan kamera pemain |
| `Sandbox_Lantern` | Programmer | Uji coba mekanik lentera dan sistem kabut |
| `Sandbox_UI` | Game Designer / UI / Audio | Uji coba HUD, menu, dan tampilan dialog |
| `Sandbox_Characters` | 3D Artist | Uji coba model karakter, rig, material, dan animasi |
| `Sandbox_Environment` | 3D Artist | Uji coba aset lingkungan, props, pencahayaan, dan shader |

## Aturan Penting

- **Scene utama (`Assets/_Project/Scenes/Zone_Mangrove/`) hanya boleh diedit oleh Game Designer.**
- Anggota lain bekerja di **prefab** atau **scene sandbox** masing-masing, lalu menyerahkan prefab/komponen siap pakai untuk diintegrasikan ke scene utama.
- Jika terjadi konflik pada file scene atau prefab, **jangan menimpa secara asal**. Hubungi pemilik file dan selesaikan bersama.
