using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;



public class JsonHelper
{
    public static T[] getJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}

public class LoadComments : MonoBehaviour {
 
    // Use this for initialization
    IEnumerator Start () {



        WWW www = new WWW("https://jsonplaceholder.typicode.com/comments");
        yield return www;
        Debug.Log(www.text.Length);
 //Comments = JsonHelper.getJsonArray<Comment>(www.text);


    }
	
	
}
