using UnityEngine;

public class reloadDone : MonoBehaviour
{
   private void ReloadDone()
   {
      PlayerManager.PlayerManagerInstance.snipeAnimator.SetBool("reload",false);
      PlayerManager.PlayerManagerInstance.reload = true;
   }
}
