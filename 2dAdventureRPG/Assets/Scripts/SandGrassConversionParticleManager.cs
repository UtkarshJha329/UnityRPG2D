using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(ParticleSystem))]
public class SandGrassConversionParticleManager : MonoBehaviour
{
    public bool emitSandParticles = false;
    public ParticleSystem sandGrassConversionParticleSystem;

    private void Awake()
    {
        sandGrassConversionParticleSystem = GetComponent<ParticleSystem>();
        if (emitSandParticles)
        {
            var textureSheetAnimation = sandGrassConversionParticleSystem.textureSheetAnimation;
            textureSheetAnimation.startFrame = 1;
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //{
        //    sandGrassConversionParticleSystem.Play();
        //}
        if (!sandGrassConversionParticleSystem.isPlaying)
        {
            gameObject.SetActive(false);
        }
    }

    public bool IsParticleSystemPlaying()
    {
        return sandGrassConversionParticleSystem.isPlaying;
    }

    public void PlayGrassParticles(Vector3 position)
    {
        transform.position = position;
        var textureSheetAnimation = sandGrassConversionParticleSystem.textureSheetAnimation;
        textureSheetAnimation.startFrame = 0;
        gameObject.SetActive(true);
        sandGrassConversionParticleSystem.Play();
    }

    public void PlaySandParticles(Vector3 position)
    {
        transform.position = position;
        var textureSheetAnimation = sandGrassConversionParticleSystem.textureSheetAnimation;
        textureSheetAnimation.startFrame = 1;
        gameObject.SetActive(true);
        sandGrassConversionParticleSystem.Play();
    }

}
