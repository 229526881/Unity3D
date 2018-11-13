let gameStatic = require('GameStatic');
let Length = 1710;
let pnlChange = null;
let player = null;
let overUI;
let gameUI;
let GameStatus = cc.Enum({
    start: 0,
    run: 1,
    relive: 2,
    over: 3,
});
let gradeLabel;
cc.Class({
    extends: cc.Component,

    properties: {
        isTest:false,
        Grade: {
            get: function (value) {
                return Global.grade;
            },
            set: function (value) {
                //cc.sys.localStorage.set('Grade',value);  //储存分数
                Global.grade = value;
                let num = value > 1000 ? value / 1000 : value;
                //其实浮点数是不支持位运算的，所以会先把1.1转成整数1再进行位运算，就好像是对浮点数向下求整。所以1|0的结果就是1。
               // num = (num | 0) === num ? num : num.toFixed(2);
                num =Math.floor(num)=== num ? num : num.toFixed(2);
                // if(gradeLabel.scale==1)
                // {
                //     gradeLabel.runAction(big);
                //     this.scheduleOnce(function(){
                //         gradeLabel.runAction(small);
                //     },0.2)
                // }

                gradeLabel.getComponent(cc.Label).string = num + (value > 1000 ? 'K' : '');
            }
        },
        BestGrade: {
            get: function () {
                return parseInt(cc.sys.localStorage.getItem('Best'));
            
            },
            set: function (value) {
                //cc.sys.localStorage.set('Grade',value);  //储存分数
                //在游戏开始前赋值一次保证不会为空
                
                if (cc.sys.localStorage.getItem('Best') == null||cc.sys.localStorage.getItem('Best') == ''||(value > parseInt(cc.sys.localStorage.getItem('Best')))) {
                    cc.sys.localStorage.setItem('Best', value + '');
                    //向排行榜更新数据Todo
                    gameStatic.rankView.submitScoreButtonFunc(value);//向排行榜提交分数
                }
            
                
            }
        },
        IsQuiet: {
            get: function () {
                let value = false;
                if (cc.sys.localStorage.getItem('IsQuiet') != null) {
                    //Global.isQuiet=false;
                    value = parseInt(cc.sys.localStorage.getItem('IsQuiet')) == 1 ? true : false
                    console.log('value' + value);
                }

                cc.sys.localStorage.setItem('IsQuiet', value + '');
                return value;
            },
            set: function (value) {
                //cc.sys.localStorage.set('Grade',value);  //储存分数
                Global.isQuiet = value;
                let save1 = value == true ? 1 : 0;
                cc.sys.localStorage.setItem('IsQuiet', save1 + '');
                console.log(cc.sys.localStorage.getItem('IsQuiet'));
            }
        },

        shotRate: 4,
        gameStatus: 0,
        rateText: cc.Label,
        powerText: cc.Label,
        colorArray: [cc.SpriteFrame],
        propArray: [cc.SpriteFrame],
    },


    //每秒射出的个数rat和射出的速度Speed有如下关系 ,len为物体与顶部的距离差
    //(rat/2)*speed*(1/rat)=len/(rat/2)
    //换算得 speed=4(len)/rat

    onLoad() {
  
      // cc.sys.localStorage.clear();
        gameStatic.gameManage = this;
        gameStatic.rankView=cc.find('Canvas/RankUI').getComponent('RankView');
        pnlChange = this.node.getComponent('PnlChange');
        player = cc.find('Canvas/GameUI/Player');
        overUI = cc.find('Canvas/OverUI');
        gameUI = cc.find('Canvas/GameUI');
        gradeLabel = cc.find('Canvas/GameUI/Title/Grade');
        Global.Boom=cc.find('Canvas/GameUI/Boom');
        Global.playerPos = cc.v2(0, -700);
        var manager = cc.director.getCollisionManager();
        // var manager = cc.director.getPhysicsManager();
        manager.enabled = true;
       //  manager.enabledDebugDraw = true;
        // manager.enabledDrawBoundingBox = true;//绘制碰撞框
        Global.leftLimit = -490;
        Global.rightLimit = 490;
        this.JudgeHeight();
        Global.isQuiet = this.IsQuiet;
    },
    start() {
        // this.ChangeRateOrPower(0, 6);
        // this.ChangeRateOrPower(1, this.shotRate);
        // Global.shotTime = 1;
        //this.BestGrade = 0;
        gameStatic.rankView.removeRank();

       // gameStatic.gateGrate.ResetBrick();
    },
    JudgeHeight() {
        //计算屏幕高度
        let windowSize = cc.view.getVisibleSize();//获得屏幕尺寸
        let height = Math.round((1080 / windowSize.width) * windowSize.height);
        let length = 750 + height / 2;
        //计算物体与顶部的距离
        Length = length;
        Global.height = height;
        return length;

    },
    JudgeShotSpeed() {
        return Length;
    },
    ReLiveForOver() {
      //cc.director.pause();
        cc.director.emit('overShot', {});
        cc.director.emit('over', {});
        //销毁子弹
      
        if (Global.reliveTime >= 1&& this.Grade>200) {
            //显示复活界面

            Global.Pause=true;
            this.scheduleOnce(function(){
                player.getComponent(cc.Sprite).enabled = false;
                Global.Pause=false;
               },0.5)
            this.gameStatus = 2;
            //隐藏player和发射的球               
           pnlChange.ShowRelive();
            //游戏暂停要重新考虑TODO 初步定为7.5秒后停止生成
            Global.reliveTime--;
        }
        else {
            this.GameOver();
        }
    },
    Relive() {
        gameStatic.gateSpawn.HideGate();
        this.gameStatus = 1;
        player.getComponent('PlayerManage').Reset();
        //恢复关卡创建的逻辑  
        cc.director.emit('relive', {});
        cc.director.emit('reliveShot', {});
    },
    GameOver() {
        //游戏结算操作
        this.BestGrade = this.Grade;//尝试保存数据
        Global.Pause=true;
        this.scheduleOnce(function(){         
            Global.Pause=false;
            this.gameStatus = 3;
            gameStatic.rankView.gameOverButtonFunc();
            overUI.active = true;
            gameUI.active = false;
           },0.5)
      
    },
    ResetGame() {
        gameStatic.gateSpawn.HideGate();
        this.gameStatus = 1;
        player.getComponent('PlayerManage').Reset();
        this.Grade = 0;
        this.ChangeRateOrPower(0,6);
        this.ChangeRateOrPower(1, 4);
        Global.shotTime =1;
        Global.row = 1;
        Global.reliveTime = 1;
        cc.director.emit('reset', {});
        cc.director.emit('start', {});
        cc.director.emit('startShot', {});
        //恢复关卡创建的逻辑
    },
    // update (dt) {},
    ChangeRateOrPower(id, num) {
        //更新显示
        if (id == 0) {
            Global.shotPower = num;
            console.log(num + '+++++++12')
            this.powerText.string = '威力：' + num;
        }
        else if (id == 1) {
            Global.shotRate = num;
            //Global.shotSpeed = this.JudgeShotSpeed();
            Global.shotSpeed = 1710;
            this.rateText.string = '射速：' + num;
            console.log(num + '+++++++1')
        }

    },

});
