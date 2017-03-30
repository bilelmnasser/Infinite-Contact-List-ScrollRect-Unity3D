using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Contact
{
    public int _ID;
    public string _name;
    public string _Surname;

    public string _profilURL;


}



public class Data : MonoBehaviour {

    public Contact current;
    public Text text_ID;
    public Text text_NameSurname;
    public Image Profil;

    public delegate void SetDataDelegate();
  public static SetDataDelegate UpdateData;
    private void Awake()
    {
        UpdateData += SetData;
    }
    private void OnDestroy()
    {
        UpdateData -= SetData;

    }
    private int id=0;
    public int ID
    {
        get { return id; }
        set { id= value ;
            SetData();
           
        }
    }
    // Use this for initialization
    void SetData () {

        try
        {
            if (InfiniteScroll.Contacts.ContainsKey(ID))
            {

                current = InfiniteScroll.Contacts[ID];

                refreshContactInfo();
                GetComponentInChildren<Text>().text = ID.ToString();
            }
            else if (current == null)
            {
                current = new Contact();
                current._ID = ID;
                GetComponentInChildren<Text>().text = ID.ToString();
                // gameObject.SetActive(false);
                SetData();
            }
        }catch (Exception error)
        {
            Debug.Log(error.Message);
        }        
	}
	void refreshContactInfo()
    {
        if (gameObject.activeInHierarchy&&InfiniteScroll.Contacts !=null&& InfiniteScroll.Contacts.ContainsKey(ID))
        {
           // print("Refreshing Contact : " +ID);
            text_NameSurname.text = InfiniteScroll.Contacts[ID]._name + " " + InfiniteScroll.Contacts[ID]._Surname;
            StartCoroutine("loadAvatar", InfiniteScroll.Contacts[ID]._profilURL);
        }
        //get webservice of that contact and get all data download

      
        //download image


    }
	public IEnumerator loadAvatar(string url)
    {
       
           WWW www = getCachedWWW(url);
        
        // Wait for download to complete
        yield return www;

       
  
        Profil.sprite =Sprite.Create( www.texture,new Rect(0,0, www.texture.width, www.texture.height),new Vector2(0.5f,0.5f));
    }



     public WWW getCachedWWW(string url)
    {
        string filePath = Application.persistentDataPath;
        filePath += "/" + url.GetHashCode();
        string loadFilepath = filePath;
        bool web = false;
        WWW www;
        bool useCached = false;
        useCached = System.IO.File.Exists(filePath);
        if (useCached)
        {
            //check how old
            System.DateTime written = File.GetLastWriteTimeUtc(filePath);
            System.DateTime now = System.DateTime.UtcNow;
            double totalHours = now.Subtract(written).TotalHours;
            if (totalHours > 300)
                useCached = false;
        }
        if (System.IO.File.Exists(filePath))
        {
            string pathforwww = "file:///" + loadFilepath;
          //  Debug.Log("TRYING FROM CACHE " + url + "  file " + pathforwww);
            www = new WWW(pathforwww);
        }
        else
        {
            web = true;
            www = new WWW(url);
        }
        StartCoroutine(doLoad(www, filePath, web));
        return www;
    }

 

     IEnumerator doLoad(WWW www, string filePath, bool web)
    {
        yield return www;

        if (www.error == null)
        {
            if (web)
            {
                //System.IO.Directory.GetFiles
               // Debug.Log("SAVING DOWNLOAD  " + www.url + " to " + filePath);
                // string fullPath = filePath;
                File.WriteAllBytes(filePath, www.bytes);
               // Debug.Log("SAVING DONE  " + www.url + " to " + filePath);
                //Debug.Log("FILE ATTRIBUTES  " + File.GetAttributes(filePath));
                //if (File.Exists(fullPath))
                // {
                //    Debug.Log("File.Exists " + fullPath);
                // }
            }
            else
            {
              //  Debug.Log("SUCCESS CACHE LOAD OF " + www.url);
            }
        }
        else
        {
            if (!web)
            {
                File.Delete(filePath);
            }
         //   Debug.Log("WWW ERROR " + www.error);
        }
    }
}
