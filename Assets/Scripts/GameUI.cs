using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance {  get; set; }
    void Awake()
    {
        Instance = this;
    }

    public void OnLocalGameButton()
    {
        Debug.Log("OnlocalGameButton");
    }
    public void OnOnlineGameButton()
    {
        Debug.Log("OnOnlineGameButton");
    }
    public void OnOnlineHostButton()
    {
        Debug.Log("OnOnlineHostButton");
    }
    public void OnOnlineConnectButton()
    {
        Debug.Log("OnOnlineConnectButton");
    }
    public void OnOnlineBackButton()
    {
        Debug.Log("OnOnlineBackButton");
    }
    //

}
