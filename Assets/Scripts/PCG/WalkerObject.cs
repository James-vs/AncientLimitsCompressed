using UnityEngine;

public class WalkerObject
{
    public Vector2 position;
    public Vector2 direction;
    public float chanceToChange;

    public WalkerObject(Vector2 pos, Vector2 dir, float cToC)
    {
        this.position = pos;
        this.direction = dir;
        this.chanceToChange = cToC;
    }
}
