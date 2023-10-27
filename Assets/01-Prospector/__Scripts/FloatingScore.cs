using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum FSState {
    idle,
    pre,
    active,
    post
}

public class FloatingScore : MonoBehaviour {
    public FSState state = FSState.idle;
    public int _score = 0;
    public string scoreString;

    public int score {
        get {
            return(_score);
        }
        set {
            _score = value;
            scoreString = Utils.AddCommasToNumber(_score);
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector2> bezierPts;
    public List<float> fontSizes;

    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCuve = Easing.InOut;
    public GameObject reportFinishTo = null;

    private RectTransform rectTrans;

    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1) {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;
        bezierPts = new List<Vector2>(ePts);

        if (ePts.Count == 1) {
            transform.position = ePts[0];
            return;
        }

        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;
        state = FSState.pre;
    }

    public void FSCallback(FloatingScore fs) {
        score += fs.score;
    }

    void Update () {
        if (state == FSState.idle) return;

        float u = (Time.time - timeStart)/timeDuration;
        float uC = Easing.Ease (u, easingCuve);

        if (u<0) {
            state = FSState.pre;
            transform.position = bezierPts[0];
        } else {
            if (u>=1) {
                uC = 1;
                state = FSState.post;
                if (reportFinishTo != null) {
                    reportFinishTo.SendMessage("FSCallback", this);
                    Destroy (gameObject);
                } else {
                    state = FSState.idle;
                }
            } else {
                state = FSState.active;
            }

            Vector2 pos = Utils.Bezier(uC, bezierPts);
            rectTrans.anchorMin = rectTrans.anchorMax = pos;

            if (fontSizes != null && fontSizes.Count>0) {
                int size = Mathf.RoundToInt( Utils.Bezier(uC, fontSizes) );
                GetComponent<Text>().fontSize = size;
            }
        }
    }
}
