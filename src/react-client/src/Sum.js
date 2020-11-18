import React, { useState } from "react";
import Section from "./Section";
import useFusionSubscription from "./lib/useFusionSubscription";
import LoadingSVG from "./LoadingSVG";

export default function SumSection() {
  const { data, loading, error } = useFusionSubscription(
    "/api/sum/getAccumulator"
  );

  return (
    <Section
      title="Sum"
      header={<Accumulator data={data} loading={loading} error={error} />}
    >
      <Sum accumulator={data} />
    </Section>
  );
}

function Accumulator({ data, loading, error }) {
  const [fetching, setFetching] = useState(false);

  const handleIncrement = () => {
    setFetching(true);
    fetch("/api/Sum/accumulate?value=1", {
      method: "POST",
    }).then(() => setFetching(false));
  };

  const handleReset = () => {
    setFetching(true);
    fetch("/api/Sum/reset", {
      method: "POST",
    }).then(() => setFetching(false));
  };

  return loading || error ? (
    <>
      {" "}
      <LoadingSVG className="inline-block align-text-top" />
    </>
  ) : (
    <>
      <span className="inline-flex align-text-top rounded-md shadow-sm">
        <button
          type="button"
          className="inline-flex items-center px-1 text-xs font-medium leading-4 text-white transition duration-150 ease-in-out bg-gray-600 border border-transparent rounded hover:bg-gray-500 focus:outline-none focus:border-gray-700 focus:shadow-outline-gray active:bg-gray-700"
          onClick={handleIncrement}
        >
          Increment
        </button>
      </span>{" "}
      <span className="inline-flex align-text-top rounded-md shadow-sm">
        <button
          type="button"
          className="inline-flex items-center px-1 text-xs font-medium leading-4 text-white transition duration-150 ease-in-out bg-gray-600 border border-transparent rounded hover:bg-gray-500 focus:outline-none focus:border-gray-700 focus:shadow-outline-gray active:bg-gray-700"
          onClick={handleReset}
        >
          Reset
        </button>
      </span>{" "}
      <span className="inline-block ml-3 text-sm font-normal align-text-bottom">
        Accumulator: {data ?? ""}
      </span>
      {fetching ? (
        <>
          {" "}
          <LoadingSVG className="inline-block align-text-top" />
        </>
      ) : null}
    </>
  );
}

function Sum({ accumulator }) {
  const [wait, setWait] = useState(3000);
  const [a, setA] = useState(5);
  const [b, setB] = useState(8);

  const handleChange = (setNum) => {
    return ({ target: { value } }) => {
      const num = Number(value);
      if (!isNaN(num)) {
        setNum(num);
      }
    };
  };

  const {
    data,
    loading,
    error,
  } = useFusionSubscription(
    `/api/sum/sum?values=${a}&values=${b}&addAccumulator=True`,
    null,
    { options: { wait: wait } }
  );

  return loading ? (
    "Loading..."
  ) : error ? (
    "There was an error!"
  ) : (
    <div className="mt-2">
      <div>
        <span className="inline-block mr-1 text-sm font-normal align-text-bottom">
          Wait
        </span>
        <input
          className="w-12 px-2 py-1 text-xs leading-5 text-center align-bottom transition duration-150 ease-in-out bg-gray-200 border border-gray-300 rounded"
          defaultValue={wait}
          onChange={handleChange(setWait)}
        />
        <span className="inline-block ml-1 text-sm font-normal align-text-bottom">
          ms
        </span>
      </div>
      <div className="mt-3">
        <span className="inline-block mr-2 text-sm font-normal align-text-bottom">
          A:
        </span>
        <input
          className="w-12 px-2 py-1 mr-6 text-xs leading-5 text-center align-bottom transition duration-150 ease-in-out bg-gray-200 border border-gray-300 rounded"
          defaultValue={a}
          onChange={handleChange(setA)}
        />
        <span className="inline-block mr-2 text-sm font-normal align-text-bottom">
          B:
        </span>
        <input
          className="w-12 px-2 py-1 text-xs leading-5 text-center align-bottom transition duration-150 ease-in-out bg-gray-200 border border-gray-300 rounded"
          defaultValue={b}
          onChange={handleChange(setB)}
        />
      </div>

      <div className="mt-3">
        {a} + {b} + {accumulator ?? ""} = {data}
      </div>
    </div>
  );
}
