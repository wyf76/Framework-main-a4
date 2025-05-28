using UnityEngine;
using UnityEngine.SceneManagement;

public class ClassSelection : MonoBehaviour
{
    public void SelectClass(string className)
    {
        GameManager.Instance.SetClass(className);
        SceneManager.LoadScene("Main");
    }
}