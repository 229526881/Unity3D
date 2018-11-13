//游戏内容初始化
let gameStatic = require('GameStatic');
let len;
let Share = cc.Class({
    name: 'Share',
    properties: {
        title: '分享',
        image: '',
    },
});
const url='https://www.7cgames.cn/GameRes/7CGamesBoxWX';
cc.Class({
    extends: cc.Component,
    properties: {    
        audioArray:
        {
            //音频
            type: cc.AudioClip,
            default: [],
        },
        //特定的属性初始化比如颜色贴图等
        colorArray: [cc.SpriteFrame],
        propArray: [cc.SpriteFrame],
        partiCleArray: [],

        shareArray: [], //分享信息
        shareMes: cc.TextAsset, //分享的文本信息
    },

    // LIFE-CYCLE CALLBACKS:

    onLoad() {
        gameStatic.resource = this;
        cc.game.addPersistRootNode(this.node);
        this.ResetColor();
        this.ResetShare();

    },

    start() {
        var self = this;
        self.RestMoreList(); //初始化更多游戏的二维码
        if (CC_WECHATGAME) {
            //初始化转发菜单
            var shareMes = self.GetShare();
            wx.showShareMenu();
            cc.loader.loadRes(shareMes.image, function (err, path) {
                wx.onShareAppMessage(function () {
                    return {
                        title: shareMes.title,
                        imageUrl: path.url
                    }
                })
            });
        }
    },

    ResetColor() {
        let color1 = cc.color(229, 45, 39, 255);
        let color2 = cc.color(28, 255, 97, 255);
        let color3 = cc.color(160, 68, 255, 255);
        let color4 = cc.color(231, 233, 62, 255);
        this.partiCleArray.push(color1)
        this.partiCleArray.push(color2)
        this.partiCleArray.push(color3)
        this.partiCleArray.push(color4)
        //  红绿紫黄
        //  红黄紫绿
    },
    // update (dt) {},
    PlayEffect(id) {
        //播放音效
        if (Global.isQuiet == false) {
            let au = this.audioArray[id];
            this.scheduleOnce(function(){
                cc.audioEngine.playEffect(au, false);
            })       
        }

    },   
    ResetShare() {
        //初始化分享信息
        var shareArr = this.shareArray;
        var shareAr = this.shareMes.text.split('|');
         len=shareAr.length-1;
        for (let i = 0; i < len; i++) {  
            var mes = shareAr[i].split('%');
            var share = new Share();
            //console.log(mes[0]===2);   
            share.title = mes[0];
            share.image = mes[1]
            shareArr.push(share);
        }
    },
    GetShare() {
        let ran = Math.floor(Math.random() * len);
        return this.shareArray[ran];
    },
  
    RestMoreList(){
        if(CC_WECHATGAME){
            wx.request({
                url: url+'QRCode/WXGamesCode.json',
                headers: {
                    'Content-Type': 'application/json'
                },
                success: function (res) {
                    Global.moreGameList = res.data.data;
                }
            })
        }
      
    },
});
