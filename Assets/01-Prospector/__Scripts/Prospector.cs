using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum ScoreEvent {
draw,
mine,
mineGold,
gameWin,
gameLoss
}

public class Prospector : MonoBehaviour {

	static public Prospector 	S;
static public int SCORE_FROM_PREV_ROUND = 0;
static public int HIGH_SCORE = 0;
	[Header("Set in Inspector")]
	public TextAsset			deckXML;
	public TextAsset layoutXML;
	public float xOffset = 3;
	public float yOffset = -2.5f;
	public Vector3 layoutCenter;
	public Vector2 fsPosMid = new Vector2( 0.5f, 0.90f );
public Vector2 fsPosRun = new Vector2( 0.5f, 0.75f );
public Vector2 fsPosMid2 = new Vector2( 0.4f, 1.0f );
public Vector2 fsPosEnd = new Vector2( 0.5f, 0.95f );
public float reloadDelay = 1f;
public Text gameOverText, roundResultText, highScoreText;
	[Header("Set Dynamically")]
	public Deck					deck;
	public Layout layout;
	public List<CardProspector> drawPile;
	public Transform layoutAnchor;
	public CardProspector target;
	public List<CardProspector> tableau;
	public List<CardProspector> discardPile;
	public int chain = 0;
public int scoreRun = 0;
public int score = 0;
public FloatingScore fsRun;

	void Awake(){
		S = this;
		if (PlayerPrefs.HasKey("ProspectorHighScore")) {
    HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
}

score += SCORE_FROM_PREV_ROUND;
SCORE_FROM_PREV_ROUND = 0;
SetUpUITexts();
	}
void SetUpUITexts() {
    GameObject go = GameObject.Find("HighScore");
    if (go != null) {
        highScoreText = go.GetComponent<Text>();
    }
    string hScore = "High Score:\n" + Utils.AddCommasToNumber(HIGH_SCORE);
    go.GetComponent<Text>().text = hScore;

    go = GameObject.Find ("GameOver");
    if (go != null) {
        gameOverText = go.GetComponent<Text>();
    }

    go = GameObject.Find ("RoundResult");
    if (go != null) {
        roundResultText = go.GetComponent<Text>();
    }

    ShowResultsUI( false );
}

void ShowResultsUI(bool show) {
    gameOverText.gameObject.SetActive(show);
    roundResultText.gameObject.SetActive(show);
}

	void Start() {
		Scoreboard.S.score = score;

		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle(ref deck.cards);
		layout = GetComponent<Layout>();
		layout.ReadLayout(layoutXML.text);
		drawPile = ConvertListCardsToListCardProspectors(deck.cards);
		LayoutGame();
	}
	List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD) {
    List<CardProspector> lCP = new List<CardProspector>();
    CardProspector tCP;
    
    foreach(Card tCD in lCD) {
        tCP = tCD as CardProspector; 
        lCP.Add(tCP);
    }
    return lCP;
	
}
	CardProspector Draw() {
    CardProspector cd = drawPile[0];
    drawPile.RemoveAt(0);
    return cd;
}

void LayoutGame() {
    if (layoutAnchor == null) {
        GameObject tGO = new GameObject("_LayoutAnchor");
        layoutAnchor = tGO.transform;
        layoutAnchor.transform.position = layoutCenter;
    }
    CardProspector cp;
    foreach (SlotDef tSD in layout.slotDefs) {
        cp = Draw();
        cp.faceUp = tSD.faceUp;
        cp.transform.parent = layoutAnchor;
        cp.transform.localPosition = new Vector3(
            layout.multiplier.x * tSD.x,
            layout.multiplier.y * tSD.y,
            -tSD.layerID 
        );
        cp.layoutID = tSD.id;
        cp.slotDef = tSD;
        cp.state = CardState.tableau;
        cp.SetSortingLayerName(tSD.layerName);
	    tableau.Add(cp);
    }
	foreach (CardProspector tCP in tableau) {
foreach( int hid in tCP.slotDef.hiddenBy ) {
cp = FindCardByLayoutID(hid);
tCP.hiddenBy.Add(cp);
}
}
	MoveToTarget(Draw());
UpdateDrawPile();
}
CardProspector FindCardByLayoutID(int layoutID) {
    foreach (CardProspector tCP in tableau) {
        if (tCP.layoutID == layoutID) {
            return tCP;
        }
    }
    return null;
}

void SetTableauFaces() {
    foreach (CardProspector cd in tableau) {
        bool fup = true;
        foreach (CardProspector cover in cd.hiddenBy) {
            if (cover.state == CardState.tableau) {
                fup = false;
            }
        }
        cd.faceUp = fup;
    }
}


public void CardClicked(CardProspector cd) {
    switch (cd.state) {
        case CardState.target:
            break;
        case CardState.drawpile:
            MoveToDiscard(target);
            MoveToTarget(Draw());
            UpdateDrawPile();
			ScoreManager(ScoreEvent.draw);
            break;
        case CardState.tableau:
		bool validMatch = true;
if (!cd.faceUp) {
    validMatch = false;
}
if (!AdjacentRank(cd, target)) {
    validMatch = false;
}
if (!validMatch) return;

tableau.Remove(cd);
MoveToTarget(cd);
SetTableauFaces();
ScoreManager(ScoreEvent.mine);

            break;
    }
	CheckForGameOver();
}
void CheckForGameOver() {
    if (tableau.Count == 0) {
        GameOver(true);
        return;
    }
    if (drawPile.Count > 0) {
        return;
    }
    foreach (CardProspector cd in tableau) {
        if (AdjacentRank(cd, target)) {
            return;
        }
    }
    GameOver(false);
}

