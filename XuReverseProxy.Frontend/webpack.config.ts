/**
 * Define paths to any stylesheets you wish to include at the top of the CSS bundle.
 */
const stylesheets = ["./src/styles/index.scss"];

/**
 * Change this to `true` to generate source maps alongside your production bundle. This is useful for debugging, but
 * will increase total bundle size and expose your source code.
 */
const sourceMapsInProduction = false;

/*********************************************************************************************************************/
/**********                                             Webpack                                             **********/
/*********************************************************************************************************************/

import Webpack from "webpack";
import WebpackDev from "webpack-dev-server";
import { VueLoaderPlugin } from "vue-loader";
import MiniCssExtractPlugin from "mini-css-extract-plugin";
import MonacoWebpackPlugin from 'monaco-editor-webpack-plugin';
import path from "path";

const mode = process.env.NODE_ENV ?? "development";
const isProduction = mode === "production";
const isDevelopment = !isProduction;

const forceAspBundle = process.env.FORCE_ASP_BUNDLE == "true";
const useAspPaths = isProduction || forceAspBundle;

const config: Configuration = {
  mode: isProduction ? "production" : "development",
  entry: {
    serverui: [...stylesheets, "./src/server-ui-public.ts"],
    'serverui-admin': [...stylesheets, "./src/server-ui-admin.ts"]
  },
  watchOptions: {
    poll: 1000
  },
  resolve: {
    alias: {
      vue: "@vue/runtime-dom",
      "@generated": path.resolve(__dirname, "./src/generated"),
      "@pages": path.resolve(__dirname, "./src/pages"),
      "@gfx": path.resolve(__dirname, "./src/gfx"),
      "@components": path.resolve(__dirname, "./src/components"),
      "@factories": path.resolve(__dirname, "./src/factories"),
      "@services": path.resolve(__dirname, "./src/services"),
      "@utils": path.resolve(__dirname, "./src/utils"),
      "src": path.resolve(__dirname, "./src")
    },
    extensions: [".js", ".ts", ".vue", ".scss"],
  },
  output: {
    path: useAspPaths
      ? path.resolve(__dirname, "../XuReverseProxy/wwwroot/dist")
      : path.resolve(__dirname, "public/build"),
    publicPath: useAspPaths ? "../XuReverseProxy/wwwroot/dist/" : "/build/",
    filename: "[name].js",
    chunkFilename: "[name].[id].js",
  },
  module: {
    rules: [
      // Rule: SASS and CSS files from Vue Single File Components
      {
        test: /\.vue\.(s?[ac]ss)$/,
        use: ['vue-style-loader', 'css-loader', 'sass-loader']
      },
      // Rule: SASS and CSS files (standalone)
      {
          test: /(?<!\.vue)\.(s?[ac]ss)$/,
          use: [MiniCssExtractPlugin.loader, 'css-loader', 'sass-loader']
      },

      // Rule: Vue
      {
        test: /\.vue$/,
        exclude: /node_modules/,
        use: {
          loader: "vue-loader",
          options: {},
        },
      },

      // Rule: TypeScript
      {
        test: /\.ts$/,
        exclude: /node_modules/,
        use: {
          loader: "ts-loader",
          options: {
            appendTsSuffixTo: [/\.vue$/],
          },
        },
      },

      // Rule: FILES
      {
        test: /\.(png|jpg|gif|svg|ttf)$/,
        loader: 'file-loader',
        options: {
          name: '[name].[ext]?[hash]'
        }
      }
    ],
  },
  devServer: {
    hot: true,
  },
  target: isDevelopment ? "web" : "browserslist",
  plugins: [
    new VueLoaderPlugin(),
    new Webpack.optimize.LimitChunkCountPlugin({
      maxChunks: 1,
    }),
    new Webpack.DefinePlugin({
      PRODUCTION: JSON.stringify(true),
      // VUE
      // - enable Options API support
      __VUE_OPTIONS_API__: true,
      // - disable devtools support in production
      __VUE_PROD_DEVTOOLS__: true
    }),
    new MonacoWebpackPlugin({
      languages: [ 'json' ],
      // filename: '[name].worker.js'
      publicPath: '/'
    }),
    new MiniCssExtractPlugin({
      filename: "[name].css",
    })
  ],
  devtool: isProduction && !sourceMapsInProduction ? false : "source-map",
  stats: {
    chunks: false,
    chunkModules: false,
    modules: false,
    assets: true,
    entrypoints: false,
  },
};

/**
 * This interface combines configuration from `webpack` and `webpack-dev-server`. You can add or override properties
 * in this interface to change the config object type used above.
 */
export interface Configuration
  extends Webpack.Configuration,
    WebpackDev.Configuration {}

/*********************************************************************************************************************/
/**********                                             Advanced                                            **********/
/*********************************************************************************************************************/

// Configuration for production bundles
if (isProduction) {
  // Minify CSS files
  config.plugins?.push(
    new Webpack.LoaderOptionsPlugin({
      minimize: true,
    })
  );

  // Minify and treeshake JS
  if (config.optimization === undefined) {
    config.optimization = {};
  }

  config.optimization.minimize = true;
}

// Parse as JSON5 to add support for comments in tsconfig.json parsing.
require("require-json5").replace();

export default config;
