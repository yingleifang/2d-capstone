using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HagfishEnemy : EnemyUnit
{
    public static int alive = 0;

    public override void Start()
    {
        base.Start();
        alive++;
    }

    public override IEnumerator performAction(BattleState state)
    {
        // Does nothing for now!
        yield break;
    }

    public override IEnumerator Die()
    {
        audio.PlayDisposable(deathSound);
        yield return StartCoroutine(PlayLastWordAnimation());
        yield return StartCoroutine(PlayDeathAnimation());
        alive--;
        if (alive == 0)
        {
            BattleManager.instance.StartNextLevel();
        }
        // Feel free to add code here that damages the boss
        Destroy(gameObject);
        yield break;
    }
}
