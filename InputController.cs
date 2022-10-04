using System;
using System.Collections;
using System.Collections.Generic;
using _Sercan.CommandPattern;
using UnityEngine;
using DG.Tweening;

namespace _Sercan
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private CharacterObject selectedChar;
        [SerializeField] private LayerMask tileLayer;
        //[SerializeField] private LayerMask cloneLayer;
        [SerializeField] private float speed = 0.1f;
        private bool moveCompleted;

        LevelEndMover levelEndMover;

        //public List<List> paths = new();

        public bool openClonesText;
        public bool makeGradientColor;

        public static bool openClonesTextStatic;
        private bool levelCompleted = false;
        private void OnEnable()
        {
            InGameEventManager.OnClickTheObject += InGameEventManagerOnOnClickTheObject;
            InGameEventManager.OnLevelCompleted += InGameEventManagerOnOnLevelCompleted;
            EventManager.OnNextLevel += OnNextLevel;
        }

        private void InGameEventManagerOnOnLevelCompleted(TileWinningController obj, CharacterObject characterobject)
        {
            levelCompleted = true;
        }

        private void OnDisable()
        {
            InGameEventManager.OnClickTheObject -= InGameEventManagerOnOnClickTheObject;
            InGameEventManager.OnLevelCompleted -= InGameEventManagerOnOnLevelCompleted;
            EventManager.OnNextLevel -= OnNextLevel;
        }

        private void OnNextLevel()
        {
            levelCompleted = false;
        }

        private void Awake()
        {
            openClonesTextStatic = openClonesText;
        }

        private void Start()
        {
            levelEndMover = FindObjectOfType<LevelEndMover>();
        }

        void Update()
        {
            if(levelCompleted) return;
            
            if (selectedChar && Input.GetMouseButton(0))
            {
                DragSelectedObject();
            }
            if (selectedChar == null && Input.GetMouseButtonDown(0))
            {
                ClickToPath();
            }
            if (Input.GetMouseButtonUp(0))
            {
                selectedChar = null;
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                HyperMonkUI.UIEventManager.instance.RestartGame();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                EventManager.instance.LevelComplete();
            }
        }

        private void ClickToPath()
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity,
                tileLayer))
            {
                TileBase hitTile = hit.transform.GetComponent<TileBase>();

                if (hitTile)
                {
                    if (hitTile.isInPath)
                    {
                        TapToGoBack(hitTile);
                        selectedChar = null; 
                    }
                }
            }
        }

        private void TapToGoBack(TileBase hitTile)
        {
            for (int i = 0; i < levelEndMover.playerCount.Count; i++)
            {
                Debug.Log(i);
                Debug.Log(levelEndMover.playerCount.Count);
               // Debug.Log("hit tile name: " + hitTile.name);
                Debug.Log("player name: " + levelEndMover.playerCount[i].name);

                if(levelEndMover.playerCount[i].CheckFirstTile(hitTile))
                {
                    selectedChar = levelEndMover.playerCount[i];

                    Debug.Log("currenttile name: " + selectedChar.currentTile.name);

                    if (selectedChar.currentTile.currentTileStatus == TileStatus.Increase)
                    {
                        selectedChar.currentTile.isPassed = !selectedChar.currentTile.isPassed;

                        selectedChar.leftMove -= selectedChar.currentTile.moveValue;

                        selectedChar.RefreshLeftMoveText(selectedChar.leftMove);
                    }

                    //Debug.Log("tikladi");

                    int pathIndexCount = selectedChar.path.Count-1;

                    

                    MoveBackMethod(selectedChar.currentTile,null,false);
                    for (int j = pathIndexCount; j > hitTile.pathIndex; j--)
                    {
                        MoveBackMethod(selectedChar.path[j],null,false);
                    }
                    MoveBackMethod(hitTile);

                    selectedChar.ChangeLeftMove(-1);
                    selectedChar.moveCountBackward--;
                    selectedChar.moveCountForward++;

                    selectedChar.transform.position = hitTile.transform.position + new Vector3(0, 1.05f, 0);

                    break;
                }
            }
        }

        private void DragSelectedObject()
        {
            if (Input.GetMouseButton(0) && moveCompleted)
            {
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity,
                    tileLayer) && moveCompleted)
                {
                    //Logger.Error(hit.transform.name);
                    TileBase tileBase = hit.transform.GetComponent<TileBase>();
                    
                    MoveCommand command = new MoveCommand(CommandType.ForwardMove,hit, tileBase, () =>
                    {
                        DragObject(hit, null);
                    });
                    
                    
                    EventManager.instance.AddCommand(CommandType.ForwardMove,command,tileBase);
                    
                }
            }
            
        }

        void DragObject(RaycastHit hit,Action completedCallback = null)
        {
            TileBase hitTile = hit.transform.GetComponent<TileBase>();

            if (selectedChar.currentTile.CheckNeighbours(hitTile))
            {
                if (selectedChar.CheckLastTile(hitTile))
                {
                    MoveBackMethod(hitTile,null);
                }
                else if (!hitTile.IsTileFilled() && selectedChar.leftMove > 1)
                {
                    if (hitTile.currentTileStatus == TileStatus.Finish)
                    {
                        TileWinningController winningTile = hitTile.GetComponent<TileWinningController>();

                        if (selectedChar.currentPlayerColorEnum != winningTile.currentTileColor)
                        {  
                            CantMoveFeedback(hitTile);
                            EventManager.instance.CommandExecuted(CommandType.ForwardMove);
                            return;
                        } 
                    }
                    else if(hitTile.currentTileStatus == TileStatus.Increase)
                    {

                        if (selectedChar.leftMove + hitTile.moveValue <= 1)
                        {
                            CantMoveFeedback(hitTile);
                            EventManager.instance.CommandExecuted(CommandType.ForwardMove);
                            return;
                        }
                    }

                    MoveForwardMethod(hitTile);
                }
                else
                {
                    CantMoveFeedback(hitTile);
                    EventManager.instance.CommandExecuted(CommandType.ForwardMove);
                }
            }
            else
                EventManager.instance.CommandExecuted(CommandType.ForwardMove);
        }

        void MoveBackMethod(TileBase hitTile, Action completedCallBack = null,bool isMove = true)
        {
            //Debug.LogError("MoveStart: " + hitTile);
            if (isMove)
            {
                Vector3 target = hitTile.transform.position + new Vector3(0, 1.05f, 0);
                moveCompleted = false;
                selectedChar.MoveTarget(target, speed, () =>
                {
                    //completedCallBack?.Invoke();
                    EventManager.instance.CommandExecuted(CommandType.ForwardMove);
                    //Debug.LogError("MoveEnd");
                    MovementCompleted();

                });
                
                selectedChar.GetComponent<CharacterObject>().childs.Remove(hitTile.GetChar());
            
                CharSpawner.Instance.ReturnToPool(hitTile.GetChar());

                selectedChar.moveCountBackward++;
                selectedChar.moveCountForward--;

                if(makeGradientColor)
                    selectedChar.GradientColorChange(false);
            
                hitTile.SetCharObject(null);
                selectedChar.currentTile.SetStartingMaterial();
                selectedChar.currentTile.SetCharObject(null);


                selectedChar.ChangeLeftMove(1);

                if (selectedChar.currentTile.currentTileStatus == TileStatus.Increase)
                {
                    selectedChar.currentTile.IncreaseAndDecreaseTile(selectedChar);
                }

                selectedChar.RemoveTileToPath(hitTile);
            }
            else
            {

                selectedChar.GetComponent<CharacterObject>().childs.Remove(hitTile.GetChar());

                CharSpawner.Instance.ReturnToPool(hitTile.GetChar());

                selectedChar.moveCountBackward++;
                selectedChar.moveCountForward--;

                if (makeGradientColor)
                    selectedChar.GradientColorChange(false);

                hitTile.SetCharObject(null);
                hitTile.SetStartingMaterial();
                //hitTile.SetCharObject(null);
                selectedChar.ChangeLeftMove(1);

                if (hitTile.currentTileStatus == TileStatus.Increase)
                {
                    hitTile.IncreaseAndDecreaseTile(selectedChar);
                }

                selectedChar.RemoveTileToPath(hitTile);
            }


            //selectedChar.backwardParticle.Play();

        }

        private void MoveForwardMethod(TileBase hitTile,Action completed = null)
        {
            selectedChar.moveCountForward++;

            moveCompleted = false;

            if (selectedChar.moveCountBackward > 1)
                selectedChar.moveCountBackward--;


            if(makeGradientColor)
                selectedChar.GradientColorChange(true);


            CharacterBase obj = CharSpawner.Instance.Get();
            selectedChar.GetComponent<CharacterObject>().childs.Add(obj);
            if (selectedChar.childs.Count == 1)
            {
                obj.GetComponent<SpawnedCharacter>().OpenRetryButton();
            }
        

            InitializeClone(obj);

            hitTile.SetCharObject(obj);

            selectedChar.ChangeLeftMove(0);

            if (hitTile.currentTileStatus == TileStatus.Increase)
            {
                hitTile.IncreaseAndDecreaseTile(selectedChar);
            }

            //adamlar renkli hali
            Material objMaterial = obj.GetMaterial();

            //tek renkli için
            //Material tileMat = selectedChar.GetMaterial();

            if (hitTile.currentTileStatus != TileStatus.Finish)
            {
                hitTile.SetTileMaterial(objMaterial);

                hitTile.SetTileColor(objMaterial.color);
            }
            
            Vector3 targetTransform = hitTile.transform.position + new Vector3(0, 1.05f, 0);

            selectedChar.MoveTarget(targetTransform, speed, ()=>
            {
                EventManager.instance.CommandExecuted(CommandType.ForwardMove);
                MovementCompleted();
            });

            selectedChar.PlayForwardParticleEffect();

            if (!selectedChar.CheckPathList(selectedChar.currentTile))
            {
                selectedChar.AddTileToPath(selectedChar.currentTile);
            }


        }

        public void InitializeClone(CharacterBase obj)
        {
            obj.SetCharMaterial(selectedChar.GetMaterial());
            obj.SetSkinnedMeshMaterial(selectedChar.GetMaterial());

            obj.SetColliderActive();

            if(makeGradientColor)
                obj.SetCharColor(selectedChar.currentCharColor);

            obj.transform.position = selectedChar.transform.position + new Vector3(0, 0.02f, 0);

        }

        

        public void CantMoveFeedback(TileBase hitTile)
        {
            hitTile.ShakeTile();
        }

        void MovementCompleted()
        {
            moveCompleted = true;

            EventManager.instance.SendHapticPulse(HapticType.LIGHT);
        }
        
        private void InGameEventManagerOnOnClickTheObject(CharacterObject obj)
        {
            selectedChar = obj;
            moveCompleted = true;

        }

    }
    
}
