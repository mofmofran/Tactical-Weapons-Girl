using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIController : MonoBehaviour
{

    [Header("UIテキスト")]
    [SerializeField] private TextMeshProUGUI _hPText;//耐久テキスト
    [SerializeField] private TextMeshProUGUI _mainAmmoText;//メイン射撃弾数テキスト
    [SerializeField] private TextMeshProUGUI _subAmmoText;//サブ射撃弾数テキスト
    [SerializeField] private TextMeshProUGUI _spAmmoText;//特殊射撃弾数テキスト

    [Header("UIスライダー")]
    [SerializeField] private Slider _mainAmmoSlider;//メイン射撃弾数スライダー
    [SerializeField] private Slider _cSSlider;//CSチャージスライダー
    [SerializeField] private Slider _subAmmoSlider;//サブ射撃弾数テキスト
    [SerializeField] private Slider _mCSSlider;//格闘CSチャージスライダー
    [SerializeField] private Slider _spAmmoSlider;//特殊射撃弾数スライダー
    [SerializeField] private Slider _boostSlider;//ブーストゲージのスライダー
    [SerializeField] private Slider _eXSlider;//EXゲージのスライダー

    [Header("プレイヤー")]
    [SerializeField] private GameObject _player;
    private PlayerMove _playerMove;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag ("Player" );
        _playerMove = _player.GetComponent<PlayerMove>();
    }

    // Update is called once per frame
    void Update()
    {
        _hPText.text = _playerMove.hp.ToString();
        _mainAmmoText.text = _playerMove.mainAmmo.ToString();
        _subAmmoText.text = _playerMove.subAmmo.ToString();
        _spAmmoText.text = _playerMove.spAmmo.ToString();

        _boostSlider.value = _playerMove.boostGauge;
        _eXSlider.value = _playerMove.eXgauge;
        _mainAmmoSlider.value = _playerMove.mainAmmo;
        _subAmmoSlider.value = _playerMove.subAmmo;
        _spAmmoSlider.value = _playerMove.spAmmo;
        _cSSlider.value = _playerMove.cShoot;
        _mCSSlider.value = _playerMove.mCShoot;
    }
}
