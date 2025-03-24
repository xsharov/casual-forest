using UnityEngine;
using UnityEngine.VFX;

// [ExecuteAlways]
public class WindController : MonoBehaviour
{
    [SerializeField] private Material targetMaterial;
    [SerializeField] private VisualEffect fvxGraph;
    [SerializeField] private Transform player;

    [Header("Default wind settings")]
    [SerializeField] private float defaultWindSpeed = 1f;
    [SerializeField] private float defaultWindPower = 1f;
    [SerializeField] private Vector3 defaultWindDirection = new Vector3(1, 0, 0);
    [SerializeField] private float materialValueMultiplayer = 2f;

    [Header("Lerp")]
    [SerializeField] private float lerpSpeed = 1f;

    [Header("Idle Factors")]
    [SerializeField] private float idleFactorMaterial = 0.001f;
    [SerializeField] private float idleFactorVfx = 0.001f;
    [SerializeField] private float multiplierLerpSpeed = 1f;

    private float _materialMultiplier = 1f;
    
    private float _currentWindSpeed;
    private float _currentWindPower;
    private Vector3 _currentWindDirection;

    private float _targetWindSpeed;
    private float _targetWindPower;
    private Vector3 _targetWindDirection;

    private bool _currentGustState = false;

    private void OnEnable()
    {
        RandomWindChanger.OnWindChanged += ChangeWind;
    }

    private void OnDisable()
    {
        RandomWindChanger.OnWindChanged -= ChangeWind;
    }

    private void Start()
    {
        _currentWindSpeed = defaultWindSpeed;
        _currentWindPower = defaultWindPower;
        _currentWindDirection = defaultWindDirection;

        _targetWindSpeed = defaultWindSpeed;
        _targetWindPower = defaultWindPower;
        _targetWindDirection = defaultWindDirection;

        _materialMultiplier = _currentGustState ? 1f : idleFactorMaterial;
        
        UpdateMaterialProperties();

        if (fvxGraph == null) return;
        
        fvxGraph.SetFloat("windSpeed", defaultWindSpeed);
        fvxGraph.SetVector3("windDirection", defaultWindDirection);
    }

    private void Update()
    {
        _currentWindSpeed = Mathf.Lerp(_currentWindSpeed, _targetWindSpeed, Time.deltaTime * lerpSpeed);
        _currentWindPower = Mathf.Lerp(_currentWindPower, _targetWindPower, Time.deltaTime * lerpSpeed);
        _currentWindDirection = Vector3.Lerp(_currentWindDirection, _targetWindDirection, Time.deltaTime * lerpSpeed);
        
        var targetMultiplier = _currentGustState ? 1f : idleFactorMaterial;
        _materialMultiplier = Mathf.Lerp(_materialMultiplier, targetMultiplier, Time.deltaTime * multiplierLerpSpeed);


        UpdateMaterialProperties();

        if (fvxGraph == null) return;
        
        var vfxWindSpeed = _currentGustState ? _currentWindSpeed : _currentWindSpeed * idleFactorVfx;
        fvxGraph.SetFloat("windSpeed", vfxWindSpeed);
        fvxGraph.SetVector3("windDirection", _currentWindDirection);
        
        // if (player == null) return;
        
        // fvxGraph.SetVector3("playerPosition", player.position);
    }

    private void UpdateMaterialProperties()
    {
        if (targetMaterial == null) return;
        
        if (_currentGustState == false) return;
        
        var materialWindPower = _currentWindPower * _materialMultiplier;
        targetMaterial.SetFloat("_WindPower", materialWindPower * materialValueMultiplayer);
        targetMaterial.SetVector("_WindDirection", _currentWindDirection * materialValueMultiplayer);
    }

    private void ChangeWind(float newWindSpeed, float newWindPower, Vector3 newWindDirection, bool isGust)
    {
        // Debug.Log($"Wind change event received: Speed={newWindSpeed:F2}, Power={newWindPower:F2}, Direction={newWindDirection}, isGust={isGust}");
        _targetWindSpeed = newWindSpeed;
        _targetWindPower = newWindPower;
        _targetWindDirection = newWindDirection;
        _currentGustState = isGust;
    }
}
