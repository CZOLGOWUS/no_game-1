using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  UnityEngine.SceneManagement;

public class LevelControl : MonoBehaviour
{

    public void LoadTestLvL(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }
    
}
