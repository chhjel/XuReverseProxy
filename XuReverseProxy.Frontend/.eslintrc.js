module.exports = {
  env: {
    node: false,
    browser: true,
    es2021: true,
  },
  parserOptions: {
    sourceType: "module",
  },
  extends: ["plugin:prettier/recommended"],
  rules: {
    "prettier/prettier": "warn",
  },
  overrides: [
    {
      files: "*.ts",
      parser: "@typescript-eslint/parser",
      plugins: ["@typescript-eslint", "import", "prettier"],
      parserOptions: {
        project: "tsconfig.json",
      },
      extends: [
        "airbnb-typescript/base",
        "eslint:recommended",
        "plugin:@typescript-eslint/recommended",
        "plugin:@typescript-eslint/eslint-recommended",
        "plugin:@typescript-eslint/recommended-requiring-type-checking",
        "plugin:prettier/recommended",
      ],
      rules: {
        "no-plusplus": "off",
        "no-underscore-dangle": "off",
        "import/prefer-default-export": "off",
        "prettier/prettier": "warn",
        "@typescript-eslint/no-explicit-any": "off",
        "@typescript-eslint/no-use-before-define": "off",
      },
    },
  ],
};
