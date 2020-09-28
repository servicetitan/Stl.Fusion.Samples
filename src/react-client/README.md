# ⚠️ Warning! ⚠️ - This code is still experimental!

**NOTE:** The sample code project currently requires running the Blazor sample server.

Basic usage:

```js
import useStlFusion from "./lib/useStlFusion";

export default function Time() {
  const { data, loading, error } = useStlFusion("/api/Time/get");

  return loading ? (
    <div>loading...</div>
  ) : error ? (
    <div>failed to load</div>
  ) : (
    <div>The time is {data}!</div>
  );
}
```

Context configuration:

```js
import StlFusionConfig from "./lib/StlFusionConfig";

ReactDOM.render(
  <React.StrictMode>
    <StlFusionConfig uri="ws://localhost:5005/fusion/ws" wait={300}>
      <App />
    </StlFusionConfig>
  </React.StrictMode>,
  document.getElementById("root")
);
```

Hook configuration:

```js
import useStlFusion from "./lib/useStlFusion";

export default function Time() {
  const { data, loading, error } = useStlFusion(
    "/api/Time/get",
    { headers: { "Content-Type": "application/json" } },
    {
      uri: "ws://localhost:5005/fusion/ws",
      options: { wait: 300 },
    }
  );

  return loading ? (
    <div>loading...</div>
  ) : error ? (
    <div>failed to load</div>
  ) : (
    <div>The time is {data}!</div>
  );
}
```

Custom fetcher, using `axios` as an example:

```js
import StlFusionConfig from "./lib/StlFusionConfig";
import axios from "axios";

ReactDOM.render(
  <React.StrictMode>
    <StlFusionConfig
      uri="ws://localhost:5005/fusion/ws"
      wait={300}
      fetcher={async (url, params) => {
        const { data, headers } = await axios.get(url, {
          ...params,
          headers: { ...params?.headers, "x-fusion-publish": 1 },
        });

        return { data, header: JSON.parse(headers["x-fusion-publication"]) };
      }}
    >
      <App />
    </StlFusionConfig>
  </React.StrictMode>,
  document.getElementById("root")
);
```

---

This project was bootstrapped with [Create React App](https://github.com/facebook/create-react-app).

## Available Scripts

In the project directory, you can run:

### `npm start`

Runs the app in the development mode.<br />
Open [http://localhost:3000](http://localhost:3000) to view it in the browser.

The page will reload if you make edits.<br />
You will also see any lint errors in the console.

### `npm test`

Launches the test runner in the interactive watch mode.<br />
See the section about [running tests](https://facebook.github.io/create-react-app/docs/running-tests) for more information.

### `npm run build`

Builds the app for production to the `build` folder.<br />
It correctly bundles React in production mode and optimizes the build for the best performance.

The build is minified and the filenames include the hashes.<br />
Your app is ready to be deployed!

See the section about [deployment](https://facebook.github.io/create-react-app/docs/deployment) for more information.

### `npm run eject`

**Note: this is a one-way operation. Once you `eject`, you can’t go back!**

If you aren’t satisfied with the build tool and configuration choices, you can `eject` at any time. This command will remove the single build dependency from your project.

Instead, it will copy all the configuration files and the transitive dependencies (webpack, Babel, ESLint, etc) right into your project so you have full control over them. All of the commands except `eject` will still work, but they will point to the copied scripts so you can tweak them. At this point you’re on your own.

You don’t have to ever use `eject`. The curated feature set is suitable for small and middle deployments, and you shouldn’t feel obligated to use this feature. However we understand that this tool wouldn’t be useful if you couldn’t customize it when you are ready for it.

## Learn More

You can learn more in the [Create React App documentation](https://facebook.github.io/create-react-app/docs/getting-started).

To learn React, check out the [React documentation](https://reactjs.org/).

### Code Splitting

This section has moved here: https://facebook.github.io/create-react-app/docs/code-splitting

### Analyzing the Bundle Size

This section has moved here: https://facebook.github.io/create-react-app/docs/analyzing-the-bundle-size

### Making a Progressive Web App

This section has moved here: https://facebook.github.io/create-react-app/docs/making-a-progressive-web-app

### Advanced Configuration

This section has moved here: https://facebook.github.io/create-react-app/docs/advanced-configuration

### Deployment

This section has moved here: https://facebook.github.io/create-react-app/docs/deployment

### `npm run build` fails to minify

This section has moved here: https://facebook.github.io/create-react-app/docs/troubleshooting#npm-run-build-fails-to-minify