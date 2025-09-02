using UnityEngine;
using TMPro;

public class TextEffects : MonoBehaviour
{
    public enum TextEffect
    {
        Wave,
        Jitter
    }

    public TMP_Text tmpTextComponent;
    public TextEffect textEffect;

    private void Awake()
    {
        tmpTextComponent = GetComponent<TMP_Text>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tmpTextComponent.ForceMeshUpdate();

        for (int i = 0; i < tmpTextComponent.textInfo.characterCount; i++)
        {
            TMP_CharacterInfo currentCharacterInfo = tmpTextComponent.textInfo.characterInfo[i];
            if (!currentCharacterInfo.isVisible)
            {
                continue;
            }

            Vector3[] vertices = tmpTextComponent.textInfo.meshInfo[currentCharacterInfo.materialReferenceIndex].vertices;
            for (int j = 0; j < 4; j++)
            {
                Vector3 originalVertexPosition = vertices[currentCharacterInfo.vertexIndex + j];
                vertices[currentCharacterInfo.vertexIndex + j] = originalVertexPosition + ReturnVertexOffsetBasedOnEffect(textEffect, originalVertexPosition);
            }
        }

        for (int i = 0; i < tmpTextComponent.textInfo.meshInfo.Length; i++)
        {
            tmpTextComponent.textInfo.meshInfo[i].mesh.vertices = tmpTextComponent.textInfo.meshInfo[i].vertices;
            tmpTextComponent.UpdateGeometry(tmpTextComponent.textInfo.meshInfo[i].mesh, i);
        }


    }

    private Vector3 ReturnVertexOffsetBasedOnEffect(TextEffect textEffect, Vector3 originalPosition)
    {
        if(textEffect == TextEffect.Jitter)
        {
            return VertexOffsetForJitterText(1.5f);
        }
        if(textEffect == TextEffect.Wave)
        {
            return VertexOffsetForWaveText(originalPosition.x);
        }

        return Vector3.zero;
    }

    private Vector3 VertexOffsetForWaveText(float vertexXPos)
    {
        return new Vector3(0.0f, Mathf.Sin(Time.time * 2.0f + vertexXPos * 0.01f) * 10.0f, 0.0f);
    }

    private Vector3 VertexOffsetForJitterText(float jitterStrength)
    {
        return new Vector3(Random.Range(-jitterStrength, jitterStrength), Random.Range(-jitterStrength, jitterStrength), 0.0f);
    }
}
