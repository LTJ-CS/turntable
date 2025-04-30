/* eslint-disable no-param-reassign */
import { getCurrTime, promisify } from './utils';
const getFriendCloudStorage = promisify(wx.getFriendCloudStorage);
const getGroupCloudStorage = promisify(wx.getGroupCloudStorage);
const setUserCloudStorage = promisify(wx.setUserCloudStorage);
const getPotentialFriendList = promisify(wx.getPotentialFriendList);
const getUserInfo = promisify(wx.getUserInfo);
/**
 * 获取用户信息
 * API文档可见：https://developers.weixin.qq.com/minigame/dev/api/open-api/data/OpenDataContext-wx.getUserInfo.html
 */
export function getSelfData() {
    return getUserInfo({
        openIdList: ['selfOpenId'],
    }).then((res) => res.data[0] || {});
}
let getSelfPromise;
/**
 * 将 UserGameData 数据反序列化
 * @param { UserGameData } item
 * https://developers.weixin.qq.com/minigame/dev/api/open-api/data/UserGameData.html
 */
function getWxGameData(item) {
    let source;
    try {
        source = JSON.parse(item.KVDataList[0].value);
    }
    catch (e) {
        source = {
            wxgame: {
                score: 0,
                update_time: getCurrTime(),
            },
        };
    }
    return source.wxgame;
}
/**
 * 处理 getFriendCloudStorage 和 getGroupCloudStorage 返回的在玩好友数据
 */
function rankDataFilter(res, selfUserInfo = false) {
    const data = (res.data || []).filter((item) => item.KVDataList && item.KVDataList.length);
    return data
        .map((item) => {
            const { score, update_time: updateTime } = getWxGameData(item);
            item.score = score;
            item.update_time = updateTime;
            /**
             * 请注意，这里判断是否为自己并不算特别严谨的做法
             * 比较严谨的做法是从游戏域将openid传进来，示例为了简化，简单通过 avatarUrl 来判断
             */
            if (selfUserInfo && selfUserInfo.avatarUrl === item.avatarUrl) {
                item.isSelf = true;
            }
            return item;
        })
        // 升序排序
        .sort((a, b) => b.score - a.score);
}
/**
 * 获取好友排行榜列表
 * API文档可见：https://developers.weixin.qq.com/minigame/dev/api/open-api/data/wx.getFriendCloudStorage.html
 */
export function getFriendRankData(key, needMarkSelf = true) {
    // console.log('[WX OpenData] getFriendRankData with key: ', key);
    return getFriendCloudStorage({
        keyList: [key],
    }).then((res) => {
        // console.log('[WX OpenData] getFriendRankData success: ', res);
        if (needMarkSelf) {
            getSelfPromise = getSelfPromise || getSelfData();
            return getSelfPromise.then(userInfo => rankDataFilter(res, userInfo));
        }
        return rankDataFilter(res);
    });
}
/**
 * 获取群同玩成员的游戏数据。小游戏通过群分享卡片打开的情况下才可以调用。该接口需要用户授权，且只在开放数据域下可用。
 * API文档可见: https://developers.weixin.qq.com/minigame/dev/api/open-api/data/wx.getGroupCloudStorage.html
 */
export function getGroupFriendsRankData(shareTicket, key, needMarkSelf = true) {
    // console.log('[WX OpenData] getGroupFriendsRankData with shareTicket and key: ', shareTicket, key);
    return getGroupCloudStorage({
        shareTicket,
        keyList: [key],
    }).then((res) => {
        // console.log('[WX OpenData] getGroupFriendsRankData success: ', res);
        if (needMarkSelf) {
            getSelfPromise = getSelfPromise || getSelfData();
            return getSelfPromise.then(userInfo => rankDataFilter(res, userInfo));
        }
        return rankDataFilter(res);
    });
}
/**
 * 写入用户排行榜数据，value 的值一般只需要为 Object 经过 JSON.stringify 的字符串即可。
 * 但排行榜支持展示在游戏中心，因此这里统一用游戏中心需要的数据结构执行上报，需要展示在游戏中心的数据可以经过以下操作：
 * mp.weixin.qq.com 的小游戏管理后台“设置 - 游戏 - 排行榜设置”下配置对应的 key 以及相关排行榜属性。
 * @param { String } key   排行榜对应的 key
 * @param { Number } score 排行榜对应的分数
 * @param { Object } extra 除了分数还需要写入的其他字段，不需要不填即可
 * @example
 * setUserRecord('user_rank', 100, { type: 'coin' })
 */
