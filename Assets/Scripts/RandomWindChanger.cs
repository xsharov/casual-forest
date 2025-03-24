using UnityEngine;

// [ExecuteAlways]
public class RandomWindChanger : MonoBehaviour
{
    public delegate void WindChangedHandler(float windSpeed, float windPower, Vector3 windDirection, bool isGust);
    public static event WindChangedHandler OnWindChanged;

    [Header("Wind Settings")]
    [Header("VFX")]
    [SerializeField] private float minWindSpeed = 0.01f;
    [SerializeField] private float maxWindSpeed = 0.2f;
    [Header("Shader")]
    [SerializeField] private float minWindPower = 0.005f;
    [SerializeField] private float maxWindPower = 0.05f;
    [Header("Direction")]
    [SerializeField] private float minHorizontal = -1f;
    [SerializeField] private float maxHorizontal = 1f;
    [SerializeField] private float minVerticalUp = 0.5f;
    [SerializeField] private float maxVerticalUp = 1f;
    [SerializeField] private float minVerticalDown = -1f;
    [SerializeField] private float maxVerticalDown = -0.5f;

    [Header("Gust Settings")]
    [SerializeField] private float gustInterval = 10f;
    [SerializeField] private float gustDuration = 3f;

    private bool _alternateUp = true;
    private float _cycleTimer = 0f;
    private bool _currentGustState = false;

    private void Update()
    {
        _cycleTimer += Time.deltaTime;
        if (_cycleTimer >= gustInterval)
        {
            _cycleTimer -= gustInterval;
        }

        var isGust = (_cycleTimer >= (gustInterval - gustDuration));
        if (isGust != _currentGustState)
        {
            _currentGustState = isGust;

            var newWindSpeed = Random.Range(minWindSpeed, maxWindSpeed);
            var newWindPower = Random.Range(minWindPower, maxWindPower);

            var horizontalX = Random.Range(minHorizontal, maxHorizontal);
            var horizontalZ = Random.Range(minHorizontal, maxHorizontal);
            var verticalComponent = _alternateUp 
                ? Random.Range(minVerticalUp, maxVerticalUp) 
                : Random.Range(minVerticalDown, maxVerticalDown);
            _alternateUp = !_alternateUp;

            var newWindDirection = new Vector3(horizontalX, verticalComponent, horizontalZ).normalized;

            OnWindChanged?.Invoke(newWindSpeed, newWindPower, newWindDirection, isGust);
        }
    }
}
