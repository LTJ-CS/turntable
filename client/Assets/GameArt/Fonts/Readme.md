## 运行时字体说明  
* FirstScreenFont.otf -- 专用于首屏字体, 里面只包含了 ASCII 字符, 足够小, 且使用的了 Text 来渲染, 减少首包大小  
* Policy SDF.asset -- 专用于的登录界面的隐私协议的静态 SDF 字体, 因为隐私协议字符很多, 动态生成SDF太卡顿, 且超过了1张1024x1024纹理的限制  
* TMPChineseFont SDF.asset -- 目前的 Dynamic SDF 字体, 没有生成静态的是为了开发方便, 也许以后需要生成静态 SDF 字体, 以优化运行时生成 SDF 字符的性能  
* TMPChineseFont SDF Bold.mat -- 粗体材质  
* TMPChineseFont SDF Underlay.mat -- 带有阴影的材质  

## 编辑时字体  
* TMPChineseFont.ttf -- 游戏中使用的中文字体, 仅支持了常用中文字, 由 "screw-puzzle-client/client/Tools/Fonts/index.js" 精简生成  