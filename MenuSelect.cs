using UnityEngine;
using System.Collections;
//using UnityEngine.SceneManagement; PARA LA VERSION Unity5.3 


public class MenuSelect : MonoBehaviour {

	public void CambiarEscena(int EscenaAcambiar)
    {
        Debug.Log("Para ver en consola");
        Application.LoadLevel (EscenaAcambiar); //version 5.3: SceneManager.LoadScene (int)
    }
}
