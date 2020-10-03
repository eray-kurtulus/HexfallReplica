using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointPoint : MonoBehaviour
{
    public int horizontalIndex;
    public int verticalIndex;
    public Hexagon[] adjointHexagons = new Hexagon[3];
    public SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        Color tempColor = spriteRenderer.color;
        tempColor.a = 0f;
        spriteRenderer.color = tempColor;
    }

    public void SetIndices(int _horizontalIndex, int _verticalIndex)
    {
        horizontalIndex = _horizontalIndex;
        verticalIndex = _verticalIndex;
    }

    public void SelectJoint()
    {
        Color tempColor = spriteRenderer.color;
        tempColor.a = 1f;
        spriteRenderer.color = tempColor;

        foreach (var hex in adjointHexagons)
        {
            hex.transform.position += 3 * Vector3.back;
        }
    }

    public void DeselectJoint()
    {
        Color tempColor = spriteRenderer.color;
        tempColor.a = 0f;
        spriteRenderer.color = tempColor;

        foreach (var hex in adjointHexagons)
        {
            hex.transform.position += 3 * Vector3.forward;
        }
    }

    public void RotateCounterClockwise()
    {
        Hexagon tempHexagon = Instantiate(adjointHexagons[0]);
        
        tempHexagon.SetAs(adjointHexagons[0]);
        adjointHexagons[0].SetAs(adjointHexagons[1]);
        adjointHexagons[1].SetAs(adjointHexagons[2]);
        adjointHexagons[2].SetAs(tempHexagon);
        
        Destroy(tempHexagon.gameObject);
    }
    
    public void RotateClockwise()
    {
        Hexagon tempHexagon = Instantiate(adjointHexagons[0]);

        tempHexagon.SetAs(adjointHexagons[0]);
        adjointHexagons[0].SetAs(adjointHexagons[2]);
        adjointHexagons[2].SetAs(adjointHexagons[1]);
        adjointHexagons[1].SetAs(tempHexagon);
        
        Destroy(tempHexagon.gameObject);
    }

    // returns true if all adjoint hexagons are the same color, and marks them,
    // returns false if they are not the same color
    public bool CheckAndMarkHexagons()
    {
        HexagonColor color = adjointHexagons[0].color;
        for (int i = 1; i < 3; i++)
        {
            if (adjointHexagons[i].color != color)
            {
                return false;
            }
        }

        // check is successful, all hexagons are the same color
        // mark them and return true
        foreach (var hex in adjointHexagons)
        {
            hex.markedForDestruction = true;
            hex.particlesOnDestroy.Play();
        }

        return true;
    }
    
    public bool CheckIfMarkableHexagons()
    {
        HexagonColor color = adjointHexagons[0].color;
        for (int i = 1; i < 3; i++)
        {
            if (adjointHexagons[i].color != color)
            {
                return false;
            }
        }

        return true;
    }
}
