using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public abstract class TileBase : MonoBehaviour
{
    MeshRenderer meshRenderer;
    public bool isPassed = false;
    public CharacterBase holdedCharacter;
    [SerializeField] private List<TileBase> neighbours;
    Vector3 localPosition;
    Coroutine hapticCoroutine;
    public int moveValue;
    public bool isInPath = false;
    [HideInInspector] public int pathIndex;

    bool doOnceShake = true;

    Material startingMaterial;

    public TileStatus currentTileStatus;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        startingMaterial = meshRenderer.material;
    }

    private void Start()
    {
        localPosition = transform.position;
    }

    public virtual bool IsTileFilled()
    {
        if (currentTileStatus == TileStatus.Blocked) return true;

        return holdedCharacter;
    }
    public void SetTileMaterial(Material material)
    {
        /*if (doOnceTile)
        {
            meshRenderer.material = material;
            doOnceTile = false;
        }
        else
        {
            if (currentTileStatus != TileStatus.Finish)
                meshRenderer.material = material;
        }*/
        meshRenderer.material = material;
    }

    public void SetTileColor(Color color)
    {
        if (currentTileStatus != TileStatus.Finish)
            meshRenderer.material.color = color;
    }

    public void SetStartingMaterial()
    {
        if (currentTileStatus != TileStatus.Finish)
            meshRenderer.material = startingMaterial;
    }

    public virtual void SetCharObject(CharacterBase characterObject)
    {
        holdedCharacter = characterObject;
    }

    public virtual void AddNeighbours(TileBase t)
    {
        if (!neighbours.Contains(t) && t != this)
            neighbours.Add(t);
    }

    public virtual bool CheckNeighbours(TileBase t)
    {
        return neighbours.Contains(t);
    }

    public virtual CharacterBase GetChar()
    {
        if (!IsTileFilled()) return null;

        SpawnedCharacter spawned = holdedCharacter.GetComponent<SpawnedCharacter>();

        if (spawned)
            return holdedCharacter;
        else
            return null;
    }

    public bool IsHoldedPlayer()
    {
        if (holdedCharacter == null)
            return false;

        CharacterObject co = holdedCharacter.GetComponent<CharacterObject>();

        if (co)
            return true;
        else
            return false;
    }

    public void ShakeTile()
    {
        if (doOnceShake)
        {
            doOnceShake = false;

            Tween myTween = transform.DOShakePosition(0.5f, new Vector3(0.3f, 0, 0.4f), 15, 1).OnComplete(() =>
            {
                doOnceShake = true;
            });

            if(hapticCoroutine == null)
                hapticCoroutine = StartCoroutine(ShakeHaptic());
        }

    }

    public IEnumerator ShakeHaptic()
    {
        int i = 0;
        while (i < 3)
        {
            EventManager.instance.SendHapticPulse(HapticType.MEDIUM);
            yield return new WaitForSeconds(0.15f);

            i++;
        }

        hapticCoroutine = null;
    }

    public bool CheckIncreaseTile(int leftMove)
    {
        if(currentTileStatus == TileStatus.Increase)
        {
            if(leftMove <= moveValue + 1)
            {
                return false;
            }

        }
        return true;
    }

    public virtual void IncreaseAndDecreaseTile(CharacterObject characterObject)
    {
        
    }

    public void ResetTile()
    {
        CharSpawner.Instance.ReturnToPool(GetChar());
        SetStartingMaterial();
        SetCharObject(null);
        

    }

}
