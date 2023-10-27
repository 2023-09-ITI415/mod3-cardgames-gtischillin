using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour {
    public static Scoreboard S;
    public GameObject prefabFloatingScore;
    public int _score = 0;
    public string _scoreString;
    private Transform canvasTrans;

    public int score {
        get {
            return(_score);
        }
        set {
            _score = value;
            scoreString = Utils.AddCommasToNumber(_score);
        }
    }

    public string scoreString {
        get {
            return(_scoreString);
        }
        set {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }

    void Awake() {
        S = this;
        canvasTrans = transform.parent;
    }

    public void FSCallback(FloatingScore fs) {
        score += fs.score;
    }

    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts) {
        GameObject go = Instantiate(prefabFloatingScore) as GameObject;
        go.transform.SetParent( canvasTrans );
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject;
        fs.Init(pts);
        return(fs);
    }
}
