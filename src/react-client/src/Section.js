import React, { useState } from "react";
import range from "lodash/range";

export default function Section({ title, header, children, footer }) {
  const [count, setCount] = useState(1);

  return (
    <section>
      <h2 className="text-lg font-medium leading-6 text-cool-gray-900">
        {title}

        {children ? (
          <>
            {" "}
            <span className="inline-flex align-text-top rounded-md shadow-sm">
              <button
                type="button"
                className="inline-flex items-center px-1 text-xs font-medium leading-4 text-white transition duration-150 ease-in-out bg-gray-600 border border-transparent rounded hover:bg-gray-500 focus:outline-none focus:border-gray-700 focus:shadow-outline-gray active:bg-gray-700"
                onClick={() => setCount(count > 0 ? count - 1 : 0)}
              >
                -
              </button>
            </span>{" "}
            <span className="inline-flex align-text-top rounded-md shadow-sm">
              <button
                type="button"
                className="inline-flex items-center px-1 text-xs font-medium leading-4 text-white transition duration-150 ease-in-out bg-gray-600 border border-transparent rounded hover:bg-gray-500 focus:outline-none focus:border-gray-700 focus:shadow-outline-gray active:bg-gray-700"
                onClick={() => setCount(count + 1)}
              >
                +
              </button>
            </span>
          </>
        ) : null}

        {header ? <> {header}</> : null}
      </h2>

      {children
        ? range(count).map((index) => <div key={index}>{children}</div>)
        : null}

      {footer ? footer : null}
    </section>
  );
}
