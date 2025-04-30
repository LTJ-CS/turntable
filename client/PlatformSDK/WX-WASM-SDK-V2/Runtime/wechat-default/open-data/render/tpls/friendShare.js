/**
 * 下面的内容分成两部分，第一部分是一个模板，模板的好处是能够有一定的语法
 * 坏处是模板引擎一般都依赖 new Function 或者 eval 能力，小游戏下面是没有的
 * 所以模板的编译需要在外部完成，可以将注释内的模板贴到下面的页面内，点击 "run"就能够得到编译后的模板函数
 * https://wechat-miniprogram.github.io/minigame-canvas-engine/playground.html
 * 如果觉得模板引擎使用过于麻烦，也可以手动拼接字符串，本文件对应函数的目标仅仅是为了创建出 xml 节点数
 */
/**
 * https://test-static.easygame2021.com
 * http://localhost:8080/image ==> open-data/render/image
 */
/*
<view class="container" id="main">
  <view class="rankList">
    <scrollview class="list" scrollY="true">
      <view class="headerFlag">
        <image class="headerFlagBg" src="open-data/render/image/flag-bg-red.png"></image>
        <text class="headerFlagName" value="有装扮的好友"></text>
      </view>
      {{~it.data :item:index}}
      <view class="listItem">
        <image src="open-data/render/image/bg.png" class="rankBg"></image>
        <view class="rankNameView">
          <text class="rankName" value="{{=item.nickname}}"></text>
        </view>
        <image class="rankAvatarBg" src="open-data/render/image/headBg.png"></image>
        <image class="rankAvatar" src="{{= item.avatarUrl }}"></image>
        <view class="shareToBtn" data-isSelf="{{= item.isSelf ? true : false}}" data-id="{{= item.openid || ''}}">          
          <image class="shareToBtnBg" src="open-data/render/image/button.png"></image>
          <text class="shareToBtnTip" value="赠送"></text>
        </view>
      </view>
      {{~}}
      <view class="headerFlag2">
        <image class="headerFlagBg" src="open-data/render/image/flag-bg-blue.png"></image>
        <text class="headerFlagName" value="没有装扮的好友"></text>
      </view>
      {{~it.data2 :item:index}}
      <view class="listItem2">
        <image src="open-data/render/image/bg.png" class="rankBg"></image>
        <view class="rankNameView">
          <text class="rankName" value="{{=item.nickname}}"></text>
        </view>
        <image class="rankAvatarBg" src="open-data/render/image/headBg.png"></image>
        <image class="rankAvatar" src="{{= item.avatarUrl }}"></image>
        <view class="shareToBtn" data-isSelf="{{= item.isSelf ? true : false}}" data-id="{{= item.openid || ''}}">          
          <image class="shareToBtnBg" src="open-data/render/image/button.png"></image>
          <text class="shareToBtnTip" value="赠送"></text>
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
export default function anonymous(it){var out='<view class="container" id="main"> <view class="rankList"> <scrollview class="list" scrollY="true"> <view class="headerFlag"> <image class="headerFlagBg" src="open-data/render/image/flag-bg-red.png"></image> <text class="headerFlagName" value="有装扮的好友"></text> </view> ';var arr1=it.data;if(arr1){var item,index=-1,l1=arr1.length-1;while(index<l1){item=arr1[index+=1];out+=' <view class="listItem"> <image src="open-data/render/image/bg.png" class="rankBg"></image> <view class="rankNameView"> <text class="rankName" value="'+(item.nickname)+'"></text> </view> <image class="rankAvatarBg" src="open-data/render/image/headBg.png"></image> <image class="rankAvatar" src="'+(item.avatarUrl)+'"></image> <view class="shareToBtn" data-isSelf="'+(item.isSelf?true:false)+'" data-id="'+(item.openid||'')+'">           <image class="shareToBtnBg" src="open-data/render/image/button.png"></image> <text class="shareToBtnTip" value="赠送"></text> </view> </view> '}}out+=' <view class="headerFlag2"> <image class="headerFlagBg" src="open-data/render/image/flag-bg-blue.png"></image> <text class="headerFlagName" value="没有装扮的好友"></text> </view> ';var arr2=it.data2;if(arr2){var item,index=-1,l2=arr2.length-1;while(index<l2){item=arr2[index+=1];out+=' <view class="listItem2"> <image src="open-data/render/image/bg.png" class="rankBg"></image> <view class="rankNameView"> <text class="rankName" value="'+(item.nickname)+'"></text> </view> <image class="rankAvatarBg" src="open-data/render/image/headBg.png"></image> <image class="rankAvatar" src="'+(item.avatarUrl)+'"></image> <view class="shareToBtn" data-isSelf="'+(item.isSelf?true:false)+'" data-id="'+(item.openid||'')+'">           <image class="shareToBtnBg" src="open-data/render/image/button.png"></image> <text class="shareToBtnTip" value="赠送"></text> </view> </view> '}}out+=' </scrollview> <image src="open-data/render/image/xuehu.png" class="xuehu" > <text class="xuehuText" value="哎？你好像没有朋友啊，快去分享吧！"></text> </image> </view></view>';return out}