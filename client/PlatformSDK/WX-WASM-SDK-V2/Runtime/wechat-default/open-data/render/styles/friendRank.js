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
            height: (201 + 10) * scale,
            flexDirection: 'row',
            alignItems: 'center',
            marginTop: 2 * scale,
        },
        // 整体bg
        rankBg: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: 552 * scale,
            height: 201 * scale,
            color: "#000",
        },
        // 头像背景
        rankAvatarBg: {
            position: 'absolute',
            top: 13 * scale,
            left: 169 * scale,
            width: 50 * scale,
            height: 50 * scale,
        },
        // 头像
        rankAvatar: {
            borderRadius: 46 * scale * 0.5,
            position: 'absolute',
            top: 15 * scale,
            left: 171 * scale,
            width: 46 * scale,
            height: 46 * scale,
        },
        // 名字view
        rankNameView: {
            position: 'absolute',
            top: 19 * scale,
            left: 186 * scale,
            width: 334 * scale,
            height: 40 * scale,
        },
        // 名字bg1
        rankNameBg1: {
            position: 'absolute',
            top: 0 * scale,
            left: 0 * scale,
            width: 334 * scale,
            height: 40 * scale,
        },
        // 名字bg2
        rankNameBg2: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: 334 * scale,
            height: 40 * scale,
        },
        // 名字
        rankName: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: 334 * scale,
            height: 40 * scale,
            textAlign: 'center',
            lineHeight: 40 * scale,
            fontSize: 30 * scale,
            textOverflow: 'ellipsis',
            color: '#ffffff',
        },
        // 今日尚未通关或未全部通关
        passCountView: {
            position: 'absolute',
            top: 80 * scale,
            left: 186 * scale,
            width: 334 * scale,
            height: 40 * scale,
        },
        // 通关数背景
        passContentBg: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: 334 * scale,
            height: 40 * scale,
        },
        // 今日通关   关
        notPassText: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: 334 * scale,
            height: 40 * scale,
            textAlign: 'center',
            lineHeight: 40 * scale,
            fontSize: 30 * scale,
            textOverflow: 'ellipsis',
            color: '#000000',
        },
        // 通关数量
        passCountText: {
            position: 'absolute',
            top: 0,
            left: 45 * scale,
            width: 334 * scale,
            height: 40 * scale,
            textAlign: 'center',
            lineHeight: 40 * scale,
            fontSize: 30 * scale,
            textOverflow: 'ellipsis',
            color: '#ff0000',
        },

        notPassCountView: {
            position: 'absolute',
            top: 80 * scale,
            left: 186 * scale,
            width: 334 * scale,
            height: 40 * scale,

        },

        tick: {
            top: 3 * scale,
            left: 40 + 20 * scale,
            width: 56 / 1.8 * scale,
            height: 59 / 1.8 * scale,
        },
        passText: {
            position: 'absolute',
            top: 0,
            left: 0 + 20 * scale,
            width: 334 * scale,
            height: 40 * scale,
            textAlign: 'center',
            lineHeight: 40 * scale,
            fontSize: 30 * scale,
            textOverflow: 'ellipsis',
            color: '#000000',
        },

        skinCountView: {
            position: 'absolute',
            top: 140 * scale,
            left: 186 * scale,
            width: 334 * scale,
            height: 40 * scale,

        },
        skinCountBg: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: 334 * scale,
            height: 40 * scale,
        },
        skinCountText: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: 334 * scale,
            height: 40 * scale,
            textAlign: 'center',
            lineHeight: 40 * scale,
            fontSize: 30 * scale,
            textOverflow: 'ellipsis',
            color: '#000000',
        },
        skinCountText2: {
            position: 'absolute',
            top: 0,
            left: 0,
            width: 334 * scale,
            height: 40 * scale,
            textAlign: 'center',
            lineHeight: 40 * scale,
            fontSize: 30 * scale,
            textOverflow: 'ellipsis',
            color: '#0042ff',
        },
        skinImg: {
            position: 'absolute',
            top: -3 * scale,
            left: 40 * scale,
            width: 100 * scale,
            height: 200 * scale,
        },

        arrowView: {
            position: 'absolute',
            bottom: 0 * scale,
            left: 0 * scale,
            width: 552 * scale,
            height: 200 * scale,
        },
        pageBg: {
            position: 'absolute',
            bottom: 50 * scale,
            left: 131.5 * scale,
            width: 289 * scale,
            height: 76 * scale,
        },
        pageText: {
            position: 'absolute',
            top: 18 * scale,
            left: -20 * scale,
            width: 334 * scale,
            height: 36 * scale,
            textAlign: 'center',
            lineHeight: 36 * scale,
            fontSize: 36 * scale,
            textOverflow: 'ellipsis',
            color: '#000000',
        },

        arrowsLeftView: {
            width: 60 * scale,
            height: 84 * scale,
            top: 70 * scale,
            left: 40 * scale,
        },

        arrowsLeft: {
            width: 60 * scale,
            height: 84 * scale,
            top: 0 * scale,
            left: 0 * scale,
        },

        arrowsRightView: {
            width: 60 * scale,
            height: 84 * scale,
            top: -12 * scale,
            left: 452 * scale,
        },

        arrowsRight: {
            width: 60 * scale,
            height: 84 * scale,
            top: 0 * scale,
            left: 0 * scale,
        },

        // 不展示
        notDisplay: {
            opacity: 0,
            left: -500 * scale,
        },

        // 加载中
        loadingText: {
            position: 'absolute',
            top: 96 * scale,
            left: 64 * scale,
            width: 30 * scale,
            height: 30 * scale,
            textAlign: 'left',
            lineHeight: 18 * scale,
            fontSize: 18 * scale,
            color: '#000000',
        },

        // 雪狐
        xuehu: {
            position: 'absolute',
            top: 353.75 * scale,
            left: 226.5 * scale,
            width: 197 / 2 * scale,
            height: 385 / 2 * scale,
        },
        // 雪狐骚话
        xuehuText: {
            position: 'absolute',
            top: 208 * scale,
            left: -150 * scale,
            width: 420 * scale,
            height: 36 * scale,
            textAlign: 'center',
            lineHeight: 24 * scale,
            fontSize: 24 * scale,
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
            top: -10 * scale,
            left: -30 * scale,
            width: 400 * scale,
            height: 180 * scale,
        },
    };
}