void GameOver(bool won) {
    if (won) {
        print("Game Over. You won! :)");
		ScoreManager(ScoreEvent.gameWin);
    } else {
        print("Game Over. You Lost. :(");
		ScoreManager(ScoreEvent.gameLoss);
    }
	Invoke("ReloadLevel", reloadDelay);
}

void ReloadLevel() {
    SceneManager.LoadScene("__Prospector");
}
void ScoreManager(ScoreEvent sEvt) {
	List<Vector2> fsPts;
    switch (sEvt) {
        case ScoreEvent.draw:
        case ScoreEvent.gameWin:
        case ScoreEvent.gameLoss:
            chain = 0;
            score += scoreRun;
            scoreRun = 0;
			if (fsRun != null) {
    fsPts = new List<Vector2>();
    fsPts.Add( fsPosRun );
    fsPts.Add( fsPosMid2 );
    fsPts.Add( fsPosEnd );
    fsRun.reportFinishTo = Scoreboard.S.gameObject;
    fsRun.Init(fsPts, 0, 1);
    fsRun.fontSizes = new List<float>(new float[] {28,36,4});
    fsRun = null;
}
            break;
        case ScoreEvent.mine:
            chain++;
            scoreRun += chain;
			FloatingScore fs;
Vector2 p0 = Input.mousePosition;
p0.x /= Screen.width;
p0.y /= Screen.height;
fsPts = new List<Vector2>();
fsPts.Add( p0 );
fsPts.Add( fsPosMid );
fsPts.Add( fsPosRun );
fs = Scoreboard.S.CreateFloatingScore(chain,fsPts);
fs.fontSizes = new List<float>(new float[] {4,50,28});
if (fsRun == null) {
    fsRun = fs;
    fsRun.reportFinishTo = null;
} else {
    fs.reportFinishTo = fsRun.gameObject;
}

            break;
    }

    switch (sEvt) {
        case ScoreEvent.gameWin:
            Prospector.SCORE_FROM_PREV_ROUND = score;
            print("You won this round! Round score: " + score);
			gameOverText.text = "Round Over";
roundResultText.text ="You won this round!\nRound Score: Ê" + score;
ShowResultsUI( true );

            break;
        case ScoreEvent.gameLoss:
		gameOverText.text = "Game Over";
            if (Prospector.HIGH_SCORE <= score) {
                print("You got the high score! High score: " + score);
				string str = "You got the high score!\nHigh score: Ê" + score;
roundResultText.text = str;

                Prospector.HIGH_SCORE = score;
                PlayerPrefs.SetInt("ProspectorHighScore", score);
            } else {
                print("Your final score for the game was: " + score);
				roundResultText.text = "Your final score was: "+score;
            }
			ShowResultsUI( true );

            break;
        default:
            print("score: " + score + " scoreRun:" + scoreRun + " chain:" + chain);
            break;
    }
}


void MoveToDiscard(CardProspector cd) {
    cd.state = CardState.discard;
    discardPile.Add(cd);
    cd.transform.parent = layoutAnchor;
    cd.transform.localPosition = new Vector3(
        layout.multiplier.x * layout.discardPile.x,
        layout.multiplier.y * layout.discardPile.y,
        -layout.discardPile.layerID + 0.5f
    );
    cd.faceUp = true;
    cd.SetSortingLayerName(layout.discardPile.layerName);
    cd.SetSortOrder(-100 + discardPile.Count);
}

void MoveToTarget(CardProspector cd) {
    if (target != null) MoveToDiscard(target);
    target = cd;
    cd.state = CardState.target;
    cd.transform.parent = layoutAnchor;
    cd.transform.localPosition = new Vector3(
        layout.multiplier.x * layout.discardPile.x,
        layout.multiplier.y * layout.discardPile.y,
        -layout.discardPile.layerID
    );
    cd.faceUp = true;
    cd.SetSortingLayerName(layout.discardPile.layerName);
    cd.SetSortOrder(0);
}

void UpdateDrawPile() {
    CardProspector cd;
    for (int i = 0; i < drawPile.Count; i++) {
        cd = drawPile[i];
        cd.transform.parent = layoutAnchor;
        Vector2 dpStagger = layout.drawPile.stagger;
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
            layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
            -layout.drawPile.layerID + 0.1f * i
        );
        cd.faceUp = false;
        cd.state = CardState.drawpile;
        cd.SetSortingLayerName(layout.drawPile.layerName);
        cd.SetSortOrder(-10 * i);
		
    }
}
public bool AdjacentRank(CardProspector c0, CardProspector c1) {
    if (!c0.faceUp || !c1.faceUp) return false;
    if (Mathf.Abs(c0.rank - c1.rank) == 1) {
        return true;
    }
    if (c0.rank == 1 && c1.rank == 13) return true;
    if (c0.rank == 13 && c1.rank == 1) return true;
    return false;
}
}


