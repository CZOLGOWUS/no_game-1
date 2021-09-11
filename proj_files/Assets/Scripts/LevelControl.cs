using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace noGame.UIControl
{
    public class LevelControl : Singleton<LevelControl>
    {

        public void LoadTestLvL( int levelIndex )
        {
            SceneManager.LoadScene( levelIndex );
        }

    }
}