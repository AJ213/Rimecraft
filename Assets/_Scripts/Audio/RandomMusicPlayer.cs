using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMusicPlayer : MonoBehaviour
{
    private AudioManager audioManager;
    private bool playingMusic = false;
    private bool waitingForMusic = false;
    private int lastMusicIndex = -1;
    [SerializeField] private Vector2 timeRange = new Vector2(0, 1000);

    private void Awake()
    {
        audioManager = this.gameObject.GetComponent<AudioManager>();
    }

    private void Update()
    {
        if (!waitingForMusic && !playingMusic)
        {
            StartCoroutine(PlayRandomMusic(Random.Range(timeRange.x, timeRange.y)));
        }
    }

    private IEnumerator PlayRandomMusic(float seconds)
    {
        waitingForMusic = true;
        yield return new WaitForSeconds(seconds);
        if (!playingMusic)
        {
            int soundIndex = Random.Range(0, audioManager.sounds.Length);
            while (lastMusicIndex == soundIndex)
            {
                soundIndex = Random.Range(0, audioManager.sounds.Length);
            }
            lastMusicIndex = soundIndex;
            audioManager.Play(soundIndex);
            playingMusic = true;
            yield return new WaitForSeconds(audioManager.sounds[soundIndex].clip.length);
            playingMusic = false;
        }
        waitingForMusic = false;
    }
}