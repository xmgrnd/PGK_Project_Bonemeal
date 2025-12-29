using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Manages enemy waves and handles the transition to the end-game state.
// Updated to play spawning sounds globally or at high volume across distances.
public class ZombieSprawner : MonoBehaviour
{
    [Header("Spawn Prefabs")]
    public GameObject normalZombiePrefab;
    public GameObject rangedZombiePrefab;

    [Header("End Game Assets")]
    public GameObject portalObject; 
    public AudioClip endSequenceSound; 
    public AudioSource musicSource; 

    [Header("Spawn Limits")]
    public int maxEnemies = 15;
    public float minInterval = 1f;
    public float maxInterval = 3f;

    [Header("Effects & Audio")]
    public GameObject spawnVFX;
    public AudioClip spawnSFX;
    public AudioSource sfxSource; 

    [Header("Spawn Locations")]
    public Transform[] spawnPoints;

    private List<GameObject> _activeEnemies = new List<GameObject>();
    private bool _isSpawningActive = false;
    private bool _hasEnded = false;

    public void StartSpawning()
    {
        if (!_isSpawningActive)
        {
            _isSpawningActive = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (_isSpawningActive)
        {
            _activeEnemies.RemoveAll(enemy => enemy == null);

            // Check if win condition is met (700 points)
            if (ScoreManager.Instance != null && ScoreManager.Instance.CurrentPoints >= 700 && !_hasEnded)
            {
                EndGameSequence();
                yield break; 
            }

            if (_activeEnemies.Count < maxEnemies)
            {
                SpawnSingleZombie();
            }

            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        }
    }

    private void SpawnSingleZombie()
    {
        if (spawnPoints.Length == 0) return;

        Transform selectedPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject prefabToSpawn = normalZombiePrefab;

        if (ScoreManager.Instance != null && ScoreManager.Instance.CurrentPoints >= 350)
        {
            if (Random.value <= 0.33f) prefabToSpawn = rangedZombiePrefab;
        }
        
        GameObject newZombie = Instantiate(prefabToSpawn, selectedPoint.position, selectedPoint.rotation);
        _activeEnemies.Add(newZombie);

        // Play visual effects at spawn location
        if (spawnVFX != null) Instantiate(spawnVFX, selectedPoint.position, Quaternion.identity);

        // Play spawn sound globally through the dedicated SFX source 
        if (sfxSource != null && spawnSFX != null)
        {
            // PlayOneShot allows multiple spawn sounds to overlap without cutting each other off
            sfxSource.PlayOneShot(spawnSFX);
        }
    }

    private void EndGameSequence()
    {
        _hasEnded = true;
        _isSpawningActive = false;

        if (musicSource != null)
        {
            musicSource.Stop();
            
            if (endSequenceSound != null)
            {
                musicSource.clip = endSequenceSound;
                musicSource.loop = false; 
                musicSource.Play();
            }
        }

        foreach (GameObject enemy in _activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        _activeEnemies.Clear();

        if (portalObject != null)
        {
            portalObject.SetActive(true);
        }
    }
}