using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMusicPlayer : MonoBehaviour
{
    protected AudioManager audioManager;
    public bool playingMusic = false;
    private bool waitingForMusic = false;
    protected int lastMusicIndex = -1;
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
            float songLength = PlayRandom();
            playingMusic = true;
            yield return new WaitForSeconds(songLength);
            playingMusic = false;
        }
        waitingForMusic = false;
    }

    protected virtual float PlayRandom()
    {
        int soundIndex = Random.Range(0, audioManager.sounds.Length);
        while (lastMusicIndex == soundIndex)
        {
            soundIndex = Random.Range(0, audioManager.sounds.Length);
        }
        lastMusicIndex = soundIndex;
        audioManager.Play(soundIndex);
        return audioManager.sounds[soundIndex].clip.length;
    }
}