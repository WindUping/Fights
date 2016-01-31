using UnityEngine;
using System.Collections;
using System.Threading;
using System.IO;
using System.Net;
using System;
using System.Text;

public class Upload : MonoBehaviour {
    //文件路径
    string fileNamePath = "";
    //保存的文件名
    string saveName = "FileName.txt";
    //上传的服务器地址
    string address = "";
  
    void Start () {          
        new Thread(UploadFiles) . Start();
    }
      
    /// <summary>
    /// 上传文件调用此方法
    /// </summary>
    public void UploadFiles ()      
    {
        int returnValue = 0;
        FileStream fs = new FileStream(fileNamePath, FileMode.Open, FileAccess.Read);
          
        BinaryReader r = new BinaryReader(fs);
          
        string strBoundary = "----------" + System.DateTime.Now.Ticks.ToString("x");
          
        byte [] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + strBoundary + "\r\n");
          
        //请求头部信息
        System.Text.StringBuilder sb = new System.Text.StringBuilder( );
        sb.Append("--");
        sb.Append(strBoundary);
        sb.Append("\r\n");
        sb.Append("Content-Disposition: form-data; name=\"");
        sb.Append("file");
        sb.Append("\"; filename=\"");
        sb.Append(saveName);
        sb.Append("\"");
        sb.Append("\r\n");
        sb.Append("Content-Type: ");
        sb.Append("application/octet-stream");
        sb.Append("\r\n");
        sb.Append("\r\n");
        string strPostHeader = sb.ToString();
        byte [] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);
        // 根据uri创建HttpWebRequest对象
        HttpWebRequest httpReq = (HttpWebRequest) WebRequest.Create(new Uri(address));
        httpReq.Method = "POST";
  
        //对发送的数据不使用缓存
        httpReq . AllowWriteStreamBuffering = false;
  
        //设置获得响应的超时时间（300秒）
        httpReq.Timeout = 300000;
        httpReq.ContentType = "multipart/form-data; boundary=" + strBoundary;
        long length = fs.Length + postHeaderBytes.Length + boundaryBytes.Length;
        long fileLength = fs.Length;
        httpReq.ContentLength = length;
        try
        {
            //每次上传4k
            int bufferLength = 4096;
            byte [] buffer = new byte[bufferLength];
              
            //已上传的字节数
            long offset = 0;
              
            //开始上传时间
            DateTime startTime = DateTime.Now;
            int size = r . Read(buffer, 0, bufferLength);
            Stream postStream = httpReq.GetRequestStream();
              
            //发送请求头部消息
            postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
              
            while (size > 0)
            {
                postStream.Write(buffer, 0, size);
                offset += size;
                  
                //progressBar.Value = (int)(offset * (int.MaxValue / length));
                TimeSpan span = DateTime.Now - startTime;
                double second = span . TotalSeconds;
                Debug.Log("已用时:" + second.ToString() + "秒");
                if (second > 0.001)
                {                     
                    var speed = " 平均速度：" + (offset / 1024 / second).ToString( "0.00" ) + "KB/秒";                     
                }
                  
                else
                {                     
                    //lblSpeed.Text = " 正在连接…";
                      
                }
                  
                Debug.Log( "已上传：" + ( offset * 100.0 / length ) . ToString( "F2" ) + "%" );
                size = r.Read(buffer , 0 , bufferLength);
            }
              
            //添加尾部的时间戳
            postStream.Write( boundaryBytes , 0 , boundaryBytes.Length );
            postStream.Close();
              
            //获取服务器端的响应
            using (WebResponse webRespon = httpReq.GetResponse())
            {
                using (Stream s = webRespon.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(s))
                    {
                        //读取服务器端返回的消息
                        String sReturnString = sr.ReadLine();
                        s.Close();
                        sr.Close();


                        if (sReturnString == "Success")
                        {
                            returnValue = 1;
                        }
                        else if (sReturnString == "Error")
                        {
                            returnValue = 0;
                        }
                        else
                        {

                        }
                    }
                }
            }
        }
        catch
        {
            returnValue = 0;
        }
        finally              
        {
            fs.Close();
            r.Close();              
        }          
    }
}

