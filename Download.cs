using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

public class Download : MonoBehaviour {

    //预编译
    //不同平台下StreamingAssets的路径是不同的 
    public static readonly string PathURL =
#if UNITY_ANDROID     
    "jar:file://" + Application.dataPath + "!/assets/";  
#elif UNITY_IPHONE   
    Application.dataPath + "/Raw/";  
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR  
 "file://" + Application.dataPath + "/StreamingAssets/";
#else  
        string.Empty;  
#endif  


    ArrayList infoall; 
 
    //皮肤资源，这里用于显示中文  
    public GUISkin skin;  

    void Start ()  
    {  
        print("当前文件路径:"+Application.persistentDataPath);  
        //删除文件  
        DeleteFile(Application.persistentDataPath,"FileName.txt");  
   
        //创建文件，写入数据  
        CreateFile(Application.persistentDataPath, "FileName.txt", "");
        //下载模型  
        StartCoroutine(loadasset("http://192.168.10.105"));  
        //得到文本中每一行的内容  
        infoall = LoadFile(Application.persistentDataPath, "FileName.txt");  
  
          
    }  
    //写入模型到本地  
    IEnumerator loadasset(string url)  
    {  
        WWW w = new WWW(url);  
        yield return w;  
        if (w.isDone)  
        {  
            byte[] model = w.bytes;  
            int length = model.Length;  
            //写入模型到本地  
            CreateModelFile(Application.persistentDataPath, "Model.assetbundle", model,length);  
        }  
    }  
  
    void CreateModelFile(string path, string name, byte[] info, int length)  
    {  
        //文件流信息  
        //StreamWriter sw;  
        Stream sw;  
        FileInfo t = new FileInfo(path + "//" + name);  
        if (!t.Exists)  
        {  
            //如果此文件不存在则创建  
            sw = t.Create();  
        }  
        else  
        {  
            //如果此文件存在则打开  
            //sw = t.Append();  
            return;  
        }  
        //以行的形式写入信息  
        //sw.WriteLine(info);  
        sw.Write(info, 0, length);  
        //关闭流  
        sw.Close();  
        //销毁流  
        sw.Dispose();  
    }   
   
    /// <summary>
    /// 创建文件
    /// </summary>
    /// <param name="path">创建文件路径</param>
    /// <param name="name">创建文件名称</param>
    /// <param name="info">创建文件信息</param>
 
   void CreateFile(string path,string name,string info)  
   {  
      //文件流信息  
      StreamWriter sw;  
      FileInfo t = new FileInfo(path+"//"+ name);  
      if(!t.Exists)  
      {  
        //如果此文件不存在则创建  
        sw = t.CreateText();  
      }  
      else  
      {  
        //如果此文件存在则打开  
        sw = t.AppendText();  
      }  
      //以行的形式写入信息  
      sw.WriteLine(info);  
      //关闭流  
      sw.Close();  
      //销毁流  
      sw.Dispose();  
   }  
  
     
   /// <summary>
   /// 读取文本文件
   /// </summary>
   /// <param name="path">读取文件的路径</param>
   /// <param name="name">读取文件的名称</param>
   /// <returns></returns>
 
   ArrayList LoadFile(string path,string name)  
   {  
        //使用流的形式读取  
        StreamReader sr =null;  
        try
        {  
            sr = File.OpenText(path+"//"+ name);  
        }
        catch(Exception ex)  
        {
            print("异常信息:" + ex.Message);
            //路径与名称未找到文件则直接返回空  
            return null;  
        }  

        string line;  
        ArrayList arrlist = new ArrayList(); 
 
        while ((line = sr.ReadLine()) != null)  
        {  
            //一行一行的读取  
            //将每一行的内容存入数组链表容器中  
            arrlist.Add(line);  
        }  
        //关闭流  
        sr.Close();  
        //销毁流  
        sr.Dispose();  
        //将数组链表容器返回  
        return arrlist;  
   }    
  
    //读取模型文件  
   IEnumerator LoadModelFromLocal(string path, string name)  
   {  
       string s = null;  
#if UNITY_ANDROID  
       s = "jar:file://"+path+"/"+name;  
#elif UNITY_IPHONE  
       s = path+"/"+name;  
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR  
       s = "file://"+path+"/"+name;  
#endif  
       WWW w = new WWW(s);  
       yield return w;  
       if (w.isDone)  
       {  
           Instantiate(w.assetBundle.mainAsset);  
       }  
   }  
  

   /// <summary>
   /// 删除文件
   /// </summary>
   /// <param name="path">删除文件的路径</param>
   /// <param name="name">删除文件的名称</param>
 
   void DeleteFile(string path,string name)  
   {  
        File.Delete(path+"//"+ name);  
   }  
   
    /// <summary>
    /// 加载文件内容
    /// </summary>
   void OnGUI()  
   {  
        //用新的皮肤资源，显示中文  
        GUI.skin = skin;  
        //读取文件中的所有内容  
        foreach(string str in infoall)  
        {  
            //绘制在屏幕当中  
            GUILayout.Label(str);  
        }  
        if (GUILayout.Button("加载模型"))  
        {  
            StartCoroutine(LoadModelFromLocal(Application.persistentDataPath, "Model.assetbundle"));  
        }  
   }  
   
} 
