/**
 * * 使用原始字库及"常用汉字.txt"生成精简的字体
 * @remark 需要先安装 node.js, 并在文件所有目录下执行 npm install 来安装依赖的 "fontmin" 库 
 */

var Fontmin = require('fontmin');

const fs = require('fs');

const text = fs.readFileSync('常用汉字.txt', 'utf8');

var fontmin = new Fontmin()
	.src('TMPChineseFont.ttf')
	.use(
		Fontmin.glyph({
			text: text + " 　", // 需要包含空格
			hinting: false,
			trim: false			// 需要包含空格
		})
	)
	.dest('./build');

fontmin.run(function (err, files) {
	if (err) {
		throw err;
	}

	// 相对路径
	const targetFontRelativePath = 'Assets/GameArt/Fonts/TMPChineseFont.ttf';
	const targetFontAbsolutePath = __dirname + '/../../' + targetFontRelativePath;
	// 复制字体文件到 Unity 资源目录
	fs.copyFileSync('./build/TMPChineseFont.ttf', targetFontAbsolutePath);
	const targetFontSize = fs.statSync(targetFontAbsolutePath).size / (1024 * 1024);
	console.log(`生成字体结束: ${targetFontRelativePath}, 大小: ${targetFontSize.toFixed(2)} MB`);
});