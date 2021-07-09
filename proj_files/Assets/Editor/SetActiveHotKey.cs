using UnityEditor;
using UnityEngine;
using System.Collections;

public class EditorHelper : MonoBehaviour
{
 [MenuItem("GameObject/Toggle Selected GameObject _g")]
static void ToggleGameObject ()
{
  //var selected = Selection.transforms;
  if( Selection.transforms != null )
  {
    foreach( Transform t in Selection.transforms)
      t.gameObject.SetActive( !t.gameObject.activeInHierarchy );
  }
}
}