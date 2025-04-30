/**
 * 下面的内容分成两部分，第一部分是一个模板，模板的好处是能够有一定的语法
 * 坏处是模板引擎一般都依赖 new Function 或者 eval 能力，小游戏下面是没有的
 * 所以模板的编译需要在外部完成，可以将注释内的模板贴到下面的页面内，点击 "run"就能够得到编译后的模板函数
 * https://wechat-miniprogram.github.io/minigame-canvas-engine/playground.html
 * 如果觉得模板引擎使用过于麻烦，也可以手动拼接字符串，本文件对应函数的目标仅仅是为了创建出 xml 节点数
 */
/**
 * https://test-static.easygame2021.com
 * http://localhost:8082/image ==> open-data/render/image
 * value="已收集装扮 件 ==> value="已收集装扮       件
 * value="今日通关 关 ==> value="今日通关   关
 * value="已收集 件装扮" ==> value="已收集       件装扮
 */
/*
<view class="container" id="main">
  <view class="rankList">
      <scrollview class="list" scrollY="true">
      {{~it.data :item:index}}
      <view class="listItem">
        <image src="open-data/render/image/bg.png" class="rankBg"></image>

        <view class="rankNameView">
          <image class="rankNameBg1" src="open-data/render/image/{{= item.isSelf ? 'nameBg1':'nameBg2'}}.png"></image>
          <text class="rankName" value="{{=item.nickname}}"></text>
        </view>
        <image class="rankAvatarBg" src="open-data/render/image/headBg.png"></image>
        <image class="rankAvatar" src="{{= item.avatarUrl }}"></image>
        
          <view class="{{= item.passCount>=4 ? 'notDisplay':'passCountView'}}">
          <image class="passContentBg" src= "open-data/render/image/infobaseBg.png"></image>
            <text class="{{= item.passCount<=0 || item.passCount>=4 ? 'notDisplay':'notPassText'}}" value="今日通关   关"></text>
            <text class="{{= item.passCount>=1 ? 'notDisplay':'notPassText'}}" value="今日尚未通关"></text>
            <text class="{{= item.passCount<=0 || item.passCount>=4  ? 'notDisplay':'passCountText'}}" value="{{=item.passCount}}"></text>
          </view>
           
          <view class="{{= item.passCount>=4 ? 'notPassCountView':'notDisplay'}}">
            <image class="passContentBg" src= "open-data/render/image/infobaseBg.png"></image>
            <image class="{{= item.passCount>=4 ? 'tick':'notDisplay'}}" src= "open-data/render/image/tick.png"></image>
            <text class="{{= item.passCount>=4 ? 'passText':'notDisplay'}}" value="今日全部通关"></text>
          </view>
        <view class="skinCountView">
          <image class="skinCountBg" src="open-data/render/image/infobaseBg.png"></image>
          <text class="skinCountText" value="已收集       件装扮"></text>
          <text class="skinCountText2" value="{{=item.skinCount}}"></text>
        </view>
        <text class="loadingText" value="加载中"></text>
        <image src="{{= item.skinPath}}" class="skinImg"></image>
        <view class="shareToBtn" data-isSelf="{{= item.isSelf ? true : false}}" data-id="{{= item.openid || ''}}">
        </view>
      </view>
      {{~}}
    </scrollview>
    <image src="open-data/render/image/xuehu.png" class="xuehu" >
      <text class="xuehuText" value="哎？你好像没有朋友啊，快去分享吧！"></text>
    </image>
  </view>
</view>
*/
/**
 * xml经过doT.js编译出的模板函数
 * 因为小游戏不支持new Function，模板函数只能外部编译
 * 可直接拷贝本函数到小游戏中使用
 */
export default function anonymous(it) { var out = '<view class="container" id="main"> <view class="rankList"> <scrollview class="list" scrollY="true"> '; var arr1 = it.data; if (arr1) { var item, index = -1, l1 = arr1.length - 1; while (index < l1) { item = arr1[index += 1]; out += ' <view class="listItem"> <image src="open-data/render/image/bg.png" class="rankBg"></image> <view class="rankNameView"> <image class="rankNameBg1" src="open-data/render/image/' + (item.isSelf ? 'nameBg1' : 'nameBg2') + '.png"></image> <text class="rankName" value="' + (item.nickname) + '"></text> </view> <image class="rankAvatarBg" src="open-data/render/image/headBg.png"></image> <image class="rankAvatar" src="' + (item.avatarUrl) + '"></image> <view class="' + (item.passCount >= 4 ? 'notDisplay' : 'passCountView') + '"> <image class="passContentBg" src= "open-data/render/image/infobaseBg.png"></image> <text class="' + (item.passCount <= 0 || item.passCount >= 4 ? 'notDisplay' : 'notPassText') + '" value="今日通关   关"></text> <text class="' + (item.passCount >= 1 ? 'notDisplay' : 'notPassText') + '" value="今日尚未通关"></text> <text class="' + (item.passCount <= 0 || item.passCount >= 4 ? 'notDisplay' : 'passCountText') + '" value="' + (item.passCount) + '"></text> </view> <view class="' + (item.passCount >= 4 ? 'notPassCountView' : 'notDisplay') + '"> <image class="passContentBg" src= "open-data/render/image/infobaseBg.png"></image> <image class="' + (item.passCount >= 4 ? 'tick' : 'notDisplay') + '" src= "open-data/render/image/tick.png"></image> <text class="' + (item.passCount >= 4 ? 'passText' : 'notDisplay') + '" value="今日全部通关"></text> </view> <view class="skinCountView"> <image class="skinCountBg" src="open-data/render/image/infobaseBg.png"></image> <text class="skinCountText" value="已收集       件装扮"></text> <text class="skinCountText2" value="' + (item.skinCount) + '"></text> </view> <text class="loadingText" value="加载中"></text> <image src="' + (item.skinPath) + '" class="skinImg"></image> <view class="shareToBtn" data-isSelf="' + (item.isSelf ? true : false) + '" data-id="' + (item.openid || '') + '"> </view> </view> '; } } out += ' </scrollview> <image src="open-data/render/image/xuehu.png" class="xuehu" > <text class="xuehuText" value="哎？你好像没有朋友啊，快去分享吧！"></text> </image> </view></view>'; return out; }