using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class Hexagon : MonoBehaviour
{
    public HexagonColor color;
    public bool markedForDestruction;
    
    [SerializeField] private int colorCount;
    [SerializeField] private int minBombMoves;
    [SerializeField] private int maxBombMoves;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private SpriteRenderer explosionCounter;
    
    public ParticleSystem particlesOnDestroy;
    
    public bool isBomb;
    public int movesBeforeExplosion;

    private static Random rand = new Random();

    private void Awake()
    {
        SetRandomColor();
        markedForDestruction = false;
        NotBomb();
    }

    public void SetRandomColor()
    {
        int randomColorIndex = rand.Next(Math.Min(GameController.Instance.colorMaterials.Length, colorCount));
        meshRenderer.material = GameController.Instance.colorMaterials[randomColorIndex];
        color = (HexagonColor) Enum.GetValues(typeof(HexagonColor)).GetValue(randomColorIndex);
    }

    public void SetAs(Hexagon other)
    {
        color = other.color;
        meshRenderer.material = other.meshRenderer.material;
        isBomb = other.isBomb;
        if (isBomb)
        {
            movesBeforeExplosion = other.movesBeforeExplosion;
            MakeBomb(false);
        }
        else
        {
            NotBomb();
        }
    }
    
    public void NewRandomHexagon()
    {
        markedForDestruction = false;
        SetRandomColor();
        NotBomb();
        
    }
    
    public void MakeBomb(bool _isNew)
    {
        isBomb = true;
        if (_isNew)
        {
            movesBeforeExplosion = rand.Next(minBombMoves, maxBombMoves);
        }
        explosionCounter.gameObject.SetActive(true);
        explosionCounter.sprite = GameController.Instance.bombNumberSprites[movesBeforeExplosion];
        GameController.Instance.bombs.Add(this);
    }

    void NotBomb()
    {
        isBomb = false;
        explosionCounter.gameObject.SetActive(false);
        GameController.Instance.bombs.Remove(this);
    }

    public void BombTick()
    {
        if (isBomb)
        {
            movesBeforeExplosion--;
            if (explosionCounter != null)
            {
                explosionCounter.sprite = GameController.Instance.bombNumberSprites[movesBeforeExplosion];
            }
            if (movesBeforeExplosion <= 0)
            {
                print("bomb exploded");
                GameController.Instance.ExplodeBomb();
            }
        }
    }
}
