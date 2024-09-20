using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Card : NetworkBehaviour
{
    public Sprite CardFront;
    public Sprite CardBack;
    
    public int CardValue;


    private Image Image;
    void Start()
    {
        Image = GetComponent<Image>();
        Image.sprite = CardBack;
    }

    public void FlipCard()
    {
        if(Image.sprite == CardBack)
        {
            Image.sprite = CardFront;
        }
        else
        {
            Image.sprite = CardBack;
        }
        
    }

}
