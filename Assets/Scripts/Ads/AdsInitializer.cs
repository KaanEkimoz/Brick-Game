using UnityEngine;
using UnityEngine.Advertisements;
using InGame;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField] bool _testMode = false;
    private string _gameId;

    [SerializeField] InterstitialAds interstitialad;
    [SerializeField] BannerAds bannerad;
    void Awake()
    {
        InitializeAds();
    }
    void Start()
    {

    }

    public void InitializeAds()
    {
        _gameId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOSGameId
            : _androidGameId;
        Advertisement.Initialize(_gameId, _testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        interstitialad.LoadAd();
        bannerad.LoadBanner();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    private void OnEnable()
    {
        PiecesController.OnGameOver += interstitialad.ShowAd;
    }
    private void OnDisable()
    {
        PiecesController.OnGameOver -= interstitialad.ShowAd;
    }
}