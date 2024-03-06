
using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    #region Singleton implementation
    public static Client Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }
    #endregion


    public NetworkDriver driver;
    private NetworkConnection connection;

    private bool isActive = false;

    public Action connectionDropped;

    //methods
    public void Init(string ip , ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndpoint endpoint = NetworkEndpoint.Parse(ip, port);

        connection = driver.Connect(endpoint);

        Debug.Log("Attempting to connect to Server on " + endpoint.Address);
        
        isActive = true;

        // RegisterToEvent();
    }
    public void ShutDown()
    {
        if (isActive)
        {
            // UnregisterToEvent();
            driver.Dispose();
            isActive = false;

            connection = default(NetworkConnection);
        }
    }
    public void OnDestroy()
    {
        ShutDown();
    }

    public void Update()
    {
        if (!isActive)
            return;


        driver.ScheduleUpdate().Complete();
        CheckAlive();

        UpdateMessagePump();
    }
    private void CheckAlive()
    {
        if(!connection.IsCreated && isActive)
        {
            Debug.Log("Something went wrong lost con to server");
            connectionDropped?.Invoke();
            ShutDown();
        }
    }
    private void UpdateMessagePump()
    {
        DataStreamReader stream;

      
        NetworkEvent.Type cmd;
       // while ((cmd = connection.PopEvent(driver, out stream))
        {
            if (cmd == NetworkEvent.Type.Data)
            {
                //NetUtility.OnData(stream, connections[i],this)
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client disconnected from server");
                connections[i] = default(NetworkConnection);
                connectionDropped?.Invoke();
                ShutDown(); //this doesnt happen usually (bcs this is a 2prsn game)
            }
        }
        
    }





}
