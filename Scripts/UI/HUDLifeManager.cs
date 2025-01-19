using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class HUDLifeManager : MonoBehaviour
    {
        [SerializeField] private GameObject _lifePrefab;
        [SerializeField] private List<Transform> _lives;
        
        public void Initialize(int maxLife)
        {
            if (_lives != null)
            {
                foreach (var life in _lives)
                {
                    Destroy(life.gameObject);
                }
            }
            _lives = new List<Transform>();
            
            for (int i = 0; i < maxLife; i++)
            {
                var life = Instantiate(_lifePrefab, transform);
                _lives.Add(life.transform);
            }
        }
        
        public async UniTask AddLife(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var life = Instantiate(_lifePrefab, transform);
                _lives.Add(life.transform);
                
                //DoScaleでスケール0から1に変化させるのをawaitする
                await life.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            }
        }
        
        public async UniTask RemoveLife(int amount)
        {
            if(_lives.Count < amount) amount = _lives.Count;
            
            for (int i = 0; i < amount; i++)
            {
                var life = _lives[^1];
                //DoScaleでスケール1から0に変化させるのをawaitする
                await life.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).AsyncWaitForCompletion();
 
                _lives.Remove(life);
                Destroy(life.gameObject);
            }
        }
    }
}