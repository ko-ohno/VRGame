﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class Player : MonoBehaviour
{
    public SteamVR_Input_Sources handTypeLeft;
    public SteamVR_Input_Sources handTypeRight;
    public SteamVR_Action_Boolean trig;
    public SteamVR_Action_Boolean stopButton;

    //移動速度の調整とかはエディタから触ってね
    [SerializeField]
    runHand rightHand;
    [SerializeField]
    runHand leftHand;

    [SerializeField]
    float cameraAcc = 1.0f;
    [SerializeField]
    float moveSpeed = 1.0f;
    [SerializeField]
    float jumpValue = 1.0f;
    [SerializeField]
    private float limitVelocitySize;

    //VRの時、エディタからチェックいれてくれい
    [SerializeField]
    bool isVR = false;

    Rigidbody rigidbody;
    [SerializeField]
    Transform cameraTransform;
    [SerializeField]
    Transform forwardObject;

    [SerializeField]
    DownCamera DownCameraScript;

    float jumpTIme = 10;
    bool isJump = false;
    bool Ldown = false;
    bool Rdown = false;

    enum PlayerStatus
    {
        Idle,
        Move,
        Jump,
        Fall,
    };

    PlayerStatus playerState = PlayerStatus.Idle;

    private void Awake()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        this.rightHand.SetIsVR(this.isVR);
        this.leftHand.SetIsVR(this.isVR);
    }

    private void Update()
    {
        this.ControllerSet();
        this.StateChange();
        this.Rot();
        this.Move();
        this.JumpSet();
        
    }

    private void ControllerSet()
    {
        //コントローラーのトリガーの取得
        this.Ldown = trig.GetState(handTypeLeft);
        this.Rdown = trig.GetState(handTypeRight);
    }

    void StateChange()
    {
        this.playerState = PlayerStatus.Idle;

        if (this.rigidbody.velocity.y > 0.1f) { this.playerState = PlayerStatus.Jump; }
        else if (this.rigidbody.velocity.y < -0.1f) { this.playerState = PlayerStatus.Fall; }
    }

    void JumpSet()
    {
        //ジャンプ中だったら滞空時間を加算してリターン
        if (this.isJump) { this.jumpTIme += Time.fixedDeltaTime; return; }

        if (isVR)
        {
            var mov = Mathf.Abs(this.leftHand.moveDistance) + Mathf.Abs(this.rightHand.moveDistance);
            //Debug.Log(this.leftHand.moveDistance);
            if (!this.Ldown || !this.Rdown || mov <= 0.1f) { return; }
            this.Jump();
        }
        else
        {
            if (!Input.GetKeyDown(KeyCode.Space)) { return; }
            this.Jump();
        }
    }

    void Jump()
    {
        this.isJump = true;
        this.jumpTIme = 0;
        this.rigidbody.AddForce(new Vector3(0, 100 * this.jumpValue, 0));
    }

    void Move()
    {
        var mov = Mathf.Abs(this.leftHand.moveDistance) + Mathf.Abs(this.rightHand.moveDistance);
        //Debug.Log(mov);
        if (this.playerState == PlayerStatus.Idle || this.playerState == PlayerStatus.Move)
        {
            var forward = this.forwardObject.forward;
            forward.y = 0;
            //this.transform.position += forward * this.moveSpeed * Time.fixedDeltaTime * mov * 2;

            //　移動をリジッドを用いたものに
            this.rigidbody.AddForce( forward * this.moveSpeed * Time.fixedDeltaTime * mov * 20);

        }
        else
        {
            var forward = this.forwardObject.forward;
            forward.y = 0;
            this.rigidbody.AddForce(forward * this.moveSpeed * Time.fixedDeltaTime * mov * 10);
        }

        limitVelocityXZ();



    }

    void Rot()
    {
        if (this.isVR) { return; }

        float X_Move = 0;
        float Y_Move = 0;
        if (Input.GetKey(KeyCode.A))
        {
            X_Move -= 1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            X_Move += 1;
        }

        if (Input.GetKey(KeyCode.W))
        {
            Y_Move += 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            Y_Move -= 1;
        }

        var rot = this.cameraTransform.eulerAngles;
        rot.x += -Y_Move * Time.fixedDeltaTime * this.cameraAcc * 10;
        rot.y += X_Move * Time.fixedDeltaTime * this.cameraAcc * 10;
        this.cameraTransform.eulerAngles = rot;
    }

    private void Stop()
    {
        rigidbody.velocity = Vector3.zero;

    }

    // 設定された加速度の丸め
    private void limitVelocityXZ()
    {
        if (this.rigidbody.velocity.x <= limitVelocitySize || this.rigidbody.velocity.z <= limitVelocitySize)
            return;

        Vector3 _newVerocity = Vector3.zero;

        //オーバーしすぎたサイズを抑える
        float _oversize = this.rigidbody.velocity.x + this.rigidbody.velocity.z;
        _oversize -= limitVelocitySize;
        //比率　x/z
        float _oversizeRatio = this.rigidbody.velocity.x / this.rigidbody.velocity.z;

        _newVerocity.x = this.rigidbody.velocity.x - _oversize * _oversizeRatio;
        _newVerocity.z = this.rigidbody.velocity.z - (_oversize - _oversize * _oversizeRatio);
        _newVerocity.y = this.rigidbody.velocity.y;

        this.rigidbody.velocity = _newVerocity;
    }


    private void OnCollisionEnter(Collision collision)
    {
        DownCameraScript.moveFlag = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        //壁とかに当たったらジャンプできるようにする
        if (this.jumpTIme > 0.1f)
        {
            this.isJump = false;
        }
    }
}
