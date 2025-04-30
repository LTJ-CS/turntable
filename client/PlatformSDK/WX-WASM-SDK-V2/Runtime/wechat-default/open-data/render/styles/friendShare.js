export default function getStyle(data) {
    // console.log(" 子域 getStyle data ", data);
    // width: 1104, height: 2000
    const scale = data.width / 552;
    return {
        container: {
            width: data.width,
            height: data.height,
            borderRadius: 0,
            paddingLeft: 0,
            paddingRight: 0,
        },
        // 头部flag
        headerFlag: {
            position: 'relative',
            width: 288 * scale,
            height: (50 + 10) * scale,
            flexDirection: 'row',
            alignItems: 'left',
            marginTop: 10 * scale,
        },
        // 头部flag
        headerFlag2: {
            position: 'relative',
            width: 288 * scale,
            height: (50 + 10) * scale,
            flexDirection: 'row',
            alignItems: 'left',
            marginTop: 10 * scale,
        },
        // 头部flag bg
        headerFlagBg: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: 268 * scale,
            height: 55 * scale,
            color: "#FFFFFF",
        },
        // 头部flag名字
        headerFlagName: {
            position: 'absolute',
            top: 3 * scale,
            left: 20 * scale,
            width: 224 * scale,
            height: 40 * scale,
            textAlign: 'left',
            lineHeight: 40 * scale,
            fontSize: 30 * scale,
            textOverflow: 'ellipsis',
            color: '#FFFFFF',
        },
        rankList: {
            width: data.width,
            height: data.height,
        },
        list: {
            width: data.width,
            height: data.height,
        },
        listItem: {
            position: 'relative',
            width: 552 * scale,
            height: (120 + 10) * scale,
            flexDirection: 'row',
            alignItems: 'center',
            marginTop: 10 * scale,
        },
        listItem2: {
            position: 'relative',
            width: 552 * scale,
            height: (120 + 10) * scale,
            flexDirection: 'row',
            alignItems: 'center',
            marginTop: 10 * scale,
        },
        // 整体bg
        rankBg: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: 552 * scale,
            height: 120 * scale,
            color: "#000",
        },
        // 头像背景
        rankAvatarBg: {
            position: 'absolute',
            top: 23 * scale,
            left: 15 * scale,
            width: 75 * scale,
            height: 75 * scale,
        },
        // 头像
        rankAvatar: {
            borderRadius: 69 * scale * 0.5,
            position: 'absolute',
            top: 26 * scale,
            left: 18 * scale,
            width: 69 * scale,
            height: 69 * scale,
        },
        // 名字view
        rankNameView: {
            position: 'absolute',
            top: 32 * scale,
            left: 110 * scale,
            width: 304 * scale,
            height: 50 * scale,
        },
        // 名字
        rankName: {
            position: 'absolute',
            top: 10 * scale,
            left: 10 * scale,
            width: 290 * scale,
            height: 50 * scale,
            textAlign: 'left',
            lineHeight: 36 * scale,
            fontSize: 36* scale,
            textOverflow: 'ellipsis',
            color: '#000000',
        },

        // 雪狐
        xuehu: {
            position: 'absolute',
            top: 180 * scale,
            left: 210 * scale,
            width: 197 * 0.7 * scale,
            height: 385 * 0.7 * scale,
        },
        // 雪狐骚话
        xuehuText: {
            position: 'absolute',
            top: 328 * scale,
            left: -200 * scale,
            width: 550 * scale,
            height: 36 * scale,
            textAlign: 'center',
            lineHeight: 30 * scale,
            fontSize: 30 * scale,
            textOverflow: 'ellipsis',
            color: '#000000',
        },


        shareNameView: {
            position: 'relative',
            marginLeft: data.width * 0.06,
            width: data.width * 0.35,
            height: (data.height / 2 / 3) * 0.4,
        },
        shareNameBg: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: data.width * 0.35,
            height: (data.height / 2 / 3) * 0.4,
        },
        shareName: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: data.width * 0.35,
            height: (data.height / 2 / 3) * 0.4,
            textAlign: 'center',
            lineHeight: (data.height / 2 / 3) * 0.4,
            fontSize: data.width * 0.043,
            textOverflow: 'ellipsis',
            color: '#fff',
        },
        shareToBtn: {
            position: 'relative',
            top: 0,
            left: 0,
            width: 552 * scale,
            height: 101 * scale,
        },
        shareToBtnBg: {
            position: 'absolute',
            top: 10 * scale,
            left: 410 * scale,
            width: 122 * scale,
            height: 74 * scale,
        },
        shareToBtnTip: {
            position: 'absolute',
            top: 25 * scale,
            left: 410 * scale,
            width: 122 * scale,
            height: 61 * scale,
            textAlign: 'center',
            lineHeight: 40 * scale,
            fontSize: 36 * scale,
            textOverflow: 'ellipsis',
            color: '#000000',
        },
    };
}
