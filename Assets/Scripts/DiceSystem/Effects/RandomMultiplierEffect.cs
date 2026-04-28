using UnityEngine;

[CreateAssetMenu(
    fileName = "RandomMultiplierEffect",
    menuName = "Effects/Dice/RandomMultiplier"
)]
public class RandomMultiplierEffect : BaseDiceEffect
{
    [SerializeField] private Vector2Int range = new(2, 5);

    public override bool ApplyOnNextAvailableRoll => true;
    public override bool RemoveAfterRoll => true;

    public override int ModifyRoll(int roll, DiceContext ctx)
    {
        int mult = Random.Range(range.x, range.y + 1);
        return roll * mult;
    }
    public void SetRange(Vector2Int r) => range = r;

}
