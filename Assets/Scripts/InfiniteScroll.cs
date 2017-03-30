using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using System;
using System.IO;

public abstract class InfiniteScroll : ScrollRect
{

    public static Dictionary<int, Contact> Contacts;
    public int AtStartID = 1;
    public int AtEndID = 1;

    [HideInInspector]
    public bool
        initOnAwake;

    protected RectTransform t
    {
        get
        {
            if (_t == null)
                _t = GetComponent<RectTransform>();
            return _t;
        }
    }

    private RectTransform _t;

    public RectTransform[] prefabItems;
    private int itemTypeStart = 0;
    private int itemTypeEnd = 0;

    private bool init;

    public Vector2 dragOffset = Vector2.zero;

    #region abstracts	
    protected abstract float GetSize(RectTransform item);

    protected abstract float GetDimension(Vector2 vector);

    protected abstract Vector2 GetVector(float value);

    protected abstract float GetPos(RectTransform item);

    protected abstract int OneOrMinusOne();
    public int totalUsersPagesCount = 0;
    public int totalUsersCountPerPage = 0;
    public int totalUsersCount = 0;
    #endregion

    #region core
    new void Awake()
    {
       
        //initializing our dictionnary of contacts
           Contacts = new Dictionary<int, Contact>();
        //start loading first page of contacts from https://reqres.in 
        GetPages(currentpage);
        RectTransform canvas = GameObject.FindObjectOfType<Canvas>().transform.GetComponent(typeof(RectTransform)) as RectTransform;
        RectTransform rt = transform.GetComponent(typeof(RectTransform)) as RectTransform;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, canvas.sizeDelta.y);

        if (!Application.isPlaying)
            return;

