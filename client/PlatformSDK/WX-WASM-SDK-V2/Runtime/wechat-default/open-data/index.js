/* eslint-disable indent */
import { getFriendRankData, getGroupFriendsRankData, setUserRecord, setCloudScore, getPotentialFriendData } from './data/index';
import getFriendRankXML from './render/tpls/friendRank';
import getFriendRankStyle from './render/styles/friendRank';
import getFriendShareXML from './render/tpls/friendShare';
import getFriendShareStyle from './render/styles/friendShare';
import getTipsXML from './render/tpls/tips';
import getTipsStyle from './render/styles/tips';
import { showLoading } from './loading';
const Layout = requirePlugin('Layout').default;
const RANK_KEY = 'user_rank';
const sharedCanvas = wx.getSharedCanvas();
const sharedContext = sharedCanvas.getContext('2d');
// test
setUserRecord(RANK_KEY, Math.ceil(Math.random() * 1000));
const MessageType = {
    WX_RENDER: 'WXRender',
    WX_DESTROY: 'WXDestroy',
    SHOW_FRIENDS_RANK: 'showFriendsRank',
    SHOW_GROUP_FRIENDS_RANK: 'showGroupFriendsRank',
    SET_USER_RECORD: 'setUserRecord',
};

// 当前的环境配置
let currentEnvPath = "";
let shareGiftName = "";

/**
 * 根据人数对排行榜进行处理
 */
const initFriendless = (length) => {
    if (length <= 1) {
        return;
    }
    // 如果有好友删掉歪头雪狐
    const xuehuList = Layout.getElementsByClassName('xuehu');
    const node = xuehuList[0];
    node.remove();
};

/**
 * 绑定邀请好友事件
 * 温馨提示，这里仅仅是示意，请注意修改 shareMessageToFriend 参数
 */
const initShareEvents = () => {
    // 绑定邀请
    const shareBtnList = Layout.getElementsByClassName('shareToBtn');
    shareBtnList
        && shareBtnList.forEach((item) => {
            item.on('click', () => {
                console.log(`---- click ${item.dataset.isSelf} ${shareGiftName} ----`);
                if (item.dataset.isSelf === 'false') {
                    let title = '养生秘籍大公开，健康生活排行榜！';
                    let imageUrl = 'open-data/render/image/share.png';
                    if (shareGiftName && shareGiftName != "") {
                        title = `好友送你一个${shareGiftName}，快来领取吧！`;
                        imageUrl = `open-data/render/image/share-gift-${Math.floor(Math.random() * 3 + 1)}.png`;
                    }
                    console.log(`---- click share ${item.dataset.id} ${title} ${imageUrl} ----`);
                    wx.shareMessageToFriend({
                        openId: item.dataset.id,
                        title: title,
                        imageUrl: imageUrl,
                    });
                }
            });
        });
};
/**
 * 初始化开放域，主要是使得 Layout 能够正确处理跨引擎的事件处理
 * 如果游戏里面有移动开放数据域对应的 RawImage，也需要抛事件过来执行Layout.updateViewPort
 */
