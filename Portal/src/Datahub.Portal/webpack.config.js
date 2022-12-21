const path = require('path');
const glob = require('glob');
const TerserPlugin = require('terser-webpack-plugin');

module.exports = {
    mode: "production",
    entry: { 'app': glob.sync('./js/**/*.js') },
    output: {
        filename: 'aebundle.js',
        path: path.resolve(__dirname, "wwwroot/js"),
        sourceMapFilename: 'aebundle.map'
    },
    module: {
        rules: [{
            exclude: /node_modules/
        }]
    },
    devtool: "source-map", // enum
    optimization: {
        minimizer: [new TerserPlugin({
            extractComments: false,
        })],
    }
};