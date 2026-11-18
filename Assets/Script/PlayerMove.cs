using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;//InputSystemに必要

public class PlayerMove : MonoBehaviour
{
    Rigidbody2D rbody;//Rigitbody2D変数
    public float speed = 5.0f; // 歩きの速度
    public float jumpPower = 9.0f; // ジャンプの力
    public float airSpeed = 0.3f;//ジャンプ中の速度
    
    public float airAcceleration = 1.0f; // 空中での加速力
    public float jumpHoldAcceleration = 20.0f; // 長押し中の上昇力
    public float maxRiseSpeed = 10.0f;//上昇速度上限
    public float boostStartSpeed = 15.0f;//ブースト初速

    public float boostSpeed = 12.0f;//ブースト速度
    public float defaultBoostEndWait = 0.3f;//ブースト終了時の猶予
    public float stepSpeed = 10f;//ステップ初速
    public float stepDuration = 0.3f;//ステップ減速時間
    public int hp = 750; //HP

    public float mainAmmo = 7.0f;//メイン射撃弾数
    public float subAmmo = 2.0f;//サブ射撃弾数
    public float spAmmo = 2.0f;//特殊射撃弾数
    public float boostGauge = 1000.0f; // ブーストゲージ
    public float eXgauge = 0f;//EXゲージ

    public float cShoot;//CSチャージゲージ
    public float mCShoot;//格闘CSチャージゲージ

    public LayerMask groundLayer; // 接地できるレイヤー



    //アニメーション対応
    Animator animator;//アニメーター対応変数
    private string idleAnime = "PlayerIdle";//停止アニメーション名
    private string startWalkAnime = "PlayerStartWalk";//歩き出しのアニメーション

    private string jumpAnime = "PlayerJump";//ジャンプアニメーション
    string nowAnime;//現在のアニメーション名
    string oldAnime;//前回のアニメーション名

    //動作フラグ関連
    private bool _onGround = false; // 接地しているかどうかのフラグ
    private bool _recoveryTime = false; // 硬直中フラグ
    [SerializeField] private float _boostWait =0.2f;//二回押しでブーストになる猶予時間
    [SerializeField] private float _stepWait =0.2f;//二回押しでステップになる猶予時間
    private float _boostEndWait=-0.1f;//ブースト移動終了の猶予
    InputAction _jumpAction;//ジャンプボタン用変数
    InputAction _moveAction;//移動ボタン用変数
    private bool _isJump = false;//ジャンプ開始フラグ
    private bool _jumpHeld =false;//ジャンプが押されているか
    private bool _moveHeld =false;//移動ボタンが押されているか
    private bool _isBoost = false;//ブースト初動
    private bool _isBoosting = false;//ブースト中か
    public bool isStep = false;//ステップ初動
    public float _stepTimer;//ステップ時間
    private bool _isSteping = false;//ステップ中か
    private bool _isGroundRecovery = false;//接地系か
    private float _angleX = 1f;//左右移動角度
    private float _angleY = 0f;//上下移動角度
    private Vector2 _moveVec;//移動ベクトル
    private int _jumpCnt=0;//ジャンプボタン入力数のカウント
    private int _moveCnt=0;//移動ボタン入力数のカウント


    public void Recovery()//硬直中フラグを解除するメソッド
    {
        _recoveryTime = false;
        _isJump = false;
        _isBoost = false;
    }
    
 
    //メソッド関連

    Vector2 GetBoostDirection()
    {
        // 入力があればその方向へ
        if (_moveVec.sqrMagnitude > 0.01f)
            return _moveVec.normalized;
        // 入力がなければ最後に入力した方向へ
        return new Vector2(_angleX, _angleY).normalized;
    }

    void EndBoost()
    {

        StartCoroutine(BoostEnd());
        _isBoosting = false;
        // 通常の重力へ戻す
        rbody.gravityScale = 1;

    }

    IEnumerator JumpWait()
    {
    
        yield return new WaitForSeconds(_boostWait);
        if (_jumpCnt>=2)
        {
            _isBoost = true;
            _isBoosting = true;
            _jumpCnt = 0;
            Invoke("Recovery", 0.3f);
            boostGauge -= 125;//ブーストゲージを減らす
            if (boostGauge <= 0)
            {
                boostGauge = 0;//ブーストゲージが0以下になる場合は0にする
            }
            _boostEndWait = defaultBoostEndWait;

        }

        if (!_isBoost)
        {
            _isJump = true;
            _jumpCnt = 0;
            Invoke("Recovery", 0.2f);
            boostGauge -= 50;//ブーストゲージを減らす
            if (boostGauge <= 0)
            {
                boostGauge = 0;//ブーストゲージが0以下になる場合は0にする
            }
        }
    }

