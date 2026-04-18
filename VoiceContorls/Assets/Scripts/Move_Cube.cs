using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Move_Cube : MonoBehaviour
{
    private Vector3 playerLocOrigin;
    public TextMeshProUGUI text;
    private float speed = 1.0f;
    public GameObject player;

    void Start()
    {
        playerLocOrigin = player.transform.position;
    }
    void Update()
    {
        var step = speed * Time.deltaTime;
        string lowerCaseText = text.text.ToLower();

        int upCount = CountOccurrences(lowerCaseText, "up");
        int downCount = CountOccurrences(lowerCaseText, "down");
        int rightCount = CountOccurrences(lowerCaseText, "right");
        int leftCount = CountOccurrences(lowerCaseText, "left");
        int forwardCount = CountOccurrences(lowerCaseText, "forward");
        int backwardsCount = CountOccurrences(lowerCaseText, "backward");

        Vector3 target = new Vector3(playerLocOrigin.x + rightCount - leftCount,
        playerLocOrigin.y + upCount - downCount, playerLocOrigin.z + forwardCount - backwardsCount);
        player.transform.position = Vector3.MoveTowards(player.transform.position, target, step);

    }

    int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int i = 0;
        while ((i = text.IndexOf(pattern, i)) != -1)
        {
            i += pattern.Length;
            count++;
        }
        return count;
    }
}