        if (initOnAwake)
            Init();
    }

    public void Init()
    {
        init = true;
        float containerSize = 0;
        while (containerSize < GetDimension(t.sizeDelta))
        {
            RectTransform nextItem = NewItemAtEnd();
            containerSize += GetSize(nextItem);
        }

    }
    public float a;
    public float b;
    public float c;
    public float d;
    public float e;
    public float f;
    public float g;
    int i = 0;
    public float VelocityY = 0;
    private void FixedUpdate()
    {
        VelocityY = velocity.y;
        if (!Application.isPlaying || !init || Input.GetMouseButton(0))
            return;

        //content Y top POS
        a = GetDimension(content.sizeDelta) - (GetDimension(content.localPosition) * OneOrMinusOne());
        //scroll View Y top Pos
        b = GetDimension(t.sizeDelta);
        //content Y down POS
        c = GetDimension(content.localPosition) * OneOrMinusOne();
        //scroll View Y down Pos
        d = GetDimension(t.sizeDelta) * 0.5f;



        //+600 is here to prepare build few NewItemAtEnd 
        if (  a< b+600)
        {
            //load element at end
            if (AtEndID < totalUsersCount)
            {
                print("building Items Down");

                NewItemAtEnd();
            }
           // PreparePages();
            //margin is used to Destroy objects. We add them at half the margin (if we do it at full margin, we continuously add and delete objects)
        }
        else if ( c<d-200)
        {

            //load element at Start
            if (AtStartID > 0)
            {
                print("building Items Up");

                NewItemAtStart();

            }
            // PreparePages();
            //Using else because when items get added, sometimes the properties in UnityGUI are only updated at the end of the frame.
            //Only Destroy objects if nothing new was added (also nice performance saver while scrolling fast).
        }
        else
        {

            //Looping through all items.
            for (i=0;i<content.childCount;i++)
            {
                RectTransform child = content.GetChild(i).GetComponent<RectTransform>();
                //Item position refer to Content 
                e = GetPos(child); 
                //ScrollRect top pos
                f = GetDimension(t.sizeDelta);
                //ScrollRect down pos
                g = -(GetDimension(t.sizeDelta) + GetSize(child));


               // child.GetChild(2).GetComponent<Text>().text = " " + (e);
                //Our prefabs are inactive
                if (!child.gameObject.activeSelf)
                    continue;
                //We Destroy an item from the end if it's too far
                if (e > f-600)
                {

                    recycleUpItems(ref child);
                  

                }
                else if (e < g-600)
                {
                    recycleDownItems( ref child);
                        
                   
                   
                   
                }
            }
        }
    }

    private void recycleUpItems(ref RectTransform child)
    {
        RecycleContacts.Enqueue(child.gameObject);
        child.gameObject.name = "RecycleContacts " + RecycleContacts.Count;
        child.SetAsFirstSibling();
        child.gameObject.SetActive(false);

        //We update the container position, since after we delete something from the top, the container moves all of it's content up
        content.localPosition -= (Vector3)GetVector(GetSize(child));
        dragOffset -= GetVector(GetSize(child));
        AtStartID++;
        if (i > 0)
            i--;
    }

    private void recycleDownItems( ref RectTransform child)
    {
        RecycleContacts.Enqueue(child.gameObject);
        child.gameObject.name = "RecycleContacts " + RecycleContacts.Count;
        child.SetAsLastSibling();

        child.gameObject.SetActive(false);
        AtEndID--;
        if (i > 0)
            i--;
    }

    public Queue<GameObject> RecycleContacts = new Queue<GameObject>();
   

    private RectTransform NewItemAtStart()
    {
        // Debug.Log("NewItemAtStart");
        //Subtract (ref itemTypeStart);
        RectTransform newItem;
        if (RecycleContacts.Count > 0)
        {

            newItem = RecycleContacts.Dequeue().GetComponent<RectTransform>();
         
            newItem.gameObject.SetActive(true);
            newItem.name = AtStartID.ToString();
            newItem.GetComponent<Data>().ID = AtStartID;
            if (AtStartID > 0)
                AtStartID--;

        }
        else
        {
           
                newItem = InstantiateNextItemAtStart(itemTypeStart);
        }

        newItem.SetAsFirstSibling();

        content.localPosition += (Vector3)GetVector(GetSize(newItem));
        dragOffset += GetVector(GetSize(newItem));
        return newItem;
    }

    private RectTransform NewItemAtEnd()
    {
       // Debug.Log("NewItemAtEnd");

        
        RectTransform newItem;

        if (RecycleContacts.Count > 0)
        {
            newItem = RecycleContacts.Dequeue().GetComponent<RectTransform>();
            newItem.transform.SetAsLastSibling();
            newItem.gameObject.SetActive(true);
            newItem.GetComponent<Data>().ID = AtEndID;
            newItem.name = AtEndID.ToString();

            

            if (!Contacts.ContainsKey(AtEndID + 10) && Contacts.Count < totalUsersCount && AtEndID < totalUsersCount)
            {
                currentpage++;
                GetPages(currentpage);

            }
if (AtEndID < totalUsersCount)
                AtEndID++;
        }
        else
        {


            newItem = InstantiateNextItemAtEnd(itemTypeEnd);




        }
        //Add (ref itemTypeEnd);

        return newItem;
    }
  



  /*  public void PreparePages()
    {
       if(AtStartID==0&& AtEndID == 0)
        {
            GetPages(2);


        }else if((Mathf.CeilToInt((float)AtEndID / 10.0f) - 1)!=1)
        {
           // Debug.Log("load cache pages middle one is  : "+(Mathf.CeilToInt((float)AtEndID / 10.0f)-1));
          //  Debug.Log("Top page : "+Mathf.CeilToInt((float)AtStartID / 10.0f));
        }









    }*/
    public string UsersBaseUrl= "https://reqres.in/api/users?page=";
    private void GetPages(int v)
    {
       // Debug.Log("GetPages : " + (v - 1) + " " + v + " " + (v + 1));
        //Contacts.Clear();
      // StartCoroutine( GetUsersPage(v - 1));
        StartCoroutine(GetUsersPage(v ));
      ///  StartCoroutine(GetUsersPage(v+1));
    }

    IEnumerator GetUsersPage(int index)
    {
       // WWWForm form = new WWWForm();
       // form.AddField("page", index);

        string url = WWW.UnEscapeURL(UsersBaseUrl + index.ToString());
        
        WWW www = getCachedWWW(url);
        yield return www;
        if (www.error == null)
        {

          //  print(www.text);
            Dictionary<string, System.Object> UsersPage = MiniJSON.Json.Deserialize(www.text) as Dictionary<string, System.Object>;

           
            // print(UsersPage["data"].ToString());
            List<object> Users = (List<object>)UsersPage["data"];
            foreach (object o in Users)
            {
                Dictionary<string, System.Object> User = (Dictionary<string, System.Object>)o;
                Contact c = new Contact();
               // print(User["id"].ToString());
                c._ID = int.Parse(User["id"].ToString());
                     c._name = User["first_name"].ToString();

                     c._Surname = User["last_name"].ToString();
                c._profilURL = User["avatar"].ToString();
               // print("Adding User ID =" + c._ID);
                Contacts.Add(c._ID, c);

                //this if will complete the 100000 contacts because https://reqres.in max uses count is 12 
                if (c._ID == 12)
                {
                    int i = 13;

                    do
                    {
                        Contact cc = new Contact();
                        // print(User["id"].ToString());
                        cc._ID = i;
                        cc._name = User["first_name"].ToString()+" "+ i.ToString();

                        cc._Surname = User["last_name"].ToString() + " " + i.ToString();
                        cc._profilURL = @"https://randomuser.me/api/portraits/men/"+ UnityEngine.Random.Range(0,100)+".jpg";
                        i++;
                        
                        // print("Adding User ID =" + c._ID);
                        Contacts.Add(cc._ID, cc);
                    } while (i < 100000);

                }
                //end of fake data 
            }
            totalUsersPagesCount = int.Parse(UsersPage["total_pages"].ToString());
               totalUsersCountPerPage = int.Parse(UsersPage["per_page"].ToString());
            totalUsersCount = 100000;
           // verticalScrollbar.value = 1.0f / 100000.0f;
              //totalUsersCount = int.Parse(UsersPage["total"].ToString());
              if(Data.UpdateData!=null)
            Data.UpdateData.Invoke();

           

        }      
        else
        {
            print(www.error);


        }

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
              //  Debug.Log("SAVING DOWNLOAD  " + www.url + " to " + filePath);
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
           // Debug.Log("WWW ERROR " + www.error);
        }
    }

    private RectTransform InstantiateNextItemAtStart(int itemType)
    {

        RectTransform nextItem = Instantiate(prefabItems[itemType]) as RectTransform;
        nextItem.name = AtStartID.ToString();
        nextItem.transform.SetParent(content.transform, false);
        nextItem.gameObject.SetActive(true);
        nextItem.GetComponent<Data>().ID = AtStartID;
        if (AtStartID > 0)
            AtStartID--;



        return nextItem;
    }
    private RectTransform InstantiateNextItemAtEnd(int itemType)
    {
        RectTransform nextItem = Instantiate(prefabItems[itemType]) as RectTransform;
        nextItem.name = AtEndID.ToString();
        nextItem.transform.SetParent(content.transform, false);
        nextItem.gameObject.SetActive(true);
        nextItem.GetComponent<Data>().ID = AtEndID;
     

        if (!Contacts.ContainsKey(AtEndID+10) && Contacts.Count< totalUsersCount && AtEndID<totalUsersCount)
        {
            currentpage++;
            GetPages(currentpage);

        }
        if (AtEndID<totalUsersCount)
        AtEndID++;

        return nextItem;
    }

    #endregion

    #region overrides
    public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        dragOffset = Vector2.zero;
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
      
        //TEMP method until I found a better solution
        if (dragOffset != Vector2.zero && eventData.scrollDelta!=Vector2.zero)
        {
            OnEndDrag(eventData);
            OnBeginDrag(eventData);
            dragOffset = Vector2.zero;
        }

        base.OnDrag(eventData);
    }
    public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        dragOffset = Vector2.zero;
        base.OnEndDrag(eventData);
    }

    #endregion

    #region convenience

    public int  currentpage=1;
    private void Subtract(ref int i)
    {
        i--;

        if (i == -1)
        {
            i = prefabItems.Length - 1;
        }
    }

    private void Add(ref int i)
    {
        i++;


        if (i == prefabItems.Length)
        {
            i = 0;
        }
    }
    #endregion
}
