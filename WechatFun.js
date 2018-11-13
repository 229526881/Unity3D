//微信广告
var gameStatic = require('GameStatic');
let width = 0;
const REWARDED_ID='adunit-38c595d4b0a681d5'//激励视频ID
const BANNER_ID= 'adunit-9b57a8421542313e'//广告条ID
let AD = cc.Class({
    extends: cc.Component,
    properties: {
    },
    statics: {
        _instance: null
    },
    onLoad() {
        gameStatic.wechatFun = this;
        var self = this;
  
       
        cc.game.addPersistRootNode(this.node);//变成常驻节点
        if(CC_WECHATGAME)
        {
            let windowSize = cc.view.getVisibleSize();//获得屏幕尺寸
            width = windowSize.height / windowSize.width < 1.4 ? 550 : wx.getSystemInfoSync().windowWidth * 43 / 52;
        }
        self.preloadBannerAD();
        self.preloadRewardedAD();

        // bannerAd.onResize(res => {
        //     console.log(res.width, res.height)
        //     console.log(bannerAd.style.realWidth, bannerAd.style.realHeight)
        //     })
        // console.log(this.bannerAd.height)

    },
    start() {


    },
    preloadBannerAD() {
        if (CC_WECHATGAME) {
            var self = this;
            if (wx.getSystemInfoSync().SDKVersion >= '2.0.4') {
                AD._instance = this;
                this.bannerAd = wx.createBannerAd({
                    adUnitId: BANNER_ID, //广告Id
                    style: {
                        left: 0,
                        top: wx.getSystemInfoSync().windowHeight - 120,// - 118,
                        width: 300,
                        height: 60,
                    }
                });
                this.bannerAd.onResize(res => {
                    //this.bannerAd.style.top = wx.getSystemInfoSync().windowHeight - this.bannerAd.style.realHeight;
                    self.setBannerADHeight(res);
                })
                this.bannerAd.onError(function () {
                    console.log('error');
                })
            }  //cc.game.addPersistRootNode(this.node);
        }
    },
    showad: function () {
        //展示广告条
        if (CC_WECHATGAME) {
            if (wx.getSystemInfoSync().SDKVersion >= '2.0.4') {
                AD._instance.bannerAd.show();
                AD._instance.bannerAd.onResize(res => {
                    AD._instance.bannerAd.style.top = wx.getSystemInfoSync().windowHeight - AD._instance.bannerAd.style.realHeight;
                });
            }
        }
    },
    closead: function () {
        //隐藏广告条
        if (CC_WECHATGAME) {
            if (wx.getSystemInfoSync().SDKVersion >= '2.0.4') {
                AD._instance.bannerAd.hide();
            }
        }
    },
    setBannerADHeight: function (res) {
        //保证2种广告条同高
        if ((res.height / res.width) > 0.346) {
            AD._instance.bannerAd.style.width = width * 43 / 52;       
            AD._instance.bannerAd.style.left = (wx.getSystemInfoSync().windowWidth - AD._instance.bannerAd.style.realWidth) / 2;
        } else {       
            AD._instance.bannerAd.style.width = width;
            AD._instance.bannerAd.style.left = (wx.getSystemInfoSync().windowWidth - AD._instance.bannerAd.style.realWidth) / 2;
        }
    },


    //激励视频广告预加载
      //Rewarded代码
      preloadRewardedAD: function () {
        if (CC_WECHATGAME) {

            if (wx.createRewardedVideoAd) {
                this.rewardedAD = wx.createRewardedVideoAd({ adUnitId: REWARDED_ID });

                this.rewardedAD.onLoad(() => {
                    console.log('激励视频 广告加载成功');
                });

                this.rewardedAD.onError(err => {
                    console.log("Rewarded 加载失败" + err);
                });
            } else {

                wx.showModal({
                    title: '提示',
                    content: '当前微信版本过低，激励广告，无法使用该功能，请升级到最新微信版本后重试。'
                });

            }

        } else {
            cc.log("预加载 Rewarded");
        }
    },

    showRewardedAD: function (CallBack) {
        var self = this;
        this.eventCallBack = CallBack;

        if (CC_WECHATGAME) {
            if (this.rewardedAD != null) {
                this.rewardedAD.show().catch(err => {
                    // this.rewardedAD.load().then(() => this.rewardedAD.show());
                    // self.showShare();
                });

                this.rewardedAD.onError(err => {
                    console.log(err)
                });

            } else {
                cc.log("Rewarded 为  null");
            }
        } else {
            cc.log("显示 Rewarded");
        }

    },

    closeRewardedAD: function () {
        if (CC_WECHATGAME) {

            if (this.rewardedAD != null) {

                this.rewardedAD.onClose(res => {
                    // 用户点击了【关闭广告】按钮
                    // 小于 2.1.0 的基础库版本，res 是一个 undefined
                    if (res && res.isEnded || res === undefined) {
                        // 正常播放结束，可以下发游戏奖励
                        console.log("正常结束");

                        if (this.eventCallBack) {
                            this.eventCallBack();
                        } else {
                            console.log("eventCallBack 没有找到");
                        }

                        return true;
                    }
                    else {
                        // 播放中途退出，不下发游戏奖励
                        console.log("中途退出");
                        return false;
                    }

                });

            } else {
                cc.log("rewardedAD 为  null");
            }

        } else {
            cc.log("关闭 Rewarded 的回调函数");
        }

    },

});