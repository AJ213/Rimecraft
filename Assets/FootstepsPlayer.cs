using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepsPlayer : MonoBehaviour
{
    private List<ushort> snowBlocks = new List<ushort>();
    private List<ushort> stoneBlocks = new List<ushort>();
    public bool playingFootstep = false;
    private bool waitingForFootstep = false;

    private AudioManager audioManager;
    [SerializeField] private float footstepInterval = 0.1f;

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

    // Update is called once per frame
    private void Update()
    {
    }

    public void PlayFootstep()
    {
        if (!playingFootstep && !waitingForFootstep)
        {
            ushort groundBlockID = RimecraftWorld.Instance.CheckForVoxel((new Vector3(this.transform.position.x, this.transform.position.y - 0.1f, this.transform.position.z)).FloorToInt3());
            Debug.Log(groundBlockID);
            if (snowBlocks.Contains(groundBlockID))
            {
                StartCoroutine(PlayRandomFootstep(new Vector2Int(0, 3)));
            }
            else if (stoneBlocks.Contains(groundBlockID))
            {
                StartCoroutine(PlayRandomFootstep(new Vector2Int(3, 6)));
            }
        }
    }

    private IEnumerator PlayRandomFootstep(Vector2Int range)
    {
        waitingForFootstep = true;
        yield return new WaitForSeconds(footstepInterval);
        if (!playingFootstep)
        {
            float footstepLength = PlayRandom(range);
            playingFootstep = true;
            yield return new WaitForSeconds(footstepLength);
            playingFootstep = false;
        }
        waitingForFootstep = false;
    }

    private float PlayRandom(Vector2Int range)
    {
        int soundIndex = Random.Range(range.x, range.y);

        audioManager.Play(soundIndex);
        return audioManager.sounds[soundIndex].clip.length;
    }
}