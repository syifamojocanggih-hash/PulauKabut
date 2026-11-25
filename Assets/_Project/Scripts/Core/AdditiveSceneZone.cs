using PulauKabut.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PulauKabut.Core
{
    /// <summary>
    /// Trigger streaming Additive Scene untuk skala dunia besar (5 Wilayah + Hub + Candi Bawah)
    /// agar pemakaian VRAM tetap di bawah 1 GB dan FPS stabil di atas 60 FPS pada GPU 4 GB VRAM.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AdditiveSceneZone : MonoBehaviour
    {
        [Header("Scene Streaming")]
        [Tooltip("Nama scene wilayah yang akan dimuat secara Additive saat pemain memasuki zona ini.")]
        [SerializeField] private string sceneToLoad = "";

        [Tooltip("Opsional: nama scene wilayah lama yang akan dilepas (Unload) untuk menghemat VRAM.")]
        [SerializeField] private string sceneToUnload = "";

        private bool _isLoading;

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isLoading || !other.TryGetComponent<PlayerController>(out _))
            {
                return;
            }

            if (!string.IsNullOrEmpty(sceneToLoad) && !IsSceneLoaded(sceneToLoad))
            {
                _isLoading = true;
                AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
                if (op != null)
                {
                    op.completed += _ => _isLoading = false;
                }
                else
                {
                    _isLoading = false;
                }
            }

            if (!string.IsNullOrEmpty(sceneToUnload) && IsSceneLoaded(sceneToUnload))
            {
                SceneManager.UnloadSceneAsync(sceneToUnload);
            }
        }

        private static bool IsSceneLoaded(string sceneName)
        {
            int count = SceneManager.sceneCount;
            for (int i = 0; i < count; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.name == sceneName && s.isLoaded)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