const initOpenDataCanvas = async (data) => {
    Layout.updateViewPort({
        x: data.x / data.devicePixelRatio,
        y: data.y / data.devicePixelRatio,
        width: data.width / data.devicePixelRatio,
        height: data.height / data.devicePixelRatio,
    });
};
// 给定 xml 和 style，渲染至 sharedCanvas
function LayoutWithTplAndStyle(xml, style) {
    Layout.clear();
    Layout.init(xml, style);
    Layout.layout(sharedContext);
    // console.log(Layout);
}
// 仅仅渲染一些提示，比如数据加载中、当前无授权等
function renderTips(tips = '') {
    LayoutWithTplAndStyle(getTipsXML({
        tips,
    }), getTipsStyle({
        width: sharedCanvas.width,
        height: sharedCanvas.height,
    }));
}
// 将好友排行榜数据渲染在 sharedCanvas
async function renderFriendsRank(scoreObjString) {
    showLoading();
    try {
        const data = await getFriendRankData(RANK_KEY);
        // console.log(" 子域 FriendRankData ", data);
        if (!data.length) {
            renderTips('暂无好友数据');
            return;
        } else {
            // const shareBtnList = Layout.getElementsByClassName('shareToBtn');
        }

        const list = [];
        data.forEach((e) => {
            // 获取游戏核心数据(属性可能没有)
            const gameInfo = JSON.parse(e.KVDataList[0].value);
            let passCount = 0;
            let skinCount = gameInfo.skinCount || 0;
            let skinInfo = gameInfo.skinInfo || "1-0-0-0-0-0-0";
            // console.log('skinInfo', gameInfo);
            if (gameInfo.dataTime && gameInfo.passCount) {
                if (isToday(gameInfo.dataTime)) {
                    // 是今天
                    passCount = gameInfo.passCount;
                }
            }
            if (scoreObjString && e.isSelf == true) {
                // 有自己的数据
                const selfInfo = JSON.parse(scoreObjString);
                skinCount = selfInfo.SkinCount || 0;
                passCount = selfInfo.PassCount || 0;
                skinInfo = selfInfo.SkinInfo || "1-0-0-0-0-0-0";
            }
            let skinPath = "open-data/render/image/1-0-0-0-0-0-0.png";
            // 环境配置有的话，就赋值cdn图片
            if (currentEnvPath.length >= 1) {
                // 皮肤地址
                skinPath = currentEnvPath + skinInfo + ".png";
            }

            if (!e.isSelf && skinCount == 0 && passCount == 0) {
                // 不是自己 且没有皮肤数据和通关数据
                return;
            }

            list.push({
                nickname: removeQuotes(e.nickname),
                openid: e.openid,
                isSelf: e.isSelf,
                avatarUrl: e.avatarUrl,
                skinCount: skinCount,
                passCount: passCount,
                skinPath: skinPath,
            });
        });

        // 自定义排序函数
        list.sort((obj1, obj2) => {
            if (obj1.passCount !== obj2.passCount) {
                // 按属性 a 进行比较
                return obj2.passCount - obj1.passCount;
            } else {
                // 属性 a 相等时，按属性 b 进行比较
                if (obj2.skinCount == obj1.skinCount) {
                    return obj1.isSelf ? -1 : 1;
                }
                return obj2.skinCount - obj1.skinCount;
            }
        });

        LayoutWithTplAndStyle(getFriendRankXML({
            data: list,
            page: 2,
            pageMax: 2,
        }), getFriendRankStyle({
            width: sharedCanvas.width,
            height: sharedCanvas.height,
        }));
        initShareEvents();
        initFriendless(list.length);
    }
    catch (e) {
        // console.error('[WX OpenData] renderFriendsRank error', e);
        renderTips('请进入设置页允许获取微信朋友信息');
    }
}

// 将好友排行榜数据渲染在 sharedCanvas
async function renderFriendsShare() {
    showLoading();
    try {
        const list = [];
        const list2 = [];

        const potentialData = await getPotentialFriendData();
        if (potentialData && potentialData.length) {
            potentialData.forEach((e) => {
                list2.push({
                    nickname: removeQuotes(e.nickname),
                    openid: e.openid,
                    isSelf: false,
                    avatarUrl: e.avatarUrl,
                });
            });
        }

        const rankData = await getFriendRankData(RANK_KEY);
        // console.log(" 子域 FriendRankData ", data);
        if (rankData && rankData.length) {
            rankData.forEach((e) => {
                if (!e.isSelf) {
                    list.push({
                        nickname: removeQuotes(e.nickname),
                        openid: e.openid,
                        isSelf: e.isSelf,
                        avatarUrl: e.avatarUrl,
                    });
                }
            });
        }

        LayoutWithTplAndStyle(getFriendShareXML({
            data: list,
            data2: list2,
            page: 2,
            pageMax: 2,
        }), getFriendShareStyle({
            width: sharedCanvas.width,
            height: sharedCanvas.height,
        }));

        if (!list.length) {
            const listFlag = Layout.getElementsByClassName('headerFlag');
            const listFlagNode = listFlag[0];
            listFlagNode.remove();
            
            const listItems = Layout.getElementsByClassName('listItem');
            if (listItems && listItems.length) {
                listItems.forEach((e) => {
                    e.remove();
                });
            }
        }

        if (!list2.length) {
            const list2Flag = Layout.getElementsByClassName('headerFlag2');
            const list2FlagNode = list2Flag[0];
            list2FlagNode.remove();

            const list2Items = Layout.getElementsByClassName('listItem2');
            if (list2Items && list2Items.length) {
                list2Items.forEach((e) => {
                    e.remove();
                });
            }
        }

        if (!list.length && !list2.length){
            // renderTips('暂无好友数据');
            return;
        }

        // 删掉歪头雪狐
        const xuehuList = Layout.getElementsByClassName('xuehu');
        const node = xuehuList[0];
        node.remove();

        initShareEvents();

    }
    catch (e) {
        console.error('[WX OpenData] renderFriendsRank error', e);
        renderTips('请进入设置页允许获取微信朋友信息');
    }
}

