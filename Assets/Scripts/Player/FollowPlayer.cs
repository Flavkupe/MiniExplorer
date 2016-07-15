using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour
{
	public Vector3 offset;			// The offset at which the Health Bar follows the player.
	
	private PlayerControl player;		// Reference to the player.

    void Start()
    {
        // Setting up the reference.
        
    }

	void Update ()
	{
        player = StageManager.Player;
        if (player == null || player.transform == null)
        {
            return;
        }

		// Set the position to the player's position with the offset.
		transform.position = player.transform.position + offset;
	}
}
