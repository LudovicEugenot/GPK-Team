using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAOEPattern : MonoBehaviour
{
    [Header("Aoe settings")]
    public List<Aoe> patternAoes;
    [Header("Prefabs")]
    public GameObject warningZonePrefab;
    public GameObject aoeFx;

    void Start()
    {
        StartCoroutine(LaunchAoepattern());
    }

    private IEnumerator LaunchAoepattern()
    {
        for(int i = 0; i < patternAoes.Count; i++)
        {
            StartCoroutine(SpawnAOE(patternAoes[i].Info()));
            yield return new WaitForSeconds(patternAoes[i].beatTimeBeforeNextAOE);
        }
    }

    private IEnumerator SpawnAOE(Aoe aoe)
    {
        GameObject warningZone = Instantiate(warningZonePrefab, aoe.position, Quaternion.identity);
        warningZone.transform.localScale = Vector2.one * aoe.radius;

        yield return new WaitForSeconds(aoe.warningBeatTime * BeatManager.Instance.BeatTime);
        Destroy(warningZone);

        GameObject fx = Instantiate(aoeFx, aoe.position, Quaternion.identity);
        fx.transform.localScale = Vector2.one * aoe.radius;

        if (Physics2D.OverlapCircle(aoe.position, aoe.radius, LayerMask.GetMask("Player")))
        {
            GameManager.Instance.playerManager.TakeDamage(aoe.damage);
        }
    }

    [System.Serializable]
    public class Aoe
    {
        [SerializeField] private Transform spawnPosition;
        [HideInInspector] public Vector2 position;
        [Range(0f, 10f)] public float beatTimeBeforeNextAOE;
        [Range(1f, 20f)] public int damage;
        [Range(0.5f, 8f)] public float radius;
        [Range(1f, 5f)] public float warningBeatTime;

        public Aoe Info()
        {
            Aoe aoe = this;
            aoe.position = spawnPosition.position;
            return aoe;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach(Aoe aoe in patternAoes)
        {
            Aoe aoeInfo = aoe.Info();
            Gizmos.DrawWireSphere(aoeInfo.position, aoeInfo.radius);
        }
    }
}
