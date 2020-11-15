using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAmbiencePlayer : RandomMusicPlayer
{
    [SerializeField] private List<Vector2Int> indexRanges = new List<Vector2Int>();
    [SerializeField] private int undergroundStartY = -10;
    [SerializeField] private RandomMusicPlayer musicPlayer = null;

    protected override float PlayRandom()
    {
        if (musicPlayer.playingMusic)
        {
            return 0;
        }

        if (indexRanges[0].x != 0 || indexRanges[indexRanges.Count - 1].y != audioManager.sounds.Length)
        {
            throw new UnityException("Ambience Player first index x must be 0 and last index y must be audio manager length");
        }
        int minimumIndex, maximumIndex;
        if (this.transform.position.y < undergroundStartY)
        {
            minimumIndex = indexRanges[0].x;
            maximumIndex = indexRanges[0].y;
        }
        else
        {
            minimumIndex = indexRanges[1].x;
            maximumIndex = indexRanges[1].y;
        }
        int soundIndex = Random.Range(minimumIndex, maximumIndex);
        while (lastMusicIndex == soundIndex)
        {
            soundIndex = Random.Range(minimumIndex, maximumIndex);
        }
        lastMusicIndex = soundIndex;
        audioManager.Play(soundIndex);
        return audioManager.sounds[soundIndex].clip.length;
    }
}