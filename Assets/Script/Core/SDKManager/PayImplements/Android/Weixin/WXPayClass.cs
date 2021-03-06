﻿using FrameWork.SDKManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WXPayClass : PayInterface
{
    string goodsID;
    string prepayID;
    GameObject androidListener;
    public override List<RuntimePlatform> GetPlatform()
    {
        return new List<RuntimePlatform>() { RuntimePlatform.Android, RuntimePlatform.WindowsEditor};
    }
    public override StoreName GetStoreName()
    {
        return StoreName.WX;
    }
    public override void Init()
    {
        Debug.LogWarning("=========WXPayClass Init===========");
        SDKManagerNew.OnPayCallBack += SetPayResult;
        GlobalEvent.AddTypeEvent<PrePay2Client>(OnPrePay);
        //WXPayReSend.Instance.ReSendPay();
    }

    /// <summary>
    /// 获得预支付订单
    /// </summary>
    /// <param name="e"></param>
    /// <param name="args"></param>
    private void OnPrePay(PrePay2Client e, object[] args)
    {

        Debug.LogWarning("OnPrePay=========：" + e.prepay_id + "=partnerId==");
        DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        string nonceStr = "Random" +UnityEngine.Random.Range(10000, 99999).ToString();
        string timeStamp = (new DateTime(DateTime.UtcNow.Ticks - dt1970.Ticks).AddHours(8).Ticks / 10000000).ToString();
        string stringA = "appid=wx3ce30b3054987098&" + "nonceStr=" + nonceStr + "&packageValue=Sign=WXPay&" + "partnerId=1526756671&" + "prepayId=" + e.prepay_id + "&timeStamp=" + timeStamp;
        string stringSignTemp = stringA + "&key=a8f73a2a5ecfafab1ea80515ef0efbad";
        string sign = MD5Tool.GetMD5FromString(stringSignTemp);

        OnPayInfo onPayInfo = new OnPayInfo();
        onPayInfo.isSuccess = true;
        onPayInfo.goodsId = e.goodsID;
        onPayInfo.storeName = StoreName.WX;
        //WXPayReSend.Instance.AddPrePayID(onPayInfo);
        IndentListener(e.prepay_id,nonceStr, timeStamp, sign);
    }

    /// <summary>
    /// 统一支付入口
    /// </summary>
    /// <param name="goodsID"></param>
    /// <param name="tag"></param>
    /// <param name="goodsType"></param>
    /// <param name="orderID"></param>
    public override void Pay(string goodsID, string tag, FrameWork.SDKManager.GoodsType goodsType = FrameWork.SDKManager.GoodsType.NORMAL, string orderID = null)
    {
        this.goodsID = goodsID;
        
        Debug.LogWarning("send WXpay----message-----" + goodsID);
        //给服务器发消息1
        PrePay2Service.SendPrePayMsg(StoreName.WX, goodsID);
    }


    /// <summary>
    /// 消息1 的监听， 获得订单信息，然后调支付sdk
    /// </summary>
    private void IndentListener(string prepayid, string nonceStr ,string timeStamp , string sign)
    {
        this.prepayID = prepayid;

        string tag = prepayid + "|" + nonceStr + "|" + timeStamp + "|" + sign;

        SDKManagerNew.Pay("WeiXin.WeiXinSDK", prepayid, tag);
    }

    /// <summary>
    /// 从sdk返回支付结果
    /// </summary>
    /// <param name="result"></param>
    /// <param name="goodID"></param>
    /// <param name="Mch_orderID"></param>
    public void SetPayResult(OnPayInfo info)
    {
        Debug.LogWarning("wxPay Result======" + info.isSuccess);

        OnPayInfo payInfo = new OnPayInfo();
        payInfo.isSuccess = info.isSuccess;
        payInfo.goodsId = goodsID;
        payInfo.goodsType = GetGoodType(goodsID);
        payInfo.receipt = prepayID;
        payInfo.storeName = StoreName.WX;

        PayCallBack(payInfo);
        
    }

    public override void ConfirmPay(string goodsID, string tag)
    {
        //WXPayReSend.Instance.ClearPrePayID(tag);
    }

}
