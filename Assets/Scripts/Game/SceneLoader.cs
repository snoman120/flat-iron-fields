using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlatIronFields.Game
{
    public sealed class SceneLoader : MonoBehaviour
    {
        public void LoadFarm()
        {
            SceneManager.LoadScene("Farm");
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
