using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class PrefBoundToggle : MonoBehaviour
{
    [SerializeField] private string _prefsKey;
    [SerializeField] private bool _defaultState = true;
    private Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();
    }

    void OnEnable()
    {
        if (_toggle == null)
            _toggle = GetComponent<Toggle>();

        bool val = PlayerPrefs.GetInt(_prefsKey, _defaultState ? 1 : 0) == 1;
        _toggle.SetIsOnWithoutNotify(val);
    }
}
