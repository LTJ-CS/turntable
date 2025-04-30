/*
 *                        .::::.
 *                      .::::::::.
 *                     :::::::::::
 *                  ..:::::::::::'
 *               '::::::::::::'
 *                 .::::::::::
 *            '::::::::::::::..
 *                 ..::::::::::::.
 *               ``::::::::::::::::
 *                ::::``:::::::::'        .:::.
 *               ::::'   ':::::'       .::::::::.
 *             .::::'      ::::     .:::::::'::::.
 *            .:::'       :::::  .:::::::::' ':::::.
 *           .::'        :::::.:::::::::'      ':::::.
 *          .::'         ::::::::::::::'         ``::::.
 *      ...:::           ::::::::::::'              ``::.
 *     ````':.          ':::::::::'                  ::::..
 *                        '.:::::'                    ':'````..
 *
 */


public static class Reportkey
{
    // ******** ******** ******** ******** ******** ******** ******** ********
    // ******** ******** ******** ****** 登录 ****** ******** ******** ********
    // ******** ******** ******** ******** ******** ******** ******** ********

    /// <summary>
    /// 注册
    /// </summary>
    public const string Register = "register";

    /// <summary>
    /// 登录（在TD登录时自动触发）
    /// </summary>
    public const string Login = "login";

    /// <summary>
    /// 登出（小游戏平台应该不会触发吧）
    /// </summary>
    public const string Logout = "logout";

    /// <summary>
    /// 登出（小游戏平台应该不会触发吧）
    /// </summary>
    public const string LaunchFrom = "launch_from";

    /// <summary>
    /// 加载游戏
    /// </summary>
    public const string LoadGame = "load_game";   
    
    /// <summary>
    /// 游戏指引
    /// </summary>
    public const string Guide = "guide";

    // ******** ******** ******** ******** ******** ******** ******** ********
    // ******** ******** ******** **** 广告相关 **** ******** ******** ********
    // ******** ******** ******** ******** ******** ******** ******** ********

    /// <summary>
    /// 广告状态
    /// </summary>
    public const string AdvStatus = "adv_status";

    // ******** ******** ******** ******** ******** ******** ******** ********
    // ******** ******** ******** *** 界面相关 *** ******** ******** ********
    // ******** ******** ******** ******** ******** ******** ******** ********

    /// <summary>
    /// 打开体力弹窗
    /// </summary>
    public const string EnergyOpen = "energy_open";

    /// <summary>
    /// 打开暂停弹窗
    /// from: 主动点击 被动触发
    /// type: 点击离开 点击继续
    /// </summary>
    public const string PauseOpen = "pause_open";

    // ******** ******** ******** ******** ******** ******** ******** ********
    // ******** ******** ******** *** 主界面设置 *** ******** ******** ********
    // ******** ******** ******** ******** ******** ******** ******** ********

    /// <summary>
    /// 关闭设置
    /// music_status    音乐开关    数值
    /// se_status    音效开关    数值
    /// vib_status    振动开关    数值
    /// </summary>
    public const string CloseSetup = "close_setup";

    /// <summary>
    /// 复制uid
    /// </summary>
    public const string ClickCopyUid = "click_copyUid";

    /// <summary>
    /// 点击用户协议
    /// </summary>
    public const string ClickAgreePage = "click_agree_page";

    /// <summary>
    /// 点击隐私政策
    /// </summary>
    public const string ClickPrivacyPage = "click_privacy_page";

    // ******** ******** ******** ******** ******** ******** ******** ********
    // ******** ******** ******** * 关卡内设置界面 * ******** ******** ********
    // ******** ******** ******** ******** ******** ******** ******** ********

    /// <summary>
    /// 关内打开设置
    /// </summary>
    public const string ClickLevelSetup = "click_level_setup";

    /// <summary>
    /// 关内关闭设置
    /// music_status    音乐开关    数值
    /// se_status    音效开关    数值
    /// vib_status    振动开关    数值
    /// </summary>
    public const string CloseLevelSetup = "close_level_setup";

    /// <summary>
    /// 关内复制uid
    /// </summary>
    public const string ClickLevelCopyUid = "click_level_copyUid";

    /// <summary>
    /// 关内点击用户协议
    /// </summary>
    public const string ClickLevelAgreePage = "click_level_agree_page";

    /// <summary>
    /// 关内点击隐私政策
    /// </summary>
    public const string ClickLevelPrivacyPage = "click_level_privacy_page";

    /// <summary>
    /// 添加侧边栏
    /// </summary>
    public const string TtClickAddSide = "tt_click_addside";

    /// <summary>
    /// 结束录屏
    /// </summary>
    public const string TtRecordEnd = "tt_record_end";

    /// <summary>
    /// 开始录屏
    /// </summary>
    public const string TtRecordStart = "tt_record_start";

    /// <summary>
    /// T台相关埋点
    /// 1. action=1,点击首页的邀请按钮后上报。
    /// 2. action=2,点击弹窗上的邀请按钮后上报。
    /// 3. action=3，从弹窗/首页邀请按钮分享的链接中进入游戏后上报。
    /// 4. action=4，成功成为伴舞后上报。
    /// </summary>
    public const string InviteClick = "invite_click";
}