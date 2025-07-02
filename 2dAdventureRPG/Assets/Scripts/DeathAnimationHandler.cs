using System.Collections;
using UnityEditor.Animations;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DeathAnimationHandler : MonoBehaviour
{
    private Animator animator;

    private AudioSource deathSoundSource;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        deathSoundSource = GetComponent<AudioSource>();

        transform.localScale = Vector3.one * 2.0f;
    }

    public void BuryInTime(float pitch, EnemyType enemyType, float duration)
    {
        deathSoundSource.pitch = pitch;
        deathSoundSource.PlayOneShot(AllAudioContainer.enemyDeadBasedOnEnemyType[enemyType]);
        StartCoroutine(BuryIn(duration));
    }

    private IEnumerator BuryIn(float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetBool("isBurried", true);
    }

    public void DestroyAfterBurrialAnimationCompleted()
    {
        Destroy(gameObject);
    }
}
