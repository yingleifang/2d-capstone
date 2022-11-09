using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public static Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = BattleManager.bossHealth;
    }
    public static void BossTakeDamage(int damage)
    {
        BattleManager.bossHealth -= damage;
        slider.value -= damage;   
    }
}
