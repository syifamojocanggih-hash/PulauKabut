using UnityEngine;

namespace PulauKabut.Dialog
{
    /// <summary>
    /// Aset data percakapan NPC atau adegan kenangan yang mudah diisi Game Designer di Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogData", menuName = "Pulau Kabut/Dialog Data")]
    public class DialogData : ScriptableObject
    {
        [Tooltip("Nama pembicara yang tampil di kotak dialog (misal: Mbah Sauh).")]
        [SerializeField] private string speakerName = "Mbah Sauh";

        [Tooltip("Daftar kalimat percakapan yang ditampilkan berurutan.")]
        [TextArea(2, 4)]
        [SerializeField] private string[] lines = new string[]
        {
            "Selamat datang kembali di Pulau Kabut, Nak.",
            "Nyalakan lenteramu dengan tombol [F] untuk menyingkap jalan bayangan.",
            "Ikuti petunjuk arah menuju Menara Lentera di puncak pulau!"
        };

        public string SpeakerName => speakerName;
        public string[] Lines => lines;
    }
}
