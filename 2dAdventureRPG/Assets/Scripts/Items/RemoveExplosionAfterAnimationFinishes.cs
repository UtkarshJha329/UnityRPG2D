using UnityEngine;

public class RemoveExplosionAfterAnimationFinishes : MonoBehaviour
{
    // Called using animation events.
    public void DestroyObjectUponAnimationCompletion()
    {
        Destroy(gameObject);
    }

}
