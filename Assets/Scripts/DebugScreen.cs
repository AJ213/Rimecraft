using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class DebugScreen : MonoBehaviour
{
    World world;
    TextMeshProUGUI text;

    float frameRate;
    float timer;
    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }

    // Update is called once per frame
    void Update()
    {
        StringBuilder debugText = new StringBuilder("Debug \n");
        debugText.Append(frameRate.ToString());
        debugText.AppendLine(" fps");

        debugText.Append("Coords: (");
        debugText.Append(Mathf.FloorToInt(world.player.transform.position.x));
        debugText.Append(", ");
        debugText.Append(Mathf.FloorToInt(world.player.transform.position.y));
        debugText.Append(", ");
        debugText.Append(Mathf.FloorToInt(world.player.transform.position.z));
        debugText.AppendLine(")");

        debugText.Append("Chunk: (");
        debugText.Append(world.playerChunkCoord.x);
        debugText.Append(", ");
        debugText.Append(world.playerChunkCoord.z);
        debugText.AppendLine(")");

        GetComponent<TextMeshProUGUI>().text = debugText.ToString();

        if(timer > 1)
        {
            frameRate = (int)(1 / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
