using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Game
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        public List<EnemyController> Enemies { get; private set; }
        public int NumberOfEnemiesTotal { get; private set; }
        public int NumberOfEnemiesRemaining => Enemies.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Enemies = new List<EnemyController>();
        }

        public void RegisterEnemy(EnemyController enemy)
        {
            if (enemy != null && !Enemies.Contains(enemy))
            {
                Enemies.Add(enemy);
                NumberOfEnemiesTotal++;
            }
        }

        public void RemoveEnemy(EnemyController enemyKilled)
        {
            if (Enemies.Contains(enemyKilled))
            {
                Enemies.Remove(enemyKilled);
            }
        }
    }
}
