using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.Events;
using System.Collections;

public class ButtonEvents : MonoBehaviour {

    public Text Txt_UpInfo = null;
    public bool UploadTime = false;
    
    void Start()
    {
        
    }

    //显示文件上传的状态
    public void DisplayUpTxt()
    {
        Button UploadButton = gameObject.GetComponent<Button>();
        UploadButton.onClick.AddListener(delegate()
        {
            Txt_UpInfo.text = "正在上传中，请稍后...";

            while (UploadTime)
            {
                Txt_UpInfo.text = "上传成功！";
            }
        });
        
    }

}
