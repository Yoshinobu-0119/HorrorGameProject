using UnityEngine;
using UnityEngine.SceneManagement;

public class JudgeEyes : MonoBehaviour
{

    public void SceneNext(string NextScene)
    {
        SceneManager.LoadScene(NextScene);
    }

    public void EndGAME()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

}
