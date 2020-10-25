using System.Text;
using TMPro;
using UnityEngine;

public class DebugScreen : MonoBehaviour
{
    private World world;
    private TextMeshProUGUI text;

    private float frameRate;
    private float timer;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    private void Update()
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

        text.text = debugText.ToString();

        if (timer > 1)
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