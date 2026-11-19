using UnityEngine;
using UnityEngine.SceneManagement;

public class JudgeEyes : MonoBehaviour
{

    public void SceneNext(string NextScene)
    {
        SceneManager.LoadScene(NextScene);
    }



}
