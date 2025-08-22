using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



[RequireComponent(typeof(Image))]
[RequireComponent(typeof(AudioSource))]
public class MainMenuMouseInImageManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private enum UISpriteType
    {
        Player,
        Enemy,
        Structure
    }

    [SerializeField] private float alphaHitThreshold = 0.75f;


    [SerializeField] private UISpriteType uiSpriteType; 
    [SerializeField] private EnemyType enemyType;

    private Image imageToDetectHoverOver;
    private AudioSource audioSource;

    private void Awake()
    {
        imageToDetectHoverOver = GetComponent<Image>();
        audioSource = GetComponent<AudioSource>();

        imageToDetectHoverOver.alphaHitTestMinimumThreshold = alphaHitThreshold;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayAppropriateHoverAudio();
        imageToDetectHoverOver.color = Color.gray;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        imageToDetectHoverOver.color = Color.white;
    }

    private void PlayAppropriateHoverAudio()
    {
        if (audioSource.isPlaying)
        {
            return;
        }

        if(uiSpriteType == UISpriteType.Player)
        {
            audioSource.PlayOneShot(AllAudioContainer.swishSoundEffects[GameObjectType.Player][Random.Range(0, AllAudioContainer.swishSoundEffects[GameObjectType.Player].Count)], Random.Range(0.65f, 0.75f));
            audioSource.PlayOneShot(AllAudioContainer.knightWalkingInArmourSoundEffects[Random.Range(0, AllAudioContainer.knightWalkingInArmourSoundEffects.Count)], Random.Range(0.45f, 0.75f));

            audioSource.PlayOneShot(AllAudioContainer.enemyDeadBasedOnEnemyType[EnemyType.TorchGoblin], Random.Range(0.25f, 0.35f));
            audioSource.PlayOneShot(AllAudioContainer.enemyDeadBasedOnEnemyType[EnemyType.TNTBarrelGoblin], Random.Range(0.25f, 0.35f));
            audioSource.PlayOneShot(AllAudioContainer.enemyDeadBasedOnEnemyType[EnemyType.BombGoblin], Random.Range(0.25f, 0.35f));
        }
        else if(uiSpriteType == UISpriteType.Structure)
        {
            audioSource.PlayOneShot(AllAudioContainer.structureCollapseAudioClips[Random.Range(0, AllAudioContainer.structureCollapseAudioClips.Count)], Random.Range(0.45f, 0.65f));
        }
        else if(uiSpriteType == UISpriteType.Enemy)
        {
            if(enemyType == EnemyType.TorchGoblin)
            {
                audioSource.pitch = Random.Range(0.75f, 1.25f);
                audioSource.PlayOneShot(AllAudioContainer.enemyHurtBasedOnEnemyType[EnemyType.TorchGoblin], Random.Range(0.25f, 0.75f));
            }
            else if(enemyType == EnemyType.TNTBarrelGoblin)
            {
                audioSource.pitch = Random.Range(0.75f, 1.25f);
                audioSource.PlayOneShot(AllAudioContainer.enemyHurtBasedOnEnemyType[EnemyType.TNTBarrelGoblin], Random.Range(0.25f, 0.75f));
            }
            else if(enemyType == EnemyType.BombGoblin)
            {
                audioSource.pitch = Random.Range(0.75f, 1.25f);
                audioSource.PlayOneShot(AllAudioContainer.enemyHurtBasedOnEnemyType[EnemyType.BombGoblin], Random.Range(0.25f, 0.75f));
            }
        }
    }
}
