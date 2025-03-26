using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

[ExecuteAlways]
public class WindController : MonoBehaviour
{
    [SerializeField] private Material vegetationMaterial;    
    [SerializeField] private Renderer waterRenderer;
    
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
    
    [Header("Idle Factors")]
    [SerializeField] private float waterTimeModifier = 1f; 
    [SerializeField] private  float waterTransitionDuration = 1f;
    [SerializeField] private float windMaterialTransitionDuration = 1f;

    private MaterialPropertyBlock _waterPropertyBlock;

    private float _materialMultiplier = 1f;
    
    private float _currentWindSpeed;
    private float _currentWindPower;
    private Vector3 _currentWindDirection;

    private float _targetWindSpeed;
    private float _targetWindPower;
    private Vector3 _targetWindDirection;

    private bool _currentGustState = false;

    private Vector3 _waterCurrentVelocity; 
    private Vector2 _waterOffset;   
    private float _waterTransitionTimer = 0f;
    private Vector3 _waterPreviousVelocity;
    
    private float _windPowerTransitionTimer = 0f;
    private float _previousWindPower = 0f;
    private float _windDirectionTransitionTimer = 0f;
    private Vector3 _previousWindDirection = Vector3.zero;
    
    private static readonly int WindSpeed = Shader.PropertyToID("_WindSpeed");
    private static readonly int WindDirection = Shader.PropertyToID("_WindDirection");
    private static readonly int WaveOffset = Shader.PropertyToID("_WaveOffset");
    private void OnEnable()
    {
        RandomWindChanger.OnWindChanged += ChangeWind;
        
        _waterPropertyBlock = new MaterialPropertyBlock();
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

        _waterCurrentVelocity = _targetWindDirection.normalized * waterTimeModifier;
        _waterPreviousVelocity = _waterCurrentVelocity;

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
        UpdateWaterProperties();

        if (fvxGraph == null) return;
        
        var vfxWindSpeed = _currentGustState ? _currentWindSpeed : _currentWindSpeed * idleFactorVfx;
        fvxGraph.SetFloat("windSpeed", vfxWindSpeed);
        fvxGraph.SetVector3("windDirection", _currentWindDirection);
        
        // if (player == null) return;
        // fvxGraph.SetVector3("playerPosition", player.position);
    }

    private void UpdateMaterialProperties()
    {
        if (vegetationMaterial == null) return;
        
        var targetWindPower = _currentWindPower * _materialMultiplier * materialValueMultiplayer;
        
        if (_previousWindPower != targetWindPower)
        {
            _previousWindPower = _currentWindPower * _materialMultiplier * materialValueMultiplayer;
            _windPowerTransitionTimer = 0f;
        }

        if (_windPowerTransitionTimer < windMaterialTransitionDuration)
        {
            _windPowerTransitionTimer += Time.deltaTime;
            var t = Mathf.Clamp01(_windPowerTransitionTimer / windMaterialTransitionDuration);
            targetWindPower = Mathf.Lerp(_previousWindPower, _currentWindPower * _materialMultiplier * materialValueMultiplayer, t);
        }

        var targetWindDirection = _currentWindDirection * materialValueMultiplayer;
        
        if (_previousWindDirection != targetWindDirection)
        {
            _previousWindDirection = _currentWindDirection * materialValueMultiplayer;
            _windDirectionTransitionTimer = 0f;
        }

        if (_windDirectionTransitionTimer < windMaterialTransitionDuration)
        {
            _windDirectionTransitionTimer += Time.deltaTime;
            var t = Mathf.Clamp01(_windDirectionTransitionTimer / windMaterialTransitionDuration);
            targetWindDirection = Vector3.Lerp(_previousWindDirection, _currentWindDirection * materialValueMultiplayer, t);
        }
        
        vegetationMaterial.SetFloat(WindSpeed, targetWindPower);
        vegetationMaterial.SetVector(WindDirection, targetWindDirection);
    }

    private void UpdateWaterProperties()
    {
        if (waterRenderer == null) return;
        
        if (_waterCurrentVelocity != _targetWindDirection.normalized * waterTimeModifier)
        {
            _waterPreviousVelocity = _waterCurrentVelocity;
            _waterCurrentVelocity = _targetWindDirection.normalized * waterTimeModifier;
            _waterTransitionTimer = 0f;
        }

        if (_waterTransitionTimer < waterTransitionDuration)
        {
            _waterTransitionTimer += Time.deltaTime;
            var t = Mathf.Clamp01(_waterTransitionTimer / waterTransitionDuration);
            _waterCurrentVelocity = Vector3.Lerp(_waterPreviousVelocity, _targetWindDirection.normalized * waterTimeModifier, t);
        }
        
        _waterOffset += new Vector2(_waterCurrentVelocity.x,_waterCurrentVelocity.z) * Time.deltaTime;
        
        waterRenderer.GetPropertyBlock(_waterPropertyBlock);
        
        _waterPropertyBlock.SetVector(WaveOffset, _waterOffset);
        
        waterRenderer.SetPropertyBlock(_waterPropertyBlock);
    }

    private void ChangeWind(float newWindSpeed, float newWindPower, Vector3 newWindDirection, bool isGust)
    {
        _targetWindSpeed = newWindSpeed;
        _targetWindPower = newWindPower;
        _targetWindDirection = newWindDirection;
        _currentGustState = isGust;
    }
}
