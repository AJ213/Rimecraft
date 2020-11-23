using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepsPlayer : MonoBehaviour
{
    private List<ushort> snowBlocks = new List<ushort>();
    private List<ushort> stoneBlocks = new List<ushort>();
    private bool waitingForFootstep = false;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = this.GetComponent<AudioManager>();
        snowBlocks.Add(1);
        snowBlocks.Add(2);
        snowBlocks.Add(4);
        snowBlocks.Add(6);
        stoneBlocks.Add(3);
        stoneBlocks.Add(5);
    }

    public void PlayFootstep(float speed)
    {
        if (!waitingForFootstep)
        {
            ushort groundBlockID = RimecraftWorld.Instance.CheckForVoxel((new Vector3(this.transform.position.x, this.transform.position.y - 0.1f, this.transform.position.z)).FloorToInt3());
            if (snowBlocks.Contains(groundBlockID))
            {
                StartCoroutine(PlayRandomFootstep(new Vector2Int(0, 3), speed));
            }
            else if (stoneBlocks.Contains(groundBlockID))
            {
                StartCoroutine(PlayRandomFootstep(new Vector2Int(3, 6), speed));
            }
        }
    }

    private IEnumerator PlayRandomFootstep(Vector2Int range, float speed)
    {
        waitingForFootstep = true;
        yield return new WaitForSeconds(speed);
        PlayRandom(range);
        waitingForFootstep = false;
    }

    private float PlayRandom(Vector2Int range)
    {
        int soundIndex = Random.Range(range.x, range.y);

        audioManager.Play(soundIndex);
        return audioManager.sounds[soundIndex].clip.length;
    }
}