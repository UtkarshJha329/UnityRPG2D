using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FinalSandToGrassConversionManager : MonoBehaviour
{
    private TurnGrassToSand grassSandConversionManager;

    public List<StructureHealth> healthsOfAllFinalStructures = new List<StructureHealth>();
    public Vector3Int startFinalConversionFromTile = Vector3Int.zero;

    public bool performFinalConversion = false;
    public bool once = true;

    private Transform playerTransform;
    private PlayerProperties s_PlayerProperties;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grassSandConversionManager = GameObject.FindGameObjectWithTag("GrassSandConversionManager").GetComponent<TurnGrassToSand>();
    }

    // Update is called once per frame
    void Update()
    {
        if (once)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            s_PlayerProperties = playerTransform.GetComponent<PlayerProperties>();
            grassSandConversionManager.finalPerformanceStartTile = startFinalConversionFromTile;
            once = false;
        }
        performFinalConversion = true;
        for (int i = 0; i < healthsOfAllFinalStructures.Count; i++)
        {
            if (healthsOfAllFinalStructures[i].structureCurrentHealth > 0)
            {
                performFinalConversion = false;
                break;
            }
        }

        if (performFinalConversion)
        {
            s_PlayerProperties.playerHitStopManager.StopTimeFor(5.0f, 0.1f);
            s_PlayerProperties.impulseSourceForScreenShake.GenerateImpulseWithVelocity(Random.insideUnitCircle * 0.05f);

            grassSandConversionManager.AddTileToTurnIntoGrassFinal(startFinalConversionFromTile);

            performFinalConversion = false;
        }
    }
}
