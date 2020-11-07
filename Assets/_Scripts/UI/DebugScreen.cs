using System.Text;
using TMPro;
using UnityEngine;

public class DebugScreen : MonoBehaviour
{
    private TextMeshProUGUI text;

    private float frameRate;
    private float timer;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    private void Update()
    {
        StringBuilder debugText = new StringBuilder("Debug \n");
        debugText.Append(frameRate.ToString());
        debugText.AppendLine(" fps");

        debugText.Append("Coords: (");
        debugText.Append(Mathf.FloorToInt(RimecraftWorld.Instance.player.transform.position.x));
        debugText.Append(", ");
        debugText.Append(Mathf.FloorToInt(RimecraftWorld.Instance.player.transform.position.y));
        debugText.Append(", ");
        debugText.Append(Mathf.FloorToInt(RimecraftWorld.Instance.player.transform.position.z));
        debugText.AppendLine(")");

        debugText.Append("Chunk: (");
        debugText.Append(RimecraftWorld.Instance.playerChunkCoord.x);
        debugText.Append(", ");
        debugText.Append(RimecraftWorld.Instance.playerChunkCoord.y);
        debugText.Append(", ");
        debugText.Append(RimecraftWorld.Instance.playerChunkCoord.z);
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