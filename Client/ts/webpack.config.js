const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');
const path = require('path');

module.exports = {
    entry: {
        App: './src/Index.ts',
    },
    output: {
        path: path.resolve(__dirname, '../wwwroot/dist'),
        filename: '[name].js'
    },
    module: {
        rules: [{
            test: /\.ttf$/,
            use: ['file-loader']
        }, {
            test: /\.css$/i,
            use: ['style-loader', 'css-loader'],
        }, {
            test: /\.scss$/i,
            use: ['style-loader', 'sass-loader'],
            exclude: /node_modules/,
        }, {
            test: /\.ts$/,
            loader: "ts-loader",
            exclude: /node_modules/,
        }]
    },
    resolve: {
        extensions: ['.js', '.ts', '.css', '.scss'],
    },
    plugins: [
        new MonacoWebpackPlugin()
    ]
};