    IEnumerator BoostEnd()
    {
        yield return new WaitForSeconds(0.5f);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();//Rigitbody2Dコンポーネントを取得
        animator = GetComponent<Animator>();//アニメーターコンポーネントを取得
        nowAnime = idleAnime;//初期アニメーションを停止アニメーションに設定
        oldAnime = idleAnime;//前回アニメーションを停止アニメーションに設定
        PlayerInput input = GetComponent<PlayerInput>();//PlayerInputを取得
        _jumpAction = input.currentActionMap.FindAction("Jump");//ジャンプアクションを探す
        _moveAction = input.currentActionMap.FindAction("Move");//ムーブアクションを探す
        _angleX = 1;
    }
    

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"boostendwait" + _boostEndWait);

        //移動関連
        _moveVec=_moveAction.ReadValue<Vector2>();
        if (_moveVec.x > 0)
        {

            transform.localScale = new Vector2(1.0f, 1.0f);//右向き
        }
        else if (_moveVec.x < 0)
        {

            transform.localScale = new Vector2(-1.0f, 1.0f);//左向き
        }
        if (_moveVec.sqrMagnitude > 0.01f)
        {
            _angleX = _moveVec.normalized.x;
            _angleY = _moveVec.normalized.y;
        }

        //ジャンプ・ブースト入力処理
        if (_jumpAction.IsPressed()) _jumpHeld = true;
        else if(_jumpAction.WasReleasedThisFrame())_jumpHeld = false; 

        if (_moveAction.IsPressed()) _moveHeld = true;
        else if(_moveAction.WasReleasedThisFrame())_moveHeld = false;


        if(_boostEndWait<=0)EndBoost(); 

        if (_jumpAction.WasPressedThisFrame())
        {
            _onGround = false;
            _jumpCnt ++;
            StartCoroutine(JumpWait());

        }



        //接地判定
        _onGround = Physics2D.CircleCast(
    transform.position,
    0.05f,
    Vector2.down,
    0.1f,
    groundLayer
);
        //ブーストゲージが残っている場合の行動
        if (boostGauge > 0)
        {
            if (_isBoost)//ブースト開始
            {
                
                // 重力を無効にする
                rbody.gravityScale = 0f;
                // 開始時から慣性に頼らず、指定速度で動く
                rbody.linearVelocity =
                    GetBoostDirection() * boostStartSpeed;
            }
            else if (_isJump)//ジャンプボタンが押された場合
            {
                if (_recoveryTime == false)//硬直中でない場合
                {
                    //ジャンプさせる
                    Vector2 jump = new Vector2(0, jumpPower);//現在地とジャンプ力をVector2型で代入
                    rbody.AddForce(jump, ForceMode2D.Impulse);//ジャンプさせる
                    nowAnime = jumpAnime;//ジャンプアニメーションを設定
                    animator.Play(nowAnime);//アニメーション再生
                    _recoveryTime = true;//硬直中フラグを立てる
                }

            }
        }
        else if (boostGauge <= 0)//ブーストが0になった時の処理
        {
            EndBoost();
        }

        //アニメーション関連
        if (_onGround && !_isJump && !_isBoost)//接地している場合でジャンプ開始中ブースト中でない場合
        {
            if (_moveVec.x == 0.0f)//水平入力がない場合
            {
                nowAnime = idleAnime;//停止アニメーションに設定
                animator.Play(nowAnime);//現在アニメを再生
            }
            else//水平入力がある場合
            {
                nowAnime = startWalkAnime;//歩きアニメーションに設定
            }
        }
        else//接地していない場合
        {
            nowAnime = jumpAnime;//ジャンプアニメーションに設定
        }
        if (nowAnime != oldAnime)//現在のアニメーションが前回のアニメーションと違う場合
        {
            animator.Play(nowAnime);//現在のアニメーションを再生する
            oldAnime = nowAnime;//前回のアニメーションを現在のアニメーションに設定
        }



    }
    void FixedUpdate()
    {
        //ブースト中処理
        if (_isBoosting||_boostEndWait>0f)
        {
            if (!_moveHeld && !_jumpHeld)
            {
                _boostEndWait -= Time.deltaTime;
            }
            else if (_moveHeld||_jumpHeld)_boostEndWait = defaultBoostEndWait;
            // ブースト中は通常移動で速度を上書きしない

            // ブースト中は入力方向へ一定速度で移動
            Vector2 direction = GetBoostDirection();
            Vector2 boostvec = rbody.linearVelocity;
            if (_isBoost)
            {
                if (_isJump)//ジャンプボタンが押された場合
                {
                    if (_recoveryTime == false)//硬直中でない場合
                    {
                        //ジャンプさせる
                        Vector2 jump = new Vector2(0, jumpPower);//現在地とジャンプ力をVector2型で代入
                        rbody.AddForce(jump, ForceMode2D.Impulse);//ジャンプさせる
                        nowAnime = jumpAnime;//ジャンプアニメーションを設定
                        animator.Play(nowAnime);//アニメーション再生
                        _recoveryTime = true;//硬直中フラグを立てる
                        _boostEndWait = 0f;
                    }
                    rbody.linearVelocity = GetBoostDirection() * boostStartSpeed;
                }
            }
            else if (boostvec != direction * boostSpeed && !_isBoost)
            {
                rbody.linearVelocity = direction * boostSpeed;
            }
            boostGauge -= 5;//ブーストゲージを減らす
            if (boostGauge <= 0)
            {
                boostGauge = 0;//ブーストゲージが0以下になる場合は0にする
            }
            if (_isJump)
            {
                
                //ジャンプさせる
                Vector2 jump = new Vector2(0, jumpPower);//現在地とジャンプ力をVector2型で代入
                rbody.AddForce(jump, ForceMode2D.Impulse);//ジャンプさせる
                nowAnime = jumpAnime;//ジャンプアニメーションを設定
                animator.Play(nowAnime);//アニメーション再生
                _recoveryTime = true;//硬直中フラグを立てる
                _boostEndWait = 0f;
                EndBoost();
            }
        }




        // ここから下は、現在の移動・ジャンプ処理
        if (_onGround && _moveVec.x != 0.0f&&!_isBoosting&&!_isBoost)//接地していて水平入力がある場合
        {
            if (_boostEndWait < 0f) rbody.linearVelocity = new Vector2(_moveVec.x * speed, rbody.linearVelocity.y); //歩き時のRigitbody2Dの速度を設定
        }
        else if (_onGround && _moveVec.x == 0.0f&&!_isBoosting && !_isBoost)//接地していて水平入力がない場合
        {
            if(_boostEndWait<0f)rbody.linearVelocity = new Vector2(_moveVec.x * speed, rbody.linearVelocity.y);//Rigitbody2Dの速度を設定
        }
        else if (!_onGround && _moveVec.x != 0.0f&&!_isBoosting)//空中にいて水平入力がある場合
        {
            // 現在の水平速度を取得
            float vx = rbody.linearVelocity.x;

            // 入力方向に少しずつ加速
            vx += _moveVec.x * airAcceleration * Time.fixedDeltaTime;

            // 水平方向の速度を更新（Y方向は維持）
            rbody.linearVelocity = new Vector2(vx, rbody.linearVelocity.y);
        }




        if (_jumpHeld && boostGauge > 0 && !_isBoosting)//ジャンプ長押し且つブースト中でない
        {
            float vy = rbody.linearVelocity.y;

            // 下向きの速度を0にする
            if (vy < 0f)
            {
                vy = 0f;
            }

            // 長押し中は上昇する力を加える
            vy += jumpHoldAcceleration * Time.fixedDeltaTime;

            // 上昇速度の上限を設定する
            vy = Mathf.Min(vy, maxRiseSpeed);

            rbody.linearVelocity =
                new Vector2(rbody.linearVelocity.x, vy);
            boostGauge -= 10f;//ブーストゲージを減らす
            if (boostGauge <= 0)
            {
                boostGauge = 0;//ブーストゲージが0以下になる場合は0にする
            }
        }
      

            if (boostGauge < 1000 && _onGround && !_recoveryTime&&!_isBoosting&&!_isBoost)//ブーストゲージが満タンでなく、接地していて、硬直、ブースト中でない場合
        {
            Debug.Log("接地＆硬直無し");
            boostGauge = 1000;//ブーストゲージを回復する
            _boostEndWait = -0.1f;
        }


       
    }
}
