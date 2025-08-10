using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/*
    04.04.2021
 */
namespace Mkey
{
	public class CoinProcAnim : MonoBehaviour
	{
        //[SerializeField]
        //private float startSpeed = 0;
        //[SerializeField]
        //private float endSpeed = 5;
        [SerializeField]
        private float coinScale = 1;
        [SerializeField]
        private int coinsCount = 5;
        [SerializeField]
        private float maxDelay = 1;

        [Space(8)]
        [Header("Fountain")]
        [SerializeField]
        private float gravity = 9.8f;
        [SerializeField]
        private float lifeTime = 4f;
        [SerializeField]
        private Vector3 V01 = new Vector3(1, 2, 0);
        [SerializeField]
        private Vector3 V02 = new Vector3(1, 2, 0);
        [SerializeField]
        private GameObject [] coinPrefab;
        [SerializeField]
        private float radius = 1;
        [SerializeField]
        private bool autoJump = false;
        [SerializeField]
        private float autoJumpdelay = 0;

        [Space(8)]
        [Header("Move to target")]
        [SerializeField]
        private Transform target;
        [SerializeField]
        private float moveTime = 1f;
        [SerializeField]
        private Vector2 maxOffset;
        [SerializeField]
        private float randomSpeed = 1;
        [SerializeField]
        private EaseAnim ease;
        [SerializeField]
        private UnityEvent StartMoveEvent;
        [SerializeField]
        private UnityEvent EndMoveEvent;
        [SerializeField]
        private UnityEvent StartMoveEachEvent;
        [SerializeField]
        private UnityEvent EndMoveEachEvent;

        #region temp vars
        private List<GameObject> coinsL;
        #endregion temp vars


        #region regular
        private void Start()
		{
            if (autoJump) StartCoroutine(JumpC());
		}

		private void Update()
		{
			
		}
       
        #endregion regular

        //private void MoveAlongCurve(GameObject source)
        //{
        //    SceneCurve sc = GetComponent<SceneCurve>();
        //    sc.MoveAlongPath(source, transform, startSpeed, endSpeed, 0f, ()=> { source.transform.position = transform.position; } );
        //}

        private void MoveToTarget(GameObject source, Transform target, float delay, float time,  Action completeCallBack)
        {
            if(!source || !target)
            {
                completeCallBack?.Invoke();
                return;
            }
            TweenExt.DelayAction(source, delay, ()=> StartMoveEachEvent?.Invoke());
            SimpleTween.Move(source, transform.position, target.position, time)
                .SetDelay(delay)
                .AddCompleteCallBack(()=>{ EndMoveEachEvent?.Invoke(); completeCallBack?.Invoke(); })
                .SetEase(ease);
        }

        public void MoveToTarget()
        {
            Debug.Log("Move to target");
            if (!target) return;
            if (coinPrefab == null || coinPrefab.Length == 0) return;
            coinsL = new List<GameObject>();

            for (int i = 0; i < coinsCount; i++)
            {
                GameObject cP = coinPrefab[(int)Mathf.Repeat(i, coinPrefab.Length)];
                GameObject coinGO = Instantiate(cP, transform);
                coinsL.Add(coinGO);
            }
            float scaleTweenTime = 0.1f;

            float delay = UnityEngine.Random.Range(0, maxDelay);
            StartMoveEvent?.Invoke();

            foreach (var item in coinsL)
            {
                item.transform.localPosition = RandomRange(new Vector3(-maxOffset.x, 0, 0), new Vector3(maxOffset.x, maxOffset.y, 0));
                item.transform.localScale *= coinScale;

                Vector3 locScale = item.transform.localScale;
                item.transform.localScale = Vector3.zero;

                //scale out 
                SimpleTween.Value(item, Vector3.zero, locScale * 1.2f, scaleTweenTime).
                    SetOnUpdate((Vector3 val)=> { item.transform.localScale = val; }).
                    SetEase(EaseAnim.EaseOutBounce).
                    SetDelay(delay);

                // move random
                MoveRandomTween(item, randomSpeed, 10);
                delay += 0.03f;
            }

            foreach (var item in coinsL)
            {
                MoveToTarget(item, target, delay + scaleTweenTime + 0.00f, moveTime, () => { Destroy(item); });
                delay += 0.15f;
            }
            TweenExt.DelayAction(gameObject, delay, () => { EndMoveEvent?.Invoke(); });
        }

        public void Jump()
        {
            Debug.Log("jump");
            if (coinPrefab== null || coinPrefab.Length == 0) return;
            coinsL = new List<GameObject>();

            for (int i = 0; i < coinsCount; i++)
            {
                GameObject cP = coinPrefab[(int)Mathf.Repeat(i, coinPrefab.Length)];
                coinsL.Add(Instantiate(cP, transform));
            }

            foreach (var item in coinsL)
            {
                item.transform.localPosition = RandomRange(new Vector3(-radius, - radius, 0), new Vector3(radius, radius, 0));
                item.transform.localEulerAngles = RandomRange(new Vector3(0, 0, -30), new Vector3(0,0, 30));
                item.transform.localScale *= coinScale;
                StartCoroutine(JumpC(item.transform, UnityEngine.Random.Range(0, maxDelay), lifeTime, () => { Destroy(item); }));
            }
        }

        private IEnumerator JumpC()
        {
            yield return new WaitForSeconds(autoJumpdelay);
            Jump();
        }

        private IEnumerator JumpC(Transform t, float delay, float time, Action completeCallBack)
        {
            yield return delay;
            WaitForEndOfFrame wfef = new WaitForEndOfFrame();
            Vector3 a = new Vector3(0, - gravity, 0); 
            float dt = 0;
            Vector3 jumpV0 = RandomRange(V01, V02);
            Vector3 lPos = t.localPosition;

            while (time > dt)
            {
              //  Debug.Log("move: " + dt);
                dt += Time.deltaTime;
                if (t) t.localPosition = lPos + jumpV0 * dt + a * dt * dt / 2f;
                yield return wfef;
            }

            completeCallBack?.Invoke();
        }

        private Vector3 RandomRange(Vector3 a, Vector3 b)
        {
            return new Vector3(UnityEngine.Random.Range(a.x, b.x), UnityEngine.Random.Range(a.y, b.y), UnityEngine.Random.Range(a.z, b.z));
        }

        private void MoveRandomTween(GameObject item, float speed, float rMoveTime)
        {
            float dir = UnityEngine.Random.Range(0, 2) < 1 ? -1 : 1;

            SimpleTween.Value(item, 0, 1, rMoveTime).SetOnUpdate((float val) => 
            {
                if (val < 0.5f * rMoveTime)
                {
                    if (item) item.transform.localPosition += new Vector3(speed, speed, 0) * dir;
                }
                else
                {
                    if (item) item.transform.localPosition -= new Vector3(speed, speed, 0) * dir;
                }
            });
        }
    }
}
/*mkmatchutils*//*mkslotutils*/