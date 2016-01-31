using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections;
public class HTTPClient : MonoBehaviour {
    public delegate void ObtainData(string JsonString);

    public static event ObtainData OnObtainData;
    private enum Method
    { POST, GET };

    private const string Enctype = "application/x-www.form-urlencoded";

    private static string JSON;

    public static string serverURL
    {
        private get;
        set;
    }


    /// <summary>
    /// HTTP通信
    /// </summary>
    /// <param name="JSONstring"></param>
    /// <returns></returns>
    public static bool HttpConnect(string JSONstring)
    {
        JSON = JSONstring;
        HttpWebRequest request = null;
        HttpWebResponse response = null;
        int lastTime = 0;

        //网络请求线程
        Thread requestThread = new Thread(delegate()
            {
                WaitForConnection(out request);
            });

        requestThread.Start();

        //通过持续时间判断网络是否异常
        lastTime = System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000;
        while(requestThread .IsAlive)
        {
            if(System.DateTime.Now.Millisecond+System.DateTime.Now.Second*1000-lastTime<0)
            {
                lastTime = System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000;
            }
            else if(System.DateTime.Now.Millisecond+System.DateTime.Now.Second*1000-lastTime>2000)
            {
                Debug.Log("Request Lost!");
                requestThread.Abort();
                return false;
            }
        }

        //网络回应线程
        Thread responseThread = new Thread(delegate()
            {
                WaitForResponse(request, response);
            });

        responseThread.Start();

        //通过持续时间判断网络是否异常
        lastTime = System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000 - lastTime;
        while(responseThread .IsAlive )
        {
            if (System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000 - lastTime < 0)
            {
                lastTime = System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000;
            }
            else if (System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000 - lastTime > 2000)
            {
                Debug.Log("Request Lost!");
                requestThread.Abort();
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 向网络发出请求
    /// </summary>
    /// <param name="request">需要转换字符流</param>
    private static void WaitForConnection(out HttpWebRequest request)
    {
        request = (HttpWebRequest)HttpWebRequest.Create(serverURL);

        byte[] byteArray = Encoding.UTF8.GetBytes(JSON);
        request.Method = Method.POST.ToString();
        request.ContentType = Enctype;
        request.ContentLength = byteArray.Length;

        Stream dataStream = request.GetRequestStream();
        dataStream.Write(byteArray, 0, byteArray.Length);

        dataStream.Close();
    }

    /// <summary>
    /// 接收网络的响应
    /// </summary>
    /// <param name="request"></param>
    /// <param name="response">可能产生异常</param>
    private static void WaitForResponse(HttpWebRequest request,HttpWebResponse response)
    {
        using(response=(HttpWebResponse )request .GetResponse())
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                #region 响应成功
                using (Stream dataStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        string responseBody = reader.ReadToEnd();
                        try
                        {
                            OnObtainData(responseBody);
                        }
                        catch (Exception ex)
                        {
                            print("网络出现异常，异常信息：" + ex.Message);
                        }
                    }
                }
                #endregion
            }
        }
    }
}