function removeQuotes(str) {
    return str.replace(/['"]/g, '');
}

// 判断时间戳是否是今天
function isToday(timestampInSeconds) {
    const today = new Date();
    const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate()).getTime() / 1000;
    const todayEnd = todayStart + 24 * 60 * 60 - 1;

    return timestampInSeconds >= todayStart && timestampInSeconds <= todayEnd;
}

// 渲染群排行榜
async function renderGroupFriendsRank(shareTicket) {
    showLoading();
    try {
        const data = await getGroupFriendsRankData(shareTicket, RANK_KEY);
        if (!data.length) {
            renderTips('暂无群同玩好友数据');
            return;
        }
        LayoutWithTplAndStyle(getFriendRankXML({
            data,
        }), getFriendRankStyle({
            width: sharedCanvas.width,
            height: sharedCanvas.height,
        }));
    }
    catch (e) {
        renderTips('群同玩好友数据加载失败');
    }
}

/**
 * @param {Object} msgData
 * @param {string} msgData.Event - 事件名，格式是：命令-排行榜名。比如：show-friendRank 。意思是，显示好友排行
 * @param {string} msgData.ParseText - 分数解析字符串,比如:"通关x次"
 * @param {string} msgData.ScoreObjString - 分数,为对象字符串如： {score:number,maxScore:number,state:string,isJudgeOld:boolean}
 * @param {string} msgData.Uid - 用户的识别id
 * @param {string} msgData.Env - 环境配置      
 *  Development, // 局域网 0
    Testing,     // 内网 1
    Beta,        // 测试服 2
    Production   // 正式服 3
 */
function main() {
    wx.onMessage((msgData) => {
        // console.log('[WX OpenData] onMessage', msgData);
        if (typeof msgData === 'string') {
            try {
                // eslint-disable-next-line no-param-reassign
                msgData = JSON.parse(msgData);
            }
            catch (e) {
                console.error('[WX OpenData] onMessage data is not a object');
                return;
            }
        }

        if (msgData.type) {
            switch (msgData.type) {
                // 来自 WX Unity SDK 的信息
                case MessageType.WX_RENDER:
                    initOpenDataCanvas(msgData);
                    break;
                // 来自 WX Unity SDK 的信息
                case MessageType.WX_DESTROY:
                    Layout.clearAll();
                    break;
                // 下面为业务自定义消息
                case MessageType.SHOW_FRIENDS_RANK:
                    renderFriendsRank();
                    break;
                case MessageType.SHOW_GROUP_FRIENDS_RANK:
                    renderGroupFriendsRank(msgData.shareTicket);
                    break;
                case MessageType.SET_USER_RECORD:
                    setUserRecord(RANK_KEY, msgData.score);
                    break;
                default:
                    console.error(`[WX OpenData] onMessage type 「${msgData.type}」 is not supported`);
                    break;
            }
        }

        if (msgData.Event) {
            // 自定义的消息  
            const command = msgData.Event.split('-')[0];
            const rankName = msgData.Event.split('-')[1];
            console.log(`---- command ${command} - ${rankName} ----`);
            if (command === 'update') {
                setCloudScore({
                    rankName: rankName,
                    scoreObjString: msgData.ScoreObjString,
                    complete: () => {
                        console.log('----update score success----');
                    }
                });
            } else if (command === 'show') {
                renderFriendsRank(msgData.ScoreObjString);
            } else if (command === 'sharegift') {
                // 分享参数
                shareGiftName = rankName;
            } else if (command === 'showshare') {
                // 分享参数
                renderFriendsShare();
            }
        }
        if (msgData.Env) {
            switch (msgData.Env) {
                case 0:
                case 1:
                case 2:
                    // 测试服
                    currentEnvPath = "cloud://baguan-3g4p0u0q15736ca4.6261-baguan-3g4p0u0q15736ca4-1328662183/skin/";
                    break;
                case 3:
                    // 正式服
                    currentEnvPath = "cloud://ba-guan-5g7rs1gl5465ec7c.6261-ba-guan-5g7rs1gl5465ec7c-1329276613/skin/";
                    break;
                default:
                    break;
            }
        }
    });
}
main();
