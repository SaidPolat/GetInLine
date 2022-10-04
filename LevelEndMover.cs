using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace _Sercan
{
    public class LevelEndMover : MonoBehaviour
    {
        //public List<Transform> path;
        //public List<CharacterBase> allCharacters;
        public List<CharacterObject> playerCount;
        public int enterCount;

        private void OnEnable()
        {
            
            InGameEventManager.OnLevelCompleted += InGameEventManagerOnLevelCompleted;

            InGameEventManager.OnLevelLoaded += MyStart;

            InGameEventManager.OnWinningCondition += InGameEventManagerOnOnWinningCondition;
        }

        private void OnDisable()
        {
            InGameEventManager.OnLevelCompleted -= InGameEventManagerOnLevelCompleted;

            InGameEventManager.OnLevelLoaded -= MyStart;

            InGameEventManager.OnWinningCondition -= InGameEventManagerOnOnWinningCondition;

        }

        public void MyStart()
        {
            enterCount = 0;

            playerCount.Clear();

            playerCount = FindObjectsOfType<CharacterObject>().ToList();
        }

        private void InGameEventManagerOnOnWinningCondition(TileWinningController obj, CharacterObject characterobject)
        {
            StartCoroutine(StartWaveMove(characterobject));
        }

        private void InGameEventManagerOnLevelCompleted(TileWinningController obj, CharacterObject characterobject)
        {
            StartCoroutine(StartMove(obj, characterobject));
        }

        public IEnumerator StartWaveMove(CharacterObject characterobject)
        {
            List<CharacterBase> allCharacters = new List<CharacterBase>();

            allCharacters.AddRange(characterobject.childs);
            allCharacters.Add(characterobject);

            yield return new WaitForSeconds(0.1f);

            /*for(int i = allCharacters.Count - 1; i >= 0; i--)
            {
                allCharacters[i].WinConditionMove();

                yield return new WaitForSeconds(0.05f);
            }*/

            for (int i = 0; i < allCharacters.Count; i++)
            {
                allCharacters[i].WinConditionMove();

                yield return new WaitForSeconds(0.02f);
            }
        }


        private IEnumerator StartMove(TileWinningController obj, CharacterObject characterobject)
        {

            //Debug.LogError("routine ici");
            List<Transform> path = new List<Transform>();
            List<CharacterBase> allCharacters = new List<CharacterBase>();
            
            allCharacters.AddRange(characterobject.childs);
            allCharacters.Add(characterobject);
            
            for (int i = 0; i < characterobject.path.Count; i++)
            {
                path.Add(characterobject.path[i].transform);
            }
            
            path.AddRange(obj.levelCompleteTiles.ToList());
            yield return null;


            /*for (int i = 0; i < allCharacters.Count; i++)
            {
               allCharacters[i].LevelEndMove(path, i);
                yield return new WaitForSeconds(0.05f);
            }*/

            yield return new WaitForSeconds(0.5f);

            for (int i = allCharacters.Count-1; i > -1; i--)
            {
                allCharacters[i].LevelEndMove(path, i);
                yield return new WaitForSeconds(0.05f);
            }

            enterCount++;

            if(enterCount == playerCount.Count)
            {
                EventManager.instance.SendHapticPulse(HapticType.MEDIUM);

                yield return new WaitForSeconds(2.5f);

                EventManager.instance.LevelComplete();
            }
                

        }
    }
}