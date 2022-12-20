using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MummyAgent : Agent
{
    private Transform tr;
    private Rigidbody rb;
    public Transform targetTr;
    public Renderer floorRd;

    public Material originMt;
    public Material badMt;
    public Material goodMt;

    // 초기화 작업을 위해 한번 호출되는 메소드
    public override void Initialize()
    {
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();

        MaxStep = 1000;
    }

    // 에피소드(학습단위)가 시작할때마다 호출
    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        tr.localPosition = new Vector3(Random.Range(-4.0f, 4.0f), 0.05f, Random.Range(-4.0f, 4.0f));
        targetTr.localPosition = new Vector3(Random.Range(-4.0f, 4.0f), 0.55f, Random.Range(-4.0f, 4.0f));

        StartCoroutine(RevertMaterial());
    }

    IEnumerator RevertMaterial()
    {
        yield return new WaitForSeconds(0.2f);
        floorRd.material = originMt;
    }

    // 환경 정보를 관측 및 수집해 정책 결정을 위해 브레인에 전달하는 메소드
    public override void CollectObservations(Unity.MLAgents.Sensors.VectorSensor sensor)
    {
        sensor.AddObservation(targetTr.localPosition);
        sensor.AddObservation(tr.localPosition);
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);

    }

    // 브레인(정책)으로부터 전달 받은 행동을 실행하는 메소드
    public override void OnActionReceived(ActionBuffers actions)
    {
        var actionsIn = actions.ContinuousActions;
        float h = Mathf.Clamp(actionsIn[0], -1.0f, 1.0f);
        float v = Mathf.Clamp(actionsIn[1], -1.0f, 1.0f);
        Vector3 dir = (Vector3.forward * v) + (Vector3.right * h);
        rb.AddForce(dir.normalized * 50.0f);

        SetReward(-0.001f);
    }
  
    // 개발자(사용자)가 직접 명령을 내릴때 호출하는 메소드(주로 테스트 용도 또는 모방학습에 사용)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
        Debug.Log($"[0]={actions[0]} [1]={actions[1]}");

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("DaedZone"))
        {
            floorRd.material = badMt;
            SetReward(-1.0f);
            EndEpisode();
        }

        if (collision.collider.CompareTag("Target"))
        {
            floorRd.material = goodMt;
            SetReward(+1.0f);
            EndEpisode();
        }
    }

}
