using UnityEngine;
public class LifecycleLogger : MonoBehaviour
{
    private const string DefaultLabelValue = "Object";
    [Header("Підпис, щоб розрізняти об'єкти в Console")]
    [SerializeField] private string label = DefaultLabelValue;
    private int updateCount;
    private void Awake() { if (label == DefaultLabelValue) label = name; Log("Awake"); }
    private void OnEnable() => Log("OnEnable");
    private void Start() => Log("Start");
    private void Update() { if (++updateCount <= 3) Log($"Update #{updateCount}"); }
    private void OnDisable() => Log("OnDisable");
    private void OnDestroy() => Log("OnDestroy");
    private void Log(string stage) => Debug.Log($"[кадр {Time.frameCount:D3}] {label} → {stage}", this);
}
