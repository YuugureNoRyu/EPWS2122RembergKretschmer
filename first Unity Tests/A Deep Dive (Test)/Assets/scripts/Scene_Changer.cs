using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  
public class Scene_Changer: MonoBehaviour {  
    public int SceneNumber=1;
    public void Menu() {  
        SceneManager.LoadScene(SceneNumber);  
    }  
    
}   