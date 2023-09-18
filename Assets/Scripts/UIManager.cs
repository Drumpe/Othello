using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI topText;
    [SerializeField]
    private TextMeshProUGUI blackScoreText;
    [SerializeField]
    private TextMeshProUGUI whiteScoreText;
    [SerializeField]
    private TextMeshProUGUI winnerText;
    [SerializeField]
    private Image blackOverlay;
    [SerializeField]
    private RectTransform playAgainButton;

    public void SetPlayerText(Player currentPlayer){
        if(currentPlayer == Player.Black){
            topText.text = "Black's Turn <sprite name=DiscBlackUp>";
        }
        else /* if(currentPlayer == Player.White) */{
            topText.text = "Whites's Turn <sprite name=DiscWhiteUp>";
        }
    }
    public void SetSkippedPlayerText(Player skippedPlayer){
        if(skippedPlayer == Player.Black){
            topText.text = "Black can't move! <sprite name=DiscBlackUp>";
        }
                else /* if(skippedPlayer == Player.White) */{
            topText.text = "White can't move! <sprite name=DiscWhiteUp>";
        }
    }

    public IEnumerator AnimateTopText(){
        topText.transform.LeanScale(Vector3.one * 1.25f, 0.25f).setLoopPingPong(4);
        yield return new WaitForSeconds(2);
    }

    public void setTopText(string message){
        topText.text = message;
    }
    private IEnumerator ScaleDown(RectTransform rect){
       rect.LeanScale(Vector3.zero, 0.2f);
       yield return new WaitForSeconds(0.2f);
       rect.gameObject.SetActive(false);
    }
    private IEnumerator ScaleUp(RectTransform rect){
        rect.gameObject.SetActive(true);
        rect.localScale = Vector3.zero;
        rect.LeanScale(Vector3.one, 0.2f);
        yield return new WaitForSeconds(0.2f);
    }
    public IEnumerator ShowScoreText(){
        yield return ScaleDown(topText.rectTransform);
        yield return ScaleUp(blackScoreText.rectTransform);
        yield return ScaleUp(whiteScoreText.rectTransform);
    }
    public void SetBlackScoreText(int score){
        blackScoreText.text = $"<sprite name=DiscBlackUp> {score}";
    }
    public void SetWhiteScoreText(int score){
        whiteScoreText.text = $"<sprite name=DiscWhiteUp> {score}";
    }
    public IEnumerator ShowOverlay(){
        blackOverlay.gameObject.SetActive(true);
        blackOverlay.color = Color.clear;
        blackOverlay.rectTransform.LeanAlpha(0.8f, 1f);
        yield return new WaitForSeconds(1); //Vänta på LeanAlpha
    }
    public IEnumerator HideOverlay(){
        blackOverlay.rectTransform.LeanAlpha(0, 1f);
        yield return new WaitForSeconds(1); //Vänta på LeanAlpha
        blackOverlay.gameObject.SetActive(false);
    }
    public IEnumerator MoveScoresDown(){
        blackScoreText.rectTransform.LeanMoveY(0,0.5f);
        whiteScoreText.rectTransform.LeanMoveY(0,0.5f);
        yield return new WaitForSeconds(0.5f); //Vänta på animation
    }
    public void SetWinnerText(Player winner){
        switch (winner)
        {
            case Player.Black:
                winnerText.text = "Black won!";
                break;
            case Player.White:
                winnerText.text = "White won!";
                break;
            case Player.None:
                winnerText.text = "It's a tie, both are winners!";
                break;
                
        }
    }
    public IEnumerator ShowEndScreen(){
        yield return ShowOverlay();
        yield return MoveScoresDown();
        yield return ScaleUp(winnerText.rectTransform);
        yield return ScaleUp(playAgainButton);
    }
    public IEnumerator HideEndScreen(){
        StartCoroutine(ScaleDown(winnerText.rectTransform));
        StartCoroutine(ScaleDown(blackScoreText.rectTransform));
        StartCoroutine(ScaleDown(whiteScoreText.rectTransform));
        StartCoroutine(ScaleDown(playAgainButton));

        yield return new WaitForSeconds(0.5f);
        yield return HideOverlay();
    }
}
