using System.Text;
using TMPro;
using UnityEngine;
using Unity.Mathematics;

public class DebugScreen : MonoBehaviour
{
    private TextMeshProUGUI text;

    private float frameRate;
    private float timer;
    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    private void Update()
    {
        StringBuilder debugText = new StringBuilder("Debug \n");
        debugText.Append(frameRate.ToString());
        debugText.AppendLine(" fps");

        debugText.Append("Coords: (");
        debugText.Append(Mathf.FloorToInt(player.transform.position.x));
        debugText.Append(", ");
        debugText.Append(Mathf.FloorToInt(player.transform.position.y));
        debugText.Append(", ");
        debugText.Append(Mathf.FloorToInt(player.transform.position.z));
        debugText.AppendLine(")");

        int3 playerCoord = WorldHelper.GetChunkCoordFromPosition(player.transform.position);
        debugText.Append("Chunk: (");
        debugText.Append(playerCoord.x);
        debugText.Append(", ");
        debugText.Append(playerCoord.y);
        debugText.Append(", ");
        debugText.Append(playerCoord.z);
        debugText.AppendLine(")");

        text.text = debugText.ToString();

        if (timer > 1)
        {
            frameRate = Mathf.FloorToInt(1 / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}