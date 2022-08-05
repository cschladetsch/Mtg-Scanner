using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    public Canvas Main;
    public Canvas Art;
    public Canvas Rulings;

    private void Start()
    {
        SelectMain();
    }

    public void SelectMain() {
        DisableAllCanvases();
        Main.enabled = true;
    }

    public void SelectArt() { 
        DisableAllCanvases();
        Art.enabled = true;
    }

    public void SelectRulings() { 
        DisableAllCanvases();
        Rulings.enabled = true;
    }

    void DisableAllCanvases() {
        Main.enabled = false;
        Art.enabled = false;
        Rulings.enabled = false;
    }
}
