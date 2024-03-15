using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour{
    public string sceneName;

    void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            SceneManager.LoadScene(sceneName);
        }
    }
}