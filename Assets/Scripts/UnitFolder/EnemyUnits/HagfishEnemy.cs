using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HagfishEnemy : EnemyUnit
{
    public static int alive = 0;

    public BossController bossController;
    public override void Start()
    {
        anim.SetBool("Hide", true);
        base.Start();
        alive++;
        bossController = FindObjectOfType<BossController>();
    }

    public override IEnumerator performAction(BattleState state)
    {
        PlayerUnit closest = FindClosestPlayerUnit(state);

        if (!closest)
        {
            // No player units? Something's wrong
            Debug.LogError("No player units detected :( (FishEnemy performAction method)");
            yield break;
        }

        // Attack the unit if they're in range
        if (!isDead && IsTileInAttackRange(closest.location))
        {
            yield return StartCoroutine(DoAttack(closest));
            yield return new WaitForSeconds(0.2f);
        }
    }

    public override IEnumerator Die()
    {
        audio.PlayDisposable(deathSound);
        yield return StartCoroutine(PlayLastWordAnimation());
        yield return StartCoroutine(PlayDeathAnimation());
        StartCoroutine(bossController.DamageAnimation());
        alive--;
        if (alive == 0)
        {
            BattleManager.instance.StartNextLevel();
            BattleManager.instance.isBattleOver = true;
        }
        // Feel free to add code here that damages the boss
        BossHealthBar.BossTakeDamage(1);
        TurnCountDown hagFishCount = FindObjectOfType<TurnCountDown>();
        hagFishCount.DecrementBoss();
        Destroy(gameObject);
        yield break;
    }
}