export function setUserRecord(key, score = 0, extra = {}) {
    // console.log('[WX OpenData] setUserRecord: ', score);
    return setUserCloudStorage({
        KVDataList: [
            {
                key,
                value: JSON.stringify({
                    wxgame: {
                        score,
                        // 时间单位：秒
                        update_time: getCurrTime(),
                    },
                    // wxgame下开发者不可自定义其他字段， wxgame同级开发者可自由定义，比如定义一个detail 字段，用于存储取得该分数的中间状态。
                    ...extra,
                }),
            },
        ],
    }).then((res) => {
        console.log('[WX OpenData] setUserRecord success: ', res);
    });
}


/**
 * @param {Object} msgData
 * @param {string} msgData.event - 事件名，格式是：命令-排行榜名。比如：show-friendRank 。意思是，显示好友排行
 * @param {string} msgData.parseText - 分数解析字符串,比如:"通关x次"
 * @param {string} msgData.scoreObjString - 分数,为对象字符串如： {score:number,maxScore:number,state:string,isJudgeOld:boolean}
 * @param {string} msgData.openId - 用户的识别id
 */
export function setCloudScore({ rankName, scoreObjString, complete }) {
    let scoreData = {
        state: 0,
        score: 0,
    };
    // 子域 测试 添加新的key可能需要后台配置 先用系统默认的
    rankName = "user_rank";
    if (scoreObjString) {
        scoreData = JSON.parse(scoreObjString);
        // console.log(" 子域 解析传入的数据 ", scoreData);
    }
    const data = {
        wxgame: {
            score: scoreData.score || 0,
            update_time: Math.floor(Date.now() / 1000),
        },
        state: scoreData.state || 0,
        score: scoreData.score || 0,
        skinCount: scoreData.SkinCount || 0,
        uid: scoreData.Uid || "",
        passCount: scoreData.PassCount || 0,
        dataTime: scoreData.DataTime || 0,
        skinInfo: scoreData.SkinInfo || "1-0-0-0-0-0-0",
    };
    const updateScore = () => {
        // 调用更新分数方法
        // console.log(" 子域 开始设置用户数据 ", rankName, data);
        wx.setUserCloudStorage({
            KVDataList: [
                {
                    key: rankName,
                    value: JSON.stringify(data),
                },
            ],
            success: (res) => {
                // console.log(`set ${rankName} UserCloudStorage suc = `, res);
            },
            fail: (err) => {
                console.log("setUserCloudStorage fail = ", err);
            },
            complete,
        });
    };
    if (scoreData.isJudgeOld) {
        getSelfCloud(rankName, (oldScore) => {
            if (typeof oldScore === 'number') {
                if (oldScore < scoreData.score) {
                    updateScore();
                } else {
                    console.log('not need update');
                }
            } else {
                updateScore();
            }
        });
    } else {
        updateScore();
    }

    /**
 * 获得自己的云数据
 */
    function getSelfCloud(rankName, cb) {
        wx.getUserCloudStorage({
            keyList: [rankName],
            success: (res) => {
                if (res.KVDataList[0]) {
                    const obj = JSON.parse(res.KVDataList[0].value);
                    cb(obj.score);
                }
            },
            fail() {
                cb(null);
            }
        });
    }
}

/**
 * 获取可能对游戏感兴趣的未注册的好友名单。每次调用最多可获得 5 个好友。该接口需要用户授权，且只在开放数据域下可用。
 * API文档可见: https://developers.weixin.qq.com/minigame/dev/api/open-api/data/wx.getPotentialFriendList.html
 * 
 * list Array.<FriendInfo> 可能对游戏感兴趣的未注册好友名单
 * string avatarUrl 用户的微信头像 url
 * string nickname 用户的微信昵称
 * string openid 用户 openid
 */
export function getPotentialFriendData() {
    console.log('[WX OpenData] getPotentialFriendData req!');
    return getPotentialFriendList().then((res) => {
        console.log('[WX OpenData] getPotentialFriendData success: ', res);
        return res.list;
    });
}