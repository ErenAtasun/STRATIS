using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;

public class MatchManager : MonoBehaviour
{
    [Header("Prefab & Spawns")]
    public GameObject agentPrefab;         // StratisAgent.prefab
    public Transform[] blueSpawns;         // tam 2 eleman
    public Transform[] redSpawns;          // tam 2 eleman

    [Header("Round Settings")]
    public float roundStartDelay = 0.5f;   // yeni tur baþlamadan bekleme
    public float roundEndDelay = 1.0f;   // takým kazanýnca bekleme

    // runtime
    private readonly List<StratisAgent> agents = new();
    private int episodeId = 0;
    public int blueScore = 0, redScore = 0;

    void Start()
    {
        if (blueSpawns.Length < 2 || redSpawns.Length < 2 || agentPrefab == null)
        {
            Debug.LogError("MatchManager: lütfen agentPrefab ve 2'þer spawn atayýn.");
            enabled = false; return;
        }
        StartCoroutine(StartRoundRoutine());
    }

    void Update()
    {
        // Ýstersen manuel reset tuþu:
        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(ResetRoundRoutine());
    }

    IEnumerator StartRoundRoutine()
    {
        yield return new WaitForSeconds(roundStartDelay);
        SpawnTeam(Team.Blue, blueSpawns, 2);
        SpawnTeam(Team.Red, redSpawns, 2);

        // downed eventlerini baðla
        foreach (var a in agents)
        {
            var hp = a.GetComponent<HealthSystem>();
            hp.OnDowned += OnAgentDowned;
        }
    }

    IEnumerator ResetRoundRoutine()
    {
        // tüm ajanlarýn ep. sonu + temizle
        foreach (var a in agents.Where(a => a != null))
            a.EndEpisode();
        ClearAgents();
        yield return new WaitForSeconds(roundStartDelay);
        StartCoroutine(StartRoundRoutine());
    }

    void SpawnTeam(Team team, Transform[] spawns, int count)
    {
        // isimlendirme Blue_1, Blue_2 / Red_1, Red_2
        for (int i = 0; i < count; i++)
        {
            var t = spawns[Mathf.Clamp(i, 0, spawns.Length - 1)];
            var go = Instantiate(agentPrefab, t.position, t.rotation);
            go.name = (team == Team.Blue ? "Blue_" : "Red_") + (i + 1);

            var ag = go.GetComponent<StratisAgent>();
            ag.team = team;

            // HealthSystem takým atamasý
            var hp = go.GetComponent<HealthSystem>();
            if (hp != null) hp.team = team;

            agents.Add(ag);
        }
    }

    void ClearAgents()
    {
        foreach (var a in agents)
            if (a != null) Destroy(a.gameObject);
        agents.Clear();
    }

    void OnAgentDowned(HealthSystem hs)
    {
        // takým durumlarýný kontrol et
        bool blueAlive = agents.Any(a => a != null && a.team == Team.Blue && !a.GetComponent<HealthSystem>().IsDowned);
        bool redAlive = agents.Any(a => a != null && a.team == Team.Red && !a.GetComponent<HealthSystem>().IsDowned);

        if (blueAlive && redAlive) return; // tur devam

        // kazanan/ kaybeden
        Team winner = blueAlive ? Team.Blue : Team.Red;
        if (winner == Team.Blue) blueScore++; else redScore++;

        // takým bonusu + bütün ajanlarýn episode’unu bitir
        foreach (var a in agents.Where(a => a != null))
        {
            if (a.team == winner) a.AddReward(+0.5f); // takým ödülü
            a.EndEpisode();
        }

        episodeId++;
        // event’leri ayýr (birden fazla tetiklenmesin)
        foreach (var a in agents)
        {
            var hp = a != null ? a.GetComponent<HealthSystem>() : null;
            if (hp != null) hp.OnDowned -= OnAgentDowned;
        }

        StartCoroutine(RoundEndThenRestart());
    }

    IEnumerator RoundEndThenRestart()
    {
        yield return new WaitForSeconds(roundEndDelay);
        ClearAgents();
        StartCoroutine(StartRoundRoutine());
    }
}
